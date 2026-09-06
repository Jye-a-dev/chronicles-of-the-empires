using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public partial class CampaignModal
{
	private static readonly string[] SkirmishMapSizes = ["32x32", "64x64", "128x128"];
	private static readonly string[] SkirmishBiomes = ["red_river", "jungle", "highlands", "steppe", "taiga", "mediterranean"];
	private static readonly int[] SkirmishRivals = [2, 4, 6, 8];
	private static readonly string[] SkirmishVictories = ["conquest", "culture_wonders", "regicide"];
	private static readonly string[] SkirmishDifficulties = ["easy", "normal", "hard", "legendary"];

	private void UpdateSkirmishSummary()
	{
		string map = _skirmishMapSizeOptionBtn.Text;
		string biome = _skirmishBiomeOptionBtn.Text;
		string rivals = _skirmishRivalsOptionBtn.Text;
		string diff = _skirmishDifficultyOptionBtn.Text;

		_skirmishSummaryLabel.Text = $"• {rivals} | {map} | {biome} | {diff}";
		UpdateStatusFooter();
	}

	private void PopulateSkirmishDropdowns()
	{
		int s1 = _skirmishMapSizeOptionBtn.Selected;
		_skirmishMapSizeOptionBtn.Clear();
		_skirmishMapSizeOptionBtn.AddItem(LocalizationManager.Get("MAP_SMALL"), 0);
		_skirmishMapSizeOptionBtn.AddItem(LocalizationManager.Get("MAP_MEDIUM"), 1);
		_skirmishMapSizeOptionBtn.AddItem(LocalizationManager.Get("MAP_LARGE"), 2);
		_skirmishMapSizeOptionBtn.Selected = s1 >= 0 ? s1 : 1;

		int s2 = _skirmishBiomeOptionBtn.Selected;
		_skirmishBiomeOptionBtn.Clear();
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_RED_RIVER"), 0);
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_JUNGLE"), 1);
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_HIGHLANDS"), 2);
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_STEPPE"), 3);
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_TAIGA"), 4);
		_skirmishBiomeOptionBtn.AddItem(LocalizationManager.Get("BIOME_MEDITERRANEAN"), 5);
		_skirmishBiomeOptionBtn.Selected = s2 >= 0 && s2 < 6 ? s2 : 0;

		int s3 = _skirmishRivalsOptionBtn.Selected;
		_skirmishRivalsOptionBtn.Clear();
		_skirmishRivalsOptionBtn.AddItem(LocalizationManager.Get("RIVALS_2"), 0);
		_skirmishRivalsOptionBtn.AddItem(LocalizationManager.Get("RIVALS_4"), 1);
		_skirmishRivalsOptionBtn.AddItem(LocalizationManager.Get("RIVALS_6"), 2);
		_skirmishRivalsOptionBtn.AddItem(LocalizationManager.Get("RIVALS_8"), 3);
		_skirmishRivalsOptionBtn.Selected = s3 >= 0 ? s3 : 1;

		int s4 = _skirmishVictoryOptionBtn.Selected;
		_skirmishVictoryOptionBtn.Clear();
		_skirmishVictoryOptionBtn.AddItem(LocalizationManager.Get("VICTORY_CONQUEST"), 0);
		_skirmishVictoryOptionBtn.AddItem(LocalizationManager.Get("VICTORY_CULTURE"), 1);
		_skirmishVictoryOptionBtn.AddItem(LocalizationManager.Get("VICTORY_REGICIDE"), 2);
		_skirmishVictoryOptionBtn.Selected = s4 >= 0 ? s4 : 0;

		int s5 = _skirmishDifficultyOptionBtn.Selected;
		_skirmishDifficultyOptionBtn.Clear();
		_skirmishDifficultyOptionBtn.AddItem(LocalizationManager.Get("DIFF_EASY"), 0);
		_skirmishDifficultyOptionBtn.AddItem(LocalizationManager.Get("DIFF_NORMAL"), 1);
		_skirmishDifficultyOptionBtn.AddItem(LocalizationManager.Get("DIFF_HARD"), 2);
		_skirmishDifficultyOptionBtn.AddItem(LocalizationManager.Get("DIFF_LEGEND"), 3);
		_skirmishDifficultyOptionBtn.Selected = s5 >= 0 ? s5 : 1;
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

