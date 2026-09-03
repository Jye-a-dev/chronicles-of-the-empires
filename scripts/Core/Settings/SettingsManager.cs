using Godot;
using System;
using System.Globalization;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Data contract for user preferences.
/// </summary>
public class SettingsData
{
    public float MasterVolume = 0.80f;
    public float SfxVolume = 0.90f;
    public float MusicVolume = 0.75f;
    public int WindowMode = 0; // 0: Windowed, 1: Borderless, 2: Fullscreen, 3: Exclusive
    public int ResolutionWidth = 1280;
    public int ResolutionHeight = 720;
    public int Vsync = 1; // 0: Disabled, 1: Enabled, 2: Adaptive
    public int MaxFps = 60; // 0: Unlimited
    public float UiScale = 1.00f; // 0.80, 1.00, 1.25, 1.50, 2.00
    public string Language = "en";
}

/// <summary>
/// Persistent settings service that synchronizes configuration with a local plain text file (settings.txt).
/// </summary>
public static class SettingsManager
{
    public const string UserSettingsPath = "user://settings.txt";
    public const string LocalSettingsPath = "res://data/settings.txt";

    public static SettingsData Current { get; set; } = new();

    public static void EnsureAudioBuses()
    {
        int sfxIdx = AudioServer.GetBusIndex("SFX");
        if (sfxIdx < 0)
        {
            sfxIdx = AudioServer.BusCount;
            AudioServer.AddBus();
            AudioServer.SetBusName(sfxIdx, "SFX");
            AudioServer.SetBusSend(sfxIdx, "Master");
        }

        int musicIdx = AudioServer.GetBusIndex("Music");
        if (musicIdx < 0)
        {
            musicIdx = AudioServer.BusCount;
            AudioServer.AddBus();
            AudioServer.SetBusName(musicIdx, "Music");
            AudioServer.SetBusSend(musicIdx, "Master");
        }
    }

    public static SettingsData Load()
    {
        var data = new SettingsData();
        string path = UserSettingsPath;

        // In development or when local settings.txt is edited, prioritize whichever file is newer
        if (FileAccess.FileExists(LocalSettingsPath))
        {
            if (!FileAccess.FileExists(UserSettingsPath) ||
                FileAccess.GetModifiedTime(LocalSettingsPath) > FileAccess.GetModifiedTime(UserSettingsPath))
            {
                path = LocalSettingsPath;
            }
        }

        if (FileAccess.FileExists(path))
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                while (!file.EofReached())
                {
                    string line = file.GetLine().Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith('#') || !line.Contains('='))
                        continue;

                    var parts = line.Split('=', 2);
                    string key = parts[0].Trim().ToLowerInvariant();
                    string val = parts[1].Trim();

                    switch (key)
                    {
                        case "master_volume":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float mv)) data.MasterVolume = mv;
                            break;
                        case "sfx_volume":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float sv)) data.SfxVolume = sv;
                            break;
                        case "music_volume":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float msv)) data.MusicVolume = msv;
                            break;
                        case "window_mode":
                            if (int.TryParse(val, out int wm)) data.WindowMode = wm;
                            break;
                        case "resolution_w":
                        case "resolution_width":
                            if (int.TryParse(val, out int rw)) data.ResolutionWidth = rw;
                            break;
                        case "resolution_h":
                        case "resolution_height":
                            if (int.TryParse(val, out int rh)) data.ResolutionHeight = rh;
                            break;
                        case "vsync":
                            if (int.TryParse(val, out int vs)) data.Vsync = vs;
                            break;
                        case "max_fps":
                            if (int.TryParse(val, out int fps)) data.MaxFps = fps;
                            break;
                        case "ui_scale":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float uis)) data.UiScale = uis;
                            break;
                        case "language":
                            data.Language = val;
                            break;
                    }
                }
            }
        }

        Current = data;
        return data;
    }

    public static void Save(SettingsData data)
    {
        Current = data;
        string content =
            "# Chronicles of the Old Empires: Mythic Ages / Vạn Quốc Phong Vân Ký - User Settings Configuration\n" +
            $"master_volume={data.MasterVolume.ToString("F2", CultureInfo.InvariantCulture)}\n" +
            $"sfx_volume={data.SfxVolume.ToString("F2", CultureInfo.InvariantCulture)}\n" +
            $"music_volume={data.MusicVolume.ToString("F2", CultureInfo.InvariantCulture)}\n" +
            $"window_mode={data.WindowMode}\n" +
            $"resolution_w={data.ResolutionWidth}\n" +
            $"resolution_h={data.ResolutionHeight}\n" +
            $"vsync={data.Vsync}\n" +
            $"max_fps={data.MaxFps}\n" +
            $"ui_scale={data.UiScale.ToString("F2", CultureInfo.InvariantCulture)}\n" +
            $"language={data.Language}\n";

        // Save persistent copy to user://
        using (var userFile = FileAccess.Open(UserSettingsPath, FileAccess.ModeFlags.Write))
        {
            userFile?.StoreString(content);
        }

        // Also sync local data/settings.txt
        if (FileAccess.FileExists(LocalSettingsPath) || DirAccess.DirExistsAbsolute("res://data"))
        {
            using var resFile = FileAccess.Open(LocalSettingsPath, FileAccess.ModeFlags.Write);
            resFile?.StoreString(content);
        }
    }

    public static void Apply(SettingsData data, SceneTree? tree = null)
    {
        Current = data;
        EnsureAudioBuses();

        // 1. Audio
        SetBusVolume(0, data.MasterVolume);
        int sfxIdx = AudioServer.GetBusIndex("SFX");
        if (sfxIdx >= 0) SetBusVolume(sfxIdx, data.SfxVolume);
        int musicIdx = AudioServer.GetBusIndex("Music");
        if (musicIdx >= 0) SetBusVolume(musicIdx, data.MusicVolume);

        // 2. Window Mode
        var rootWindow = tree?.Root;
        switch (data.WindowMode)
        {
            case 0: // Windowed
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                if (rootWindow != null)
                {
                    rootWindow.Mode = Window.ModeEnum.Windowed;
                    rootWindow.Borderless = false;
                }
                break;
            case 1: // Borderless Window
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                if (rootWindow != null)
                {
                    rootWindow.Mode = Window.ModeEnum.Windowed;
                    rootWindow.Borderless = true;
                }
                break;
            case 2: // Fullscreen
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                if (rootWindow != null)
                {
                    rootWindow.Mode = Window.ModeEnum.Fullscreen;
                }
                break;
            case 3: // Exclusive Fullscreen
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                if (rootWindow != null)
                {
                    rootWindow.Mode = Window.ModeEnum.ExclusiveFullscreen;
                }
                break;
        }

        // 3. Resolution (when running Windowed or Borderless)
        if (data.ResolutionWidth > 0 && data.ResolutionHeight > 0 && data.WindowMode <= 1)
        {
            Vector2I res = new(data.ResolutionWidth, data.ResolutionHeight);
            DisplayServer.WindowSetSize(res);
            if (rootWindow != null)
            {
                rootWindow.Size = res;
            }

            int screen = DisplayServer.WindowGetCurrentScreen();
            Rect2I screenRect = DisplayServer.ScreenGetUsableRect(screen);
            Vector2I pos = screenRect.Position + (screenRect.Size - res) / 2;
            pos.X = Math.Max(screenRect.Position.X, pos.X);
            pos.Y = Math.Max(screenRect.Position.Y, pos.Y);
            DisplayServer.WindowSetPosition(pos);
            if (rootWindow != null)
            {
                rootWindow.Position = pos;
            }
        }

        // 4. VSync
        DisplayServer.WindowSetVsyncMode((DisplayServer.VSyncMode)data.Vsync);

        // 5. Max FPS
        Engine.MaxFps = data.MaxFps;

        // 6. UI Scale
        if (tree != null && data.UiScale > 0.1f)
        {
            tree.Root.ContentScaleFactor = data.UiScale;
        }

        // 7. Language
        if (!string.IsNullOrEmpty(data.Language))
        {
            LocalizationManager.SetLanguage(data.Language);
        }
    }

    public static void SetBusVolume(int busIndex, float linearVal)
    {
        if (busIndex < 0 || busIndex >= AudioServer.BusCount) return;
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
}
