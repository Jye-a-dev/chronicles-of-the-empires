using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Modal settings panel for audio volume, display mode, and real-time English/Vietnamese language switching.
/// Designed for 640x360 canvas resolution with Đông Sơn bronze styling.
/// </summary>
public partial class SettingsModal : Control
{
	[Signal]
	public delegate void ClosedEventHandler();

	private PanelContainer _modalPanel = null!;
	private Button _btnClose = null!;
	private Label _titleLabel = null!;

	// Tabs
	private Button _tabAudioBtn = null!;
	private Button _tabVideoBtn = null!;
	private Button _tabLangBtn = null!;
	private Control _audioTab = null!;
	private Control _videoTab = null!;
	private Control _langTab = null!;

	// Audio controls
	private Label _masterLabel = null!;
	private HSlider _masterSlider = null!;
	private Label _sfxLabel = null!;
	private HSlider _sfxSlider = null!;
	private Label _musicLabel = null!;
	private HSlider _musicSlider = null!;

	// Video controls
	private CheckBox _fullscreenCheck = null!;
	private CheckBox _vsyncCheck = null!;

	// Language controls
	private Label _langLabel = null!;
	private OptionButton _langOptionBtn = null!;

	private Tween? _tween;

	public bool IsOpen => Visible && Modulate.A > 0.05f;

	public override void _Ready()
	{
		_modalPanel = GetNode<PanelContainer>("%SettingsPanel");
		_btnClose = GetNode<Button>("%BtnCloseSettings");
		_titleLabel = GetNode<Label>("%SettingsTitleLabel");

		_tabAudioBtn = GetNode<Button>("%TabAudioBtn");
		_tabVideoBtn = GetNode<Button>("%TabVideoBtn");
		_tabLangBtn = GetNode<Button>("%TabLangBtn");

		_audioTab = GetNode<Control>("%AudioTabContent");
		_videoTab = GetNode<Control>("%VideoTabContent");
		_langTab = GetNode<Control>("%LangTabContent");

		_masterLabel = GetNode<Label>("%MasterVolLabel");
		_masterSlider = GetNode<HSlider>("%MasterVolSlider");
		_sfxLabel = GetNode<Label>("%SfxVolLabel");
		_sfxSlider = GetNode<HSlider>("%SfxVolSlider");
		_musicLabel = GetNode<Label>("%MusicVolLabel");
		_musicSlider = GetNode<HSlider>("%MusicVolSlider");

		_fullscreenCheck = GetNode<CheckBox>("%FullscreenCheck");
		_vsyncCheck = GetNode<CheckBox>("%VsyncCheck");

		_langLabel = GetNode<Label>("%LanguageOptionLabel");
		_langOptionBtn = GetNode<OptionButton>("%LanguageOptionBtn");

		// Populate Language Options
		_langOptionBtn.Clear();
		_langOptionBtn.AddItem("English", 0);
		_langOptionBtn.AddItem("Tiếng Việt", 1);
		_langOptionBtn.Selected = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? 1 : 0;

		// Bind events
		_btnClose.Pressed += Close;
		_tabAudioBtn.Pressed += () => SwitchTab(0);
		_tabVideoBtn.Pressed += () => SwitchTab(1);
		_tabLangBtn.Pressed += () => SwitchTab(2);

		_masterSlider.ValueChanged += OnMasterVolumeChanged;
		_sfxSlider.ValueChanged += OnSfxVolumeChanged;
		_musicSlider.ValueChanged += OnMusicVolumeChanged;

		_fullscreenCheck.Toggled += OnFullscreenToggled;
		_vsyncCheck.Toggled += OnVsyncToggled;
		_langOptionBtn.ItemSelected += OnLanguageSelected;

		LocalizationManager.LanguageChanged += UpdateLocalizedStrings;
		UpdateLocalizedStrings();
		SwitchTab(0);

		Visible = false;
		Modulate = new Color(1, 1, 1, 0);
	}

	public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
	{
		Button[] buttons = [_btnClose, _tabAudioBtn, _tabVideoBtn, _tabLangBtn];
		foreach (var btn in buttons)
		{
			MenuButtonAnimator.Attach(btn, sfxHover, sfxClick);
		}
	}

	public void Open()
	{
		Visible = true;
		_fullscreenCheck.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
		_vsyncCheck.ButtonPressed = DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;

		_tween?.Kill();
		_tween = CreateTween().SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(this, "modulate:a", 1.0f, 0.20f);
		_tween.TweenProperty(_modalPanel, "scale", Vector2.One, 0.20f);
	}

	public void Close()
	{
		_tween?.Kill();
		_tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		_tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
		_tween.TweenCallback(Callable.From(() =>
		{
			Visible = false;
			EmitSignal(SignalName.Closed);
		}));
	}

	private void SwitchTab(int tabIndex)
	{
		_audioTab.Visible = tabIndex == 0;
		_videoTab.Visible = tabIndex == 1;
		_langTab.Visible = tabIndex == 2;

		HighlightTabButton(_tabAudioBtn, tabIndex == 0);
		HighlightTabButton(_tabVideoBtn, tabIndex == 1);
		HighlightTabButton(_tabLangBtn, tabIndex == 2);
	}

	private static void HighlightTabButton(Button btn, bool active)
	{
		btn.Modulate = active ? new Color(1.0f, 0.92f, 0.5f, 1.0f) : new Color(0.7f, 0.68f, 0.65f, 0.8f);
	}

	private void OnMasterVolumeChanged(double value)
	{
		SetBusVolume(0, (float)value);
	}

	private void OnSfxVolumeChanged(double value)
	{
		int busIdx = AudioServer.GetBusIndex("SFX");
		if (busIdx >= 0) SetBusVolume(busIdx, (float)value);
	}

	private void OnMusicVolumeChanged(double value)
	{
		int busIdx = AudioServer.GetBusIndex("Music");
		if (busIdx >= 0) SetBusVolume(busIdx, (float)value);
	}

	private static void SetBusVolume(int busIndex, float linearVal)
	{
		linearVal = Mathf.Clamp(linearVal, 0.0f, 1.0f);
		if (linearVal <= 0.001f)
		{
			AudioServer.SetBusMute(busIndex, true);
		}
		else
		{
			AudioServer.SetBusMute(busIndex, false);
			AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linearVal));
		}
	}

	private static void OnFullscreenToggled(bool isFullscreen)
	{
		DisplayServer.WindowSetMode(isFullscreen
			? DisplayServer.WindowMode.Fullscreen
			: DisplayServer.WindowMode.Windowed);
	}

	private static void OnVsyncToggled(bool isVsync)
	{
		DisplayServer.WindowSetVsyncMode(isVsync
			? DisplayServer.VSyncMode.Enabled
			: DisplayServer.VSyncMode.Disabled);
	}

	private void OnLanguageSelected(long index)
	{
		string newLang = index == 1 ? LocalizationManager.LangVietnamese : LocalizationManager.LangEnglish;
		LocalizationManager.SetLanguage(newLang);
	}

	private void UpdateLocalizedStrings()
	{
		_titleLabel.Text = LocalizationManager.Get("SETTINGS_TITLE");
		_tabAudioBtn.Text = LocalizationManager.Get("SETTINGS_TAB_AUDIO");
		_tabVideoBtn.Text = LocalizationManager.Get("SETTINGS_TAB_VIDEO");
		_tabLangBtn.Text = LocalizationManager.Get("SETTINGS_TAB_GAMEPLAY");

		_masterLabel.Text = LocalizationManager.Get("SETTINGS_MASTER_VOL");
		_sfxLabel.Text = LocalizationManager.Get("SETTINGS_SFX_VOL");
		_musicLabel.Text = LocalizationManager.Get("SETTINGS_MUSIC_VOL");

		_fullscreenCheck.Text = LocalizationManager.Get("SETTINGS_FULLSCREEN");
		_vsyncCheck.Text = LocalizationManager.Get("SETTINGS_VSYNC");

		_langLabel.Text = LocalizationManager.Get("SETTINGS_LANGUAGE");
	}

	public override void _ExitTree()
	{
		LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
	}
}
