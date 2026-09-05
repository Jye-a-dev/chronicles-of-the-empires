using Godot;
using System;
using System.Collections.Generic;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Config;

public record HexConfig(
    string Id,
    string Name,
    string Description,
    string Biome,
    int MoveCost,
    int FoodYield,
    int ProdYield,
    int GoldYield,
    int SciYield,
    int FaithYield,
    bool IsSolid,
    bool IsBlocked,
    Color BaseColor,
    Color BorderColor,
    Dictionary<string, Color> ExtraColors
)
{
    public ResourceBundle BaseYield => new(FoodYield, ProdYield, GoldYield, SciYield, FaithYield);
}

public record UnitConfig(
    string Id,
    string Name,
    string Description,
    int FactionId,
    int HpMax,
    int Attack,
    int Defense,
    int MovementMax,
    ResourceBundle Cost,
    ResourceBundle Upkeep,
    string Symbol,
    Color VisualColor,
    Color BorderColor
);

public record DepositConfig(
    string Id,
    string Name,
    DepositCategory Category,
    string RequiredImprovement,
    ResourceBundle BonusYield,
    bool IsTradeable
);

public record BuildingConfig(
    string Id,
    string Name,
    string Category,
    ResourceBundle Cost,
    ResourceBundle Upkeep,
    ResourceBundle YieldBonus,
    bool IsDefense
);

public record FactionConfig(
    int Id,
    string Key,
    string Name,
    string CulturalSphere,
    ResourceBundle StartingTreasury
);

/// <summary>
/// Central configuration engine parsing external .txt files for hexagonal terrain, tactical units,
/// resource deposits, buildings, and factions.
/// Allows full runtime data-driven customization of all simulation objects without code changes.
/// </summary>
public static class GameConfigManager
{
    public const string HexSettingsPath = "res://data/gameplay/hex_settings.txt";
    public const string UnitSettingsPath = "res://data/gameplay/unit_settings.txt";
    public const string DepositsSettingsPath = "res://data/economy/deposits.txt";
    public const string BuildingsSettingsPath = "res://data/economy/buildings.txt";
    public const string FactionsSettingsPath = "res://data/economy/factions.txt";

    private static string ResolvePath(string path, string fallback) =>
        FileAccess.FileExists(path) ? path : fallback;

    private static readonly Dictionary<string, HexConfig> _hexConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, UnitConfig> _unitConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, DepositConfig> _depositConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, BuildingConfig> _buildingConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, FactionConfig> _factionConfigs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, FactionConfig> _factionsById = new();

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
        LoadDepositsSettings();
        LoadBuildingsSettings();
        LoadFactionsSettings();
        _isLoaded = true;
    }

    #region Accessors

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

    public static DepositConfig? GetDepositConfig(string id)
    {
        EnsureLoaded();
        return _depositConfigs.TryGetValue(id, out var config) ? config : null;
    }

    public static IReadOnlyDictionary<string, DepositConfig> GetAllDepositConfigs()
    {
        EnsureLoaded();
        return _depositConfigs;
    }

    public static BuildingConfig? GetBuildingConfig(string id)
    {
        EnsureLoaded();
        return _buildingConfigs.TryGetValue(id, out var config) ? config : null;
    }

    public static IReadOnlyDictionary<string, BuildingConfig> GetAllBuildingConfigs()
    {
        EnsureLoaded();
        return _buildingConfigs;
    }

    public static FactionConfig? GetFactionConfig(string key)
    {
        EnsureLoaded();
        return _factionConfigs.TryGetValue(key, out var config) ? config : null;
    }

    public static FactionConfig? GetFactionConfig(int factionId)
    {
        EnsureLoaded();
        return _factionsById.TryGetValue(factionId, out var config) ? config : null;
    }

    public static IReadOnlyDictionary<string, FactionConfig> GetAllFactionConfigs()
    {
        EnsureLoaded();
        return _factionConfigs;
    }

    #endregion

    #region Parsers

    private static void LoadHexSettings()
    {
        _hexConfigs.Clear();
        ParseIniSections(ResolvePath(HexSettingsPath, "res://data/hex_settings.txt"), (section, dict) =>
        {
            string name = dict.GetValueOrDefault("name", section);
            string desc = dict.GetValueOrDefault("description", string.Empty);
            string biome = dict.GetValueOrDefault("biome", "Plains");
            int.TryParse(dict.GetValueOrDefault("move_cost", "1"), out int moveCost);
            int.TryParse(dict.GetValueOrDefault("food_yield", "0"), out int foodYield);
            int.TryParse(dict.GetValueOrDefault("prod_yield", "0"), out int prodYield);
            int.TryParse(dict.GetValueOrDefault("gold_yield", "0"), out int goldYield);
            int.TryParse(dict.GetValueOrDefault("sci_yield", "0"), out int sciYield);
            int.TryParse(dict.GetValueOrDefault("faith_yield", "0"), out int faithYield);
            bool.TryParse(dict.GetValueOrDefault("is_solid", "false"), out bool isSolid);
            bool.TryParse(dict.GetValueOrDefault("is_blocked", isSolid.ToString()), out bool isBlocked);

            Color baseColor = TryParseColor(dict.GetValueOrDefault("base_color", "#3e732c"), new Color(0.28f, 0.52f, 0.22f));
            Color borderColor = TryParseColor(dict.GetValueOrDefault("border_color", "#346028"), baseColor.Darkened(0.2f));

            var extraColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in dict)
            {
                if (k.EndsWith("_color") && k != "base_color" && k != "border_color")
                {
                    extraColors[k] = TryParseColor(v, Colors.White);
                }
            }

            _hexConfigs[section] = new HexConfig(
                section,
                name,
                desc,
                biome,
                moveCost,
                foodYield,
                prodYield,
                goldYield,
                sciYield,
                faithYield,
                isSolid,
                isBlocked,
                baseColor,
                borderColor,
                extraColors
            );
        });
    }

    private static void LoadUnitSettings()
    {
        _unitConfigs.Clear();
        ParseIniSections(ResolvePath(UnitSettingsPath, "res://data/unit_settings.txt"), (section, dict) =>
        {
            string name = dict.GetValueOrDefault("name", section);
            string desc = dict.GetValueOrDefault("description", string.Empty);
            int.TryParse(dict.GetValueOrDefault("faction_id", "0"), out int factionId);
            int.TryParse(dict.GetValueOrDefault("hp_max", "20"), out int hpMax);
            int.TryParse(dict.GetValueOrDefault("attack", "5"), out int attack);
            int.TryParse(dict.GetValueOrDefault("defense", "2"), out int defense);
            int.TryParse(dict.GetValueOrDefault("movement_max", "4"), out int moveMax);

            int.TryParse(dict.GetValueOrDefault("cost_food", "0"), out int costFood);
            int.TryParse(dict.GetValueOrDefault("cost_prod", "0"), out int costProd);
            int.TryParse(dict.GetValueOrDefault("cost_gold", "0"), out int costGold);
            var cost = new ResourceBundle(costFood, costProd, costGold, 0, 0);

            int.TryParse(dict.GetValueOrDefault("upkeep_food", "1"), out int upkeepFood);
            int.TryParse(dict.GetValueOrDefault("upkeep_gold", "0"), out int upkeepGold);
            var upkeep = new ResourceBundle(upkeepFood, 0, upkeepGold, 0, 0);

            string symbol = dict.GetValueOrDefault("symbol", factionId == 0 ? "★" : "◆");

            Color visualColor = TryParseColor(
                dict.GetValueOrDefault("visual_color", factionId == 0 ? "#2d241e" : "#2e1614"),
                factionId == 0 ? new Color(0.18f, 0.14f, 0.12f) : new Color(0.18f, 0.08f, 0.08f)
            );
            Color borderColor = TryParseColor(
                dict.GetValueOrDefault("border_color", factionId == 0 ? "#f5c842" : "#e03b24"),
                factionId == 0 ? new Color(0.96f, 0.78f, 0.26f) : new Color(0.88f, 0.23f, 0.14f)
            );

            _unitConfigs[section] = new UnitConfig(
                section,
                name,
                desc,
                factionId,
                hpMax,
                attack,
                defense,
                moveMax,
                cost,
                upkeep,
                symbol,
                visualColor,
                borderColor
            );
        });
    }

    private static void LoadDepositsSettings()
    {
        _depositConfigs.Clear();
        ParseIniSections(ResolvePath(DepositsSettingsPath, "res://data/deposits.txt"), (section, dict) =>
        {
            string name = dict.GetValueOrDefault("name", section);
            string catStr = dict.GetValueOrDefault("category", "Mineral");
            DepositCategory category = catStr.ToLowerInvariant() switch
            {
                "agricultural" => DepositCategory.Agricultural,
                "cultural" => DepositCategory.Cultural,
                _ => DepositCategory.Mineral
            };

            string reqImp = dict.GetValueOrDefault("required_improvement", "");
            int.TryParse(dict.GetValueOrDefault("food", "0"), out int food);
            int.TryParse(dict.GetValueOrDefault("prod", "0"), out int prod);
            int.TryParse(dict.GetValueOrDefault("gold", "0"), out int gold);
            int.TryParse(dict.GetValueOrDefault("sci", "0"), out int sci);
            int.TryParse(dict.GetValueOrDefault("faith", "0"), out int faith);
            bool.TryParse(dict.GetValueOrDefault("is_tradeable", "true"), out bool isTradeable);

            var bonusYield = new ResourceBundle(food, prod, gold, sci, faith);
            _depositConfigs[section] = new DepositConfig(section, name, category, reqImp, bonusYield, isTradeable);
        });
    }

    private static void LoadBuildingsSettings()
    {
        _buildingConfigs.Clear();
        ParseIniSections(ResolvePath(BuildingsSettingsPath, "res://data/buildings.txt"), (section, dict) =>
        {
            string name = dict.GetValueOrDefault("name", section);
            string cat = dict.GetValueOrDefault("category", "infrastructure");

            int.TryParse(dict.GetValueOrDefault("cost_food", "0"), out int costFood);
            int.TryParse(dict.GetValueOrDefault("cost_prod", "0"), out int costProd);
            int.TryParse(dict.GetValueOrDefault("cost_gold", "0"), out int costGold);
            var cost = new ResourceBundle(costFood, costProd, costGold, 0, 0);

            int.TryParse(dict.GetValueOrDefault("upkeep_food", "0"), out int upkeepFood);
            int.TryParse(dict.GetValueOrDefault("upkeep_gold", "0"), out int upkeepGold);
            var upkeep = new ResourceBundle(upkeepFood, 0, upkeepGold, 0, 0);

            int.TryParse(dict.GetValueOrDefault("yield_food", "0"), out int yieldFood);
            int.TryParse(dict.GetValueOrDefault("yield_prod", "0"), out int yieldProd);
            int.TryParse(dict.GetValueOrDefault("yield_gold", "0"), out int yieldGold);
            int.TryParse(dict.GetValueOrDefault("yield_sci", "0"), out int yieldSci);
            int.TryParse(dict.GetValueOrDefault("yield_faith", "0"), out int yieldFaith);
            var yieldBonus = new ResourceBundle(yieldFood, yieldProd, yieldGold, yieldSci, yieldFaith);

            bool.TryParse(dict.GetValueOrDefault("is_defense", "false"), out bool isDefense);

            _buildingConfigs[section] = new BuildingConfig(section, name, cat, cost, upkeep, yieldBonus, isDefense);
        });
    }

    private static void LoadFactionsSettings()
    {
        _factionConfigs.Clear();
        _factionsById.Clear();
        ParseIniSections(ResolvePath(FactionsSettingsPath, "res://data/factions.txt"), (section, dict) =>
        {
            int.TryParse(dict.GetValueOrDefault("id", "0"), out int id);
            string name = dict.GetValueOrDefault("name", section);
            string culture = dict.GetValueOrDefault("cultural_sphere", "EastAsian");

            int.TryParse(dict.GetValueOrDefault("treasury_food", "100"), out int food);
            int.TryParse(dict.GetValueOrDefault("treasury_prod", "50"), out int prod);
            int.TryParse(dict.GetValueOrDefault("treasury_gold", "100"), out int gold);
            int.TryParse(dict.GetValueOrDefault("treasury_sci", "0"), out int sci);
            int.TryParse(dict.GetValueOrDefault("treasury_faith", "0"), out int faith);
            var starting = new ResourceBundle(food, prod, gold, sci, faith);

            var config = new FactionConfig(id, section, name, culture, starting);
            _factionConfigs[section] = config;
            _factionsById[id] = config;
        });
    }

    private static void ParseIniSections(string filePath, Action<string, Dictionary<string, string>> onSection)
    {
        if (!FileAccess.FileExists(filePath)) return;

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        string? currentSection = null;
        var sectionDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Flush()
        {
            if (!string.IsNullOrWhiteSpace(currentSection) && sectionDict.Count > 0)
            {
                onSection(currentSection, sectionDict);
            }
            sectionDict.Clear();
        }

        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith(';')) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                Flush();
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
        Flush();
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

    #endregion
}
