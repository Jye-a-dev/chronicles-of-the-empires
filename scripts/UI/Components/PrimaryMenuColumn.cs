using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Component managing primary 4X actions with reactive multi-language support.
/// </summary>
public partial class PrimaryMenuColumn : VBoxContainer
{
    [Signal]
    public delegate void NewCampaignRequestedEventHandler();

    [Signal]
    public delegate void LoadGameRequestedEventHandler();

    [Signal]
    public delegate void SettingsRequestedEventHandler();

    [Signal]
    public delegate void QuitRequestedEventHandler();

    private Button _btnNewGame = null!;
    private Button _btnLoadGame = null!;
    private Button _btnSettings = null!;
    private Button _btnQuit = null!;

    public override void _Ready()
    {
        _btnNewGame = GetNodeOrNull<Button>("%BtnNewGame") ?? GetNodeOrNull<Button>("BtnNewGame")!;
        _btnLoadGame = GetNodeOrNull<Button>("%BtnLoadGame") ?? GetNodeOrNull<Button>("BtnLoadGame")!;
        _btnSettings = GetNodeOrNull<Button>("%BtnSettings") ?? GetNodeOrNull<Button>("BtnSettings")!;
        _btnQuit = GetNodeOrNull<Button>("%BtnQuit") ?? GetNodeOrNull<Button>("BtnQuit")!;

        if (_btnNewGame != null)
        {
            _btnNewGame.Pressed += () => EmitSignal(SignalName.NewCampaignRequested);
        }

        if (_btnLoadGame != null)
        {
            _btnLoadGame.Pressed += () => EmitSignal(SignalName.LoadGameRequested);
        }

        if (_btnSettings != null)
        {
            _btnSettings.Pressed += () => EmitSignal(SignalName.SettingsRequested);
        }

        if (_btnQuit != null)
        {
            _btnQuit.Pressed += () => EmitSignal(SignalName.QuitRequested);
        }

        LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
        UpdateLocalizedStrings();
    }

    public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
    {
        Button[] buttons = [_btnNewGame, _btnLoadGame, _btnSettings, _btnQuit];
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                MenuButtonAnimator.Attach(btn, sfxHover, sfxClick);
            }
        }
    }

    public void SetButtonsDisabled(bool disabled)
    {
        Button[] buttons = [_btnNewGame, _btnLoadGame, _btnSettings, _btnQuit];
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.Disabled = disabled;
            }
        }
    }

    private void UpdateLocalizedStrings()
    {
        if (_btnNewGame != null) _btnNewGame.Text = LocalizationManager.Get("MENU_NEW_CAMPAIGN");
        if (_btnLoadGame != null) _btnLoadGame.Text = LocalizationManager.Get("MENU_LOAD_GAME");
        if (_btnSettings != null) _btnSettings.Text = LocalizationManager.Get("MENU_SETTINGS");
        if (_btnQuit != null) _btnQuit.Text = LocalizationManager.Get("MENU_EXIT");
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
    }
}
