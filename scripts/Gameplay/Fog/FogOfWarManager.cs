using System;
using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Node2D Fog of War overlay component positioned directly inside WorldRoot.
/// Manages 3-state visibility (Unexplored, Fogged, Visible) and zero-allocation hex rendering.
/// Ensures 100% pixel-perfect alignment with GridMapManager across all camera zoom levels.
/// </summary>
public partial class FogOfWarManager : Node2D
{
    private static readonly Vector2[] HexOffsets =
    [
        new Vector2(0, -16),
        new Vector2(16f, -8),
        new Vector2(16f, 8),
        new Vector2(0, 16),
        new Vector2(-16f, 8),
        new Vector2(-16f, -8)
    ];

    private readonly Color _unexploredColor = new(0.04f, 0.04f, 0.05f, 1.0f);
    private readonly Color _foggedColor = new(0.05f, 0.05f, 0.07f, 0.52f);
    private readonly Vector2[] _polyBuffer = new Vector2[6];

    private int _width;
    private int _height;
    private FogState[,] _fogMap = new FogState[0, 0];

    public override void _Ready()
    {
        ZIndex = 6;
    }

    public void Initialize(int width, int height, GridMapManager gridMap)
    {
        _width = width;
        _height = height;
        _fogMap = new FogState[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _fogMap[x, y] = FogState.Unexplored;
                var cell = gridMap.GetCell(new Vector2I(x, y));
                if (cell != null) cell.Fog = FogState.Unexplored;
            }
        }

        QueueRedraw();
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
    /// Transitions previous Visible tiles to Fogged, sets active 2-hex vision radius to Visible,
    /// and synchronizes visibility flags on all enemy unit visual controllers.
    /// </summary>
    public void UpdatePlayerVisibility(IReadOnlyList<UnitController> allUnits, GridMapManager gridMap)
    {
        if (_width <= 0 || _height <= 0) return;

        // Step 1: Demote all currently Visible cells to Fogged
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_fogMap[x, y] == FogState.Visible)
                {
                    _fogMap[x, y] = FogState.Fogged;
                    var cell = gridMap.GetCell(new Vector2I(x, y));
                    if (cell != null) cell.Fog = FogState.Fogged;
                }
            }
        }

        // Step 2: Light up cells within SightRange (2 hexes) for all active player units
        for (int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            if (!IsInstanceValid(unit) || unit.FactionId != 0 || unit.IsSurrendered) continue;

            Vector2I center = unit.GridPosition;
            SetCellVisible(center, gridMap);

            // Ring 1 neighbors
            var ring1 = gridMap.GetNeighbors(center);
            for (int r1 = 0; r1 < ring1.Count; r1++)
            {
                var p1 = ring1[r1];
                SetCellVisible(p1, gridMap);

                // Ring 2 neighbors
                var ring2 = gridMap.GetNeighbors(p1);
                for (int r2 = 0; r2 < ring2.Count; r2++)
                {
                    SetCellVisible(ring2[r2], gridMap);
                }
            }
        }

        // Step 3: Redraw fog overlays
        QueueRedraw();

        // Step 4: Synchronize enemy unit visibility
        for (int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            if (!IsInstanceValid(unit)) continue;

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

    private void SetCellVisible(Vector2I pos, GridMapManager gridMap)
    {
        if (pos.X < 0 || pos.X >= _width || pos.Y < 0 || pos.Y >= _height) return;

        _fogMap[pos.X, pos.Y] = FogState.Visible;
        var cell = gridMap.GetCell(pos);
        if (cell != null) cell.Fog = FogState.Visible;
    }

    public override void _Draw()
    {
        if (_width <= 0 || _height <= 0) return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var state = _fogMap[x, y];
                if (state == FogState.Visible) continue;

                Vector2 center = GridMapManager.GridToWorldCenter(new Vector2I(x, y));
                Color col = (state == FogState.Unexplored) ? _unexploredColor : _foggedColor;

                for (int i = 0; i < 6; i++)
                {
                    _polyBuffer[i] = center + HexOffsets[i];
                }

                DrawColoredPolygon(_polyBuffer, col);
            }
        }
    }
}

