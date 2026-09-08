using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.UI;

/// <summary>
/// Centralized icon cache and lookup service for HUD, economy, and entity telemetry.
/// Provides O(1) texture retrieval, procedural fallback textures for missing assets,
/// and factory methods for pixel-perfect nearest-filtered TextureRect widgets.
/// </summary>
public static class IconManager
{
    public const string IconDirectory = "res://assets/sprites/icons/";

    private static readonly Dictionary<string, Texture2D> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> _warnedMissingKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves and returns a cached Texture2D for the given standardized icon key.
    /// Returns a procedural fallback texture if the physical file does not exist on disk.
    /// </summary>
    public static Texture2D GetIcon(string iconKey)
    {
        string normalizedKey = NormalizeKey(iconKey);

        if (_cache.TryGetValue(normalizedKey, out var cachedTexture))
        {
            return cachedTexture;
        }

        string fullPath = $"{IconDirectory}icon_{normalizedKey}.png";
        if (ResourceLoader.Exists(fullPath))
        {
            var loaded = ResourceLoader.Load<Texture2D>(fullPath);
            if (loaded != null)
            {
                _cache[normalizedKey] = loaded;
                return loaded;
            }
        }

        // Check direct path without "icon_" prefix if user passed exact name
        string directPath = $"{IconDirectory}{normalizedKey}.png";
        if (ResourceLoader.Exists(directPath))
        {
            var loaded = ResourceLoader.Load<Texture2D>(directPath);
            if (loaded != null)
            {
                _cache[normalizedKey] = loaded;
                return loaded;
            }
        }

        if (_warnedMissingKeys.Add(normalizedKey))
        {
            GD.PushWarning($"[IconManager] Icon '{fullPath}' not found on disk. Utilizing procedural fallback icon.");
        }

        var fallback = GenerateFallbackTexture(normalizedKey);
        _cache[normalizedKey] = fallback;
        return fallback;
    }

    /// <summary>
    /// Retrieves the texture corresponding to a core ResourceType enum.
    /// </summary>
    public static Texture2D GetResourceIcon(ResourceType type)
    {
        string key = type switch
        {
            ResourceType.Food => "food",
            ResourceType.Production => "production",
            ResourceType.Gold => "gold",
            ResourceType.Science => "science",
            ResourceType.Faith => "faith",
            _ => "gold"
        };
        return GetIcon(key);
    }

    /// <summary>
    /// Constructs a standardized pixel-perfect TextureRect configured for compact 640x360 HUD layout.
    /// </summary>
    public static TextureRect CreateIconRect(string iconKey, int size = 18)
    {
        var texture = GetIcon(iconKey);
        return new TextureRect
        {
            Name = $"Icon_{NormalizeKey(iconKey)}",
            Texture = texture,
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    /// <summary>
    /// Constructs a standardized pixel-perfect TextureRect for a given ResourceType.
    /// </summary>
    public static TextureRect CreateResourceIconRect(ResourceType type, int size = 18)
    {
        string key = type switch
        {
            ResourceType.Food => "food",
            ResourceType.Production => "production",
            ResourceType.Gold => "gold",
            ResourceType.Science => "science",
            ResourceType.Faith => "faith",
            _ => "gold"
        };
        return CreateIconRect(key, size);
    }

    private static string NormalizeKey(string rawKey)
    {
        string key = rawKey.Trim().ToLowerInvariant();
        if (key.EndsWith(".png"))
        {
            key = key[..^4];
        }
        if (key.StartsWith("icon_"))
        {
            key = key[5..];
        }
        return key;
    }

    private static ImageTexture GenerateFallbackTexture(string key)
    {
        const int size = 18;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);

        Color primary = GetFallbackColor(key);
        Color border = new Color(primary.R * 0.4f, primary.G * 0.4f, primary.B * 0.4f, 1.0f);
        Color accent = new Color(Mathf.Min(1f, primary.R * 1.3f), Mathf.Min(1f, primary.G * 1.3f), Mathf.Min(1f, primary.B * 1.3f), 1.0f);

        // 18x18 procedural icon with solid 1px border and distinct inner geometry
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                {
                    image.SetPixel(x, y, border);
                }
                else if (x >= 5 && x <= 12 && y >= 5 && y <= 12)
                {
                    image.SetPixel(x, y, accent);
                }
                else
                {
                    image.SetPixel(x, y, primary);
                }
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static Color GetFallbackColor(string key)
    {
        return key switch
        {
            "food" => new Color("#8ec252"),
            "production" => new Color("#c4894d"),
            "gold" => new Color("#f5c842"),
            "science" => new Color("#42a5f5"),
            "faith" => new Color("#ab47bc"),
            "hp" => new Color("#e03b24"),
            "morale" => new Color("#ffb300"),
            "movement" => new Color("#26a69a"),
            "attack" => new Color("#d32f2f"),
            "defense" => new Color("#3f51b5"),
            "flag_surrender" or "surrender" => new Color("#f0f0f0"),
            "recruit" => new Color("#ffd54f"),
            "city" => new Color("#ff7043"),
            "improvement" => new Color("#8d6e63"),
            "mountain" => new Color("#6c757d"),
            "copper" => new Color("#d35400"),
            "iron" => new Color("#7f8c8d"),
            "herbs" => new Color("#27ae60"),
            "jade" => new Color("#1abc9c"),
            "farm" => new Color("#f39c12"),
            "mine" => new Color("#95a5a6"),
            "lumber" or "lumbermill" => new Color("#a0522d"),
            "watchtower" => new Color("#4a69bd"),
            "ownership" or "owner" or "flag" => new Color("#f5c842"),
            _ => new Color("#888888")
        };
    }
}

