using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Components;

/// <summary>
/// Component managing the game title emblem, primary title typography, and localized subtitle.
/// </summary>
public partial class MenuTitle : VBoxContainer
{
    private Label? _titleLabel;
    private Label? _subtitleLabel;

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("%TitleLabel") ?? GetNodeOrNull<Label>("TitleLabel");
        _subtitleLabel = GetNodeOrNull<Label>("%SubtitleLabel") ?? GetNodeOrNull<Label>("SubtitleLabel");

        LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
        UpdateLocalizedStrings();
    }

    public void UpdateLocalizedStrings()
    {
        if (_titleLabel != null)
        {
            _titleLabel.Text = LocalizationManager.Get("MENU_TITLE");
        }

        if (_subtitleLabel != null)
        {
            _subtitleLabel.Text = LocalizationManager.Get("MENU_SUBTITLE");
        }
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
    }
}

