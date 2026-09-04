using System.Collections.Generic;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Manages the turn-based game loop, turn counters, national stockpiles, and unit movement refresh cycles.
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
    public int Food { get; private set; } = 150;
    public int Production { get; private set; } = 80;
    public int Gold { get; private set; } = 120;

    public int FoodYield { get; set; } = 12;
    public int ProductionYield { get; set; } = 8;
    public int GoldYield { get; set; } = 10;

    private readonly List<UnitController> _playerUnits = new();

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

    /// <summary>
    /// Executes the End Turn cycle: increments turn, harvests yields, and refreshes unit movement budgets.
    /// </summary>
    public void EndTurn()
    {
        TurnCount++;
        Food += FoodYield;
        Production += ProductionYield;
        Gold += GoldYield;

        foreach (var unit in _playerUnits)
        {
            if (IsInstanceValid(unit))
            {
                unit.ResetTurnMovement();
            }
        }

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
        Food -= foodCost;
        Production -= prodCost;
        Gold -= goldCost;
        EmitSignal(SignalName.EconomyUpdated, Food, Production, Gold);
    }
}

