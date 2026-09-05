using System;
using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public enum TerrainType
{
    Plains = 0,
    Forest = 1,
    River = 2,
    Mountain = 3
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
    [Export] public bool SpawnTileInstances = true;

    /// <summary>
    /// Custom map layout array editable directly in the Godot inspector.
    /// Format: Each row contains characters separated by spaces or continuous:
    /// 'P' = Plains (Đồng Bằng), 'F' = Forest (Rừng Rậm), 'R' = River (Sông Hồng), 'M' = Mountain (Núi)
    /// </summary>
    [Export] public Godot.Collections.Array<string> ManualMapLayout = new();

    public int MapWidth { get; private set; } = 32;
    public int MapHeight { get; private set; } = 32;
    public string ActiveBiome { get; private set; } = "red_river";

    private HexCell[,] _cells = new HexCell[0, 0];
    private TileMapLayer? _tileMapLayer;
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

    private void BuildProceduralTileSet()
    {
        if (_tileMapLayer == null) return;
        if (_tileMapLayer.TileSet != null) return;

        ChroniclesOfTheEmpires.Core.Config.GameConfigManager.EnsureLoaded();

        // Create a 128x32 atlas texture containing 4 colored 32x32 pointy-topped hexagons
        var img = Image.CreateEmpty(CellDimension * 4, CellDimension, false, Image.Format.Rgba8);

        for (int tileIdx = 0; tileIdx < 4; tileIdx++)
        {
            var terrain = (TerrainType)tileIdx;
            var cfg = ChroniclesOfTheEmpires.Core.Config.GameConfigManager.GetHexConfig(terrain);

            Color baseCol = cfg?.BaseColor ?? terrain switch
            {
                TerrainType.Plains => new Color(0.25f, 0.48f, 0.20f),
                TerrainType.Forest => new Color(0.12f, 0.30f, 0.11f),
                TerrainType.River => new Color(0.17f, 0.36f, 0.56f),
                TerrainType.Mountain => new Color(0.35f, 0.38f, 0.41f),
                _ => new Color(0.2f, 0.2f, 0.2f)
            };

            // Soft grid line: low alpha overlay to avoid graph-paper look
            Color borderCol = cfg?.BorderColor ?? baseCol.Darkened(0.14f);

            int startX = tileIdx * CellDimension;

            for (int y = 0; y < CellDimension; y++)
            {
                float dy = Mathf.Abs(y - 15.5f);
                for (int x = 0; x < CellDimension; x++)
                {
                    float dx = Mathf.Abs(x - 15.5f);
                    float diag = 0.5f * dx + dy;

                    if (diag <= 15.7f && dx <= 15.5f)
                    {
                        float distToEdge = Mathf.Min(15.7f - diag, 15.5f - dx);
                        bool isBorder = distToEdge < 1.35f;

                        Color pixelCol;
                        if (terrain == TerrainType.River)
                        {
                            // 1px silt brown / alluvial bank at boundary
                            Color bankCol = cfg?.ExtraColors.GetValueOrDefault("bank_color", new Color(0.45f, 0.29f, 0.13f)) ?? new Color(0.45f, 0.29f, 0.13f);
                            Color shallowCol = cfg?.ExtraColors.GetValueOrDefault("shallow_color", new Color(0.25f, 0.48f, 0.72f)) ?? new Color(0.25f, 0.48f, 0.72f);
                            Color waveCol = cfg?.ExtraColors.GetValueOrDefault("wave_color", new Color(0.34f, 0.59f, 0.85f)) ?? new Color(0.34f, 0.59f, 0.85f);

                            if (distToEdge < 1.35f)
                            {
                                pixelCol = bankCol;
                            }
                            else if (distToEdge < 3.2f)
                            {
                                pixelCol = shallowCol;
                            }
                            else
                            {
                                // Deep core water with subtle ripples
                                bool isWave = ((x + y * 2) % 7 == 0) && distToEdge > 4.5f;
                                pixelCol = isWave ? waveCol : baseCol;
                            }
                        }
                        else if (terrain == TerrainType.Plains)
                        {
                            if (isBorder)
                            {
                                pixelCol = borderCol;
                            }
                            else
                            {
                                // Dithered grass tufts
                                int hash = (x * 73856093 ^ y * 19349663) & 0x7FFFFFFF;
                                Color grassLight = cfg?.ExtraColors.GetValueOrDefault("grass_light_color", new Color(0.33f, 0.59f, 0.24f)) ?? new Color(0.33f, 0.59f, 0.24f);
                                Color grassWarm = cfg?.ExtraColors.GetValueOrDefault("grass_warm_color", new Color(0.43f, 0.64f, 0.22f)) ?? new Color(0.43f, 0.64f, 0.22f);

                                if (hash % 13 == 0) pixelCol = grassLight;
                                else if (hash % 19 == 0) pixelCol = grassWarm;
                                else pixelCol = baseCol;
                            }
                        }
                        else if (terrain == TerrainType.Forest)
                        {
                            if (isBorder)
                            {
                                pixelCol = borderCol;
                            }
                            else
                            {
                                // Canopy clusters
                                int hash = (x * 73856093 ^ y * 19349663) & 0x7FFFFFFF;
                                Color canopyLight = cfg?.ExtraColors.GetValueOrDefault("canopy_light_color", new Color(0.17f, 0.38f, 0.15f)) ?? new Color(0.17f, 0.38f, 0.15f);
                                Color canopyDark = cfg?.ExtraColors.GetValueOrDefault("canopy_dark_color", new Color(0.07f, 0.18f, 0.06f)) ?? new Color(0.07f, 0.18f, 0.06f);

                                if (hash % 6 == 0) pixelCol = canopyLight;
                                else if (hash % 8 == 0) pixelCol = canopyDark;
                                else pixelCol = baseCol;
                            }
                        }
                        else // Mountain
                        {
                            Color peakCol = cfg?.ExtraColors.GetValueOrDefault("peak_color", new Color(0.71f, 0.76f, 0.81f)) ?? new Color(0.71f, 0.76f, 0.81f);
                            Color baseDarkCol = cfg?.ExtraColors.GetValueOrDefault("base_dark_color", new Color(0.12f, 0.13f, 0.15f)) ?? new Color(0.12f, 0.13f, 0.15f);

                            if (y <= 7 && distToEdge >= 2.0f)
                            {
                                // Highlighted snowy ridge
                                pixelCol = peakCol;
                            }
                            else if (y >= 24)
                            {
                                // Heavy dark shadowed mountain base
                                pixelCol = baseDarkCol;
                            }
                            else if (isBorder)
                            {
                                pixelCol = borderCol;
                            }
                            else
                            {
                                float heightFactor = (float)y / CellDimension;
                                pixelCol = baseCol.Lerp(baseDarkCol, heightFactor * 0.7f);
                            }
                        }

                        img.SetPixel(startX + x, y, pixelCol);
                    }
                    else
                    {
                        img.SetPixel(startX + x, y, new Color(0, 0, 0, 0));
                    }
                }
            }
        }

        var texture = ImageTexture.CreateFromImage(img);
        var atlasSource = new TileSetAtlasSource
        {
            Texture = texture,
            TextureRegionSize = new Vector2I(CellDimension, CellDimension)
        };

        for (int i = 0; i < 4; i++)
        {
            atlasSource.CreateTile(new Vector2I(i, 0));
        }

        var tileSet = new TileSet 
        { 
            TileSize = new Vector2I(CellDimension, CellDimension),
            TileShape = TileSet.TileShapeEnum.Hexagon,
            TileOffsetAxis = TileSet.TileOffsetAxisEnum.Horizontal,
            TileLayout = TileSet.TileLayoutEnum.Stacked
        };
        tileSet.AddSource(atlasSource, 0);

        _tileMapLayer.TileSet = tileSet;
    }

    private void GenerateTerrain()
    {
        var random = new Random(1337);

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                TerrainType terrain = DetermineTerrain(x, y, random);
                var coords = new Vector2I(x, y);
                Vector2 worldPos = GridToWorldCenter(coords);
                _cells[x, y] = new HexCell(coords, worldPos, terrain);
            }
        }
    }

    private TerrainType DetermineTerrain(int x, int y, Random rand)
    {
        if (ActiveBiome == "red_river")
        {
            float riverCenter = (float)x / MapWidth * MapHeight + Mathf.Sin(x * 0.35f) * 2.5f;
            if (Mathf.Abs(y - riverCenter) < 1.4f)
            {
                return TerrainType.River;
            }

            if ((x < 3 && y < MapHeight / 2) || (y < 3 && x < MapWidth / 2) || (x > MapWidth - 4 && y > MapHeight - 4))
            // Keep player deployment & maneuver corridor (top-left) open for full unit movement
            if (x <= 4 && y <= 4)
            {
                if (rand.NextDouble() < 0.7) return TerrainType.Mountain;
                if (rand.NextDouble() < 0.20) return TerrainType.Forest;
                return TerrainType.Plains;
            }

            if ((x < 2 && y >= 5 && y < MapHeight / 2) || (y < 2 && x >= 5 && x < MapWidth / 2) || (x > MapWidth - 4 && y > MapHeight - 4))
            {
                if (rand.NextDouble() < 0.45) return TerrainType.Mountain;
            }

            if (rand.NextDouble() < 0.18)
            {
                return TerrainType.Forest;
            }

            return TerrainType.Plains;
        }

        if (ActiveBiome == "jungle")
        {
            if (rand.NextDouble() < 0.08) return TerrainType.River;
            if (rand.NextDouble() < 0.55) return TerrainType.Forest;
            if (rand.NextDouble() < 0.08) return TerrainType.Mountain;
            return TerrainType.Plains;
        }

        if (ActiveBiome == "highlands")
        {
            if (rand.NextDouble() < 0.28) return TerrainType.Mountain;
            if (rand.NextDouble() < 0.30) return TerrainType.Forest;
            if (rand.NextDouble() < 0.06) return TerrainType.River;
            return TerrainType.Plains;
        }

        if (rand.NextDouble() < 0.10) return TerrainType.Forest;
        if (rand.NextDouble() < 0.04) return TerrainType.River;
        if (rand.NextDouble() < 0.05) return TerrainType.Mountain;
        return TerrainType.Plains;
    }

    /// <summary>
    /// Scans the map from ManualMapLayout array or from painted TileMapLayer cells.
    /// Allows designers to author maps directly in the Godot Inspector or 2D TileMap editor.
    /// </summary>
    public void ScanMapArray()
    {
        bool hasManualArray = ManualMapLayout != null && ManualMapLayout.Count > 0;
        bool hasPaintedCells = _tileMapLayer != null && _tileMapLayer.GetUsedCells().Count > 0;

        if (hasManualArray)
        {
            ScanFromLayoutArray();
            BuildProceduralTileSet();
            RenderTileMap();
        }
        else if (hasPaintedCells && _tileMapLayer != null)
        {
            ScanFromTileMapLayer();
        }
        else
        {
            GenerateTerrain();
            BuildProceduralTileSet();
            RenderTileMap();
        }

        SpawnOrSyncTiles();
    }

    private void ScanFromLayoutArray()
    {
        if (ManualMapLayout == null || ManualMapLayout.Count == 0) return;

        MapHeight = ManualMapLayout.Count;
        var parsedRows = new List<TerrainType[]>();
        int maxW = 0;

        foreach (string row in ManualMapLayout)
        {
            var tokens = ParseRowTokens(row);
            if (tokens.Length > maxW) maxW = tokens.Length;
            parsedRows.Add(tokens);
        }

        MapWidth = Math.Max(1, maxW);
        _cells = new HexCell[MapWidth, MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            var row = parsedRows[y];
            for (int x = 0; x < MapWidth; x++)
            {
                TerrainType terrain = x < row.Length ? row[x] : TerrainType.Plains;
                var coords = new Vector2I(x, y);
                Vector2 worldPos = GridToWorldCenter(coords);
                _cells[x, y] = new HexCell(coords, worldPos, terrain);
            }
        }
    }

    private void ScanFromTileMapLayer()
    {
        if (_tileMapLayer == null) return;
        var used = _tileMapLayer.GetUsedCells();
        if (used.Count == 0) return;

        int maxX = 0, maxY = 0;
        foreach (var pos in used)
        {
            if (pos.X > maxX) maxX = pos.X;
            if (pos.Y > maxY) maxY = pos.Y;
        }

        MapWidth = Math.Max(MapWidth, maxX + 1);
        MapHeight = Math.Max(MapHeight, maxY + 1);
        _cells = new HexCell[MapWidth, MapHeight];

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                var coords = new Vector2I(x, y);
                TerrainType terrain = TerrainType.Plains;

                if (_tileMapLayer.GetCellSourceId(coords) != -1)
                {
                    int atlasX = _tileMapLayer.GetCellAtlasCoords(coords).X;
                    terrain = (TerrainType)Math.Clamp(atlasX, 0, 3);
                }

                Vector2 worldPos = GridToWorldCenter(coords);
                _cells[x, y] = new HexCell(coords, worldPos, terrain);
            }
        }
    }

    private static TerrainType[] ParseRowTokens(string row)
    {
        var tokens = row.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length <= 1 && row.Trim().Length > 1)
        {
            string clean = row.Trim();
            var list = new TerrainType[clean.Length];
            for (int i = 0; i < clean.Length; i++)
            {
                list[i] = CharToTerrain(clean[i]);
            }
            return list;
        }

        var result = new TerrainType[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
        {
            result[i] = CharToTerrain(tokens[i][0]);
        }
        return result;
    }

    private static TerrainType CharToTerrain(char c) => char.ToUpperInvariant(c) switch
    {
        'P' or '0' => TerrainType.Plains,
        'F' or '1' => TerrainType.Forest,
        'R' or '2' => TerrainType.River,
        'M' or '3' => TerrainType.Mountain,
        _ => TerrainType.Plains
    };

    public void SpawnOrSyncTiles()
    {
        if (!SpawnTileInstances || _tileContainer == null) return;

        TileScene ??= GD.Load<PackedScene>("res://scenes/tile.tscn");
        if (TileScene == null) return;

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

                _tileContainer.AddChild(tile);
            }
        }
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

    public HexCell? GetCell(Vector2I pos)
    {
        if (!IsWithinBounds(pos)) return null;
        return _cells[pos.X, pos.Y];
    }

    public HexCell? GetCellAtWorld(Vector2 worldPos)
    {
        Vector2I gridPos = WorldToGrid(worldPos);
        return GetCell(gridPos);
    }

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
            return Instance._tileMapLayer.MapToLocal(gridPos);
        }
        return new Vector2(gridPos.X * CellDimension + CellDimension * 0.5f, gridPos.Y * CellDimension + CellDimension * 0.5f);
    }

    public static Vector2I WorldToGrid(Vector2 worldPos)
    {
        if (Instance?._tileMapLayer != null)
        {
            return Instance._tileMapLayer.LocalToMap(worldPos);
        }
        return new Vector2I(Mathf.FloorToInt(worldPos.X / CellDimension), Mathf.FloorToInt(worldPos.Y / CellDimension));
    }
}
