using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Combat;

/// <summary>
/// Immutable snapshot of a unit's tactical parameters at the initiation of combat.
/// Decoupled from Godot Node2D and SceneTree lifecycle.
/// </summary>
public sealed class UnitCombatSnapshot
{
    public int UnitId { get; init; }
    public int FactionId { get; init; }
    public string UnitType { get; init; } = "";
    public int HpCurrent { get; init; }
    public int HpMax { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int MoraleCurrent { get; init; }
    public int MoraleMax { get; init; }
    public bool IsRanged { get; init; }

    public UnitCombatSnapshot() { }

    public UnitCombatSnapshot(
        int unitId,
        int factionId,
        string unitType,
        int hpCurrent,
        int hpMax,
        int attack,
        int defense,
        int moraleCurrent,
        int moraleMax,
        bool isRanged)
    {
        UnitId = unitId;
        FactionId = factionId;
        UnitType = unitType;
        HpCurrent = hpCurrent;
        HpMax = hpMax;
        Attack = attack;
        Defense = defense;
        MoraleCurrent = moraleCurrent;
        MoraleMax = moraleMax;
        IsRanged = isRanged;
    }
}

/// <summary>
/// Data contract dispatched to decouple turn-based encounters from micro-tactical pausable RTS arenas.
/// </summary>
public sealed class BattlePayload
{
    public string BattleId { get; init; } = Guid.NewGuid().ToString("N");
    public Vector2I HexPosition { get; init; }
    public BiomeType TerrainType { get; init; }
    public List<UnitCombatSnapshot> Attackers { get; init; } = new();
    public List<UnitCombatSnapshot> Defenders { get; init; } = new();

    public BattlePayload() { }

    public BattlePayload(
        string battleId,
        Vector2I hexPosition,
        BiomeType terrainType,
        List<UnitCombatSnapshot> attackers,
        List<UnitCombatSnapshot> defenders)
    {
        BattleId = battleId;
        HexPosition = hexPosition;
        TerrainType = terrainType;
        Attackers = attackers;
        Defenders = defenders;
    }
}

/// <summary>
/// Output data contract returned by the tactical battle resolver (instant or RTS sub-scene)
/// to apply state mutations back onto the strategic WorldMap.
/// </summary>
public sealed class BattleResolutionResult
{
    public string BattleId { get; init; } = "";
    public int VictorFactionId { get; init; }
    public List<UnitCombatSnapshot> SurvivingUnits { get; init; } = new();
    public List<int> DestroyedUnitIds { get; init; } = new();

    public BattleResolutionResult() { }

    public BattleResolutionResult(
        string battleId,
        int victorFactionId,
        List<UnitCombatSnapshot> survivingUnits,
        List<int> destroyedUnitIds)
    {
        BattleId = battleId;
        VictorFactionId = victorFactionId;
        SurvivingUnits = survivingUnits;
        DestroyedUnitIds = destroyedUnitIds;
    }
}

