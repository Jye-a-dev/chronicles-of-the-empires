using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Component managing secondary utility buttons with reactive multi-language support.
/// </summary>
public partial class SecondaryActionBar : HBoxContainer
{
    [Signal]
    public delegate void CivilopediaRequestedEventHandler();

    [Signal]
    public delegate void CreditsRequestedEventHandler();

    [Signal]
    public delegate void CommunityRequestedEventHandler();

    private Button _btnCivilopedia = null!;
    private Button _btnCredits = null!;
    private Button _btnCommunity = null!;

    public override void _Ready()
    {
        _btnCivilopedia = GetNodeOrNull<Button>("%BtnCivilopedia") ?? GetNodeOrNull<Button>("BtnCivilopedia")!;
        _btnCredits = GetNodeOrNull<Button>("%BtnCredits") ?? GetNodeOrNull<Button>("BtnCredits")!;
        _btnCommunity = GetNodeOrNull<Button>("%BtnCommunity") ?? GetNodeOrNull<Button>("BtnCommunity")!;

        if (_btnCivilopedia != null)
        {
            _btnCivilopedia.Pressed += () => EmitSignal(SignalName.CivilopediaRequested);
        }

        if (_btnCredits != null)
        {
            _btnCredits.Pressed += () => EmitSignal(SignalName.CreditsRequested);
        }

        if (_btnCommunity != null)
        {
            _btnCommunity.Pressed += () => EmitSignal(SignalName.CommunityRequested);
        }

        LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
        UpdateLocalizedStrings();
    }

    public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
    {
        Button[] buttons = [_btnCivilopedia, _btnCredits, _btnCommunity];
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                MenuButtonAnimator.Attach(btn, sfxHover, sfxClick, hoverScale: 1.08f, pressScale: 0.94f);
            }
        }
    }

    public void SetButtonsDisabled(bool disabled)
    {
        Button[] buttons = [_btnCivilopedia, _btnCredits, _btnCommunity];
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
        if (_btnCivilopedia != null)
        {
            _btnCivilopedia.Text = LocalizationManager.Get("MENU_ARCHIVES");
            _btnCivilopedia.TooltipText = LocalizationManager.Get("MENU_ARCHIVES_TOOLTIP");
        }

        if (_btnCredits != null)
        {
            _btnCredits.Text = LocalizationManager.Get("MENU_CREDITS");
            _btnCredits.TooltipText = LocalizationManager.Get("MENU_CREDITS_TOOLTIP");
        }

        if (_btnCommunity != null)
        {
            _btnCommunity.Text = LocalizationManager.Get("MENU_DISCORD");
            _btnCommunity.TooltipText = LocalizationManager.Get("MENU_DISCORD_TOOLTIP");
        }
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
    }
}
