using System;
using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Hexagonal tactical pathfinding manager using Godot 4 AStar2D.
/// Maps 6-directional hex graph adjacency and synchronizes HexCell terrain traversal costs and obstacles.
/// </summary>
public sealed class PathfindingManager
{
    private readonly AStar2D _aStar = new();
    private int _width = 32;
    private int _height = 32;

    public void Initialize(int width, int height, GridMapManager gridMap)
    {
        _width = width;
        _height = height;
        _aStar.Clear();

        // 1. Register all hex vertices with world coordinate positions
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var pos = new Vector2I(x, y);
                long id = GetPointId(pos);
                _aStar.AddPoint(id, GridMapManager.GridToWorldCenter(pos));
            }
        }

        // 2. Connect each hex cell with its 6 surrounding adjacent neighbors
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var pos = new Vector2I(x, y);
                long id = GetPointId(pos);
                var neighbors = gridMap.GetSurroundingCells(pos);

                foreach (var neighbor in neighbors)
                {
                    if (IsInBounds(neighbor))
                    {
                        long neighborId = GetPointId(neighbor);
                        if (!_aStar.ArePointsConnected(id, neighborId))
                        {
                            _aStar.ConnectPoints(id, neighborId);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Synchronizes terrain obstacles and movement costs from HexCell objects.
    /// </summary>
    public void SyncWithMap(GridMapManager gridMap)
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var pos = new Vector2I(x, y);
                long id = GetPointId(pos);
                var cell = gridMap.GetCell(pos);

                if (cell != null)
                {
                    _aStar.SetPointDisabled(id, cell.IsSolid);
                    _aStar.SetPointWeightScale(id, cell.MoveCost);
                }
            }
        }
    }

    public void SetPointSolid(Vector2I pos, bool solid)
    {
        if (IsInBounds(pos))
        {
            _aStar.SetPointDisabled(GetPointId(pos), solid);
        }
    }

    /// <summary>
    /// Re-evaluates traversal weight scale and impassable obstacle state for a cell based on TileTerrainData and defense buildings.
    /// </summary>
    public void UpdateCellObstacle(Vector2I pos, HexCell cell)
    {
        if (IsInBounds(pos))
        {
            long id = GetPointId(pos);
            _aStar.SetPointDisabled(id, cell.IsSolid);
            _aStar.SetPointWeightScale(id, cell.MoveCost);
        }
    }

    public bool IsPointSolid(Vector2I pos)
    {
        return !IsInBounds(pos) || _aStar.IsPointDisabled(GetPointId(pos));
    }

    /// <summary>
    /// Computes optimal 6-way hexagonal path between two coordinates.
    private Vector2I[] _pathBuffer = new Vector2I[128];
    private int _pathBufferCount = 0;

    /// <summary>
    /// Computes optimal 6-way hexagonal path into a reusable buffer, returning ReadOnlySpan with 0 allocations.
    /// </summary>
    public ReadOnlySpan<Vector2I> FindPathSpan(Vector2I start, Vector2I target)
    {
        _pathBufferCount = 0;
        if (!IsInBounds(start) || !IsInBounds(target) || IsPointSolid(target))
        {
            return ReadOnlySpan<Vector2I>.Empty;
        }

        long startId = GetPointId(start);
        long targetId = GetPointId(target);

        long[] idPath = _aStar.GetIdPath(startId, targetId);
        if (idPath == null || idPath.Length == 0)
        {
            return ReadOnlySpan<Vector2I>.Empty;
        }

        if (_pathBuffer.Length < idPath.Length)
        {
            _pathBuffer = new Vector2I[Math.Max(_pathBuffer.Length * 2, idPath.Length)];
        }

        _pathBufferCount = idPath.Length;
        for (int i = 0; i < idPath.Length; i++)
        {
            _pathBuffer[i] = GetCoordFromId(idPath[i]);
        }

        return new ReadOnlySpan<Vector2I>(_pathBuffer, 0, _pathBufferCount);
    }

    /// <summary>
    /// Computes optimal 6-way hexagonal path between two coordinates.
    /// </summary>
    public Vector2I[] FindPath(Vector2I start, Vector2I target)
    {
        var span = FindPathSpan(start, target);
        return span.ToArray();
    }

    public int CalculatePathCost(ReadOnlySpan<Vector2I> path, GridMapManager gridMap)
    {
        if (path.Length <= 1) return 0;

        int totalCost = 0;
        for (int i = 1; i < path.Length; i++)
        {
            var cell = gridMap.GetCell(path[i]);
            totalCost += cell?.MoveCost ?? 1;
        }
        return totalCost;
    }

    public int CalculatePathCost(Vector2I[] path, GridMapManager gridMap) =>
        CalculatePathCost(new ReadOnlySpan<Vector2I>(path), gridMap);

    public bool IsInBounds(Vector2I pos) =>
        pos.X >= 0 && pos.X < _width && pos.Y >= 0 && pos.Y < _height;

    private long GetPointId(Vector2I pos) => ((long)pos.Y * _width) + pos.X;

    private Vector2I GetCoordFromId(long id) =>
        new((int)(id % _width), (int)(id / _width));
}
