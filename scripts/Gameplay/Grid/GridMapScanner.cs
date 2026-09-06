using System;
using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class GridMapManager
{
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
            BuildProceduralTileSet();
            ScanFromLayoutArray();
            RenderTileMap();
        }
        else if (hasPaintedCells && _tileMapLayer != null)
        {
            ScanFromTileMapLayer();
        }
        else
        {
            BuildProceduralTileSet();
            GenerateTerrain();
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
}

