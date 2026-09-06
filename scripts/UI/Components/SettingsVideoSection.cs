using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public partial class SettingsModal
{
	private static readonly (int W, int H, string Label)[] ResolutionPresets =
	[
		(640, 360, "640 x 360 (Native 1x)"),
		(960, 540, "960 x 540 (1.5x)"),
		(1280, 720, "1280 x 720 (HD 2x)"),
		(1600, 900, "1600 x 900 (2.5x)"),
		(1920, 1080, "1920 x 1080 (FHD 3x)"),
		(2560, 1440, "2560 x 1440 (2K 4x)")
	];

	private static readonly int[] MaxFpsPresets = [30, 60, 120, 144, 240, 0];
	private static readonly (float Scale, string Label)[] UiScalePresets =
	[
		(0.80f, "80% (Compact)"),
		(1.00f, "100% (Standard)"),
		(1.25f, "125% (Large)"),
		(1.50f, "150% (Extra Large)"),
		(2.00f, "200% (Huge)")
	];

	private void OnWindowModeSelected(long index)
	{
		if (_isPopulating) return;
		_stagedSettings.WindowMode = (int)index;
		SettingsManager.Apply(_stagedSettings, GetTree());
	}

	private void OnResolutionSelected(long index)
	{
		if (_isPopulating) return;
		if (index >= 0 && index < ResolutionPresets.Length)
		{
			_stagedSettings.ResolutionWidth = ResolutionPresets[index].W;
			_stagedSettings.ResolutionHeight = ResolutionPresets[index].H;
			SettingsManager.Apply(_stagedSettings, GetTree());
		}
	}

	private void OnVsyncSelected(long index)
	{
		if (_isPopulating) return;
		_stagedSettings.Vsync = (int)index;
		SettingsManager.Apply(_stagedSettings, GetTree());
	}

	private void OnMaxFpsSelected(long index)
	{
		if (_isPopulating) return;
		if (index >= 0 && index < MaxFpsPresets.Length)
		{
			_stagedSettings.MaxFps = MaxFpsPresets[index];
			SettingsManager.Apply(_stagedSettings, GetTree());
		}
	}

	private void OnUiScaleSelected(long index)
	{
		if (_isPopulating) return;
		if (index >= 0 && index < UiScalePresets.Length)
		{
			_stagedSettings.UiScale = UiScalePresets[index].Scale;
			SettingsManager.Apply(_stagedSettings, GetTree());
		}
	}

	private void OnLanguageSelected(long index)
	{
		if (_isPopulating) return;
		_stagedSettings.Language = (index == 1) ? LocalizationManager.LangVietnamese : LocalizationManager.LangEnglish;
		LocalizationManager.SetLanguage(_stagedSettings.Language);
	}

	private void PopulateDropdownItems()
	{
		bool prevPopulating = _isPopulating;
		_isPopulating = true;

		// 1. Window Mode
		int prevMode = _windowModeOptionBtn.Selected;
		_windowModeOptionBtn.Clear();
		_windowModeOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_MODE_WINDOWED"), 0);
		_windowModeOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_MODE_BORDERLESS"), 1);
		_windowModeOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_MODE_FULLSCREEN"), 2);
		_windowModeOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_MODE_EXCLUSIVE"), 3);
		_windowModeOptionBtn.Selected = prevMode >= 0 ? prevMode : _stagedSettings.WindowMode;

		// 2. Resolutions
		int prevRes = _resolutionOptionBtn.Selected;
		_resolutionOptionBtn.Clear();
		for (int i = 0; i < ResolutionPresets.Length; i++)
		{
			_resolutionOptionBtn.AddItem(ResolutionPresets[i].Label, i);
		}
		_resolutionOptionBtn.Selected = prevRes >= 0 ? prevRes : 2;

		// 3. VSync
		int prevVsync = _vsyncOptionBtn.Selected;
		_vsyncOptionBtn.Clear();
		_vsyncOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_VSYNC_DISABLED"), 0);
		_vsyncOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_VSYNC_ENABLED"), 1);
		_vsyncOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_VSYNC_ADAPTIVE"), 2);
		_vsyncOptionBtn.Selected = prevVsync >= 0 ? prevVsync : _stagedSettings.Vsync;

		// 4. Max FPS
		int prevFps = _maxFpsOptionBtn.Selected;
		_maxFpsOptionBtn.Clear();
		_maxFpsOptionBtn.AddItem("30 FPS", 0);
		_maxFpsOptionBtn.AddItem("60 FPS", 1);
		_maxFpsOptionBtn.AddItem("120 FPS", 2);
		_maxFpsOptionBtn.AddItem("144 FPS", 3);
		_maxFpsOptionBtn.AddItem("240 FPS", 4);
		_maxFpsOptionBtn.AddItem(LocalizationManager.Get("SETTINGS_FPS_UNLIMITED"), 5);
		_maxFpsOptionBtn.Selected = prevFps >= 0 ? prevFps : 1;

		// 5. UI Scale
		int prevScale = _uiScaleOptionBtn.Selected;
		_uiScaleOptionBtn.Clear();
		for (int i = 0; i < UiScalePresets.Length; i++)
		{
			_uiScaleOptionBtn.AddItem(UiScalePresets[i].Label, i);
		}
		_uiScaleOptionBtn.Selected = prevScale >= 0 ? prevScale : 1;

		// 6. Language
		int prevLang = _langOptionBtn.Selected;
		_langOptionBtn.Clear();
		_langOptionBtn.AddItem("English", 0);
		_langOptionBtn.AddItem("Tiếng Việt", 1);
		_langOptionBtn.Selected = prevLang >= 0 ? prevLang : (_stagedSettings.Language == LocalizationManager.LangVietnamese ? 1 : 0);

		_isPopulating = prevPopulating;
	}

	private static void StylePopup(OptionButton btn)
	{
		var popup = btn.GetPopup();
		popup.AddThemeFontSizeOverride("font_size", 7);
		popup.AddThemeFontSizeOverride("font_separator_size", 7);
		popup.AddThemeColorOverride("font_color", new Color(0.88f, 0.84f, 0.76f, 1f));
		popup.AddThemeColorOverride("font_hover_color", new Color(0.96f, 0.78f, 0.26f, 1f));
		popup.AddThemeColorOverride("font_separator_color", new Color(0.70f, 0.50f, 0.15f, 1f));
	}
}

