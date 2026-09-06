using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Standalone campaign modal controller providing three distinct interfaces:
/// 1. Grand Campaign: Mission progression & stage inspection.
/// 2. Skirmish: Tactical map generator & game configuration.
/// 3. Training & Sandbox: Core mechanics tutorial & unit testing playground.
/// </summary>
public partial class CampaignModal : Control
{
	[Signal]
	public delegate void CampaignStartedEventHandler(string mode, string stageId, string mapSize, string biome, int rivals, string victory, string difficulty);

	[Signal]
	public delegate void ClosedEventHandler();

	private PanelContainer _modalPanel = null!;
	private Label _titleLabel = null!;
	private Button _btnCloseHeader = null!;

	// Tabs
	private Button _btnTabCampaign = null!;
	private Button _btnTabSkirmish = null!;
	private Button _btnTabTraining = null!;

	// Views
	private Control _campaignView = null!;
	private Control _skirmishView = null!;
	private Control _trainingView = null!;

	// Campaign Stage Details
	private Button[] _stageButtons = [];
	private Label _stageTitleLabel = null!;
	private Label _stageDescLabel = null!;
	private Label _stageObjLabel = null!;
	private Label _stageMapLabel = null!;
	private Label _stageEnemyLabel = null!;
	private Label _stageRewardLabel = null!;

	// Skirmish Controls
	private Label _skirmishMapSizeLabel = null!;
	private OptionButton _skirmishMapSizeOptionBtn = null!;
	private Label _skirmishBiomeLabel = null!;
	private OptionButton _skirmishBiomeOptionBtn = null!;
	private Label _skirmishRivalsLabel = null!;
	private OptionButton _skirmishRivalsOptionBtn = null!;
	private Label _skirmishVictoryLabel = null!;
	private OptionButton _skirmishVictoryOptionBtn = null!;
	private Label _skirmishDifficultyLabel = null!;
	private OptionButton _skirmishDifficultyOptionBtn = null!;
	private Label _skirmishSummaryLabel = null!;

	// Training Controls
	private Button _btnSelectTutorial = null!;
	private Button _btnSelectSandbox = null!;

	// Footer
	private Label _statusInfoLabel = null!;
	private Button _btnCloseModal = null!;
	private Button _btnDeploy = null!;

	private Tween? _tween;
	private int _activeTab = 0; // 0: Campaign, 1: Skirmish, 2: Training
	private int _selectedStageIndex = 0;
	private bool _isSandboxSelected = false;

	private readonly List<StageInfo> _stages = new(5);

	public bool IsOpen => Visible && Modulate.A > 0.05f;

	public override void _Ready()
	{
		_modalPanel = GetNode<PanelContainer>("%ModalPanel");
		_titleLabel = GetNode<Label>("%ModalTitleLabel");
		_btnCloseHeader = GetNode<Button>("%BtnCloseHeader");

		_btnTabCampaign = GetNode<Button>("%BtnTabCampaign");
		_btnTabSkirmish = GetNode<Button>("%BtnTabSkirmish");
		_btnTabTraining = GetNode<Button>("%BtnTabTraining");

		_campaignView = GetNode<Control>("%CampaignView");
		_skirmishView = GetNode<Control>("%SkirmishView");
		_trainingView = GetNode<Control>("%TrainingView");

		_stageButtons =
		[
			GetNode<Button>("%BtnStage1"),
			GetNode<Button>("%BtnStage2"),
			GetNode<Button>("%BtnStage3"),
			GetNode<Button>("%BtnStage4"),
			GetNode<Button>("%BtnStage5")
		];

		_stageTitleLabel = GetNode<Label>("%StageTitleLabel");
		_stageDescLabel = GetNode<Label>("%StageDescLabel");
		_stageObjLabel = GetNode<Label>("%StageObjLabel");
		_stageMapLabel = GetNode<Label>("%StageMapLabel");
		_stageEnemyLabel = GetNode<Label>("%StageEnemyLabel");
		_stageRewardLabel = GetNode<Label>("%StageRewardLabel");

		_skirmishMapSizeLabel = GetNode<Label>("%SkirmishMapSizeLabel");
		_skirmishMapSizeOptionBtn = GetNode<OptionButton>("%SkirmishMapSizeOptionBtn");
		_skirmishBiomeLabel = GetNode<Label>("%SkirmishBiomeLabel");
		_skirmishBiomeOptionBtn = GetNode<OptionButton>("%SkirmishBiomeOptionBtn");
		_skirmishRivalsLabel = GetNode<Label>("%SkirmishRivalsLabel");
		_skirmishRivalsOptionBtn = GetNode<OptionButton>("%SkirmishRivalsOptionBtn");
		_skirmishVictoryLabel = GetNode<Label>("%SkirmishVictoryLabel");
		_skirmishVictoryOptionBtn = GetNode<OptionButton>("%SkirmishVictoryOptionBtn");
		_skirmishDifficultyLabel = GetNode<Label>("%SkirmishDifficultyLabel");
		_skirmishDifficultyOptionBtn = GetNode<OptionButton>("%SkirmishDifficultyOptionBtn");
		_skirmishSummaryLabel = GetNode<Label>("%SkirmishSummaryLabel");

		_btnSelectTutorial = GetNode<Button>("%BtnSelectTutorial");
		_btnSelectSandbox = GetNode<Button>("%BtnSelectSandbox");

		_statusInfoLabel = GetNode<Label>("%StatusInfoLabel");
		_btnCloseModal = GetNode<Button>("%BtnCloseModal");
		_btnDeploy = GetNode<Button>("%BtnDeploy");

		// Style OptionButton popups
		StylePopup(_skirmishMapSizeOptionBtn);
		StylePopup(_skirmishBiomeOptionBtn);
		StylePopup(_skirmishRivalsOptionBtn);
		StylePopup(_skirmishVictoryOptionBtn);
		StylePopup(_skirmishDifficultyOptionBtn);

		// Bind events
		_btnCloseHeader.Pressed += Close;
		_btnCloseModal.Pressed += Close;
		_btnDeploy.Pressed += OnDeployPressed;

		_btnTabCampaign.Pressed += () => SwitchTab(0);
		_btnTabSkirmish.Pressed += () => SwitchTab(1);
		_btnTabTraining.Pressed += () => SwitchTab(2);

		for (int i = 0; i < _stageButtons.Length; i++)
		{
			int idx = i;
			_stageButtons[i].Pressed += () => SelectStage(idx);
		}

		_skirmishMapSizeOptionBtn.ItemSelected += _ => UpdateSkirmishSummary();
		_skirmishBiomeOptionBtn.ItemSelected += _ => UpdateSkirmishSummary();
		_skirmishRivalsOptionBtn.ItemSelected += _ => UpdateSkirmishSummary();
		_skirmishVictoryOptionBtn.ItemSelected += _ => UpdateSkirmishSummary();
		_skirmishDifficultyOptionBtn.ItemSelected += _ => UpdateSkirmishSummary();

		_btnSelectTutorial.Pressed += () => SelectTrainingMode(false);
		_btnSelectSandbox.Pressed += () => SelectTrainingMode(true);

		LocalizationManager.LanguageChanged += UpdateLocalizedStrings;

		_modalPanel.Resized += () => _modalPanel.PivotOffset = _modalPanel.Size / 2.0f;
		_modalPanel.PivotOffset = _modalPanel.CustomMinimumSize / 2.0f;

		Visible = false;
		Modulate = new Color(1, 1, 1, 0);

		UpdateLocalizedStrings();
		SwitchTab(0);
		SelectStage(0);
		SelectTrainingMode(false);
	}

	public void InitializeInteractions(AudioStreamPlayer? sfxHover, AudioStreamPlayer? sfxClick)
	{
		List<Button> buttons =
		[
			_btnCloseHeader,
			_btnCloseModal,
			_btnDeploy,
			_btnTabCampaign,
			_btnTabSkirmish,
			_btnTabTraining,
			_btnSelectTutorial,
			_btnSelectSandbox,
			_skirmishMapSizeOptionBtn,
			_skirmishBiomeOptionBtn,
			_skirmishRivalsOptionBtn,
			_skirmishVictoryOptionBtn,
			_skirmishDifficultyOptionBtn
		];
		buttons.AddRange(_stageButtons);

		foreach (var btn in buttons)
		{
			if (btn != null)
			{
				MenuButtonAnimator.Attach(btn, sfxHover, sfxClick);
				btn.MouseDefaultCursorShape = CursorShape.PointingHand;
			}
		}
	}

	public void Open()
	{
		Visible = true;
		Modulate = new Color(1, 1, 1, 0.0f);
		_modalPanel.PivotOffset = _modalPanel.Size.X > 0 ? _modalPanel.Size / 2.0f : _modalPanel.CustomMinimumSize / 2.0f;
		_modalPanel.Scale = new Vector2(0.95f, 0.95f);

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

	public void Toggle()
	{
		if (IsOpen) Close();
		else Open();
	}

	private void SwitchTab(int tabIndex)
	{
		_activeTab = tabIndex;
		_campaignView.Visible = (tabIndex == 0);
		_skirmishView.Visible = (tabIndex == 1);
		_trainingView.Visible = (tabIndex == 2);

		HighlightTab(_btnTabCampaign, tabIndex == 0);
		HighlightTab(_btnTabSkirmish, tabIndex == 1);
		HighlightTab(_btnTabTraining, tabIndex == 2);

		UpdateStatusFooter();
	}

	private static void HighlightTab(Button btn, bool active)
	{
		btn.AddThemeColorOverride("font_color", active ? new Color(0.96f, 0.78f, 0.26f, 1f) : new Color(0.85f, 0.82f, 0.75f, 1f));
	}

	private void UpdateStatusFooter()
	{
		if (_activeTab == 0)
		{
			string stageName = _selectedStageIndex < _stages.Count ? _stages[_selectedStageIndex].Title : "Ải";
			_statusInfoLabel.Text = $"• {stageName}";
			_btnDeploy.Text = LocalizationManager.Get("CAMPAIGN_DEPLOY");
		}
		else if (_activeTab == 1)
		{
			_statusInfoLabel.Text = $"• {LocalizationManager.Get("CAMPAIGN_BTN_SKIRMISH")}: {_skirmishMapSizeOptionBtn.Text}";
			_btnDeploy.Text = LocalizationManager.Get("CAMPAIGN_START_SKIRMISH");
		}
		else
		{
			string trainingName = _isSandboxSelected ? LocalizationManager.Get("TRAINING_SANDBOX_NAME") : LocalizationManager.Get("TRAINING_TUTORIAL_NAME");
			_statusInfoLabel.Text = $"• {trainingName}";
			_btnDeploy.Text = LocalizationManager.Get("CAMPAIGN_START_TRAINING");
		}
	}

	private void OnDeployPressed()
	{
		string mode;
		string modeTitle;
		string stageId = string.Empty;
		string stageTitle = string.Empty;
		string mapSize = "64x64";
		string biome = "red_river";
		int rivals = 2;
		string victory = "conquest";
		string difficulty = "normal";

		if (_activeTab == 0)
		{
			mode = "campaign";
			modeTitle = LocalizationManager.Get("CAMPAIGN_TAB_CAMPAIGN");
			if (_selectedStageIndex < _stages.Count)
			{
				var s = _stages[_selectedStageIndex];
				stageId = s.Id;
				stageTitle = s.Title;
				mapSize = s.MapSize;
			}
		}
		else if (_activeTab == 1)
		{
			mode = "skirmish";
			modeTitle = LocalizationManager.Get("CAMPAIGN_TAB_SKIRMISH");
			mapSize = SkirmishMapSizes[Mathf.Clamp(_skirmishMapSizeOptionBtn.Selected, 0, SkirmishMapSizes.Length - 1)];
			biome = SkirmishBiomes[Mathf.Clamp(_skirmishBiomeOptionBtn.Selected, 0, SkirmishBiomes.Length - 1)];
			rivals = SkirmishRivals[Mathf.Clamp(_skirmishRivalsOptionBtn.Selected, 0, SkirmishRivals.Length - 1)];
			victory = SkirmishVictories[Mathf.Clamp(_skirmishVictoryOptionBtn.Selected, 0, SkirmishVictories.Length - 1)];
			difficulty = SkirmishDifficulties[Mathf.Clamp(_skirmishDifficultyOptionBtn.Selected, 0, SkirmishDifficulties.Length - 1)];
			stageTitle = $"{mapSize} - {biome}";
		}
		else
		{
			mode = _isSandboxSelected ? "sandbox" : "tutorial";
			modeTitle = _isSandboxSelected ? LocalizationManager.Get("TRAINING_SANDBOX_NAME") : LocalizationManager.Get("TRAINING_TUTORIAL_NAME");
			stageId = mode;
			stageTitle = modeTitle;
			mapSize = _isSandboxSelected ? "128x128" : "32x32";
		}

		GameSession.ActiveConfig = new GameSessionConfig(
			Mode: mode,
			ModeTitle: modeTitle,
			StageId: stageId,
			StageTitle: stageTitle,
			MapSize: mapSize,
			Biome: biome,
			RivalsCount: rivals,
			VictoryCondition: victory,
			Difficulty: difficulty
		);

		EmitSignal(SignalName.CampaignStarted, mode, stageId, mapSize, biome, rivals, victory, difficulty);
		Close();
	}

	private void UpdateLocalizedStrings()
	{
		_titleLabel.Text = LocalizationManager.Get("CAMPAIGN_MODAL_TITLE");
		_btnTabCampaign.Text = LocalizationManager.Get("CAMPAIGN_TAB_CAMPAIGN");
		_btnTabSkirmish.Text = LocalizationManager.Get("CAMPAIGN_TAB_SKIRMISH");
		_btnTabTraining.Text = LocalizationManager.Get("CAMPAIGN_TAB_TRAINING");
		_btnCloseModal.Text = LocalizationManager.Get("SETTINGS_BTN_CLOSE");

		RebuildStagesList();

		// Skirmish labels
		_skirmishMapSizeLabel.Text = LocalizationManager.Get("SKIRMISH_MAP_SIZE");
		_skirmishBiomeLabel.Text = LocalizationManager.Get("SKIRMISH_BIOME");
		_skirmishRivalsLabel.Text = LocalizationManager.Get("SKIRMISH_RIVALS");
		_skirmishVictoryLabel.Text = LocalizationManager.Get("SKIRMISH_VICTORY");
		_skirmishDifficultyLabel.Text = LocalizationManager.Get("SKIRMISH_DIFFICULTY");

		PopulateSkirmishDropdowns();
		SelectStage(_selectedStageIndex);
		SelectTrainingMode(_isSandboxSelected);
		UpdateSkirmishSummary();
	}

	public override void _ExitTree()
	{
		LocalizationManager.LanguageChanged -= UpdateLocalizedStrings;
	}
}
