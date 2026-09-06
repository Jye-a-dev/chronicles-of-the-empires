using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Stack-allocated zero-GC collection of the 6 hexagonal cell neighbors.
/// Uses TileMapLayer.GetNeighborCell to eliminate all array/heap allocations.
/// </summary>
public readonly struct HexNeighbors : IEnumerable<Vector2I>
{
    private readonly Vector2I _n0;
    private readonly Vector2I _n1;
    private readonly Vector2I _n2;
    private readonly Vector2I _n3;
    private readonly Vector2I _n4;
    private readonly Vector2I _n5;

    public int Count { get; }

    public HexNeighbors(TileMapLayer? layer, Vector2I pos)
    {
        if (layer == null)
        {
            _n0 = _n1 = _n2 = _n3 = _n4 = _n5 = default;
            Count = 0;
            return;
        }

        _n0 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.RightSide);
        _n1 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.BottomRightSide);
        _n2 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.BottomLeftSide);
        _n3 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.LeftSide);
        _n4 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.TopLeftSide);
        _n5 = layer.GetNeighborCell(pos, TileSet.CellNeighbor.TopRightSide);
        Count = 6;
    }

    public Vector2I this[int index] => index switch
    {
        0 => _n0,
        1 => _n1,
        2 => _n2,
        3 => _n3,
        4 => _n4,
        5 => _n5,
        _ => throw new IndexOutOfRangeException($"HexNeighbor index {index} out of range [0..{Count - 1}].")
    };

    public bool Contains(Vector2I target)
    {
        if (Count == 0) return false;
        return _n0 == target || _n1 == target || _n2 == target ||
               _n3 == target || _n4 == target || _n5 == target;
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<Vector2I> IEnumerable<Vector2I>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<Vector2I>
    {
        private readonly HexNeighbors _neighbors;
        private int _index;

        public Enumerator(in HexNeighbors neighbors)
        {
            _neighbors = neighbors;
            _index = -1;
        }

        public Vector2I Current => _neighbors[_index];
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _neighbors.Count;
        }

        public void Reset() => _index = -1;
        public void Dispose() { }
    }
}

