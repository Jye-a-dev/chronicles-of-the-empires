using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Central asset manager and texture cache for tactical military units and terrain hexes.
/// Guarantees O(1) texture retrieval, zero-allocation runtime spawns, and standardized
/// fallback protocols across all 18 factions (0..17).
/// </summary>
public static class UnitTextureManager
{
    public const string UnitAssetDirectory = "res://assets/sprites/units/";
    public const string TerrainAssetDirectory = "res://assets/sprites/terrain/";
    public const string DefaultUnitKey = "cam_ve_quan";
    public const int DefaultFactionId = 0;
    public const string FallbackUnitPath = "res://assets/sprites/units/unit_cam_ve_quan_0.png";

    private static readonly Dictionary<string, Texture2D?> _unitTextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Texture2D?> _terrainTextureCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Retrieves a unit texture for the specified unit key and faction ID.
    /// Implements strict fallback hierarchy:
    /// 1. Specific faction asset: res://assets/sprites/units/unit_{unitKey}_{factionId}.png
    /// 2. Faction 0 fallback:    res://assets/sprites/units/unit_{unitKey}_0.png
    /// 3. Global default:        res://assets/sprites/units/unit_cam_ve_quan_0.png
    /// All textures are cached in memory to eliminate repeated disk I/O and prevent frame drops during batch spawning.
    /// </summary>
    public static Texture2D? GetUnitTexture(string unitKey, int factionId)
    {
        string normalizedKey = NormalizeUnitKey(unitKey);
        string cacheKey = $"{normalizedKey}_{factionId}";

        if (_unitTextureCache.TryGetValue(cacheKey, out var cachedTexture))
        {
            return cachedTexture;
        }

        Texture2D? texture = ResolveUnitTextureWithFallback(normalizedKey, factionId);
        _unitTextureCache[cacheKey] = texture;
        return texture;
    }

    private static Texture2D? ResolveUnitTextureWithFallback(string unitKey, int factionId)
    {
        // 1. Exact match for current faction
        string primaryPath = $"{UnitAssetDirectory}unit_{unitKey}_{factionId}.png";
        if (ResourceLoader.Exists(primaryPath))
        {
            return LoadTexture(primaryPath);
        }

        // Secondary stripped key match (e.g., 'tien_phong_dich' -> 'tien_phong')
        if (unitKey.EndsWith("_dich", StringComparison.OrdinalIgnoreCase))
        {
            string strippedKey = unitKey[..^5];
            string strippedPath = $"{UnitAssetDirectory}unit_{strippedKey}_{factionId}.png";
            if (ResourceLoader.Exists(strippedPath))
            {
                return LoadTexture(strippedPath);
            }
        }

        // 2. Fallback to faction 0 asset
        string faction0Path = $"{UnitAssetDirectory}unit_{unitKey}_0.png";
        if (ResourceLoader.Exists(faction0Path))
        {
            return LoadTexture(faction0Path);
        }

        if (unitKey.EndsWith("_dich", StringComparison.OrdinalIgnoreCase))
        {
            string strippedKey = unitKey[..^5];
            string stripped0Path = $"{UnitAssetDirectory}unit_{strippedKey}_0.png";
            if (ResourceLoader.Exists(stripped0Path))
            {
                return LoadTexture(stripped0Path);
            }
        }

        // 3. Fallback to global default (Cam Ve Quan of Faction 0)
        if (ResourceLoader.Exists(FallbackUnitPath))
        {
            return LoadTexture(FallbackUnitPath);
        }

        GD.PushWarning($"[UnitTextureManager] Unable to resolve any sprite for unit '{unitKey}' (Faction {factionId}). Fallback failed.");
        return null;
    }

    /// <summary>
    /// Retrieves the completed 32x32 point-to-point terrain texture mapped by BiomeType.
    /// Plains  -> tile_plains.png
    /// Forest  -> tile_forest.png
    /// River   -> tile_water.png
    /// Mountain-> tile_mountain.png (or null for procedural generation fallback)
    /// </summary>
    public static Texture2D? GetTerrainTexture(BiomeType biome)
    {
        string fileName = biome switch
        {
            BiomeType.Plains => "tile_plains.png",
            BiomeType.Forest => "tile_forest.png",
            BiomeType.River => "tile_water.png",
            BiomeType.Mountain => "tile_mountain.png",
            BiomeType.Ocean => "tile_ocean.png",
            BiomeType.Hill => "tile_hill.png",
            _ => "tile_plains.png"
        };

        string path = $"{TerrainAssetDirectory}{fileName}";
        if (_terrainTextureCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        Texture2D? texture = ResourceLoader.Exists(path) ? LoadTexture(path) : null;
        _terrainTextureCache[path] = texture;
        return texture;
    }

    /// <summary>
    /// TerrainType forwarding overload for Pointy-topped Hexagon tile sets.
    /// </summary>
    public static Texture2D? GetTerrainTexture(TerrainType terrain)
    {
        BiomeType biome = terrain switch
        {
            TerrainType.Plains => BiomeType.Plains,
            TerrainType.Forest => BiomeType.Forest,
            TerrainType.River => BiomeType.River,
            TerrainType.Mountain => BiomeType.Mountain,
            TerrainType.Ocean => BiomeType.Ocean,
            TerrainType.Hill => BiomeType.Hill,
            _ => BiomeType.Plains
        };
        return GetTerrainTexture(biome);
    }

    private static Texture2D? LoadTexture(string path)
    {
        try
        {
            return GD.Load<Texture2D>(path);
        }
        catch (Exception ex)
        {
            GD.PushError($"[UnitTextureManager] Error loading texture at '{path}': {ex.Message}");
            return null;
        }
    }

    private static string NormalizeUnitKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey)) return DefaultUnitKey;
        string key = rawKey.Trim().ToLowerInvariant();
        if (key.StartsWith("unit_"))
        {
            key = key[5..];
        }
        return key;
    }

    /// <summary>
    /// Configures a CanvasItem or Sprite2D to enforce Nearest texture filtering
    /// for razor-sharp 32x32 pixel art rendering.
    /// </summary>
    public static void ApplyPixelFilter(CanvasItem item)
    {
        item.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
    }

    /// <summary>
    /// Pre-warms the texture cache with specified unit keys and factions to eliminate JIT stalls.
    /// </summary>
    public static void PreloadUnitTextures(IEnumerable<string> unitKeys, IEnumerable<int> factionIds)
    {
        foreach (var key in unitKeys)
        {
            foreach (var fId in factionIds)
            {
                GetUnitTexture(key, fId);
            }
        }
    }

    /// <summary>
    /// Audits all 18 factions (0..17) against standard unit archetypes and logs warnings for any missing assets.
    /// </summary>
    public static void ValidateFactionAssets(int maxFactionId = 17)
    {
        var unitKeys = new[] { "cam_ve_quan", "cung_thu", "tien_phong", "tien_phong_dich", "xa_thu" };
        int total = 0;
        int custom = 0;
        int fallbacks = 0;
        int missing = 0;

        GD.Print($"=== [ASSET REGISTRY] Starting Faction Asset Audit (Factions 0..{maxFactionId}) ===");

        for (int f = 0; f <= maxFactionId; f++)
        {
            foreach (var key in unitKeys)
            {
                total++;
                string specificPath = $"{UnitAssetDirectory}unit_{key}_{f}.png";
                if (ResourceLoader.Exists(specificPath))
                {
                    custom++;
                }
                else
                {
                    string fallback0 = $"{UnitAssetDirectory}unit_{key}_0.png";
                    if (ResourceLoader.Exists(fallback0))
                    {
                        fallbacks++;
                        GD.PushWarning($"[AssetRegistry] Faction {f} missing '{key}' sprite. Falling back to '{fallback0}'.");
                    }
                    else if (ResourceLoader.Exists(FallbackUnitPath))
                    {
                        fallbacks++;
                        GD.PushWarning($"[AssetRegistry] Faction {f} missing '{key}' and fallback 0. Falling back to global default '{FallbackUnitPath}'.");
                    }
                    else
                    {
                        missing++;
                        GD.PushError($"[AssetRegistry] CRITICAL: No sprite or fallback available for unit '{key}' in Faction {f}!");
                    }
                }
            }
        }

        GD.Print($"=== [ASSET REGISTRY] Audit Complete: {total} checked | {custom} custom | {fallbacks} fallbacks | {missing} missing ===");
    }

    public static void ClearCache()
    {
        _unitTextureCache.Clear();
        _terrainTextureCache.Clear();
    }
}

/// <summary>
/// Alias class ensuring full API compatibility with UnitAssetRegistry specification.
/// </summary>
public static class UnitAssetRegistry
{
    public static Texture2D? GetUnitTexture(string unitKey, int factionId) =>
        UnitTextureManager.GetUnitTexture(unitKey, factionId);

    public static Texture2D? GetTerrainTexture(BiomeType biome) =>
        UnitTextureManager.GetTerrainTexture(biome);

    public static Texture2D? GetTerrainTexture(TerrainType terrain) =>
        UnitTextureManager.GetTerrainTexture(terrain);

    public static void ValidateFactionAssets(int maxFactionId = 17) =>
        UnitTextureManager.ValidateFactionAssets(maxFactionId);

    public static void ApplyPixelFilter(CanvasItem item) =>
        UnitTextureManager.ApplyPixelFilter(item);

    public static void ClearCache() =>
        UnitTextureManager.ClearCache();
}

