using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Coordinates the turn cycle, synchronizes with EconomyManager, and refreshes unit tactical budgets.
/// Single source of truth for the turn progression pipeline.
/// </summary>
public partial class TurnManager : Node
{
    [Signal]
    public delegate void TurnChangedEventHandler(
        int turn,
        int food,
        int prod,
        int gold,
        int deltaFood,
        int deltaProd,
        int deltaGold
    );

    [Signal]
    public delegate void EconomyUpdatedEventHandler(int food, int prod, int gold);

    public int TurnCount { get; private set; } = 1;

    public int Food => _playerFaction?.Treasury.Food ?? 0;
    public int Production => _playerFaction?.Treasury.Production ?? 0;
    public int Gold => _playerFaction?.Treasury.Gold ?? 0;
    public int Science => _playerFaction?.Treasury.Science ?? 0;
    public int Faith => _playerFaction?.Treasury.Faith ?? 0;

    public int FoodYield { get; private set; }
    public int ProductionYield { get; private set; }
    public int GoldYield { get; private set; }

    private readonly List<UnitController> _playerUnits = new();
    private FactionData? _playerFaction;
    private EconomyManager? _economyManager;
    private UnitRegistry? _unitRegistry;

    public void Initialize(FactionData playerFaction, EconomyManager economyManager, UnitRegistry? unitRegistry = null)
    {
        _playerFaction = playerFaction;
        _economyManager = economyManager;
        _unitRegistry = unitRegistry;
        UpdateYields();
    }

    public void RegisterPlayerUnit(UnitController unit)
    {
        if (!_playerUnits.Contains(unit))
        {
            _playerUnits.Add(unit);
        }
    }

    public void UnregisterPlayerUnit(UnitController unit)
    {
        _playerUnits.Remove(unit);
    }

    private void UpdateYields()
    {
        if (_playerFaction != null && _economyManager != null)
        {
            var (_, _, net) = _economyManager.CalculateTurnIncome(_playerFaction);
            FoodYield = net.Food;
            ProductionYield = net.Production;
            GoldYield = net.Gold;
        }
    }

    /// <summary>
    /// Executes atomic End Turn cycle:
    /// 1. Increments turn counter.
    /// 2. Processes economy for all factions via EconomyManager.
    /// 3. Resets tactical movement budgets for player units.
    /// 4. Emits synchronized signals.
    /// </summary>
    public void EndTurn()
    {
        TurnCount++;

        // 1. Process all factions economy atomically
        if (_economyManager != null)
        {
            _economyManager.ProcessAllFactionsEndTurn();
        }

        UpdateYields();

        // 2. Refresh player military movement points
        if (_unitRegistry != null)
        {
            var playerUnits = _unitRegistry.GetUnitsForFaction(0);
            for (int i = 0; i < playerUnits.Count; i++)
            {
                var unit = playerUnits[i];
                if (IsInstanceValid(unit))
                {
                    unit.ResetTurnMovement();
                }
            }
        }
        else
        {
            for (int i = 0; i < _playerUnits.Count; i++)
            {
                var unit = _playerUnits[i];
                if (IsInstanceValid(unit))
                {
                    unit.ResetTurnMovement();
                }
            }
        }

        // 3. Emit turn changed signal
        EmitSignal(
            SignalName.TurnChanged,
            TurnCount,
            Food,
            Production,
            Gold,
            FoodYield,
            ProductionYield,
            GoldYield
        );
    }

    public void SpendResources(int foodCost, int prodCost, int goldCost)
    {
        if (_playerFaction != null)
        {
            var cost = new ResourceBundle(foodCost, prodCost, goldCost, 0, 0);
            _playerFaction.Treasury -= cost;
            EmitSignal(SignalName.EconomyUpdated, Food, Production, Gold);
        }
    }
}
