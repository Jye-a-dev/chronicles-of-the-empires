using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Native Godot 4 C# controller for the asynchronous scene loading screen.
/// Guarantees a minimum display threshold (min 3.0s) so fast/instant loads still render smoothly.
/// </summary>
public partial class LoadingScreen : Control
{
    [Export]
    private float _minLoadingDuration = 3.0f;

    [Export(PropertyHint.File, "*.tscn")]
    private string _defaultScenePath = "res://scenes/main_menu.tscn";

    [Export(PropertyHint.File, "*.txt")]
    private string _statusStagesPath = "res://data/loading_stages.txt";

    private string[] _statusStages = [];

    public static string TargetScenePath { get; set; } = string.Empty;

    private Control _spinner = null!;
    private ProgressBar _progressBar = null!;
    private Label _percentLabel = null!;
    private Label _statusLabel = null!;
    private Label? _titleLabel;
    private Label? _subtitleLabel;

    private string _activeScenePath = string.Empty;
    private float _elapsedTime;
    private float _displayedProgress;
    private bool _isLoadingActive;
    private bool _isTransitioningOut;

    public static void TransitionTo(SceneTree tree, string targetScenePath, string loadingScreenPath = "res://scenes/loading_screen.tscn")
    {
        TargetScenePath = targetScenePath;
        tree.ChangeSceneToFile(loadingScreenPath);
    }

    public override void _Ready()
    {
        // 1. Resolve UI node references using unique names with structural fallbacks
        _spinner = GetNodeOrNull<Control>("%Spinner")
            ?? GetNode<Control>("CenterContainer/ContentVBox/SpinnerCenter/Spinner");

        _progressBar = GetNodeOrNull<ProgressBar>("%ProgressBar")
            ?? GetNode<ProgressBar>("CenterContainer/ContentVBox/ProgressContainer/ProgressBar");

        _percentLabel = GetNodeOrNull<Label>("%PercentLabel")
            ?? GetNode<Label>("CenterContainer/ContentVBox/ProgressContainer/PercentLabel");

        _statusLabel = GetNodeOrNull<Label>("%StatusLabel")
            ?? GetNode<Label>("CenterContainer/ContentVBox/StatusLabel");

        _titleLabel = GetNodeOrNull<Label>("%TitleLabel")
            ?? GetNodeOrNull<Label>("CenterContainer/ContentVBox/TitleLabel");

        _subtitleLabel = GetNodeOrNull<Label>("%SubtitleLabel")
            ?? GetNodeOrNull<Label>("CenterContainer/ContentVBox/SubtitleLabel");

        // Keep spinner pivot dynamically centered for smooth rotation
        _spinner.Resized += () => _spinner.PivotOffset = _spinner.Size / 2.0f;
        _spinner.PivotOffset = _spinner.Size / 2.0f;

        // Load and apply persistent settings from local text file
        SettingsManager.Apply(SettingsManager.Load(), GetTree());

        // Load status stage strings and localized headers
        LocalizationManager.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();

        // Reset visual progress
        _displayedProgress = 0.0f;
        _progressBar.Value = 0.0;
        _percentLabel.Text = "0%";
        if (_statusStages.Length > 0)
        {
            _statusLabel.Text = _statusStages[0];
        }

        // 2. Resolve destination scene path (defaults to Main Menu on initial game boot)
        _activeScenePath = !string.IsNullOrEmpty(TargetScenePath) ? TargetScenePath : _defaultScenePath;
        TargetScenePath = string.Empty;
        _elapsedTime = 0.0f;
        _isTransitioningOut = false;

        // 3. Initiate background threaded loading
        if (ResourceLoader.Exists(_activeScenePath))
        {
            Error err = ResourceLoader.LoadThreadedRequest(_activeScenePath);
            if (err == Error.Ok)
            {
                _isLoadingActive = true;
            }
            else
            {
                GD.PrintErr($"[LoadingScreen] LoadThreadedRequest failed with error: {err}");
                _isLoadingActive = false;
            }
        }
        else
        {
            GD.PrintErr($"[LoadingScreen] Target scene '{_activeScenePath}' not found on disk. Simulating load preview.");
            _isLoadingActive = false;
        }

        // 4. Entrance fade-in animation
        Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        var enterTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        enterTween.TweenProperty(this, "modulate:a", 1.0f, 0.25f);
    }

    public override void _Process(double delta)
    {
        if (_isTransitioningOut)
        {
            return;
        }

        float dt = (float)delta;
        _elapsedTime += dt;

        // Rotate bronze spinner continuously (1 revolution per second)
        _spinner.Rotation += dt * Mathf.Tau;

        // Linear ratio of elapsed time against minimum required duration (3.0s)
        float timeRatio = Mathf.Clamp(_elapsedTime / _minLoadingDuration, 0.0f, 1.0f);

        float targetProgress;
        bool isAssetReady = false;

        if (_isLoadingActive)
        {
            var progressArray = new Godot.Collections.Array();
            var status = ResourceLoader.LoadThreadedGetStatus(_activeScenePath, progressArray);
            float realLoadProgress = progressArray.Count > 0 ? (float)progressArray[0] : 0.0f;

            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                isAssetReady = true;
                // Asset is already loaded in RAM, but progress is gated by timeRatio to guarantee min 3s display
                targetProgress = timeRatio;
            }
            else if (status == ResourceLoader.ThreadLoadStatus.InProgress)
            {
                // In progress: progress cannot advance beyond actual loading progress nor time ratio
                targetProgress = Mathf.Min(realLoadProgress, timeRatio);
            }
            else
            {
                GD.PrintErr($"[LoadingScreen] Threaded load failed or invalid: {status}");
                isAssetReady = false;
                targetProgress = timeRatio;
            }
        }
        else
        {
            // Simulation fallback if target scene is pending creation
            targetProgress = timeRatio;
            isAssetReady = (_elapsedTime >= _minLoadingDuration);
        }

        // Smooth visual progression
        _displayedProgress = Mathf.MoveToward(_displayedProgress, targetProgress, dt * 1.5f);

        UpdateUi(_displayedProgress);

        // Transition out ONLY when BOTH minimum display time (3.0s) has elapsed AND asset is loaded in RAM
        if (_elapsedTime >= _minLoadingDuration && _displayedProgress >= 0.999f && isAssetReady)
        {
            CompleteTransition();
        }
    }

    private void UpdateUi(float progress)
    {
        _progressBar.Value = progress * 100.0;
        _percentLabel.Text = $"{Mathf.RoundToInt(progress * 100.0f)}%";

        if (_statusStages.Length > 0)
        {
            int stageIdx = Mathf.Clamp((int)(progress * _statusStages.Length), 0, _statusStages.Length - 1);
            _statusLabel.Text = _statusStages[stageIdx];
        }
    }

    private void CompleteTransition()
    {
        _isTransitioningOut = true;
        _progressBar.Value = 100.0;
        _percentLabel.Text = "100%";
        _statusLabel.Text = "Ready!";

        // Exit fade-out tween
        var exitTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        exitTween.TweenProperty(this, "modulate:a", 0.0f, 0.35f);
        exitTween.TweenCallback(Callable.From(() =>
        {
            if (_isLoadingActive && ResourceLoader.Exists(_activeScenePath))
            {
                var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(_activeScenePath);
                if (packedScene != null)
                {
                    GetTree().ChangeSceneToPacked(packedScene);
                    return;
                }
            }

            if (ResourceLoader.Exists(_activeScenePath))
            {
                GetTree().ChangeSceneToFile(_activeScenePath);
            }
            else
            {
                GD.PrintErr($"[LoadingScreen] Cannot switch to nonexistent scene: '{_activeScenePath}'. Returning to Main Menu.");
                GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
            }
        }));
    }

    private void OnLanguageChanged()
    {
        LoadStatusStages();
        if (_titleLabel != null) _titleLabel.Text = LocalizationManager.Get("MENU_TITLE");
        if (_subtitleLabel != null) _subtitleLabel.Text = LocalizationManager.Get("MENU_SUBTITLE");
    }

    private void LoadStatusStages()
    {
        string path = _statusStagesPath;
        if (LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese && path.EndsWith(".txt"))
        {
            string viPath = path.Insert(path.Length - 4, "_vi");
            if (FileAccess.FileExists(viPath))
            {
                path = viPath;
            }
        }

        if (FileAccess.FileExists(path))
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var lines = new System.Collections.Generic.List<string>();
                while (!file.EofReached())
                {
                    string line = file.GetLine().Trim();
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                    {
                        lines.Add(line);
                    }
                }
                if (lines.Count > 0)
                {
                    _statusStages = [.. lines];
                    return;
                }
            }
        }

        // Fallback default stages
        _statusStages =
        [
            "Inscribing continental provinces & ancient scroll maps...",
            "Awakening eighteen mythic dynasties across four cultural spheres...",
            "Mustering armored regiments & deploying tactical pathfinding...",
            "Igniting pixel shaders, dynamic shadows & mystic embers...",
            "Chronicles of the Old Empires begins..."
        ];
    }

    public override void _ExitTree()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
    }
}
