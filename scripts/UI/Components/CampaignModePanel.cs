using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public record CampaignModeInfo(
    string Id,
    string DisplayName,
    string EraTag,
    string Description,
    string MapSpec,
    string FactionsSpec,
    string VictorySpec
);

/// <summary>
/// Adjacent 640x360 pixel-fitted side panel detailing 4X campaign modes
/// with reactive multi-language support and real-time parameter binding.
/// </summary>
public partial class CampaignModePanel : PanelContainer
{
    [Signal]
    public delegate void CampaignStartedEventHandler(string modeId);

    [Signal]
    public delegate void PanelClosedEventHandler();

    private readonly List<CampaignModeInfo> _modes = new(3);
    private string _selectedModeId = "grand_campaign";
    private Tween? _panelTween;

    private Label _headerLabel = null!;
    private Button _btnClose = null!;
    private Button _btnDeploy = null!;
    private Button _btnModeGrand = null!;
    private Button _btnModeSkirmish = null!;
    private Button _btnModeTutorial = null!;

    private Label _titleLabel = null!;
    private Label _eraLabel = null!;
    private Label _descriptionLabel = null!;
    private Label _mapSpecLabel = null!;
    private Label _factionsSpecLabel = null!;
    private Label _victorySpecLabel = null!;

    public bool IsOpen => Visible && Modulate.A > 0.05f;

    public override void _Ready()
    {
        _headerLabel = GetNodeOrNull<Label>("%HeaderLabel") ?? GetNode<Label>("Inner/VBox/Header/HeaderLabel");
        _btnClose = GetNodeOrNull<Button>("%BtnCloseModePanel") ?? GetNode<Button>("Inner/VBox/Header/BtnCloseModePanel");
        _btnDeploy = GetNodeOrNull<Button>("%BtnDeployCampaign") ?? GetNode<Button>("Inner/VBox/Footer/BtnDeployCampaign");

        _btnModeGrand = GetNodeOrNull<Button>("%BtnSelectGrand") ?? GetNode<Button>("Inner/VBox/ModesHBox/BtnSelectGrand");
        _btnModeSkirmish = GetNodeOrNull<Button>("%BtnSelectSkirmish") ?? GetNode<Button>("Inner/VBox/ModesHBox/BtnSelectSkirmish");
        _btnModeTutorial = GetNodeOrNull<Button>("%BtnSelectTutorial") ?? GetNode<Button>("Inner/VBox/ModesHBox/BtnSelectTutorial");

        _titleLabel = GetNodeOrNull<Label>("%ModeTitleLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/ModeTitleLabel");
        _eraLabel = GetNodeOrNull<Label>("%ModeEraLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/ModeEraLabel");
        _descriptionLabel = GetNodeOrNull<Label>("%ModeDescLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/ModeDescLabel");
        _mapSpecLabel = GetNodeOrNull<Label>("%SpecMapLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/SpecsGrid/SpecMapLabel");
        _factionsSpecLabel = GetNodeOrNull<Label>("%SpecFactionsLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/SpecsGrid/SpecFactionsLabel");
        _victorySpecLabel = GetNodeOrNull<Label>("%SpecVictoryLabel") ?? GetNode<Label>("Inner/VBox/DetailsPanel/Margin/VBox/SpecsGrid/SpecVictoryLabel");

        _btnClose.Pressed += Close;
        _btnDeploy.Pressed += OnDeployPressed;

        _btnModeGrand.Pressed += () => SelectMode("grand_campaign");
        _btnModeSkirmish.Pressed += () => SelectMode("skirmish");
        _btnModeTutorial.Pressed += () => SelectMode("tutorial");

        LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
        UpdateLocalizedStrings();

        Visible = false;
        Modulate = new Color(1, 1, 1, 0);
    }

    public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
    {
        Button[] buttons = [_btnClose, _btnDeploy, _btnModeGrand, _btnModeSkirmish, _btnModeTutorial];
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                MenuButtonAnimator.Attach(btn, sfxHover, sfxClick);
            }
        }
    }

    public void SelectMode(string modeId)
    {
        var info = _modes.Find(m => m.Id == modeId) ?? (_modes.Count > 0 ? _modes[0] : null);
        if (info == null) return;
        _selectedModeId = info.Id;

        _titleLabel.Text = info.DisplayName;
        _eraLabel.Text = info.EraTag;
        _descriptionLabel.Text = info.Description;
        _mapSpecLabel.Text = $"{LocalizationManager.Get("SPEC_MAP_PREFIX")}{info.MapSpec}";
        _factionsSpecLabel.Text = $"{LocalizationManager.Get("SPEC_RIVALS_PREFIX")}{info.FactionsSpec}";
        _victorySpecLabel.Text = $"{LocalizationManager.Get("SPEC_VICTORY_PREFIX")}{info.VictorySpec}";

        UpdateTabStyles();
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        Visible = true;
        _panelTween?.Kill();
        _panelTween = CreateTween().SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        _panelTween.TweenProperty(this, "modulate:a", 1.0f, 0.22f);
        _panelTween.TweenProperty(this, "scale", Vector2.One, 0.22f);
    }

    public void Close()
    {
        _panelTween?.Kill();
        _panelTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        _panelTween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
        _panelTween.TweenCallback(Callable.From(() =>
        {
            Visible = false;
            EmitSignal(SignalName.PanelClosed);
        }));
    }

    private void OnDeployPressed()
    {
        EmitSignal(SignalName.CampaignStarted, _selectedModeId);
    }

    private void UpdateTabStyles()
    {
        HighlightTab(_btnModeGrand, _selectedModeId == "grand_campaign");
        HighlightTab(_btnModeSkirmish, _selectedModeId == "skirmish");
        HighlightTab(_btnModeTutorial, _selectedModeId == "tutorial");
    }

    private static void HighlightTab(Button btn, bool isSelected)
    {
        btn.Modulate = isSelected ? new Color(1.0f, 0.92f, 0.5f, 1.0f) : new Color(0.72f, 0.70f, 0.65f, 0.85f);
    }

    private void UpdateLocalizedStrings()
    {
        _modes.Clear();
        _modes.Add(new(
            Id: "grand_campaign",
            DisplayName: LocalizationManager.Get("MODE_GRAND_NAME"),
            EraTag: LocalizationManager.Get("MODE_GRAND_ERA"),
            Description: LocalizationManager.Get("MODE_GRAND_DESC"),
            MapSpec: LocalizationManager.Get("MODE_GRAND_MAP"),
            FactionsSpec: LocalizationManager.Get("MODE_GRAND_RIVALS"),
            VictorySpec: LocalizationManager.Get("MODE_GRAND_VICTORY")
        ));
        _modes.Add(new(
            Id: "skirmish",
            DisplayName: LocalizationManager.Get("MODE_SKIRMISH_NAME"),
            EraTag: LocalizationManager.Get("MODE_SKIRMISH_ERA"),
            Description: LocalizationManager.Get("MODE_SKIRMISH_DESC"),
            MapSpec: LocalizationManager.Get("MODE_SKIRMISH_MAP"),
            FactionsSpec: LocalizationManager.Get("MODE_SKIRMISH_RIVALS"),
            VictorySpec: LocalizationManager.Get("MODE_SKIRMISH_VICTORY")
        ));
        _modes.Add(new(
            Id: "tutorial",
            DisplayName: LocalizationManager.Get("MODE_TUTORIAL_NAME"),
            EraTag: LocalizationManager.Get("MODE_TUTORIAL_ERA"),
            Description: LocalizationManager.Get("MODE_TUTORIAL_DESC"),
            MapSpec: LocalizationManager.Get("MODE_TUTORIAL_MAP"),
            FactionsSpec: LocalizationManager.Get("MODE_TUTORIAL_RIVALS"),
            VictorySpec: LocalizationManager.Get("MODE_TUTORIAL_VICTORY")
        ));

        if (_headerLabel != null) _headerLabel.Text = LocalizationManager.Get("CAMPAIGN_HEADER");
        if (_btnModeGrand != null) _btnModeGrand.Text = LocalizationManager.Get("CAMPAIGN_BTN_GRAND");
        if (_btnModeSkirmish != null) _btnModeSkirmish.Text = LocalizationManager.Get("CAMPAIGN_BTN_SKIRMISH");
        if (_btnModeTutorial != null) _btnModeTutorial.Text = LocalizationManager.Get("CAMPAIGN_BTN_TUTORIAL");
        if (_btnDeploy != null) _btnDeploy.Text = LocalizationManager.Get("CAMPAIGN_DEPLOY");

        SelectMode(_selectedModeId);
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
    }
}
