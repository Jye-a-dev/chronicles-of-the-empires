using System;
using Godot;

namespace ChroniclesOfTheEmpires.Core.Mathematics;

/// <summary>
/// High-efficiency, zero-allocation mathematical utilities for Pointy-Topped Hexagonal Grids (Odd-r layout).
/// Eliminates P/Invoke calls to TileMapLayer and replaces invalid Manhattan distance with exact Cube Coordinates.
/// </summary>
public static class HexMath
{
    // Order of 6 neighbor directions for Pointy-topped Hex (Odd-r layout)
    // Even row (y % 2 == 0)
    private static readonly Vector2I[] EvenRowNeighbors =
    [
        new Vector2I(1, 0),   // E
        new Vector2I(0, 1),   // SE
        new Vector2I(-1, 1),  // SW
        new Vector2I(-1, 0),  // W
        new Vector2I(-1, -1), // NW
        new Vector2I(0, -1)   // NE
    ];

    // Odd row (Math.Abs(y) % 2 == 1)
    private static readonly Vector2I[] OddRowNeighbors =
    [
        new Vector2I(1, 0),   // E
        new Vector2I(1, 1),   // SE
        new Vector2I(0, 1),   // SW
        new Vector2I(-1, 0),  // W
        new Vector2I(0, -1),  // NW
        new Vector2I(1, -1)   // NE
    ];

    public readonly struct CubeCoord
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public CubeCoord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public static CubeCoord OddRToCube(Vector2I hex)
    {
        int x = hex.X - (hex.Y - (hex.Y & 1)) / 2;
        int z = hex.Y;
        int y = -x - z;
        return new CubeCoord(x, y, z);
    }

    public static int GetDistance(Vector2I a, Vector2I b)
    {
        CubeCoord ac = OddRToCube(a);
        CubeCoord bc = OddRToCube(b);
        return (System.Math.Abs(ac.X - bc.X) +
                System.Math.Abs(ac.Y - bc.Y) +
                System.Math.Abs(ac.Z - bc.Z)) / 2;
    }

    public static Vector2I[] GetNeighborOffsets(int gridY)
    {
        return (System.Math.Abs(gridY) & 1) == 0 ? EvenRowNeighbors : OddRowNeighbors;
    }

    /// <summary>
    /// Writes 6 adjacent hex coordinates directly into the caller-provided span buffer without heap allocation.
    /// </summary>
    public static int GetNeighborsNonAlloc(Vector2I center, Span<Vector2I> outNeighbors)
    {
        if (outNeighbors.Length < 6)
            throw new ArgumentException("Span buffer must contain at least 6 elements.", nameof(outNeighbors));

        ReadOnlySpan<Vector2I> offsets = GetNeighborOffsets(center.Y);
        for (int i = 0; i < 6; i++)
        {
            outNeighbors[i] = center + offsets[i];
        }
        return 6;
    }
}
