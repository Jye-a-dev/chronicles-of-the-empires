using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Native Godot 4 C# controller for the Đông Sơn bronze-themed Main Menu.
/// Coordinates the Left-aligned Sidebar layout (~30% screen width), leaving 70% viewport
/// dedicated to pixel art scenery, with an adjacent campaign selector and full settings modal.
/// </summary>
public partial class MainMenu : Control
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _campaignScenePath = "res://scenes/world_map.tscn";

    [Export(PropertyHint.File, "*.tscn")]
    private string _loadingScreenPath = "res://scenes/loading_screen.tscn";

    [Export]
    private string _communityUrl = "https://discord.gg/chronicles-of-the-empires";

    private Control _menuPanel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;

    private PrimaryMenuColumn? _primaryColumn;
    private SecondaryActionBar? _secondaryActionBar;
    private CampaignModePanel? _campaignPanel;
    private SettingsModal? _settingsModal;

    private AudioStreamPlayer _sfxHover = null!;
    private AudioStreamPlayer _sfxClick = null!;

    private Button? _btnFallbackNewGame;
    private Button? _btnFallbackLoadGame;
    private Button? _btnFallbackSettings;
    private Button? _btnFallbackQuit;

    public override void _Ready()
    {
        // 1. Resolve panel and labels
        _menuPanel = GetNodeOrNull<Control>("%MenuPanel")
            ?? GetNodeOrNull<Control>("SidebarMargin/MenuHBox/MenuPanel")
            ?? GetNodeOrNull<Control>("SidebarMargin/MenuPanel")
            ?? GetNode<Control>("CenterContainer/MenuPanel");

        _titleLabel = GetNodeOrNull<Label>("%TitleLabel")
            ?? _menuPanel.GetNode<Label>("InnerPanel/VBoxContainer/TitleLabel");

        _subtitleLabel = GetNodeOrNull<Label>("%SubtitleLabel")
            ?? _menuPanel.GetNode<Label>("InnerPanel/VBoxContainer/SubtitleLabel");

        _sfxHover = GetNodeOrNull<AudioStreamPlayer>("%SfxHover")
            ?? GetNode<AudioStreamPlayer>("SfxHover");

        _sfxClick = GetNodeOrNull<AudioStreamPlayer>("%SfxClick")
            ?? GetNode<AudioStreamPlayer>("SfxClick");

        MenuAudioHelper.EnsureProceduralStreams(_sfxHover, _sfxClick);

        // 2. Resolve decoupled UI components
        _primaryColumn = GetNodeOrNull<PrimaryMenuColumn>("%PrimaryColumn")
            ?? _menuPanel.GetNodeOrNull<PrimaryMenuColumn>("InnerPanel/VBoxContainer/PrimaryColumn");

        _secondaryActionBar = GetNodeOrNull<SecondaryActionBar>("%SecondaryActionBar")
            ?? _menuPanel.GetNodeOrNull<SecondaryActionBar>("InnerPanel/VBoxContainer/SecondaryActionBar");

        _campaignPanel = GetNodeOrNull<CampaignModePanel>("%CampaignModePanel")
            ?? GetNodeOrNull<CampaignModePanel>("SidebarMargin/MenuHBox/CampaignModePanel");

        _settingsModal = GetNodeOrNull<SettingsModal>("%SettingsModal")
            ?? GetNodeOrNull<SettingsModal>("SettingsModal");

        if (_primaryColumn != null)
        {
            _primaryColumn.NewCampaignRequested += OnNewCampaignRequested;
            _primaryColumn.LoadGameRequested += OnLoadGameRequested;
            _primaryColumn.SettingsRequested += OnSettingsRequested;
            _primaryColumn.QuitRequested += OnQuitRequested;
            _primaryColumn.InitializeInteractions(_sfxHover, _sfxClick);
        }
        else
        {
            BindLegacyButtons();
        }

        if (_secondaryActionBar != null)
        {
            _secondaryActionBar.CivilopediaRequested += OnCivilopediaRequested;
            _secondaryActionBar.CreditsRequested += OnCreditsRequested;
            _secondaryActionBar.CommunityRequested += OnCommunityRequested;
            _secondaryActionBar.InitializeInteractions(_sfxHover, _sfxClick);
        }

        if (_campaignPanel != null)
        {
            _campaignPanel.CampaignStarted += OnCampaignModeSelected;
            _campaignPanel.InitializeInteractions(_sfxHover, _sfxClick);
        }

        if (_settingsModal != null)
        {
            _settingsModal.InitializeInteractions(_sfxHover, _sfxClick);
        }

        LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
        UpdateLocalizedStrings();

        PlaySidebarEntranceAnimation();
    }

    private void PlaySidebarEntranceAnimation()
    {
        _menuPanel.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Vector2 originalPos = _menuPanel.Position;
        _menuPanel.Position = new Vector2(originalPos.X - 30.0f, originalPos.Y);

        var tween = CreateTween().SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(_menuPanel, "modulate:a", 1.0f, 0.40f);
        tween.TweenProperty(_menuPanel, "position:x", originalPos.X, 0.40f);
    }

    private void OnNewCampaignRequested()
    {
        MenuAudioHelper.PlaySound(_sfxClick);
        if (_campaignPanel != null)
        {
            _campaignPanel.Toggle();
        }
        else
        {
            OnCampaignModeSelected("grand_campaign");
        }
    }

    private void OnCampaignModeSelected(string mode)
    {
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print($"[Chronicles] Starting campaign mode: '{mode}' via loading screen...");

        SetInputLocked(true);

        var tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.35f);
        tween.TweenCallback(Callable.From(() =>
        {
            LoadingScreen.TransitionTo(GetTree(), _campaignScenePath, _loadingScreenPath);
        }));
    }

    private void OnLoadGameRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening load game dialog...");
    }

    private void OnSettingsRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        if (_settingsModal != null)
        {
            _settingsModal.Open();
        }
        else
        {
            GD.Print("[Chronicles] SettingsModal node not found in scene tree.");
        }
    }

    private void OnQuitRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Exiting application to desktop...");
        GetTree().Quit();
    }

    private void OnCivilopediaRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening Civilopedia / Archives...");
    }

    private void OnCreditsRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening Credits...");
    }

    private void OnCommunityRequested()
    {
        _campaignPanel?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print($"[Chronicles] Opening Community Discord link: {_communityUrl}");
        OS.ShellOpen(_communityUrl);
    }

    private void UpdateLocalizedStrings()
    {
        if (_titleLabel != null) _titleLabel.Text = LocalizationManager.Get("MENU_TITLE");
        if (_subtitleLabel != null) _subtitleLabel.Text = LocalizationManager.Get("MENU_SUBTITLE");
    }

    private void SetInputLocked(bool locked)
    {
        _primaryColumn?.SetButtonsDisabled(locked);
        _secondaryActionBar?.SetButtonsDisabled(locked);

        if (_btnFallbackNewGame != null) _btnFallbackNewGame.Disabled = locked;
        if (_btnFallbackLoadGame != null) _btnFallbackLoadGame.Disabled = locked;
        if (_btnFallbackSettings != null) _btnFallbackSettings.Disabled = locked;
        if (_btnFallbackQuit != null) _btnFallbackQuit.Disabled = locked;
    }

    private void BindLegacyButtons()
    {
        _btnFallbackNewGame = GetNodeOrNull<Button>("%BtnNewGame")
            ?? _menuPanel.GetNodeOrNull<Button>("InnerPanel/VBoxContainer/BtnNewGame");
        _btnFallbackLoadGame = GetNodeOrNull<Button>("%BtnLoadGame")
            ?? _menuPanel.GetNodeOrNull<Button>("InnerPanel/VBoxContainer/BtnLoadGame");
        _btnFallbackSettings = GetNodeOrNull<Button>("%BtnSettings")
            ?? _menuPanel.GetNodeOrNull<Button>("InnerPanel/VBoxContainer/BtnSettings");
        _btnFallbackQuit = GetNodeOrNull<Button>("%BtnQuit")
            ?? _menuPanel.GetNodeOrNull<Button>("InnerPanel/VBoxContainer/BtnQuit");

        if (_btnFallbackNewGame != null)
        {
            _btnFallbackNewGame.Pressed += OnNewCampaignRequested;
            MenuButtonAnimator.Attach(_btnFallbackNewGame, _sfxHover, _sfxClick);
        }

        if (_btnFallbackLoadGame != null)
        {
            _btnFallbackLoadGame.Pressed += OnLoadGameRequested;
            MenuButtonAnimator.Attach(_btnFallbackLoadGame, _sfxHover, _sfxClick);
        }

        if (_btnFallbackSettings != null)
        {
            _btnFallbackSettings.Pressed += OnSettingsRequested;
            MenuButtonAnimator.Attach(_btnFallbackSettings, _sfxHover, _sfxClick);
        }

        if (_btnFallbackQuit != null)
        {
            _btnFallbackQuit.Pressed += OnQuitRequested;
            MenuButtonAnimator.Attach(_btnFallbackQuit, _sfxHover, _sfxClick);
        }
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
    }
}
