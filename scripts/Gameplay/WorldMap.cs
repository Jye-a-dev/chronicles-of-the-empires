using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Active battlefield controller displaying session context for Campaign, Skirmish, or Sandbox.
/// </summary>
public partial class WorldMap : Control
{
    public override void _Ready()
    {
        var titleLabel = GetNodeOrNull<Label>("CenterContainer/VBoxContainer/TitleLabel");
        var descLabel = GetNodeOrNull<Label>("CenterContainer/VBoxContainer/DescLabel");
        var btnBack = GetNodeOrNull<Button>("%BtnBack") ?? GetNodeOrNull<Button>("CenterContainer/VBoxContainer/BtnBack");

        var session = GameSession.ActiveConfig;
        if (titleLabel != null)
        {
            titleLabel.Text = session.ModeTitle.ToUpperInvariant();
        }

        if (descLabel != null)
        {
            string mapPrefix = LocalizationManager.Get("SPEC_MAP_PREFIX");
            string rivalsPrefix = LocalizationManager.Get("SPEC_RIVALS_PREFIX");
            string victoryPrefix = LocalizationManager.Get("SPEC_VICTORY_PREFIX");
            descLabel.Text =
                $"[ {session.StageTitle} ]\n" +
                $"{mapPrefix}{session.MapSize} | Biome: {session.Biome} | {rivalsPrefix}{session.RivalsCount} | {victoryPrefix}{session.VictoryCondition} | Diff: {session.Difficulty}";
        }

        if (btnBack != null)
        {
            btnBack.Text = LocalizationManager.CurrentLanguage == LocalizationManager.LangVietnamese ? "Trở Về Menu Chính" : "Return to Main Menu";
            btnBack.MouseDefaultCursorShape = CursorShape.PointingHand;
            btnBack.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
        }
    }
}
