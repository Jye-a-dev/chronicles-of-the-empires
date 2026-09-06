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
