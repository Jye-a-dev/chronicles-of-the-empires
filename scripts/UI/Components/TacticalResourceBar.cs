using System;
using Godot;
using ChroniclesOfTheEmpires.UI;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Components;

/// <summary>
/// Component managing top-left tactical HUD resource yields, session branding, and turn telemetry.
/// </summary>
public partial class TacticalResourceBar : PanelContainer
{
    private Label? _sessionLabel;
    private RichTextLabel? _foodLabel;
    private RichTextLabel? _prodLabel;
    private RichTextLabel? _goldLabel;
    private Label? _turnTitleLabel;
    private Label? _turnLabel;

    private string _stageTitle = "";

    public override void _Ready()
    {
        _sessionLabel = GetNodeOrNull<Label>("%SessionLabel") ?? GetNodeOrNull<Label>("Margin/HBox/SessionBadge/SessionLabel");
        _foodLabel = GetNodeOrNull<RichTextLabel>("%FoodLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/FoodLabel");
        _prodLabel = GetNodeOrNull<RichTextLabel>("%ProdLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/ProdLabel");
        _goldLabel = GetNodeOrNull<RichTextLabel>("%GoldLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/GoldLabel");
        _turnTitleLabel = GetNodeOrNull<Label>("%TurnTitleLabel") ?? GetNodeOrNull<Label>("Margin/HBox/TurnBadge/VBox/TurnTitleLabel");
        _turnLabel = GetNodeOrNull<Label>("%TurnLabel") ?? GetNodeOrNull<Label>("Margin/HBox/TurnBadge/VBox/TurnLabel");

        ApplyPlateStyling();

        LocalizationManager.LanguageChanged += UpdateLocalizedTexts;
        UpdateLocalizedTexts();
    }

    public void Initialize(string stageTitle)
    {
        _stageTitle = stageTitle;
        UpdateSessionTitle();
    }

    public void UpdateEconomy(int food, int foodYield, int prod, int prodYield, int gold, int goldYield, int turn)
    {
        if (_foodLabel != null)
        {
            _foodLabel.Text = $"🌾 {food} [color=#68b87d]+{foodYield}[/color]";
        }

        if (_prodLabel != null)
        {
            _prodLabel.Text = $"🔨 {prod} [color=#68b87d]+{prodYield}[/color]";
        }

        if (_goldLabel != null)
        {
            _goldLabel.Text = $"🪙 {gold} [color=#68b87d]+{goldYield}[/color]";
        }

        if (_turnLabel != null)
        {
            _turnLabel.Text = $"{turn}";
        }
    }

    private void UpdateSessionTitle()
    {
        if (_sessionLabel != null && !string.IsNullOrEmpty(_stageTitle))
        {
            _sessionLabel.Text = $"⚔ {_stageTitle.ToUpperInvariant()}";
        }
    }

    private void UpdateLocalizedTexts()
    {
        UpdateSessionTitle();

        if (_turnTitleLabel != null)
        {
            _turnTitleLabel.Text = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? "LƯỢT" : "TURN";
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
