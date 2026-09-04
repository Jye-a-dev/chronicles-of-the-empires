using Godot;
using System;
using System.Collections.Generic;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Config;

public record HexConfig(
    string Id,
    string Name,
    string Description,
    int MoveCost,
    int FoodYield,
    int ProdYield,
    int GoldYield,
    bool IsSolid,
    Color BaseColor,
    Color BorderColor,
    Dictionary<string, Color> ExtraColors
);

public record UnitConfig(
    string Id,
    string Name,
    string Description,
    int FactionId,
    int HpMax,
    int Attack,
    int Defense,
    int MovementMax,
    string Symbol,
    Color VisualColor,
    Color BorderColor
);

/// <summary>
/// Central configuration engine parsing external .txt files for hexagonal terrain and tactical units.
/// Allows full runtime data-driven customization of stats, visual hues, and military lore.
/// </summary>
public static class GameConfigManager
{
    public const string HexSettingsPath = "res://data/hex_settings.txt";
    public const string UnitSettingsPath = "res://data/unit_settings.txt";

    private static readonly Dictionary<string, HexConfig> _hexConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, UnitConfig> _unitConfigs = new(StringComparer.OrdinalIgnoreCase);

    private static bool _isLoaded;

    static GameConfigManager()
    {
        LoadAll();
    }

    public static void EnsureLoaded()
    {
        if (!_isLoaded)
        {
            LoadAll();
        }
    }

    public static void LoadAll()
    {
        LoadHexSettings();
        LoadUnitSettings();
        _isLoaded = true;
    }

    public static HexConfig? GetHexConfig(TerrainType terrain)
    {
        EnsureLoaded();
        string key = terrain switch
        {
            TerrainType.Plains => "plains",
            TerrainType.Forest => "forest",
            TerrainType.River => "river",
            TerrainType.Mountain => "mountain",
            _ => "plains"
        };
        return _hexConfigs.TryGetValue(key, out var config) ? config : null;
    }

    public static HexConfig? GetHexConfig(string id)
    {
        EnsureLoaded();
        return _hexConfigs.TryGetValue(id, out var config) ? config : null;
    }

    public static UnitConfig? GetUnitConfig(string id)
    {
        EnsureLoaded();
        return _unitConfigs.TryGetValue(id, out var config) ? config : null;
    }

    public static IReadOnlyDictionary<string, UnitConfig> GetAllUnitConfigs()
    {
        EnsureLoaded();
        return _unitConfigs;
    }

    private static void LoadHexSettings()
    {
        _hexConfigs.Clear();
        if (!FileAccess.FileExists(HexSettingsPath)) return;

        using var file = FileAccess.Open(HexSettingsPath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        string? currentSection = null;
        var sectionDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void FlushCurrentSection()
        {
            if (string.IsNullOrWhiteSpace(currentSection) || sectionDict.Count == 0) return;

            string name = sectionDict.GetValueOrDefault("name", currentSection);
            string desc = sectionDict.GetValueOrDefault("description", string.Empty);
            int.TryParse(sectionDict.GetValueOrDefault("move_cost", "1"), out int moveCost);
            int.TryParse(sectionDict.GetValueOrDefault("food_yield", "0"), out int foodYield);
            int.TryParse(sectionDict.GetValueOrDefault("prod_yield", "0"), out int prodYield);
            int.TryParse(sectionDict.GetValueOrDefault("gold_yield", "0"), out int goldYield);
            bool.TryParse(sectionDict.GetValueOrDefault("is_solid", "false"), out bool isSolid);

            Color baseColor = TryParseColor(sectionDict.GetValueOrDefault("base_color", "#3e732c"), new Color(0.28f, 0.52f, 0.22f));
            Color borderColor = TryParseColor(sectionDict.GetValueOrDefault("border_color", "#346028"), baseColor.Darkened(0.2f));

            var extraColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in sectionDict)
            {
                if (k.EndsWith("_color") && k != "base_color" && k != "border_color")
                {
                    extraColors[k] = TryParseColor(v, Colors.White);
                }
            }

            _hexConfigs[currentSection] = new HexConfig(
                currentSection,
                name,
                desc,
                moveCost,
                foodYield,
                prodYield,
                goldYield,
                isSolid,
                baseColor,
                borderColor,
                extraColors
            );
            sectionDict.Clear();
        }

        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith(';')) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                FlushCurrentSection();
                currentSection = line[1..^1].Trim();
                continue;
            }

            int eqIdx = line.IndexOf('=');
            if (eqIdx > 0)
            {
                string key = line[..eqIdx].Trim().ToLowerInvariant();
                string val = line[(eqIdx + 1)..].Trim();
                sectionDict[key] = val;
            }
        }
        FlushCurrentSection();
    }

    private static void LoadUnitSettings()
    {
        _unitConfigs.Clear();
        if (!FileAccess.FileExists(UnitSettingsPath)) return;

        using var file = FileAccess.Open(UnitSettingsPath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        string? currentSection = null;
        var sectionDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void FlushCurrentSection()
        {
            if (string.IsNullOrWhiteSpace(currentSection) || sectionDict.Count == 0) return;

            string name = sectionDict.GetValueOrDefault("name", currentSection);
            string desc = sectionDict.GetValueOrDefault("description", string.Empty);
            int.TryParse(sectionDict.GetValueOrDefault("faction_id", "0"), out int factionId);
            int.TryParse(sectionDict.GetValueOrDefault("hp_max", "20"), out int hpMax);
            int.TryParse(sectionDict.GetValueOrDefault("attack", "5"), out int attack);
            int.TryParse(sectionDict.GetValueOrDefault("defense", "2"), out int defense);
            int.TryParse(sectionDict.GetValueOrDefault("movement_max", "4"), out int moveMax);
            string symbol = sectionDict.GetValueOrDefault("symbol", factionId == 0 ? "★" : "◆");

            Color visualColor = TryParseColor(
                sectionDict.GetValueOrDefault("visual_color", factionId == 0 ? "#2d241e" : "#2e1614"),
                factionId == 0 ? new Color(0.18f, 0.14f, 0.12f) : new Color(0.18f, 0.08f, 0.08f)
            );
            Color borderColor = TryParseColor(
                sectionDict.GetValueOrDefault("border_color", factionId == 0 ? "#f5c842" : "#e03b24"),
                factionId == 0 ? new Color(0.96f, 0.78f, 0.26f) : new Color(0.88f, 0.23f, 0.14f)
            );

            _unitConfigs[currentSection] = new UnitConfig(
                currentSection,
                name,
                desc,
                factionId,
                hpMax,
                attack,
                defense,
                moveMax,
                symbol,
                visualColor,
                borderColor
            );
            sectionDict.Clear();
        }

        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith(';')) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                FlushCurrentSection();
                currentSection = line[1..^1].Trim();
                continue;
            }

            int eqIdx = line.IndexOf('=');
            if (eqIdx > 0)
            {
                string key = line[..eqIdx].Trim().ToLowerInvariant();
                string val = line[(eqIdx + 1)..].Trim();
                sectionDict[key] = val;
            }
        }
        FlushCurrentSection();
    }

    private static Color TryParseColor(string hex, Color fallback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            return new Color(hex);
        }
        catch
        {
            return fallback;
        }
    }
}
