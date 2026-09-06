using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Native Godot 4 C# controller for the mythic bronze-themed Main Menu.
/// Coordinates the Left-aligned Sidebar layout (~30% screen width), leaving 70% viewport
/// dedicated to pixel art scenery, with an adjacent campaign selector and full settings modal.
/// </summary>
public partial class MainMenu : Control
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _campaignScenePath = "res://scenes/gameplay/world_map.tscn";

    [Export(PropertyHint.File, "*.tscn")]
    private string _loadingScreenPath = "res://scenes/ui/screens/loading_screen.tscn";

    [Export]
    private string _communityUrl = "https://discord.gg/chronicles-of-the-empires";

    private Control _menuPanel = null!;
    private Label? _titleLabel;
    private Label? _subtitleLabel;
    private ChroniclesOfTheEmpires.UI.Components.MenuTitle? _menuTitle;

    private PrimaryMenuColumn? _primaryColumn;
    private SecondaryActionBar? _secondaryActionBar;
    private CampaignModal? _campaignModal;
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

        _menuTitle = GetNodeOrNull<ChroniclesOfTheEmpires.UI.Components.MenuTitle>("%MenuTitle")
            ?? _menuPanel.GetNodeOrNull<ChroniclesOfTheEmpires.UI.Components.MenuTitle>("InnerPanel/VBoxContainer/MenuTitle");

        _titleLabel = GetNodeOrNull<Label>("%TitleLabel")
            ?? _menuPanel.GetNodeOrNull<Label>("InnerPanel/VBoxContainer/MenuTitle/TitleLabel")
            ?? _menuPanel.GetNodeOrNull<Label>("InnerPanel/VBoxContainer/TitleLabel");

        _subtitleLabel = GetNodeOrNull<Label>("%SubtitleLabel")
            ?? _menuPanel.GetNodeOrNull<Label>("InnerPanel/VBoxContainer/MenuTitle/SubtitleLabel")
            ?? _menuPanel.GetNodeOrNull<Label>("InnerPanel/VBoxContainer/SubtitleLabel");

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

        _campaignModal = GetNodeOrNull<CampaignModal>("%CampaignModal")
            ?? GetNodeOrNull<CampaignModal>("CampaignModal");

        if (_campaignModal == null)
        {
            var campaignScene = GD.Load<PackedScene>("res://scenes/ui/modals/campaign_modal.tscn");
            if (campaignScene != null)
            {
                _campaignModal = campaignScene.Instantiate<CampaignModal>();
                _campaignModal.Name = "CampaignModal";
                AddChild(_campaignModal);
            }
        }

        _settingsModal = GetNodeOrNull<SettingsModal>("%SettingsModal")
            ?? GetNodeOrNull<SettingsModal>("SettingsModal");

        if (_settingsModal == null)
        {
            var modalScene = GD.Load<PackedScene>("res://scenes/ui/modals/settings_modal.tscn");
            if (modalScene != null)
            {
                _settingsModal = modalScene.Instantiate<SettingsModal>();
                _settingsModal.Name = "SettingsModal";
                AddChild(_settingsModal);
            }
        }

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

        if (_campaignModal != null)
        {
            _campaignModal.CampaignStarted += OnCampaignModalStarted;
            _campaignModal.InitializeInteractions(_sfxHover, _sfxClick);
        }

        if (_settingsModal != null)
        {
            _settingsModal.InitializeInteractions(_sfxHover, _sfxClick);
        }

        // Synchronize and apply persistent configuration from local settings.txt
        SettingsManager.Apply(SettingsManager.Load(), GetTree());

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
        _settingsModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        if (_campaignModal == null)
        {
            var campaignScene = GD.Load<PackedScene>("res://scenes/ui/modals/campaign_modal.tscn");
            if (campaignScene != null)
            {
                _campaignModal = campaignScene.Instantiate<CampaignModal>();
                _campaignModal.Name = "CampaignModal";
                AddChild(_campaignModal);
                _campaignModal.CampaignStarted += OnCampaignModalStarted;
                _campaignModal.InitializeInteractions(_sfxHover, _sfxClick);
            }
        }

        if (_campaignModal != null)
        {
            _campaignModal.Open();
        }
        else
        {
            OnCampaignModeSelected("grand_campaign");
        }
    }

    private void OnCampaignModalStarted(string mode, string stageId, string mapSize, string biome, int rivals, string victory, string difficulty)
    {
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print($"[Chronicles] Starting game session: mode={mode}, stage={stageId}, map={mapSize}, rivals={rivals}...");

        SetInputLocked(true);

        var tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.35f);
        tween.TweenCallback(Callable.From(() =>
        {
            LoadingScreen.TransitionTo(GetTree(), _campaignScenePath, _loadingScreenPath);
        }));
    }

    private void OnCampaignModeSelected(string mode)
    {
        OnCampaignModalStarted(mode, "stage_1", "32x32", "red_river", 2, "conquest", "normal");
    }

    private void OnLoadGameRequested()
    {
        _campaignModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening load game dialog...");
    }

    private void OnSettingsRequested()
    {
        _campaignModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        if (_settingsModal == null)
        {
            var modalScene = GD.Load<PackedScene>("res://scenes/ui/modals/settings_modal.tscn");
            if (modalScene != null)
            {
                _settingsModal = modalScene.Instantiate<SettingsModal>();
                _settingsModal.Name = "SettingsModal";
                AddChild(_settingsModal);
                _settingsModal.InitializeInteractions(_sfxHover, _sfxClick);
            }
        }

        if (_settingsModal != null)
        {
            _settingsModal.Open();
        }
        else
        {
            GD.PrintErr("[Chronicles] SettingsModal node not found in scene tree.");
        }
    }

    private void OnQuitRequested()
    {
        _campaignModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Exiting application to desktop...");
        GetTree().Quit();
    }

    private void OnCivilopediaRequested()
    {
        _campaignModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening Civilopedia / Archives...");
    }

    private void OnCreditsRequested()
    {
        _campaignModal?.Close();
        MenuAudioHelper.PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening Credits...");
    }

    private void OnCommunityRequested()
    {
        _campaignModal?.Close();
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
