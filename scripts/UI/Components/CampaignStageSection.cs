using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public record StageInfo(
	string Id,
	string Title,
	string Description,
	string Objective,
	string MapSize,
	string Enemy,
	string Reward
);

public partial class CampaignModal
{
	private void SelectStage(int stageIdx)
	{
		_selectedStageIndex = Mathf.Clamp(stageIdx, 0, _stages.Count - 1);
		for (int i = 0; i < _stageButtons.Length; i++)
		{
			bool isSelected = (i == _selectedStageIndex);
			_stageButtons[i].AddThemeColorOverride("font_color", isSelected ? new Color(0.96f, 0.78f, 0.26f, 1f) : new Color(0.85f, 0.82f, 0.75f, 0.9f));
		}

		if (_selectedStageIndex < _stages.Count)
		{
			var s = _stages[_selectedStageIndex];
			_stageTitleLabel.Text = s.Title;
			_stageDescLabel.Text = s.Description;
			_stageObjLabel.Text = $"• {LocalizationManager.Get("SPEC_VICTORY_PREFIX")}{s.Objective}";
			_stageMapLabel.Text = $"• {LocalizationManager.Get("SPEC_MAP_PREFIX")}{s.MapSize}";
			_stageEnemyLabel.Text = $"• {LocalizationManager.Get("SPEC_RIVALS_PREFIX")}{s.Enemy}";
			_stageRewardLabel.Text = $"• {LocalizationManager.Get("SPEC_REWARD_PREFIX")}{s.Reward}";
		}

		UpdateStatusFooter();
	}

	private void SelectTrainingMode(bool isSandbox)
	{
		_isSandboxSelected = isSandbox;
		_btnSelectTutorial.Text = !isSandbox ? "✓ " + LocalizationManager.Get("TRAINING_SELECTED") : LocalizationManager.Get("TRAINING_SELECT");
		_btnSelectSandbox.Text = isSandbox ? "✓ " + LocalizationManager.Get("TRAINING_SELECTED") : LocalizationManager.Get("TRAINING_SELECT");

		_btnSelectTutorial.AddThemeColorOverride("font_color", !isSandbox ? new Color(0.96f, 0.78f, 0.26f, 1f) : new Color(0.85f, 0.82f, 0.75f, 1f));
		_btnSelectSandbox.AddThemeColorOverride("font_color", isSandbox ? new Color(0.96f, 0.78f, 0.26f, 1f) : new Color(0.85f, 0.82f, 0.75f, 1f));

		UpdateStatusFooter();
	}

	private void RebuildStagesList()
	{
		_stages.Clear();
		_stages.Add(new(
			Id: "stage_1",
			Title: LocalizationManager.Get("STAGE_1_TITLE"),
			Description: LocalizationManager.Get("STAGE_1_DESC"),
			Objective: LocalizationManager.Get("STAGE_1_OBJ"),
			MapSize: "32x32",
			Enemy: LocalizationManager.Get("STAGE_1_ENEMY"),
			Reward: LocalizationManager.Get("STAGE_1_REWARD")
		));
		_stages.Add(new(
			Id: "stage_2",
			Title: LocalizationManager.Get("STAGE_2_TITLE"),
			Description: LocalizationManager.Get("STAGE_2_DESC"),
			Objective: LocalizationManager.Get("STAGE_2_OBJ"),
			MapSize: "48x48",
			Enemy: LocalizationManager.Get("STAGE_2_ENEMY"),
			Reward: LocalizationManager.Get("STAGE_2_REWARD")
		));
		_stages.Add(new(
			Id: "stage_3",
			Title: LocalizationManager.Get("STAGE_3_TITLE"),
			Description: LocalizationManager.Get("STAGE_3_DESC"),
			Objective: LocalizationManager.Get("STAGE_3_OBJ"),
			MapSize: "64x64",
			Enemy: LocalizationManager.Get("STAGE_3_ENEMY"),
			Reward: LocalizationManager.Get("STAGE_3_REWARD")
		));
		_stages.Add(new(
			Id: "stage_4",
			Title: LocalizationManager.Get("STAGE_4_TITLE"),
			Description: LocalizationManager.Get("STAGE_4_DESC"),
			Objective: LocalizationManager.Get("STAGE_4_OBJ"),
			MapSize: "64x64",
			Enemy: LocalizationManager.Get("STAGE_4_ENEMY"),
			Reward: LocalizationManager.Get("STAGE_4_REWARD")
		));
		_stages.Add(new(
			Id: "stage_5",
			Title: LocalizationManager.Get("STAGE_5_TITLE"),
			Description: LocalizationManager.Get("STAGE_5_DESC"),
			Objective: LocalizationManager.Get("STAGE_5_OBJ"),
			MapSize: "128x128",
			Enemy: LocalizationManager.Get("STAGE_5_ENEMY"),
			Reward: LocalizationManager.Get("STAGE_5_REWARD")
		));

		for (int i = 0; i < _stageButtons.Length && i < _stages.Count; i++)
		{
			_stageButtons[i].Text = _stages[i].Title;
		}
	}
}

