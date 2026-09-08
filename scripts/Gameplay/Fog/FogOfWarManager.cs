using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Mathematics;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// High-performance TileMapLayer Fog of War overlay positioned directly inside WorldRoot.
/// Manages 3-state visibility (Unexplored, Fogged, Visible) using a dedicated TileMapLayer and Dirty-Tile Set.
/// Reduces turn update complexity to O(ActiveVisionTiles) with zero-allocation GPU rendering.
/// </summary>
public partial class FogOfWarManager : Node2D
{
    private readonly Color _unexploredColor = new(0.04f, 0.04f, 0.05f, 1.0f);
    private readonly Color _foggedColor = new(0.05f, 0.05f, 0.07f, 0.58f);

    private int _width;
    private int _height;
    private FogState[,] _fogMap = new FogState[0, 0];
    private readonly HashSet<Vector2I> _activeVisibleTiles = new();
    private readonly HashSet<Vector2I> _nextVisibleTiles = new();

    private TileMapLayer? _fogTileMapLayer;
    public TileMapLayer? FogTileMapLayer => _fogTileMapLayer;

    public override void _Ready()
    {
        ZIndex = 6;
        EnsureFogTileMapLayer();
    }

    private void EnsureFogTileMapLayer()
    {
        _fogTileMapLayer = GetNodeOrNull<TileMapLayer>("FogTileMapLayer");
        if (_fogTileMapLayer == null)
        {
            _fogTileMapLayer = new TileMapLayer
            {
                Name = "FogTileMapLayer",
                TextureFilter = TextureFilterEnum.Nearest,
                ZIndex = 6
            };
            AddChild(_fogTileMapLayer);
        }
    }

    public void Initialize(int width, int height, GridMapManager gridMap)
    {
        _width = width;
        _height = height;
        _fogMap = new FogState[_width, _height];
        _activeVisibleTiles.Clear();

        EnsureFogTileMapLayer();
        SetupFogTileSet(gridMap);

        if (_fogTileMapLayer != null)
        {
            _fogTileMapLayer.Clear();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var pos = new Vector2I(x, y);
                    _fogMap[x, y] = FogState.Unexplored;
                    _fogTileMapLayer.SetCell(pos, sourceId: 0, atlasCoords: new Vector2I(0, 0)); // Tile 0: Unexplored

                    var cell = gridMap.GetCell(pos);
                    if (cell != null) cell.Fog = FogState.Unexplored;
                }
            }
        }
    }

    private void SetupFogTileSet(GridMapManager gridMap)
    {
        if (_fogTileMapLayer == null) return;
        if (_fogTileMapLayer.TileSet != null) return;

        const int dim = GridMapManager.CellDimension;
        var img = Image.CreateEmpty(dim * 2, dim, false, Image.Format.Rgba8);

        // Draw hexagonal tiles into atlas: (0, 0) = Unexplored, (1, 0) = Fogged
        for (int tileIdx = 0; tileIdx < 2; tileIdx++)
        {
            int startX = tileIdx * dim;
            Color baseColor = tileIdx == 0 ? _unexploredColor : _foggedColor;

            for (int y = 0; y < dim; y++)
            {
                float dy = Mathf.Abs(y - 15.5f);
                for (int x = 0; x < dim; x++)
                {
                    float dx = Mathf.Abs(x - 15.5f);
                    float diag = 0.5f * dx + dy;

                    if (diag <= 15.7f && dx <= 15.5f)
                    {
                        img.SetPixel(startX + x, y, baseColor);
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
            TextureRegionSize = new Vector2I(dim, dim)
        };

        atlasSource.CreateTile(new Vector2I(0, 0)); // Unexplored
        atlasSource.CreateTile(new Vector2I(1, 0)); // Fogged

        var gridTileSet = gridMap.TileMapLayer?.TileSet;
        var tileSet = new TileSet
        {
            TileSize = new Vector2I(dim, dim),
            TileShape = gridTileSet?.TileShape ?? TileSet.TileShapeEnum.Hexagon,
            TileOffsetAxis = gridTileSet?.TileOffsetAxis ?? TileSet.TileOffsetAxisEnum.Horizontal,
            TileLayout = gridTileSet?.TileLayout ?? TileSet.TileLayoutEnum.Stacked
        };
        tileSet.AddSource(atlasSource, 0);

        _fogTileMapLayer.TileSet = tileSet;
    }

    public FogState GetFogState(Vector2I pos)
    {
        if (pos.X < 0 || pos.X >= _width || pos.Y < 0 || pos.Y >= _height) return FogState.Unexplored;
        return _fogMap[pos.X, pos.Y];
    }

    public bool IsTileVisible(Vector2I pos)
    {
        return GetFogState(pos) == FogState.Visible;
    }

    /// <summary>
    /// Recalculates player vision from all active allied units (FactionId == 0).
    /// Uses Dirty-Tile Set comparison to minimize TileMapLayer cell updates to O(ActiveVisionTiles).
    /// </summary>
    public void UpdatePlayerVisibility(IReadOnlyList<UnitController> allUnits, GridMapManager gridMap)
    {
        if (_width <= 0 || _height <= 0 || _fogTileMapLayer == null) return;

        _nextVisibleTiles.Clear();

        Span<Vector2I> ring1 = stackalloc Vector2I[6];
        Span<Vector2I> ring2 = stackalloc Vector2I[6];

        // 1. Collect vision perimeter (2-hex sight radius) for all active player units
        for (int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            if (!GodotObject.IsInstanceValid(unit) || unit.FactionId != 0 || unit.IsSurrendered) continue;

            Vector2I center = unit.GridPosition;
            if (gridMap.IsWithinBounds(center))
            {
                _nextVisibleTiles.Add(center);
            }

            // Ring 1
            HexMath.GetNeighborsNonAlloc(center, ring1);
            for (int r1 = 0; r1 < 6; r1++)
            {
                var p1 = ring1[r1];
                if (gridMap.IsWithinBounds(p1))
                {
                    _nextVisibleTiles.Add(p1);
                }

                // Ring 2
                HexMath.GetNeighborsNonAlloc(p1, ring2);
                for (int r2 = 0; r2 < 6; r2++)
                {
                    var p2 = ring2[r2];
                    if (gridMap.IsWithinBounds(p2))
                    {
                        _nextVisibleTiles.Add(p2);
                    }
                }
            }
        }

        // 2. Demote tiles that were visible but now lost sight: Visible -> Fogged
        foreach (var pos in _activeVisibleTiles)
        {
            if (!_nextVisibleTiles.Contains(pos))
            {
                _fogMap[pos.X, pos.Y] = FogState.Fogged;
                _fogTileMapLayer.SetCell(pos, sourceId: 0, atlasCoords: new Vector2I(1, 0)); // Tile 1: Fogged

                var cell = gridMap.GetCell(pos);
                if (cell != null) cell.Fog = FogState.Fogged;
            }
        }

        // 3. Promote newly visible tiles: Unexplored/Fogged -> Visible
        foreach (var pos in _nextVisibleTiles)
        {
            if (!_activeVisibleTiles.Contains(pos))
            {
                _fogMap[pos.X, pos.Y] = FogState.Visible;
                _fogTileMapLayer.SetCell(pos, sourceId: -1); // Cleared cell = Visible terrain

                var cell = gridMap.GetCell(pos);
                if (cell != null) cell.Fog = FogState.Visible;
            }
        }

        _activeVisibleTiles.Clear();
        foreach (var pos in _nextVisibleTiles)
        {
            _activeVisibleTiles.Add(pos);
        }

        // 4. Synchronize enemy unit visibility
        for (int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            if (!GodotObject.IsInstanceValid(unit)) continue;

            if (unit.FactionId != 0)
            {
                unit.Visible = IsTileVisible(unit.GridPosition);
            }
            else
            {
                unit.Visible = true;
            }
        }
    }

    /// <summary>
    /// Dev / Map Editor tool: Reveals the entire map and reveals all units.
    /// </summary>
    public void RevealAll(IReadOnlyList<UnitController>? allUnits = null)
    {
        _fogTileMapLayer?.Clear();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _fogMap[x, y] = FogState.Visible;
            }
        }

        if (allUnits != null)
        {
            for (int i = 0; i < allUnits.Count; i++)
            {
                var unit = allUnits[i];
                if (GodotObject.IsInstanceValid(unit))
                {
                    unit.Visible = true;
                }
            }
        }
    }

    /// <summary>
    /// Restores full fog coverage across all cells.
    /// </summary>
    public void ResetFog(GridMapManager gridMap)
    {
        Initialize(_width, _height, gridMap);
    }
}

