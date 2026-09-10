using Godot;
using System;
using System.Collections.Generic;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Config;

/// <summary>
/// Central configuration engine parsing external .txt files for hexagonal terrain, tactical units,
/// resource deposits, buildings, and factions.
/// Allows full runtime data-driven customization of all simulation objects without code changes.
/// </summary>
public static partial class GameConfigManager
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
            TerrainType.Ocean => "ocean",
            TerrainType.Hill => "hill",
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
}
