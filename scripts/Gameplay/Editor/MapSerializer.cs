using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay.Editor;

public class MapHeaderDto
{
    public int Width { get; set; } = 32;
    public int Height { get; set; } = 32;
    public string Biome { get; set; } = "plains";
    public string StageTitle { get; set; } = "Stage 1";
}

public class MapCellDto
{
    public int Q { get; set; }
    public int R { get; set; }
    public int Terrain { get; set; }
    public string? Deposit { get; set; }
    public int Improvement { get; set; }
    public int Owner { get; set; } = -1;
}

public class MapUnitDto
{
    public string Type { get; set; } = "cam_ve_quan";
    public int Q { get; set; }
    public int R { get; set; }
    public int Faction { get; set; }
    public int Hp { get; set; } = 20;
}

public class MapFileDto
{
    public MapHeaderDto Header { get; set; } = new();
    public List<MapCellDto> Cells { get; set; } = new();
    public List<MapUnitDto> Units { get; set; } = new();
}

/// <summary>
/// Serializer responsible for reading and writing tactical hex map definitions (JSON format).
/// Enforces strict 4-step graph synchronization and entity reconstitution upon import.
/// </summary>
public static class MapSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static bool ExportToJson(
        GridMapManager gridMap,
        UnitRegistry registry,
        int width,
        int height,
        string biome,
        string stageTitle,
        string filePath)
    {
        try
        {
            var mapDto = new MapFileDto
            {
                Header = new MapHeaderDto
                {
                    Width = width,
                    Height = height,
                    Biome = biome,
                    StageTitle = stageTitle
                }
            };

            // Collect Cell data
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = gridMap.GetCell(new Vector2I(x, y));
                    if (cell == null) continue;

                    mapDto.Cells.Add(new MapCellDto
                    {
                        Q = x,
                        R = y,
                        Terrain = (int)cell.Terrain,
                        Deposit = cell.Deposit?.Id,
                        Improvement = (int)cell.TerrainData.Improvement,
                        Owner = cell.OwnerFactionId
                    });
                }
            }

            // Collect Units
            var allUnits = registry.AllUnits;
            for (int i = 0; i < allUnits.Count; i++)
            {
                var unit = allUnits[i];
                if (!GodotObject.IsInstanceValid(unit)) continue;

                mapDto.Units.Add(new MapUnitDto
                {
                    Type = unit.UnitConfigId,
                    Q = unit.GridPosition.X,
                    R = unit.GridPosition.Y,
                    Faction = unit.FactionId,
                    Hp = unit.HpCurrent
                });
            }

            string json = JsonSerializer.Serialize(mapDto, JsonOptions);

            string globalPath = ProjectSettings.GlobalizePath(filePath);
            string? dir = Path.GetDirectoryName(globalPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(globalPath, json);
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MapSerializer] Export error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads a tactical map JSON with strict 4-step execution order:
    /// 1. Wipe old units from UnitRegistry and QueueFree visual nodes.
    /// 2. Reset entire grid on GridMapManager and call PathfindingManager.ResetGraph(width, height).
    /// 3. Lock static terrain obstacles (Water, Mountain, Watchtower).
    /// 4. Spawn unit instances, register in UnitRegistry/FactionData, and lock SetPointSolid for each unit.
    /// </summary>
    public static bool LoadFromJson(
        string filePath,
        WorldMap world,
        GridMapManager gridMap,
        UnitRegistry registry,
        PathfindingManager pathfinding)
    {
        try
        {
            string globalPath = ProjectSettings.GlobalizePath(filePath);
            if (!File.Exists(globalPath))
            {
                GD.PrintErr($"[MapSerializer] File not found: {globalPath}");
                return false;
            }

            string json = File.ReadAllText(globalPath);
            var mapDto = JsonSerializer.Deserialize<MapFileDto>(json, JsonOptions);
            if (mapDto == null) return false;

            int w = mapDto.Header.Width;
            int h = mapDto.Header.Height;

            // STEP 1: Wipe old units from UnitRegistry and free visual nodes
            world.ClearAllUnits();

            // STEP 2: Reset grid and rebuild A* graph
            gridMap.ResetCells(w, h);
            pathfinding.ResetGraph(w, h, gridMap);

            // Populate cells from data
            for (int i = 0; i < mapDto.Cells.Count; i++)
            {
                var cellDto = mapDto.Cells[i];
                var pos = new Vector2I(cellDto.Q, cellDto.R);
                if (!gridMap.IsWithinBounds(pos)) continue;

                var terrain = (TerrainType)cellDto.Terrain;
                gridMap.SetCellTerrain(pos, terrain);

                var cell = gridMap.GetCell(pos);
                if (cell != null)
                {
                    cell.OwnerFactionId = cellDto.Owner;

                    if (!string.IsNullOrEmpty(cellDto.Deposit))
                    {
                        var depCfg = GameConfigManager.GetDepositConfig(cellDto.Deposit);
                        if (depCfg != null)
                        {
                            cell.Deposit = new ResourceDepositData
                            {
                                Id = cellDto.Deposit,
                                Name = depCfg.Name,
                                Category = depCfg.Category,
                                RequiredImprovement = depCfg.RequiredImprovement,
                                BonusYield = depCfg.BonusYield,
                                IsTradeable = depCfg.IsTradeable,
                                IsExploited = false
                            };
                        }
                    }

                    if (cellDto.Improvement > 0)
                    {
                        var impType = (ImprovementType)cellDto.Improvement;
                        cell.TerrainData.Improvement = impType;
                        cell.TerrainData.IsConstructed = true;
                        cell.TerrainData.ConstructionTurnsRemaining = 0;
                        cell.TerrainData.ImprovementBonusYield = impType switch
                        {
                            ImprovementType.Farm => new ResourceBundle(2, 0, 0, 0, 0),
                            ImprovementType.LumberMill => new ResourceBundle(0, 2, 0, 0, 0),
                            ImprovementType.Mine => new ResourceBundle(0, 2, 1, 0, 0),
                            ImprovementType.Watchtower => new ResourceBundle(0, 0, 0, 0, 1),
                            _ => ResourceBundle.Zero
                        };

                        if (cell.Deposit != null)
                        {
                            cell.Deposit.IsExploited = true;
                        }
                    }
                }
            }

            // STEP 3: Lock static terrain obstacles (Water, Mountain, Watchtower)
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var pos = new Vector2I(x, y);
                    var cell = gridMap.GetCell(pos);
                    if (cell != null)
                    {
                        pathfinding.UpdateCellObstacle(pos, cell);
                    }
                }
            }

            // STEP 4: Spawn units, register in UnitRegistry, and lock SetPointSolid(pos, true)
            for (int i = 0; i < mapDto.Units.Count; i++)
            {
                var unitDto = mapDto.Units[i];
                var pos = new Vector2I(unitDto.Q, unitDto.R);
                if (!gridMap.IsWithinBounds(pos)) continue;

                var controller = world.SpawnCustomUnit(unitDto.Type, unitDto.Faction, pos, unitDto.Hp);
                if (controller != null)
                {
                    pathfinding.SetPointSolid(pos, true);
                }
            }

            // Sync visual elements & Fog
            world.FogOfWarManager.Initialize(w, h, gridMap);
            world.FogOfWarManager.UpdatePlayerVisibility(registry.AllUnits, gridMap);
            world.RefreshEconomyUI();

            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MapSerializer] Load error: {ex.Message}");
            return false;
        }
    }
}
