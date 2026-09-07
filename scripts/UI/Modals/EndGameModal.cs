using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Game;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Terminal game-over modal presentation component (Victory or Defeat).
/// Hosted on CanvasLayer (Layer = 25) to cleanly occlude tactical HUD, camera, and map interactions.
/// </summary>
public partial class EndGameModal : CanvasLayer
{
    private Label? _titleLabel;
    private Label? _subtitleLabel;
    private Label? _turnsLabel;
    private Label? _killsLabel;
    private Label? _typeLabel;
    private Button? _btnReturnMenu;
    private Control? _modalContainer;

    public override void _Ready()
    {
        Layer = 25;
        _titleLabel = GetNodeOrNull<Label>("%TitleLabel") ?? GetNodeOrNull<Label>("Center/Panel/Margin/VBox/TitleLabel");
        _subtitleLabel = GetNodeOrNull<Label>("%SubtitleLabel") ?? GetNodeOrNull<Label>("Center/Panel/Margin/VBox/SubtitleLabel");
        _turnsLabel = GetNodeOrNull<Label>("%StatTurns") ?? GetNodeOrNull<Label>("Center/Panel/Margin/VBox/StatsContainer/StatTurns");
        _killsLabel = GetNodeOrNull<Label>("%StatEliminations") ?? GetNodeOrNull<Label>("Center/Panel/Margin/VBox/StatsContainer/StatEliminations");
        _typeLabel = GetNodeOrNull<Label>("%StatVictoryType") ?? GetNodeOrNull<Label>("Center/Panel/Margin/VBox/StatsContainer/StatVictoryType");
        _btnReturnMenu = GetNodeOrNull<Button>("%BtnReturnMenu") ?? GetNodeOrNull<Button>("Center/Panel/Margin/VBox/BtnReturnMenu");
        _modalContainer = GetNodeOrNull<Control>("Center");

        if (_btnReturnMenu != null)
        {
            _btnReturnMenu.Pressed += OnReturnToMenuPressed;
        }

        Visible = false;
    }

    /// <summary>
    /// Displays terminal victory/defeat modal with match analytics on the Main Thread.
    /// </summary>
    public void ShowResult(GameResult result, VictoryType type, int turnsCompleted, int enemiesEliminated = 0)
    {
        Visible = true;

        bool isVictory = result == GameResult.Victory;
        Color bannerColor = isVictory ? new Color("#f5c842") : new Color("#e03b24");

        if (_titleLabel != null)
        {
            _titleLabel.Text = isVictory ? "KHẢI HOÀN ĐẠI THẮNG" : "SƠN HÀ NGUY BIẾN";
            _titleLabel.AddThemeColorOverride("font_color", bannerColor);
        }

        if (_subtitleLabel != null)
        {
            _subtitleLabel.Text = isVictory
                ? "Giang sơn quy về một mối, muôn dân thái bình ca khúc khải hoàn!"
                : "Quân cơ lỡ vận, thành trì sụp đổ, đại nghiệp chưa thành!";
            _subtitleLabel.AddThemeColorOverride("font_color", isVictory ? new Color("#e8d8b8") : new Color("#d4a0a0"));
        }

        if (_turnsLabel != null)
        {
            _turnsLabel.Text = $"Số lượt hoàn tất: {turnsCompleted}";
        }

        if (_killsLabel != null)
        {
            _killsLabel.Text = $"Quân địch tiêu diệt: {enemiesEliminated}";
        }

        if (_typeLabel != null)
        {
            string typeDesc = type switch
            {
                VictoryType.Conquest => isVictory ? "Hình thức: Bình Định Thiên Hạ (Quét sạch quân thù)" : "Nguyên nhân: Toàn quân bị tận diệt",
                VictoryType.Regicide => "Hình thức: Trảm Tướng Đoạt Kỳ",
                VictoryType.Domination => "Hình thức: Bá Quyền Lãnh Thổ",
                VictoryType.DeficitCollapse => "Nguyên nhân: Quốc khố khánh kiệt / Nạn đói",
                _ => isVictory ? "Hình thức: Đại Thắng" : "Nguyên nhân: Chiến bại"
            };
            _typeLabel.Text = typeDesc;
        }

        // Play entrance scale and alpha fade animation
        if (_modalContainer != null)
        {
            _modalContainer.Modulate = new Color(1, 1, 1, 0);
            _modalContainer.Scale = new Vector2(0.95f, 0.95f);
            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(_modalContainer, "modulate:a", 1.0f, 0.35).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(_modalContainer, "scale", Vector2.One, 0.35).SetTrans(Tween.TransitionType.Back);
        }
    }

    private void OnReturnToMenuPressed()
    {
        const string menuScene = "res://scenes/ui/screens/main_menu.tscn";
        const string fallbackScene = "res://scenes/main_menu.tscn";

        if (ResourceLoader.Exists(menuScene))
        {
            GetTree().ChangeSceneToFile(menuScene);
        }
        else if (ResourceLoader.Exists(fallbackScene))
        {
            GetTree().ChangeSceneToFile(fallbackScene);
        }
        else
        {
            GetTree().ChangeSceneToFile(menuScene);
        }
    }
}
