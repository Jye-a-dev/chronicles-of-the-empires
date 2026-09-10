using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.Entities;
using ChroniclesOfTheEmpires.Gameplay.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay.Editor;

public enum EditorBrushCategory
{
    Terrain = 0,
    Deposit = 1,
    Improvement = 2,
    Unit = 3,
    Ownership = 4
}

public enum BrushShape
{
    Single = 0,
    Cluster = 1, // Radius 1 (7 hexes)
    FloodFill = 2
}

/// <summary>
/// Controller governing interactive map editing, brush stamping, flood filling,
/// and live synchronization with GridMapManager, UnitRegistry, and PathfindingManager.
/// </summary>
public class MapEditorController
{
    private readonly WorldMap _world;
    private readonly GridMapManager _gridMap;
    private readonly UnitRegistry _unitRegistry;
    private readonly PathfindingManager _pathfinding;

    public bool IsEditorActive { get; set; } = false;

    // Brush Settings
    public EditorBrushCategory ActiveCategory { get; set; } = EditorBrushCategory.Terrain;
    public BrushShape ActiveShape { get; set; } = BrushShape.Single;

    public TerrainType SelectedTerrain { get; set; } = TerrainType.Plains;
    public string SelectedDepositId { get; set; } = "copper";
    public ImprovementType SelectedImprovement { get; set; } = ImprovementType.Farm;
    public int SelectedFactionId { get; set; } = 0;
    public string SelectedUnitType { get; set; } = "cam_ve_quan";

    public MapEditorController(
        WorldMap world,
        GridMapManager gridMap,
        UnitRegistry unitRegistry,
        PathfindingManager pathfinding)
    {
        _world = world;
        _gridMap = gridMap;
        _unitRegistry = unitRegistry;
        _pathfinding = pathfinding;
    }

    public void ApplyPaint(Vector2I gridPos)
    {
        if (!_gridMap.IsWithinBounds(gridPos)) return;

        switch (ActiveShape)
        {
            case BrushShape.Single:
                PaintSingleCell(gridPos);
                break;
            case BrushShape.Cluster:
                PaintCluster(gridPos);
                break;
            case BrushShape.FloodFill:
                PaintFloodFill(gridPos);
                break;
        }
    }

    public void ApplyErase(Vector2I gridPos)
    {
        if (!_gridMap.IsWithinBounds(gridPos)) return;

        // Erase any unit present atomically
        var unit = _unitRegistry.GetUnitAt(gridPos);
        if (unit != null && GodotObject.IsInstanceValid(unit))
        {
            UnitLifecycleManager.TerminateUnit(unit, _unitRegistry, _pathfinding, _gridMap, EconomyManager.Instance);
            return;
        }

        // Erase deposit & improvement, revert terrain to Plains
        var hexCell = _gridMap.GetCell(gridPos);
        if (hexCell != null)
        {
            hexCell.Deposit = null;
            hexCell.TerrainData.Improvement = ImprovementType.None;
            hexCell.TerrainData.IsConstructed = false;
            hexCell.TerrainData.ImprovementBonusYield = ResourceBundle.Zero;
            hexCell.OwnerFactionId = -1;

            _gridMap.SetCellTerrain(gridPos, TerrainType.Plains);
            _pathfinding.UpdateCellObstacle(gridPos, hexCell);
        }
    }

    private void PaintCluster(Vector2I center)
    {
        PaintSingleCell(center);
        var neighbors = _gridMap.GetNeighbors(center);
        for (int i = 0; i < neighbors.Count; i++)
        {
            var n = neighbors[i];
            if (_gridMap.IsWithinBounds(n))
            {
                PaintSingleCell(n);
            }
        }
    }

    private void PaintFloodFill(Vector2I start)
    {
        var startCell = _gridMap.GetCell(start);
        if (startCell == null) return;

        var targetTerrain = startCell.Terrain;
        if (ActiveCategory == EditorBrushCategory.Terrain && targetTerrain == SelectedTerrain) return;

        var queue = new Queue<Vector2I>();
        var visited = new HashSet<Vector2I>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            PaintSingleCell(current);

            var neighbors = _gridMap.GetNeighbors(current);
            for (int i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                if (_gridMap.IsWithinBounds(n) && !visited.Contains(n))
                {
                    var neighborCell = _gridMap.GetCell(n);
                    if (neighborCell != null && neighborCell.Terrain == targetTerrain)
                    {
                        visited.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }
        }
    }

    private void PaintSingleCell(Vector2I pos)
    {
        var cell = _gridMap.GetCell(pos);
        if (cell == null) return;

        switch (ActiveCategory)
        {
            case EditorBrushCategory.Terrain:
                _gridMap.SetCellTerrain(pos, SelectedTerrain);
                _pathfinding.UpdateCellObstacle(pos, cell);

                // Clear deposit if water, ocean or mountain
                if (SelectedTerrain == TerrainType.River || SelectedTerrain == TerrainType.Mountain || SelectedTerrain == TerrainType.Ocean)
                {
                    cell.Deposit = null;
                    cell.TerrainData.Improvement = ImprovementType.None;
                }
                break;

            case EditorBrushCategory.Deposit:
                if (cell.Terrain != TerrainType.River && cell.Terrain != TerrainType.Mountain && cell.Terrain != TerrainType.Ocean)
                {
                    var depCfg = GameConfigManager.GetDepositConfig(SelectedDepositId);
                    cell.Deposit = new ResourceDepositData
                    {
                        Id = SelectedDepositId,
                        Name = depCfg?.Name ?? SelectedDepositId,
                        Category = depCfg?.Category ?? DepositCategory.Mineral,
                        RequiredImprovement = depCfg?.RequiredImprovement ?? "mine",
                        BonusYield = depCfg?.BonusYield ?? new ResourceBundle(0, 0, 2, 0, 0),
                        IsTradeable = depCfg?.IsTradeable ?? true,
                        IsExploited = false
                    };
                }
                break;

            case EditorBrushCategory.Improvement:
                if (cell.Terrain != TerrainType.River && cell.Terrain != TerrainType.Ocean)
                {
                    cell.TerrainData.Improvement = SelectedImprovement;
                    cell.TerrainData.IsConstructed = true;
                    cell.TerrainData.ConstructionTurnsRemaining = 0;
                    cell.TerrainData.ImprovementBonusYield = SelectedImprovement switch
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
                break;

            case EditorBrushCategory.Ownership:
                cell.OwnerFactionId = SelectedFactionId;
                var faction = _world.FindFactionData(SelectedFactionId);
                if (faction != null && !faction.ControlledTiles.Contains(pos))
                {
                    faction.ControlledTiles.Add(pos);
                }
                break;

            case EditorBrushCategory.Unit:
                if (!cell.IsSolid && cell.OccupyingUnit == null && !_pathfinding.IsPointSolid(pos))
                {
                    var controller = _world.SpawnCustomUnit(SelectedUnitType, SelectedFactionId, pos);
                    if (controller != null)
                    {
                        _pathfinding.SetPointSolid(pos, true);
                    }
                }
                break;
        }
    }
}
