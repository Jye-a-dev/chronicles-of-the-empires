using System;
using Godot;
using ChroniclesOfTheEmpires.UI;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Components;

/// <summary>
/// Component managing top-right tactical function buttons (Settings, Return to Menu, End Turn).
/// </summary>
public partial class TacticalFunctionBar : PanelContainer
{
    [Signal]
    public delegate void SettingsRequestedEventHandler();

    [Signal]
    public delegate void ExitToMenuRequestedEventHandler();

    [Signal]
    public delegate void EndTurnRequestedEventHandler();

    private Button? _btnSettings;
    private Button? _btnBack;
    private Button? _btnEndTurn;

    public override void _Ready()
    {
        _btnSettings = GetNodeOrNull<Button>("%BtnSettings") ?? GetNodeOrNull<Button>("Margin/HBox/BtnSettings");
        _btnBack = GetNodeOrNull<Button>("%BtnBack") ?? GetNodeOrNull<Button>("Margin/HBox/BtnBack");
        _btnEndTurn = GetNodeOrNull<Button>("%BtnEndTurn") ?? GetNodeOrNull<Button>("Margin/HBox/BtnEndTurn");

        if (_btnSettings != null)
        {
            _btnSettings.Pressed += () => EmitSignal(SignalName.SettingsRequested);
        }

        if (_btnBack != null)
        {
            _btnBack.Pressed += () => EmitSignal(SignalName.ExitToMenuRequested);
        }

        if (_btnEndTurn != null)
        {
            _btnEndTurn.Pressed += () => EmitSignal(SignalName.EndTurnRequested);
        }

        ApplyPlateStyling();

        LocalizationManager.LanguageChanged += UpdateLocalizedTexts;
        UpdateLocalizedTexts();
    }

    public void SetButtonsDisabled(bool disabled)
    {
        if (_btnSettings != null) _btnSettings.Disabled = disabled;
        if (_btnBack != null) _btnBack.Disabled = disabled;
        if (_btnEndTurn != null) _btnEndTurn.Disabled = disabled;
    }

    private void UpdateLocalizedTexts()
    {
        if (_btnEndTurn != null)
        {
            _btnEndTurn.Text = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? "🚩 HẾT LƯỢT" : "🚩 END TURN";
        }

        if (_btnSettings != null)
        {
            _btnSettings.TooltipText = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? "Cài đặt" : "Settings";
        }

        if (_btnBack != null)
        {
            _btnBack.TooltipText = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? "Thoát ra Menu" : "Exit to Menu";
        }
    }

    private void ApplyPlateStyling()
    {
        var plateStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.07f, 0.08f, 0.92f),
            BorderWidthTop = 1,
            BorderWidthBottom = 2,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderColor = new Color("#8a6a2e"),
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3,
            ContentMarginLeft = 6,
            ContentMarginRight = 6,
            ContentMarginTop = 2,
            ContentMarginBottom = 2,
            ShadowColor = new Color(0, 0, 0, 0.6f),
            ShadowSize = 2,
            ShadowOffset = new Vector2(0, 2)
        };
        AddThemeStyleboxOverride("panel", plateStyle);
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedTexts;
    }
}
