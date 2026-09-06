using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public partial class SettingsModal
{
	private void OnMasterVolumeChanged(double val)
	{
		_stagedSettings.MasterVolume = (float)val;
		_masterVolValLabel.Text = $"{Mathf.RoundToInt(_stagedSettings.MasterVolume * 100)}%";
		SettingsManager.SetBusVolume(0, _stagedSettings.MasterVolume);
	}

	private void OnSfxVolumeChanged(double val)
	{
		_stagedSettings.SfxVolume = (float)val;
		_sfxVolValLabel.Text = $"{Mathf.RoundToInt(_stagedSettings.SfxVolume * 100)}%";
		int bus = AudioServer.GetBusIndex("SFX");
		if (bus >= 0) SettingsManager.SetBusVolume(bus, _stagedSettings.SfxVolume);
	}

	private void OnMusicVolumeChanged(double val)
	{
		_stagedSettings.MusicVolume = (float)val;
		_musicVolValLabel.Text = $"{Mathf.RoundToInt(_stagedSettings.MusicVolume * 100)}%";
		int bus = AudioServer.GetBusIndex("Music");
		if (bus >= 0) SettingsManager.SetBusVolume(bus, _stagedSettings.MusicVolume);
	}
}

