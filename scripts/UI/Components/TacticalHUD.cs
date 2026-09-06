using System;
using Godot;
using ChroniclesOfTheEmpires.Gameplay;
using ChroniclesOfTheEmpires.UI.Modals;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Components;

/// <summary>
/// Master HUD coordinator managing tactical resource telemetry, action bars, inspector modals, and settings dialogs.
/// </summary>
public partial class TacticalHUD : Control
{
    [Signal]
    public delegate void SettingsRequestedEventHandler();

    [Signal]
    public delegate void ExitToMenuRequestedEventHandler();

    [Signal]
    public delegate void EndTurnRequestedEventHandler();

    [Signal]
    public delegate void RecruitRequestedEventHandler(string unitConfigId, Vector2I coords);

    private TacticalResourceBar? _resourceBar;
    private TacticalFunctionBar? _functionBar;
    private TileInfoModal? _tileInfoModal;
    private UnitInfoModal? _unitInfoModal;
    private SettingsModal? _settingsModal;

    public bool IsSettingsOpen => _settingsModal?.IsOpen ?? false;

    public override void _Ready()
    {
        _resourceBar = GetNodeOrNull<TacticalResourceBar>("%ResourceBar") ?? GetNodeOrNull<TacticalResourceBar>("ResourceBar");
        _functionBar = GetNodeOrNull<TacticalFunctionBar>("%FunctionBar") ?? GetNodeOrNull<TacticalFunctionBar>("FunctionBar");
        _tileInfoModal = GetNodeOrNull<TileInfoModal>("%TileInfoModal") ?? GetNodeOrNull<TileInfoModal>("ModalContainer/TileInfoModal");
        _unitInfoModal = GetNodeOrNull<UnitInfoModal>("%UnitInfoModal") ?? GetNodeOrNull<UnitInfoModal>("ModalContainer/UnitInfoModal");
        _settingsModal = GetNodeOrNull<SettingsModal>("%SettingsModal") ?? GetNodeOrNull<SettingsModal>("../SettingsModal");

        if (_tileInfoModal != null)
        {
            _tileInfoModal.RecruitRequested += (unitConfigId, coords) =>
                EmitSignal(SignalName.RecruitRequested, unitConfigId, coords);
        }

        if (_functionBar != null)
        {
            _functionBar.SettingsRequested += () =>
            {
                OpenSettings();
                EmitSignal(SignalName.SettingsRequested);
            };
            _functionBar.ExitToMenuRequested += () => EmitSignal(SignalName.ExitToMenuRequested);
            _functionBar.EndTurnRequested += () => EmitSignal(SignalName.EndTurnRequested);
        }
    }

    public void Initialize(string stageTitle)
    {
        _resourceBar?.Initialize(stageTitle);
    }

    public void UpdateEconomy(int food, int foodYield, int prod, int prodYield, int gold, int goldYield, int turn)
    {
        _resourceBar?.UpdateEconomy(food, foodYield, prod, prodYield, gold, goldYield, turn);
    }

    public void UpdateEconomy(in ChroniclesOfTheEmpires.Core.Economy.ResourceBundle treasury, in ChroniclesOfTheEmpires.Core.Economy.ResourceBundle netIncome, int turn)
    {
        _resourceBar?.UpdateEconomy(treasury, netIncome, turn);
    }

    public void DisplayTile(HexCell cell)
    {
        _unitInfoModal?.CloseModal();
        _tileInfoModal?.DisplayCell(cell);
    }

    public void DisplayUnit(UnitController unit)
    {
        _tileInfoModal?.CloseModal();
        _unitInfoModal?.DisplayUnit(unit);
    }

    public void RefreshUnitInfo()
    {
        _unitInfoModal?.RefreshMovementInfo();
    }

    public void DeselectAll()
    {
        _tileInfoModal?.CloseModal();
        _unitInfoModal?.CloseModal();
    }

    public void OpenSettings()
    {
        _settingsModal?.Open();
    }

    public void CloseSettings()
    {
        _settingsModal?.Close();
    }

    public void SetButtonsDisabled(bool disabled)
    {
        _functionBar?.SetButtonsDisabled(disabled);
    }
}
