using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public enum TerrainType
{
    Plains = 0,
    Forest = 1,
    River = 2,
    Mountain = 3,
    Ocean = 4,
    Hill = 5
}

/// <summary>
/// Manages procedural map data, 32x32 hexagonal grid generation, and TileMapLayer rendering.
/// Holds first-class HexCell objects for every tile on the sa bàn.
/// </summary>
public partial class GridMapManager : Node2D
{
    public const int CellDimension = 32;
    public static readonly Vector2 CellSize = new(CellDimension, CellDimension);

    [Export] public NodePath? TileMapLayerPath;
    [Export] public NodePath? TileContainerPath;
    [Export] public PackedScene? TileScene;
    [Export] public bool UseManualMap = false;
    [Export] public bool SpawnTileInstances = false;

    /// <summary>
    /// Custom map layout array editable directly in the Godot inspector.
    /// Format: Each row contains characters separated by spaces or continuous:
    /// 'P' = Plains (Đồng Bằng), 'F' = Forest (Rừng Rậm), 'R' = River (Đại Hà), 'M' = Mountain (Núi), 'O' = Ocean (Biển Khơi), 'H' = Hill (Gò Đồi)
    /// </summary>
    [Export] public Godot.Collections.Array<string> ManualMapLayout = new();

    public int MapWidth { get; private set; } = 32;
    public int MapHeight { get; private set; } = 32;
    public string ActiveBiome { get; private set; } = "red_river";

    private HexCell[,] _cells = new HexCell[0, 0];
    private readonly Dictionary<Vector2I, HexTile> _tileInstances = new();
    private TileMapLayer? _tileMapLayer;
    public TileMapLayer? TileMapLayer => _tileMapLayer;
    private Node2D? _tileContainer;

    public static GridMapManager? Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        if (TileMapLayerPath != null)
        {
            _tileMapLayer = GetNodeOrNull<TileMapLayer>(TileMapLayerPath);
        }

        _tileMapLayer ??= GetNodeOrNull<TileMapLayer>("TerrainLayer") ?? new TileMapLayer { Name = "TerrainLayer" };
        if (_tileMapLayer.GetParent() == null)
        {
            AddChild(_tileMapLayer);
        }

        if (TileContainerPath != null)
        {
            _tileContainer = GetNodeOrNull<Node2D>(TileContainerPath);
        }
        _tileContainer ??= GetNodeOrNull<Node2D>("TileContainer");
        if (_tileContainer == null && SpawnTileInstances)
        {
            _tileContainer = new Node2D { Name = "TileContainer" };
            AddChild(_tileContainer);
        }
    }

    /// <summary>
    /// Initializes map dimensions and terrain according to active session configuration or manual array.
    /// </summary>
    public void InitializeFromSession(string mode, string stageId, string mapSize, string biome)
    {
        Instance = this;
        ActiveBiome = string.IsNullOrWhiteSpace(biome) ? "red_river" : biome.ToLowerInvariant();

        var (w, h) = ResolveDimensions(mode, stageId, mapSize);
        MapWidth = w;
        MapHeight = h;

        bool hasManualLayout = ManualMapLayout != null && ManualMapLayout.Count > 0;
        bool hasPaintedCells = _tileMapLayer != null && _tileMapLayer.GetUsedCells().Count > 0;

        if (UseManualMap || hasManualLayout || hasPaintedCells)
        {
            ScanMapArray();
        }
        else
        {
            _cells = new HexCell[MapWidth, MapHeight];
            BuildProceduralTileSet();
            GenerateTerrain();
            RenderTileMap();
            SpawnOrSyncTiles();
        }
    }

    public static (int Width, int Height) ResolveDimensions(string mode, string stageId, string mapSize)
    {
        return mode.ToLowerInvariant() switch
        {
            "training" or "tutorial" => (32, 32),
            "campaign" => stageId.ToLowerInvariant() switch
            {
                "stage_1" => (32, 32),
                "stage_2" => (48, 48),
                _ => (64, 64)
            },
            _ => ParseDimensions(mapSize)
        };
    }

    private static (int, int) ParseDimensions(string sizeStr)
    {
        if (string.IsNullOrWhiteSpace(sizeStr)) return (32, 32);
        string[] parts = sizeStr.ToLowerInvariant().Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
        {
            return (Math.Clamp(w, 16, 128), Math.Clamp(h, 16, 128));
        }
        return (32, 32);
    }

    public void SpawnOrSyncTiles()
    {
        if (!SpawnTileInstances || _tileContainer == null) return;

        TileScene ??= GD.Load<PackedScene>("res://scenes/gameplay/tile.tscn");
        if (TileScene == null) return;

        _tileInstances.Clear();
        foreach (Node child in _tileContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                var cell = _cells[x, y];
                if (cell == null) continue;

                var tile = TileScene.Instantiate<HexTile>();
                tile.GridPosition = cell.Coords;
                tile.Position = cell.WorldPosition;
                tile.Terrain = cell.Terrain;
                tile.AssociatedCell = cell;

                _tileInstances[cell.Coords] = tile;
                _tileContainer.AddChild(tile);
            }
        }
    }

    public HexTile? GetTileInstance(Vector2I pos)
    {
        if (_tileInstances.TryGetValue(pos, out var tile) && GodotObject.IsInstanceValid(tile))
        {
            return tile;
        }
        return null;
    }

    public void RefreshTileOwnerVisual(Vector2I pos, int ownerFactionId)
    {
        var tile = GetTileInstance(pos);
        tile?.UpdateOwnerVisual(ownerFactionId);
    }

    private void RenderTileMap()
    {
        if (_tileMapLayer == null) return;
        _tileMapLayer.Clear();

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                var cell = _cells[x, y];
                _tileMapLayer.SetCell(new Vector2I(x, y), sourceId: 0, atlasCoords: new Vector2I((int)cell.Terrain, 0));
            }
        }
    }

    public bool IsWithinBounds(Vector2I pos) =>
        pos.X >= 0 && pos.X < MapWidth && pos.Y >= 0 && pos.Y < MapHeight;

    public void SetCellTerrain(Vector2I pos, TerrainType terrain)
    {
        if (!IsWithinBounds(pos)) return;
        var cell = _cells[pos.X, pos.Y];
        if (cell == null) return;

        cell.ConfigureTerrainDefaults(terrain);
        _tileMapLayer?.SetCell(pos, sourceId: 0, atlasCoords: new Vector2I((int)terrain, 0));
        if (_tileInstances.TryGetValue(pos, out var tileView) && GodotObject.IsInstanceValid(tileView))
        {
            tileView.Configure(cell);
        }
    }

    public void ResetCells(int width, int height)
    {
        MapWidth = width;
        MapHeight = height;
        _cells = new HexCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var coords = new Vector2I(x, y);
                Vector2 worldPos = GridToWorldCenter(coords);
                _cells[x, y] = new HexCell(coords, worldPos, TerrainType.Plains);
            }
        }
        RenderTileMap();
        SpawnOrSyncTiles();
    }

    public HexCell? GetCell(Vector2I pos)
    {
        if (!IsWithinBounds(pos)) return null;
        return _cells[pos.X, pos.Y];
    }

    public HexCell? GetCellAt(Vector2I pos) => GetCell(pos);

    public HexCell? GetCellAtWorld(Vector2 worldPos)
    {
        Vector2I gridPos = WorldToGrid(worldPos);
        return GetCell(gridPos);
    }

    public TileTerrainData? GetTileTerrainData(Vector2I pos) => GetCell(pos)?.TerrainData;

    /// <summary>
    /// Returns 0-allocation struct enumerable of the 6 hex neighbors.
    /// </summary>
    public HexNeighbors GetNeighbors(Vector2I gridPos) => new(_tileMapLayer, gridPos);

    public Vector2I[] GetSurroundingCells(Vector2I gridPos)
    {
        if (_tileMapLayer == null) return Array.Empty<Vector2I>();
        var cells = _tileMapLayer.GetSurroundingCells(gridPos);
        var result = new Vector2I[cells.Count];
        for (int i = 0; i < cells.Count; i++)
        {
            result[i] = cells[i];
        }
        return result;
    }

    public static Vector2 GridToWorldCenter(Vector2I gridPos)
    {
        if (Instance?._tileMapLayer != null)
        {
            return Instance._tileMapLayer.ToGlobal(Instance._tileMapLayer.MapToLocal(gridPos));
        }
        return new Vector2(gridPos.X * CellDimension + CellDimension * 0.5f, gridPos.Y * CellDimension + CellDimension * 0.5f);
    }

    public static Vector2I WorldToGrid(Vector2 worldPos)
    {
        if (Instance?._tileMapLayer != null)
        {
            return Instance._tileMapLayer.LocalToMap(Instance._tileMapLayer.ToLocal(worldPos));
        }
        return new Vector2I(Mathf.FloorToInt(worldPos.X / CellDimension), Mathf.FloorToInt(worldPos.Y / CellDimension));
    }
}
