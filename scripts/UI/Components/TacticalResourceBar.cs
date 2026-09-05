using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.UI;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Components;

/// <summary>
/// Component managing top-left tactical HUD resource telemetry, session branding, and turn telemetry.
/// Strictly non-blocking (MouseFilter = Ignore), compact 640x360 layout, and consistent 9px typography.
/// </summary>
public partial class TacticalResourceBar : PanelContainer
{
    private Label? _sessionLabel;
    private RichTextLabel? _foodLabel;
    private RichTextLabel? _prodLabel;
    private RichTextLabel? _goldLabel;
    private RichTextLabel? _sciLabel;
    private RichTextLabel? _faithLabel;
    private Label? _turnTitleLabel;
    private Label? _turnLabel;

    private string _stageTitle = "";

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        var margin = GetNodeOrNull<MarginContainer>("Margin");
        if (margin != null) margin.MouseFilter = MouseFilterEnum.Ignore;

        var hbox = GetNodeOrNull<HBoxContainer>("Margin/HBox");
        if (hbox != null) hbox.MouseFilter = MouseFilterEnum.Ignore;

        _sessionLabel = GetNodeOrNull<Label>("%SessionLabel") ?? GetNodeOrNull<Label>("Margin/HBox/SessionBadge/SessionLabel");
        _foodLabel = GetNodeOrNull<RichTextLabel>("%FoodLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/FoodLabel");
        _prodLabel = GetNodeOrNull<RichTextLabel>("%ProdLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/ProdLabel");
        _goldLabel = GetNodeOrNull<RichTextLabel>("%GoldLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/GoldLabel");
        _sciLabel = GetNodeOrNull<RichTextLabel>("%SciLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/SciLabel");
        _faithLabel = GetNodeOrNull<RichTextLabel>("%FaithLabel") ?? GetNodeOrNull<RichTextLabel>("Margin/HBox/FaithLabel");

        _turnTitleLabel = GetNodeOrNull<Label>("%TurnTitleLabel") ?? GetNodeOrNull<Label>("Margin/HBox/TurnBadge/VBox/TurnTitleLabel");
        _turnLabel = GetNodeOrNull<Label>("%TurnLabel") ?? GetNodeOrNull<Label>("Margin/HBox/TurnBadge/VBox/TurnLabel");

        // Ensure all resource labels have consistent font size and non-blocking mouse filters
        ConfigureLabel(_foodLabel);
        ConfigureLabel(_prodLabel);
        ConfigureLabel(_goldLabel);
        ConfigureLabel(_sciLabel);
        ConfigureLabel(_faithLabel);

        ApplyPlateStyling();

        LocalizationManager.LanguageChanged += UpdateLocalizedTexts;
        UpdateLocalizedTexts();
    }

    private static void ConfigureLabel(RichTextLabel? label)
    {
        if (label == null) return;
        label.MouseFilter = MouseFilterEnum.Ignore;
        label.AddThemeFontSizeOverride("normal_font_size", 9);
        label.BbcodeEnabled = true;
        label.FitContent = true;
        label.AutowrapMode = TextServer.AutowrapMode.Off;
    }

    public void Initialize(string stageTitle)
    {
        _stageTitle = stageTitle;
        UpdateSessionTitle();
    }

    public void UpdateEconomy(in ResourceBundle treasury, in ResourceBundle netIncome, int turn)
    {
        UpdateResourceLabel(_foodLabel, "🌾", treasury.Food, netIncome.Food);
        UpdateResourceLabel(_prodLabel, "🔨", treasury.Production, netIncome.Production);
        UpdateResourceLabel(_goldLabel, "🪙", treasury.Gold, netIncome.Gold);
        UpdateResourceLabel(_sciLabel, "🔬", treasury.Science, netIncome.Science);
        UpdateResourceLabel(_faithLabel, "📿", treasury.Faith, netIncome.Faith);

        if (_turnLabel != null)
        {
            _turnLabel.Text = $"{turn}";
        }
    }

    public void UpdateEconomy(int food, int foodYield, int prod, int prodYield, int gold, int goldYield, int turn)
    {
        UpdateEconomy(
            new ResourceBundle(food, prod, gold, 0, 0),
            new ResourceBundle(foodYield, prodYield, goldYield, 0, 0),
            turn
        );
    }

    private static void UpdateResourceLabel(RichTextLabel? label, string icon, int current, int delta)
    {
        if (label == null) return;
        string deltaText = delta switch
        {
            > 0 => $"[color=#68b87d]+{delta}[/color]",
            < 0 => $"[color=#e03b24]{delta}[/color]",
            _ => "[color=#888888]+0[/color]"
        };
        label.Text = $"[font_size=9]{icon} {current} {deltaText}[/font_size]";
    }

    private void UpdateSessionTitle()
    {
        if (_sessionLabel == null || string.IsNullOrEmpty(_stageTitle)) return;

        // Keep stage title concise in top bar so it doesn't push resource metrics
        string cleanTitle = _stageTitle.Trim();
        int parenIdx = cleanTitle.IndexOf('(');
        if (parenIdx > 0)
        {
            cleanTitle = cleanTitle[..parenIdx].Trim();
        }

        _sessionLabel.Text = $"⚔ {cleanTitle.ToUpperInvariant()}";
        _sessionLabel.TooltipText = _stageTitle;
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
