using Godot;
using System;
using System.Collections.Generic;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Config;

public static partial class GameConfigManager
{
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

            string symbol = dict.GetValueOrDefault("symbol", factionId == 0 ? "â˜…" : "â—†");

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
