using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class GridMapManager
{
    private void BuildProceduralTileSet()
    {
        if (_tileMapLayer == null) return;
        if (_tileMapLayer.TileSet != null) return;

        _tileMapLayer.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

        GameConfigManager.EnsureLoaded();

        // Create a 192x32 atlas texture containing 6 colored 32x32 pointy-topped hexagons
        var img = Image.CreateEmpty(CellDimension * 6, CellDimension, false, Image.Format.Rgba8);

        for (int tileIdx = 0; tileIdx < 6; tileIdx++)
        {
            var terrain = (TerrainType)tileIdx;
            int startX = tileIdx * CellDimension;

            // Load finalized terrain sprite asset if available (plains, forest, water, ocean, hill)
            var terrainTex = UnitTextureManager.GetTerrainTexture(terrain);
            if (terrainTex != null)
            {
                var srcImg = terrainTex.GetImage();
                if (srcImg != null)
                {
                    srcImg.Convert(Image.Format.Rgba8);
                    if (srcImg.GetWidth() != CellDimension || srcImg.GetHeight() != CellDimension)
                    {
                        srcImg.Resize(CellDimension, CellDimension, Image.Interpolation.Nearest);
                    }

                    // Apply recessed / sunken depth shading (làm tile hơi chìm xuống sa bàn)
                    for (int py = 0; py < CellDimension; py++)
                    {
                        float dy = Mathf.Abs(py - 15.5f);
                        for (int px = 0; px < CellDimension; px++)
                        {
                            Color pCol = srcImg.GetPixel(px, py);
                            if (pCol.A <= 0.01f) continue;

                            float dx = Mathf.Abs(px - 15.5f);
                            float diag = 0.5f * dx + dy;
                            float distToEdge = Mathf.Min(15.7f - diag, 15.5f - dx);

                            // 1. Subdued depth: slightly lower brightness to push ground plane into background
                            float depthFactor = 0.86f;

                            // 2. Top-down sunken lip shadow from upper tray rim
                            if (py < 6 && distToEdge < 5.0f)
                            {
                                depthFactor *= Mathf.Lerp(0.58f, 0.95f, (float)py / 6f);
                            }

                            // 3. Inset perimeter bevel / ambient occlusion groove
                            if (distToEdge < 1.8f)
                            {
                                depthFactor *= Mathf.Lerp(0.50f, 0.92f, distToEdge / 1.8f);
                            }

                            pCol = new Color(pCol.R * depthFactor, pCol.G * depthFactor, pCol.B * depthFactor, pCol.A);
                            srcImg.SetPixel(px, py, pCol);
                        }
                    }

                    img.BlitRect(srcImg, new Rect2I(0, 0, CellDimension, CellDimension), new Vector2I(startX, 0));
                    continue;
                }
            }

            var cfg = GameConfigManager.GetHexConfig(terrain);

            Color baseCol = cfg?.BaseColor ?? terrain switch
            {
                TerrainType.Plains => new Color(0.25f, 0.48f, 0.20f),
                TerrainType.Forest => new Color(0.12f, 0.30f, 0.11f),
                TerrainType.River => new Color(0.17f, 0.36f, 0.56f),
                TerrainType.Mountain => new Color(0.48f, 0.50f, 0.54f),
                TerrainType.Ocean => new Color(0.09f, 0.23f, 0.41f),
                TerrainType.Hill => new Color(0.32f, 0.40f, 0.20f),
                _ => new Color(0.2f, 0.2f, 0.2f)
            };

            // Soft grid line: low alpha overlay to avoid graph-paper look
            Color borderCol = cfg?.BorderColor ?? baseCol.Darkened(0.14f);

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
                        else if (terrain == TerrainType.Ocean)
                        {
                            Color deepCol = cfg?.ExtraColors.GetValueOrDefault("deep_water_color", new Color(0.06f, 0.16f, 0.29f)) ?? new Color(0.06f, 0.16f, 0.29f);
                            Color waveCol = cfg?.ExtraColors.GetValueOrDefault("wave_color", new Color(0.17f, 0.38f, 0.64f)) ?? new Color(0.17f, 0.38f, 0.64f);

                            if (isBorder)
                            {
                                pixelCol = borderCol;
                            }
                            else
                            {
                                bool isWave = ((x * 3 + y * 2) % 11 == 0) && distToEdge > 3.0f;
                                pixelCol = isWave ? waveCol : (distToEdge > 5.0f ? deepCol : baseCol);
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
                        else if (terrain == TerrainType.Hill)
                        {
                            Color slopeCol = cfg?.ExtraColors.GetValueOrDefault("slope_color", new Color(0.42f, 0.51f, 0.27f)) ?? new Color(0.42f, 0.51f, 0.27f);
                            Color shadeCol = cfg?.ExtraColors.GetValueOrDefault("shade_color", new Color(0.24f, 0.29f, 0.15f)) ?? new Color(0.24f, 0.29f, 0.15f);

                            if (isBorder)
                            {
                                pixelCol = borderCol;
                            }
                            else if (y < 12 && distToEdge >= 2.5f)
                            {
                                pixelCol = slopeCol;
                            }
                            else if (y >= 20)
                            {
                                pixelCol = shadeCol;
                            }
                            else
                            {
                                pixelCol = baseCol;
                            }
                        }
                        else // Mountain
                        {
                            Color peakCol = cfg?.ExtraColors.GetValueOrDefault("peak_color", new Color(0.78f, 0.82f, 0.86f)) ?? new Color(0.78f, 0.82f, 0.86f);
                            Color baseDarkCol = cfg?.ExtraColors.GetValueOrDefault("base_dark_color", new Color(0.28f, 0.30f, 0.34f)) ?? new Color(0.28f, 0.30f, 0.34f);

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

        for (int i = 0; i < 6; i++)
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

        DistributeResourceDeposits(random);
    }

    private void DistributeResourceDeposits(Random rand)
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                var cell = _cells[x, y];
                if (cell == null) continue;

                string? depositKey = null;
                if (cell.Terrain == TerrainType.Plains && rand.NextDouble() < 0.16)
                {
                    depositKey = "paddy_field";
                }
                else if (cell.Terrain == TerrainType.Forest && rand.NextDouble() < 0.18)
                {
                    depositKey = "ancient_forest";
                }
                else if (cell.Terrain == TerrainType.Mountain && rand.NextDouble() < 0.28)
                {
                    depositKey = rand.NextDouble() < 0.6 ? "iron_mine" : "gold_vein";
                }
                else if (cell.Terrain == TerrainType.Hill && rand.NextDouble() < 0.22)
                {
                    depositKey = rand.NextDouble() < 0.6 ? "iron_mine" : "gold_vein";
                }
                else if (rand.NextDouble() < 0.02)
                {
                    depositKey = "sacred_spring";
                }

                if (depositKey != null)
                {
                    var cfg = GameConfigManager.GetDepositConfig(depositKey);
                    if (cfg != null)
                    {
                        cell.Deposit = new ResourceDepositData
                        {
                            Id = cfg.Id,
                            Name = cfg.Name,
                            Category = cfg.Category,
                            RequiredImprovement = cfg.RequiredImprovement,
                            BonusYield = cfg.BonusYield,
                            IsTradeable = cfg.IsTradeable,
                            IsExploited = false
                        };
                    }
                }
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

            if (x > MapWidth - 3 && rand.NextDouble() < 0.45)
            {
                return TerrainType.Ocean;
            }

            if (x < 1 || y < 1 || (x > MapWidth - 2 && y > MapHeight - 2))
            {
                if (rand.NextDouble() < 0.35) return TerrainType.Mountain;
                if (rand.NextDouble() < 0.25) return TerrainType.Hill;
                if (rand.NextDouble() < 0.25) return TerrainType.Forest;
                return TerrainType.Plains;
            }

            if ((x < 2 && y >= 5 && y < MapHeight / 2) || (y < 2 && x >= 5 && x < MapWidth / 2) || (x > MapWidth - 4 && y > MapHeight - 4))
            {
                if (rand.NextDouble() < 0.45) return TerrainType.Mountain;
                if (rand.NextDouble() < 0.30) return TerrainType.Hill;
            }

            if (rand.NextDouble() < 0.18)
            {
                return TerrainType.Forest;
            }

            if (rand.NextDouble() < 0.10)
            {
                return TerrainType.Hill;
            }

            return TerrainType.Plains;
        }

        if (ActiveBiome == "jungle")
        {
            if (rand.NextDouble() < 0.08) return TerrainType.River;
            if (rand.NextDouble() < 0.50) return TerrainType.Forest;
            if (rand.NextDouble() < 0.12) return TerrainType.Hill;
            if (rand.NextDouble() < 0.08) return TerrainType.Mountain;
            return TerrainType.Plains;
        }

        if (ActiveBiome == "highlands")
        {
            if (rand.NextDouble() < 0.25) return TerrainType.Mountain;
            if (rand.NextDouble() < 0.25) return TerrainType.Hill;
            if (rand.NextDouble() < 0.25) return TerrainType.Forest;
            if (rand.NextDouble() < 0.06) return TerrainType.River;
            return TerrainType.Plains;
        }

        if (rand.NextDouble() < 0.10) return TerrainType.Forest;
        if (rand.NextDouble() < 0.08) return TerrainType.Hill;
        if (rand.NextDouble() < 0.04) return TerrainType.River;
        if (rand.NextDouble() < 0.05) return TerrainType.Mountain;
        return TerrainType.Plains;
    }
}

