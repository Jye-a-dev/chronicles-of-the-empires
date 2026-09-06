using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Controls the system settings modal dialog.
/// Supports Audio sliders, Display settings (Window mode, Resolution, VSync, Max FPS, UI Scale),
/// Language switcher, and persistent local storage synchronization via plain text file.
/// </summary>
public partial class SettingsModal : Control
{
	[Signal]
	public delegate void ClosedEventHandler();

	private PanelContainer _modalPanel = null!;
	private Label _titleLabel = null!;
	private Button _btnCloseSettings = null!;

	// Sidebar
	private Button _tabAudioBtn = null!;
	private Button _tabVideoBtn = null!;
	private Button _tabLangBtn = null!;

	// Content Tabs
	private Control _audioTab = null!;
	private Control _videoTab = null!;
	private Control _langTab = null!;

	// Audio Controls
	private Label _masterVolLabel = null!;
	private HSlider _masterSlider = null!;
	private Label _masterVolValLabel = null!;

	private Label _sfxVolLabel = null!;
	private HSlider _sfxSlider = null!;
	private Label _sfxVolValLabel = null!;

	private Label _musicVolLabel = null!;
	private HSlider _musicSlider = null!;
	private Label _musicVolValLabel = null!;

	// Display Controls
	private Label _windowModeLabel = null!;
	private OptionButton _windowModeOptionBtn = null!;

	private Label _resolutionLabel = null!;
	private OptionButton _resolutionOptionBtn = null!;

	private Label _vsyncLabel = null!;
	private OptionButton _vsyncOptionBtn = null!;

	private Label _maxFpsLabel = null!;
	private OptionButton _maxFpsOptionBtn = null!;

	private Label _uiScaleLabel = null!;
	private OptionButton _uiScaleOptionBtn = null!;

	// Language Controls
	private Label _languageOptionLabel = null!;
	private OptionButton _langOptionBtn = null!;

	// Footer
	private Label _statusFeedbackLabel = null!;
	private Button _btnSaveSettings = null!;
	private Button _btnCloseModal = null!;

	private Tween? _tween;
	private Tween? _feedbackTween;
	private int _currentTabIndex = 0;
	private SettingsData _stagedSettings = new();
	private bool _isPopulating;

	public bool IsOpen => Visible && Modulate.A > 0.05f;

	public override void _Ready()
	{
		_modalPanel = GetNode<PanelContainer>("%SettingsPanel");
		_titleLabel = GetNode<Label>("%SettingsTitleLabel");
		_btnCloseSettings = GetNode<Button>("%BtnCloseSettings");

		_tabAudioBtn = GetNode<Button>("%TabAudioBtn");
		_tabVideoBtn = GetNode<Button>("%TabVideoBtn");
		_tabLangBtn = GetNode<Button>("%TabLangBtn");

		_audioTab = GetNode<Control>("%AudioTabContent");
		_videoTab = GetNode<Control>("%VideoTabContent");
		_langTab = GetNode<Control>("%LangTabContent");

		_masterVolLabel = GetNode<Label>("%MasterVolLabel");
		_masterSlider = GetNode<HSlider>("%MasterVolSlider");
		_masterVolValLabel = GetNode<Label>("%MasterVolValLabel");

		_sfxVolLabel = GetNode<Label>("%SfxVolLabel");
		_sfxSlider = GetNode<HSlider>("%SfxVolSlider");
		_sfxVolValLabel = GetNode<Label>("%SfxVolValLabel");

		_musicVolLabel = GetNode<Label>("%MusicVolLabel");
		_musicSlider = GetNode<HSlider>("%MusicVolSlider");
		_musicVolValLabel = GetNode<Label>("%MusicVolValLabel");

		_windowModeLabel = GetNode<Label>("%WindowModeLabel");
		_windowModeOptionBtn = GetNode<OptionButton>("%WindowModeOptionBtn");

		_resolutionLabel = GetNode<Label>("%ResolutionLabel");
		_resolutionOptionBtn = GetNode<OptionButton>("%ResolutionOptionBtn");

		_vsyncLabel = GetNode<Label>("%VsyncLabel");
		_vsyncOptionBtn = GetNode<OptionButton>("%VsyncOptionBtn");

		_maxFpsLabel = GetNode<Label>("%MaxFpsLabel");
		_maxFpsOptionBtn = GetNode<OptionButton>("%MaxFpsOptionBtn");

		_uiScaleLabel = GetNode<Label>("%UiScaleLabel");
		_uiScaleOptionBtn = GetNode<OptionButton>("%UiScaleOptionBtn");

		_languageOptionLabel = GetNode<Label>("%LanguageOptionLabel");
		_langOptionBtn = GetNode<OptionButton>("%LanguageOptionBtn");

		_statusFeedbackLabel = GetNode<Label>("%StatusFeedbackLabel");
		_btnSaveSettings = GetNode<Button>("%BtnSaveSettings");
		_btnCloseModal = GetNode<Button>("%BtnCloseModal");

		// Style OptionButton popups for compact display
		StylePopup(_windowModeOptionBtn);
		StylePopup(_resolutionOptionBtn);
		StylePopup(_vsyncOptionBtn);
		StylePopup(_maxFpsOptionBtn);
		StylePopup(_uiScaleOptionBtn);
		StylePopup(_langOptionBtn);

		// Event bindings
		_btnCloseSettings.Pressed += Close;
		_btnCloseModal.Pressed += Close;
		_btnSaveSettings.Pressed += OnSaveClicked;

		_tabAudioBtn.Pressed += () => SwitchTab(0);
		_tabVideoBtn.Pressed += () => SwitchTab(1);
		_tabLangBtn.Pressed += () => SwitchTab(2);

		_masterSlider.ValueChanged += OnMasterVolumeChanged;
		_sfxSlider.ValueChanged += OnSfxVolumeChanged;
		_musicSlider.ValueChanged += OnMusicVolumeChanged;

		_windowModeOptionBtn.ItemSelected += OnWindowModeSelected;
		_resolutionOptionBtn.ItemSelected += OnResolutionSelected;
		_vsyncOptionBtn.ItemSelected += OnVsyncSelected;
		_maxFpsOptionBtn.ItemSelected += OnMaxFpsSelected;
		_uiScaleOptionBtn.ItemSelected += OnUiScaleSelected;
		_langOptionBtn.ItemSelected += OnLanguageSelected;

		LocalizationManager.LanguageChanged += UpdateLocalizedStrings;

		// Center pivot dynamically
		_modalPanel.Resized += () => _modalPanel.PivotOffset = _modalPanel.Size / 2.0f;
		_modalPanel.PivotOffset = _modalPanel.CustomMinimumSize / 2.0f;

		Visible = false;
		Modulate = new Color(1, 1, 1, 0);

		// Initial load & application of saved settings on startup
		_stagedSettings = SettingsManager.Load();
		SettingsManager.Apply(_stagedSettings, GetTree());

		UpdateLocalizedStrings();
		SwitchTab(0);
	}

	public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
	{
		Button[] buttons =
		[
			_btnCloseSettings,
			_btnCloseModal,
			_btnSaveSettings,
			_tabAudioBtn,
			_tabVideoBtn,
			_tabLangBtn,
			_windowModeOptionBtn,
			_resolutionOptionBtn,
			_vsyncOptionBtn,
			_maxFpsOptionBtn,
			_uiScaleOptionBtn,
			_langOptionBtn
		];

		foreach (var btn in buttons)
		{
			if (btn != null)
			{
				MenuButtonAnimator.Attach(btn, sfxHover, sfxClick);
				btn.MouseDefaultCursorShape = CursorShape.PointingHand;
			}
		}
	}

	public void Open()
	{
		Visible = true;
		Modulate = new Color(1, 1, 1, 0.0f);
		_modalPanel.PivotOffset = _modalPanel.Size.X > 0 ? _modalPanel.Size / 2.0f : _modalPanel.CustomMinimumSize / 2.0f;
		_modalPanel.Scale = new Vector2(0.95f, 0.95f);
		_statusFeedbackLabel.Text = string.Empty;

		// Load latest persistent settings
		_stagedSettings = SettingsManager.Load();
		PopulateUiFromSettings(_stagedSettings);

		_tween?.Kill();
		_tween = CreateTween().SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(this, "modulate:a", 1.0f, 0.18f);
		_tween.TweenProperty(_modalPanel, "scale", Vector2.One, 0.18f);
	}

	public void Close()
	{
		_tween?.Kill();
		_tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		_tween.TweenProperty(this, "modulate:a", 0.0f, 0.14f);
		_tween.TweenCallback(Callable.From(() =>
		{
			Visible = false;
			EmitSignal(SignalName.Closed);
		}));
	}

	private void OnSaveClicked()
	{
		SettingsManager.Save(_stagedSettings);
		SettingsManager.Apply(_stagedSettings, GetTree());

		_statusFeedbackLabel.Text = LocalizationManager.Get("SETTINGS_STATUS_SAVED");
		_statusFeedbackLabel.Modulate = Colors.White;

		_feedbackTween?.Kill();
		_feedbackTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		_feedbackTween.TweenInterval(2.0);
		_feedbackTween.TweenProperty(_statusFeedbackLabel, "modulate:a", 0.0f, 0.8f);
	}

	private void SwitchTab(int tabIndex)
	{
		_currentTabIndex = tabIndex;

		_audioTab.Visible = (tabIndex == 0);
		_videoTab.Visible = (tabIndex == 1);
		_langTab.Visible = (tabIndex == 2);

		HighlightTabButton(_tabAudioBtn, tabIndex == 0);
		HighlightTabButton(_tabVideoBtn, tabIndex == 1);
		HighlightTabButton(_tabLangBtn, tabIndex == 2);
	}

	private static void HighlightTabButton(Button btn, bool active)
	{
		if (active)
		{
			btn.AddThemeColorOverride("font_color", new Color(0.96f, 0.78f, 0.26f, 1f));
			btn.Modulate = Colors.White;
		}
		else
		{
			btn.AddThemeColorOverride("font_color", new Color(0.87f, 0.83f, 0.75f, 0.85f));
			btn.Modulate = new Color(0.85f, 0.85f, 0.85f, 1f);
		}
	}

	private void PopulateUiFromSettings(SettingsData data)
	{
		_isPopulating = true;

		// Audio
		_masterSlider.Value = data.MasterVolume;
		_masterVolValLabel.Text = $"{Mathf.RoundToInt(data.MasterVolume * 100)}%";

		_sfxSlider.Value = data.SfxVolume;
		_sfxVolValLabel.Text = $"{Mathf.RoundToInt(data.SfxVolume * 100)}%";

		_musicSlider.Value = data.MusicVolume;
		_musicVolValLabel.Text = $"{Mathf.RoundToInt(data.MusicVolume * 100)}%";

		// Window mode
		_windowModeOptionBtn.Selected = Mathf.Clamp(data.WindowMode, 0, _windowModeOptionBtn.ItemCount - 1);

		// Resolution
		int resIndex = 2; // Default 1280x720
		for (int i = 0; i < ResolutionPresets.Length; i++)
		{
			if (ResolutionPresets[i].W == data.ResolutionWidth && ResolutionPresets[i].H == data.ResolutionHeight)
			{
				resIndex = i;
				break;
			}
		}
		_resolutionOptionBtn.Selected = resIndex;

		// VSync
		_vsyncOptionBtn.Selected = Mathf.Clamp(data.Vsync, 0, _vsyncOptionBtn.ItemCount - 1);

		// Max FPS
		int fpsIndex = 1; // Default 60
		for (int i = 0; i < MaxFpsPresets.Length; i++)
		{
			if (MaxFpsPresets[i] == data.MaxFps)
			{
				fpsIndex = i;
				break;
			}
		}
		_maxFpsOptionBtn.Selected = fpsIndex;

		// UI Scale
		int scaleIndex = 1; // Default 1.0x
		for (int i = 0; i < UiScalePresets.Length; i++)
		{
			if (Mathf.IsEqualApprox(UiScalePresets[i].Scale, data.UiScale))
			{
				scaleIndex = i;
				break;
			}
		}
		_uiScaleOptionBtn.Selected = scaleIndex;

		// Language
		_langOptionBtn.Selected = (data.Language == LocalizationManager.LangVietnamese) ? 1 : 0;

		_isPopulating = false;
	}

	private void UpdateLocalizedStrings()
	{
		_titleLabel.Text = LocalizationManager.Get("SETTINGS_TITLE");
		_tabAudioBtn.Text = LocalizationManager.Get("SETTINGS_TAB_AUDIO");
		_tabVideoBtn.Text = LocalizationManager.Get("SETTINGS_TAB_VIDEO");
		_tabLangBtn.Text = LocalizationManager.Get("SETTINGS_TAB_GAMEPLAY");

		_masterVolLabel.Text = LocalizationManager.Get("SETTINGS_MASTER_VOL");
		_sfxVolLabel.Text = LocalizationManager.Get("SETTINGS_SFX_VOL");
		_musicVolLabel.Text = LocalizationManager.Get("SETTINGS_MUSIC_VOL");

		_windowModeLabel.Text = LocalizationManager.Get("SETTINGS_WINDOW_MODE");
		_resolutionLabel.Text = LocalizationManager.Get("SETTINGS_RESOLUTION");
		_vsyncLabel.Text = LocalizationManager.Get("SETTINGS_VSYNC_LABEL");
		_maxFpsLabel.Text = LocalizationManager.Get("SETTINGS_MAX_FPS");
		_uiScaleLabel.Text = LocalizationManager.Get("SETTINGS_UI_SCALE");
		_languageOptionLabel.Text = LocalizationManager.Get("SETTINGS_LANGUAGE");

		_btnSaveSettings.Text = LocalizationManager.Get("SETTINGS_BTN_SAVE");
		_btnCloseModal.Text = LocalizationManager.Get("SETTINGS_BTN_CLOSE");

		PopulateDropdownItems();
	}

	public override void _ExitTree()
	{
		LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
	}
}
