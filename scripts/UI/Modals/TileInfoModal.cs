using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Controller for the Đông Sơn Bronze Age styled Tile Information & Settlement Inspection Modal.
/// Provides data binding for HexCell, TileTerrainData, ResourceDepositData, and BuildingData.
/// Sub-features (Improvements and Recruitment) are modularized into partial components.
/// </summary>
public partial class TileInfoModal : PanelContainer
{
    [Signal]
    public delegate void RecruitRequestedEventHandler(string unitConfigId, Vector2I coords);

    [Signal]
    public delegate void UpgradeRequestedEventHandler(string buildingConfigId, Vector2I coords);

    // Aesthetic color palette constants (Đông Sơn Bronze & Lacquer)
    private static readonly Color GoldColor = new("#f5c842");
    private static readonly Color DimBronzeColor = new("#b37e1a");
    private static readonly Color OffWhiteColor = new("#dfd8cb");
    private static readonly Color PositiveYieldColor = new("#68b87d");
    private static readonly Color GoldCostColor = new("#ffd678");
    private static readonly Color OrangeStatusColor = new("#e67e22");
    private static readonly Color DimFlavorColor = new("#8c857b");

    // Section 1: Header Nodes
    private Label? _titleLabel;
    private Button? _btnClose;
    private Label? _descLabel;
    private HBoxContainer? _yieldBadgesRow;
    private Label? _yieldFood;
    private Label? _yieldProd;
    private Label? _yieldGold;
    private Label? _yieldSci;
    private Label? _yieldFaith;

    // Section 2: Tile Status Nodes
    private Label? _moveCostLabel;
    private Label? _terrainStateLabel;
    private Label? _factionOwnerLabel;

    // Section 3: Feature & Improvement Card Nodes
    private PanelContainer? _improvementCard;
    private Label? _featureNameLabel;
    private Label? _featureStatusLabel;
    private Label? _featureYieldLabel;
    private Button? _btnBuildImprovement;
    private Label? _btnBuildLabel;
    private HBoxContainer? _btnBuildCostRow;

    // Section 4: Recruitment Section Nodes
    private VBoxContainer? _recruitSection;
    private GridContainer? _recruitGrid;

    private string? _pendingImprovementId;

    public HexCell? ActiveCell { get; private set; }

    public override void _Ready()
    {
        // Resolve Section 1: Header
        _titleLabel = GetNodeOrNull<Label>("%TileTitle");
        _btnClose = GetNodeOrNull<Button>("%BtnCloseTile");
        _descLabel = GetNodeOrNull<Label>("%TileDesc");
        _yieldBadgesRow = GetNodeOrNull<HBoxContainer>("%YieldBadgesRow");

        if (_btnClose != null)
        {
            _btnClose.Pressed += CloseModal;
        }

        if (_yieldBadgesRow != null)
        {
            SetupBaseYieldBadges();
        }

        // Resolve Section 2: Status Bar
        _moveCostLabel = GetNodeOrNull<Label>("%MoveCostLabel");
        _terrainStateLabel = GetNodeOrNull<Label>("%TerrainStateLabel");
        _factionOwnerLabel = GetNodeOrNull<Label>("%FactionOwnerLabel");

        // Resolve Section 3: Improvement Card
        _improvementCard = GetNodeOrNull<PanelContainer>("%ImprovementCard");
        _featureNameLabel = GetNodeOrNull<Label>("%FeatureNameLabel");
        _featureStatusLabel = GetNodeOrNull<Label>("%FeatureStatusLabel");
        _featureYieldLabel = GetNodeOrNull<Label>("%FeatureYieldLabel");
        _btnBuildImprovement = GetNodeOrNull<Button>("%BtnBuildImprovement");
        _btnBuildLabel = GetNodeOrNull<Label>("%BtnBuildLabel");
        _btnBuildCostRow = GetNodeOrNull<HBoxContainer>("%BtnBuildCostRow");

        if (_btnBuildImprovement != null)
        {
            _btnBuildImprovement.Pressed += OnBuildImprovementPressed;
        }

        // Resolve Section 4: Recruitment Section
        _recruitSection = GetNodeOrNull<VBoxContainer>("%RecruitSection");
        _recruitGrid = GetNodeOrNull<GridContainer>("%RecruitGrid");

        Visible = false;
    }

    private void SetupBaseYieldBadges()
    {
        if (_yieldBadgesRow == null) return;

        foreach (Node child in _yieldBadgesRow.GetChildren())
        {
            child.QueueFree();
        }

        _yieldFood = AddBaseYieldItem(_yieldBadgesRow, ResourceType.Food);
        _yieldProd = AddBaseYieldItem(_yieldBadgesRow, ResourceType.Production);
        _yieldGold = AddBaseYieldItem(_yieldBadgesRow, ResourceType.Gold);
        _yieldSci = AddBaseYieldItem(_yieldBadgesRow, ResourceType.Science);
        _yieldFaith = AddBaseYieldItem(_yieldBadgesRow, ResourceType.Faith);
    }

    private static Label AddBaseYieldItem(HBoxContainer parent, ResourceType type)
    {
        var badge = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        badge.AddThemeConstantOverride("separation", 2);

        var icon = IconManager.CreateResourceIconRect(type, 12);
        icon.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var lbl = new Label
        {
            Text = "+0",
            MouseFilter = MouseFilterEnum.Ignore
        };
        lbl.AddThemeFontSizeOverride("font_size", 8);
        lbl.AddThemeColorOverride("font_color", DimFlavorColor);

        badge.AddChild(icon);
        badge.AddChild(lbl);
        parent.AddChild(badge);

        return lbl;
    }

    /// <summary>
    /// Binds data from an inspected HexCell and populates all 4 modal sections.
    /// </summary>
    public void DisplayCell(HexCell cell)
    {
        ActiveCell = cell;

        // 1. Header Section
        if (_titleLabel != null)
        {
            _titleLabel.Text = $"{cell.TerrainData.Biome.ToString().ToUpperInvariant()}: {cell.Name.ToUpperInvariant()} ({cell.Coords.X}, {cell.Coords.Y})";
        }

        if (_descLabel != null)
        {
            _descLabel.Text = string.IsNullOrWhiteSpace(cell.Description)
                ? "Địa hình tự nhiên của vùng đất cổ."
                : cell.Description;
        }

        UpdateYieldBadge(_yieldFood, cell.FoodYield);
        UpdateYieldBadge(_yieldProd, cell.ProdYield);
        UpdateYieldBadge(_yieldGold, cell.GoldYield);
        UpdateYieldBadge(_yieldSci, cell.SciYield);
        UpdateYieldBadge(_yieldFaith, cell.FaithYield);

        // 2. Status Bar Section
        if (_moveCostLabel != null)
        {
            string costText = cell.IsSolid ? "∞" : $"{cell.MoveCost}";
            _moveCostLabel.Text = $"Tiêu hao: {costText}";
        }

        if (_terrainStateLabel != null)
        {
            _terrainStateLabel.Text = cell.IsSolid ? "Vật cản" : "Thông thoáng";
        }

        if (_factionOwnerLabel != null)
        {
            string owner = cell.OwnerFactionId >= 0 ? $"Phe {cell.OwnerFactionId}" : "Vùng hoang dã";
            _factionOwnerLabel.Text = owner;
        }

        // 3. Feature & Improvement Card Section
        UpdateImprovementCard(cell);

        // 4. Recruitment Section
        UpdateRecruitmentSection(cell);

        Visible = true;
        AnimateOpen();
    }

    private static void UpdateYieldBadge(Label? lbl, int value)
    {
        if (lbl == null) return;
        lbl.Text = $"+{value}";
        lbl.AddThemeColorOverride("font_color", value > 0 ? PositiveYieldColor : DimFlavorColor);
    }

    public void CloseModal()
    {
        ActiveCell = null;
        Visible = false;
    }

    private void AnimateOpen()
    {
        Modulate = new Color(1f, 1f, 1f, 0f);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.12);
    }

    public override void _ExitTree()
    {
        if (_btnClose != null)
        {
            _btnClose.Pressed -= CloseModal;
        }

        if (_btnBuildImprovement != null)
        {
            _btnBuildImprovement.Pressed -= OnBuildImprovementPressed;
        }
    }
}
