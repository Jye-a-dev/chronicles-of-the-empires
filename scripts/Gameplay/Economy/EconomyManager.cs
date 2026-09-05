using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay.Economy;

/// <summary>
/// High-performance turn-based economic engine.
/// Calculates gross yields, maintenance upkeeps, net treasury updates, and handles deficit states.
/// Zero-allocation inner loop optimized for 16-18 factions per turn cycle.
/// </summary>
public partial class EconomyManager : Node
{
    public static EconomyManager? Instance { get; private set; }

    /// <summary>
    /// Event broadcast when a faction's turn income is computed.
    /// Parameters: Faction, GrossYield, TotalUpkeep, NetIncome.
    /// </summary>
    public event Action<FactionData, ResourceBundle, ResourceBundle, ResourceBundle>? OnTurnIncomeCalculated;

    /// <summary>
    /// Event broadcast when a deficit penalty or disbanding event occurs.
    /// </summary>
    public event Action<FactionData, string>? OnDeficitTriggered;

    private readonly List<FactionData> _factions = new();
    private Func<Vector2I, TileTerrainData?>? _tileDataProvider;

    public IReadOnlyList<FactionData> Factions => _factions;

    public override void _Ready()
    {
        Instance = this;
    }

    public void SetTileDataProvider(Func<Vector2I, TileTerrainData?> provider)
    {
        _tileDataProvider = provider;
    }

    public void RegisterFaction(FactionData faction)
    {
        if (!_factions.Contains(faction))
        {
            _factions.Add(faction);
        }
    }

    public void UnregisterFaction(FactionData faction)
    {
        _factions.Remove(faction);
    }

    public void ClearFactions()
    {
        _factions.Clear();
    }

    /// <summary>
    /// Computes turn income for a single faction:
    /// 1. Sums BaseYield from controlled terrain and BonusYield from exploited resource deposits.
    /// 2. Sums YieldBonus from operational faction buildings.
    /// 3. Computes cumulative Upkeep costs from active military units and buildings.
    /// </summary>
    public (ResourceBundle GrossYield, ResourceBundle Upkeep, ResourceBundle NetIncome) CalculateTurnIncome(
        FactionData faction,
        Func<Vector2I, TileTerrainData?>? tileProvider = null)
    {
        var provider = tileProvider ?? _tileDataProvider;
        var grossYield = ResourceBundle.Zero;
        var totalUpkeep = ResourceBundle.Zero;

        // 1. Process Controlled Territorial Tiles & Exploited Deposits
        if (provider != null && faction.ControlledTiles.Count > 0)
        {
            foreach (var coord in faction.ControlledTiles)
            {
                var tile = provider(coord);
                if (tile == null) continue;

                grossYield += tile.BaseYield;

                if (tile.Deposit is { IsExploited: true } deposit)
                {
                    grossYield += deposit.BonusYield;
                }
            }
        }

        // 2. Process Active Faction Buildings (Production & Upkeep)
        var buildings = faction.Buildings;
        int buildingCount = buildings.Count;
        for (int i = 0; i < buildingCount; i++)
        {
            var building = buildings[i];
            if (!building.IsActive) continue;

            grossYield += building.YieldBonus;
            totalUpkeep += building.Upkeep;
        }

        // 3. Process Active Military Units (Upkeep)
        var units = faction.Units;
        int unitCount = units.Count;
        for (int i = 0; i < unitCount; i++)
        {
            var unit = units[i];
            if (!unit.IsActive) continue;

            totalUpkeep += unit.Upkeep;
        }

        var netIncome = grossYield - totalUpkeep;
        return (grossYield, totalUpkeep, netIncome);
    }

    /// <summary>
    /// Executes End Turn economics for a single faction:
    /// Updates Treasury, handles famine/bankruptcy deficits, and notifies listeners.
    /// </summary>
    public void ProcessFactionEndTurn(FactionData faction)
    {
        var (gross, upkeep, net) = CalculateTurnIncome(faction);

        // Update Treasury
        faction.Treasury += net;

        // Resolve Deficit States
        ResolveDeficitState(faction);

        // Notify Listeners (HUD, AI controllers, telemetry)
        OnTurnIncomeCalculated?.Invoke(faction, gross, upkeep, net);
    }

    /// <summary>
    /// Batch processes all registered factions (16-18) in sub-millisecond execution time.
    /// </summary>
    public void ProcessAllFactionsEndTurn()
    {
        int count = _factions.Count;
        for (int i = 0; i < count; i++)
        {
            ProcessFactionEndTurn(_factions[i]);
        }
    }

    /// <summary>
    /// Evaluates deficit states (Food < 0 or Gold < 0) and applies severe gameplay penalties:
    /// - Food Deficit: Decreases morale and inflicts starvation attrition on troops.
    /// - Gold Deficit: Locks military recruitment and disbands the highest-upkeep military unit.
    /// </summary>
    public void ResolveDeficitState(FactionData faction)
    {
        // 1. Food Deficit (Famine & Starvation)
        if (faction.Treasury.Food < 0)
        {
            faction.MoralePenalty = Math.Clamp(faction.MoralePenalty + 2, 0, 10);

            // Apply starvation attrition: reduce current HP of active units
            var units = faction.Units;
            int unitCount = units.Count;
            for (int i = 0; i < unitCount; i++)
            {
                var unit = units[i];
                if (unit.IsActive && unit.CurrentHp > 1)
                {
                    unit.CurrentHp = Math.Max(1, unit.CurrentHp - 2);
                }
            }

            OnDeficitTriggered?.Invoke(faction, $"[Thâm hụt Lương thực] Sĩ khí giảm -{faction.MoralePenalty}, quân sĩ chịu suy thoái do đói kém.");
        }
        else if (faction.MoralePenalty > 0)
        {
            // Gradually recover morale when food is sufficient
            faction.MoralePenalty = Math.Max(0, faction.MoralePenalty - 1);
        }

        // 2. Gold Deficit (Bankruptcy & Desertion)
        if (faction.Treasury.Gold < 0)
        {
            faction.IsRecruitmentLocked = true;

            // Disband the unit with the highest upkeep to alleviate bankruptcy
            UnitData? highestUpkeepUnit = null;
            int maxGoldUpkeep = -1;

            var units = faction.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u.IsActive && u.Upkeep.Gold > maxGoldUpkeep)
                {
                    maxGoldUpkeep = u.Upkeep.Gold;
                    highestUpkeepUnit = u;
                }
            }

            if (highestUpkeepUnit != null)
            {
                highestUpkeepUnit.IsActive = false;
                units.Remove(highestUpkeepUnit);
                OnDeficitTriggered?.Invoke(faction, $"[Thâm hụt Ngân khố] Quân đoàn {highestUpkeepUnit.Name} đã tự giải tán do không có bổng lộc!");
            }
            else
            {
                OnDeficitTriggered?.Invoke(faction, "[Thâm hụt Ngân khố] Vỡ nợ! Cấm chiêu mộ binh sĩ mới.");
            }
        }
        else
        {
            faction.IsRecruitmentLocked = false;
        }
    }
}

