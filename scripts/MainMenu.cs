using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Native Godot 4 C# controller for the Đông Sơn bronze-themed Main Menu.
/// Manages procedural UI tween micro-interactions, scene transitions, and procedural 8-bit/16-bit audio fallback.
/// </summary>
public partial class MainMenu : Control
{
    [Export(PropertyHint.File, "*.tscn")] 
    private string _campaignScenePath = "res://scenes/world_map.tscn";

    private PanelContainer _menuPanel = null!;
    private Button _btnNewGame = null!;
    private Button _btnLoadGame = null!;
    private Button _btnSettings = null!;
    private Button _btnQuit = null!;
    private AudioStreamPlayer _sfxHover = null!;
    private AudioStreamPlayer _sfxClick = null!;

    public override void _Ready()
    {
        // 1. Resolve node bindings using Scene Unique Names (%Name) with structural fallback
        _menuPanel = GetNodeOrNull<PanelContainer>("%MenuPanel") 
            ?? GetNode<PanelContainer>("CenterContainer/MenuPanel");

        _btnNewGame = GetNodeOrNull<Button>("%BtnNewGame") 
            ?? GetNode<Button>("CenterContainer/MenuPanel/InnerPanel/VBoxContainer/BtnNewGame");

        _btnLoadGame = GetNodeOrNull<Button>("%BtnLoadGame") 
            ?? GetNode<Button>("CenterContainer/MenuPanel/InnerPanel/VBoxContainer/BtnLoadGame");

        _btnSettings = GetNodeOrNull<Button>("%BtnSettings") 
            ?? GetNode<Button>("CenterContainer/MenuPanel/InnerPanel/VBoxContainer/BtnSettings");

        _btnQuit = GetNodeOrNull<Button>("%BtnQuit") 
            ?? GetNode<Button>("CenterContainer/MenuPanel/InnerPanel/VBoxContainer/BtnQuit");

        _sfxHover = GetNodeOrNull<AudioStreamPlayer>("%SfxHover") 
            ?? GetNode<AudioStreamPlayer>("SfxHover");

        _sfxClick = GetNodeOrNull<AudioStreamPlayer>("%SfxClick") 
            ?? GetNode<AudioStreamPlayer>("SfxClick");

        // 2. Synthesize retro 8-bit/16-bit audio buffers if no external audio files are attached
        InitializeProceduralAudio();

        // 3. Connect button signal handlers
        _btnNewGame.Pressed += OnNewGamePressed;
        _btnLoadGame.Pressed += OnLoadGamePressed;
        _btnSettings.Pressed += OnSettingsPressed;
        _btnQuit.Pressed += OnQuitPressed;

        // 4. Bind procedural micro-interaction tweens (hover scale, press depress)
        SetupButtonMicroInteractions(_btnNewGame);
        SetupButtonMicroInteractions(_btnLoadGame);
        SetupButtonMicroInteractions(_btnSettings);
        SetupButtonMicroInteractions(_btnQuit);

        // 5. Trigger entrance sequence: upward float + fade-in
        PlayEntranceAnimation();
    }

    /// <summary>
    /// Binds dynamic centering pivot and tween animations for hover and press interactions.
    /// </summary>
    private void SetupButtonMicroInteractions(Button btn)
    {
        // Keep pivot dynamically centered to ensure uniform scaling
        btn.Resized += () => btn.PivotOffset = btn.Size / 2.0f;
        btn.PivotOffset = btn.CustomMinimumSize / 2.0f;

        btn.MouseEntered += () =>
        {
            PlaySound(_sfxHover);
            var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", new Vector2(1.05f, 1.05f), 0.10f);
        };

        btn.MouseExited += () =>
        {
            var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", Vector2.One, 0.12f);
        };

        btn.ButtonDown += () =>
        {
            var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", new Vector2(0.96f, 0.96f), 0.05f);
        };

        btn.ButtonUp += () =>
        {
            Vector2 targetScale = btn.IsHovered() ? new Vector2(1.05f, 1.05f) : Vector2.One;
            var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(btn, "scale", targetScale, 0.12f);
        };
    }

    /// <summary>
    /// Executes a smooth entrance animation for the central floating bronze container.
    /// </summary>
    private void PlayEntranceAnimation()
    {
        _menuPanel.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        _menuPanel.Resized += () => _menuPanel.PivotOffset = _menuPanel.Size / 2.0f;
        _menuPanel.PivotOffset = _menuPanel.CustomMinimumSize / 2.0f;
        _menuPanel.Scale = new Vector2(0.95f, 0.95f);

        var tween = CreateTween().SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(_menuPanel, "modulate:a", 1.0f, 0.40f);
        tween.TweenProperty(_menuPanel, "scale", Vector2.One, 0.40f);
    }

    /// <summary>
    /// Handles "New Campaign": plays click SFX, fades out the scene, and transitions to the world map.
    /// </summary>
    private void OnNewGamePressed()
    {
        PlaySound(_sfxClick);
        GD.Print("[Chronicles] Starting new campaign...");

        // Disable button input during scene transition
        SetButtonsDisabled(true);

        var tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.35f);
        tween.TweenCallback(Callable.From(() =>
        {
            if (ResourceLoader.Exists(_campaignScenePath))
            {
                GetTree().ChangeSceneToFile(_campaignScenePath);
            }
            else
            {
                GD.PrintErr($"[Chronicles] Target scene '{_campaignScenePath}' does not exist yet.");
                SetButtonsDisabled(false);
                var restoreTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
                restoreTween.TweenProperty(this, "modulate:a", 1.0f, 0.25f);
            }
        }));
    }

    /// <summary>
    /// Handles "Load Game": logs debug state / signals save-game modal.
    /// </summary>
    private void OnLoadGamePressed()
    {
        PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening load game dialog...");
    }

    /// <summary>
    /// Handles "Settings": logs debug state / signals settings modal.
    /// </summary>
    private void OnSettingsPressed()
    {
        PlaySound(_sfxClick);
        GD.Print("[Chronicles] Opening settings panel...");
    }

    /// <summary>
    /// Handles "Exit to Desktop": safely terminates the application.
    /// </summary>
    private void OnQuitPressed()
    {
        PlaySound(_sfxClick);
        GD.Print("[Chronicles] Exiting application to desktop...");
        GetTree().Quit();
    }

    private void SetButtonsDisabled(bool disabled)
    {
        _btnNewGame.Disabled = disabled;
        _btnLoadGame.Disabled = disabled;
        _btnSettings.Disabled = disabled;
        _btnQuit.Disabled = disabled;
    }

    private static void PlaySound(AudioStreamPlayer player)
    {
        if (player.Stream != null)
        {
            player.Play();
        }
    }

    /// <summary>
    /// Injects synthesized 16-bit PCM retro audio samples if inspector audio slots are empty.
    /// </summary>
    private void InitializeProceduralAudio()
    {
        if (_sfxHover.Stream == null)
        {
            _sfxHover.Stream = GenerateSyntheticTone(frequency: 880.0f, durationSec: 0.045f, isMetallic: false);
        }

        if (_sfxClick.Stream == null)
        {
            _sfxClick.Stream = GenerateSyntheticTone(frequency: 380.0f, durationSec: 0.12f, isMetallic: true);
        }
    }

    /// <summary>
    /// Generates short procedural 16-bit PCM audio waveforms with exponential decay envelopes.
    /// </summary>
    private static AudioStreamWav GenerateSyntheticTone(float frequency, float durationSec, bool isMetallic)
    {
        const int sampleRate = 22050;
        int numSamples = (int)(sampleRate * durationSec);
        byte[] pcmData = new byte[numSamples * 2];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / numSamples;
            float envelope = Mathf.Exp(-progress * (isMetallic ? 8.0f : 16.0f));

            float wave;
            if (isMetallic)
            {
                // Metallic resonance: bronze fundamental (380 Hz) + harmonic overtone (817 Hz) + transient strike
                float fundamental = Mathf.Sin(2.0f * Mathf.Pi * frequency * t);
                float harmonic = 0.35f * Mathf.Sin(2.0f * Mathf.Pi * (frequency * 2.15f) * t);
                float transient = (float)(GD.Randf() * 2.0 - 1.0) * Mathf.Exp(-progress * 50.0f) * 0.25f;
                wave = (fundamental + harmonic + transient) * envelope;
            }
            else
            {
                // UI hover chirp: quick downward pitch sweep
                float currentFreq = frequency * (1.0f - progress * 0.25f);
                wave = Mathf.Sin(2.0f * Mathf.Pi * currentFreq * t) * envelope;
            }

            short sample = (short)Mathf.Clamp(wave * short.MaxValue * 0.6f, short.MinValue, short.MaxValue);
            pcmData[i * 2] = (byte)(sample & 0xFF);
            pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return new AudioStreamWav
        {
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Stereo = false,
            Data = pcmData
        };
    }
}
