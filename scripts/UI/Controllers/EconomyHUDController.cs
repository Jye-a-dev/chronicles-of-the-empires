using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay;
using ChroniclesOfTheEmpires.Gameplay.Economy;
using ChroniclesOfTheEmpires.UI.Components;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Controllers;

/// <summary>
/// Controller bridging the pure EconomyManager engine and decoupled UI HUD components.
/// Listens to turn income and deficit events to update TacticalResourceBar and TileInfoModal.
/// Strictly event-driven; avoids direct mutating calls into gameplay data.
/// </summary>
public partial class EconomyHUDController : Node
{
    [Export] public NodePath? TacticalHudPath;
    [Export] public NodePath? EconomyManagerPath;
    [Export] public NodePath? TurnManagerPath;

    private TacticalHUD? _hud;
    private EconomyManager? _economyManager;
    private TurnManager? _turnManager;

    public override void _Ready()
    {
        if (TacticalHudPath != null)
        {
            _hud = GetNodeOrNull<TacticalHUD>(TacticalHudPath);
        }
        if (EconomyManagerPath != null)
        {
            _economyManager = GetNodeOrNull<EconomyManager>(EconomyManagerPath);
        }
        if (TurnManagerPath != null)
        {
            _turnManager = GetNodeOrNull<TurnManager>(TurnManagerPath);
        }
    }

    /// <summary>
    /// Explicit dependency injection binding for decoupled scene instantiation.
    /// </summary>
    public void Bind(EconomyManager economyManager, TacticalHUD hud, TurnManager turnManager)
    {
        Unbind();

        _economyManager = economyManager;
        _hud = hud;
        _turnManager = turnManager;

        _economyManager.OnTurnIncomeCalculated += HandleTurnIncomeCalculated;
        _economyManager.OnDeficitTriggered += HandleDeficitTriggered;
    }

    public void Unbind()
    {
        if (_economyManager != null)
        {
            _economyManager.OnTurnIncomeCalculated -= HandleTurnIncomeCalculated;
            _economyManager.OnDeficitTriggered -= HandleDeficitTriggered;
        }
    }

    /// <summary>
    /// Event handler responding to OnTurnIncomeCalculated for player faction (FactionId == 0).
    /// Updates TacticalResourceBar telemetry via TacticalHUD.
    /// </summary>
    private void HandleTurnIncomeCalculated(
        FactionData faction,
        ResourceBundle gross,
        ResourceBundle upkeep,
        ResourceBundle netIncome)
    {
        if (faction.FactionId != 0 || _hud == null) return;

        int currentTurn = _turnManager?.TurnCount ?? 1;
        _hud.UpdateEconomy(faction.Treasury, netIncome, currentTurn);
    }

    /// <summary>
    /// Event handler responding to economic deficit penalties.
    /// </summary>
    private void HandleDeficitTriggered(FactionData faction, string message)
    {
        if (faction.FactionId != 0) return;
        GD.PrintRich($"[color=#ff4444][Báo Động Kinh Tế][/color] {message}");
    }

    /// <summary>
    /// Displays detailed terrain, resource deposit, and required improvement costs in the TileInfoModal.
    /// </summary>
    public void OnTileSelected(HexCell cell)
    {
        _hud?.DisplayTile(cell);
    }

    public override void _ExitTree()
    {
        Unbind();
    }
}

