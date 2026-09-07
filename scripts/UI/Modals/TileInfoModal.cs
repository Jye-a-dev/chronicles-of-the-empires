using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Controller for the Đông Sơn Bronze Age styled Tile Information & Settlement Inspection Modal.
/// Provides data binding for HexCell, TileTerrainData, ResourceDepositData, and BuildingData,
/// with dynamic recruitment grid, cost badges, and pixel-perfect 640x360 layout scaling.
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

    private void UpdateImprovementCard(HexCell cell)
    {
        if (_improvementCard == null) return;

        if (cell.Deposit != null)
        {
            _improvementCard.Visible = true;
            var dep = cell.Deposit;

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = dep.Name;
            }

            if (_featureStatusLabel != null)
            {
                if (dep.IsExploited)
                {
                    _featureStatusLabel.Text = "[Đã khai thác]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", PositiveYieldColor);
                }
                else
                {
                    _featureStatusLabel.Text = "[Chưa khai thác]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", OrangeStatusColor);
                }
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = FormatBundle(dep.BonusYield);
            }

            // Configure compact action button
            if (!dep.IsExploited && cell.OwnerFactionId == 0)
            {
                _pendingImprovementId = dep.RequiredImprovement;
                var bldCfg = GameConfigManager.GetBuildingConfig(_pendingImprovementId);
                string bldName = bldCfg?.Name ?? dep.RequiredImprovement;

                if (_btnBuildLabel != null)
                {
                    _btnBuildLabel.Text = $"Xây {bldName}";
                }

                PopulateCostBadges(_btnBuildCostRow, bldCfg?.Cost ?? ResourceBundle.Zero);

                if (_btnBuildImprovement != null)
                {
                    _btnBuildImprovement.Visible = true;
                }
            }
            else if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
        else if (cell.ConstructedBuilding != null)
        {
            _improvementCard.Visible = true;
            var bld = cell.ConstructedBuilding;

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = bld.Name;
            }

            if (_featureStatusLabel != null)
            {
                _featureStatusLabel.Text = "[Công trình]";
                _featureStatusLabel.AddThemeColorOverride("font_color", PositiveYieldColor);
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = FormatBundle(bld.YieldBonus);
            }

            if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
        else
        {
            _improvementCard.Visible = true;

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = "Địa hình tự nhiên";
            }

            if (_featureStatusLabel != null)
            {
                _featureStatusLabel.Text = "[Không có mỏ]";
                _featureStatusLabel.AddThemeColorOverride("font_color", DimFlavorColor);
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = "Không có sản lượng thưởng";
            }

            if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
    }

    private static void PopulateCostBadges(HBoxContainer? targetRow, in ResourceBundle cost)
    {
        if (targetRow == null) return;

        foreach (Node child in targetRow.GetChildren())
        {
            child.QueueFree();
        }

        if (cost.Food > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Food, cost.Food));
        }
        if (cost.Production > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Production, cost.Production));
        }
        if (cost.Gold > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Gold, cost.Gold));
        }
    }

    private static HBoxContainer CreateMiniCostBadge(ResourceType type, int amount)
    {
        var badge = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        badge.AddThemeConstantOverride("separation", 1);

        var icon = IconManager.CreateResourceIconRect(type, 10);
        icon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        badge.AddChild(icon);

        var lbl = new Label
        {
            Text = $"{amount}",
            MouseFilter = MouseFilterEnum.Ignore
        };
        lbl.AddThemeFontSizeOverride("font_size", 7);
        lbl.AddThemeColorOverride("font_color", GoldCostColor);
        badge.AddChild(lbl);

        return badge;
    }

    private void UpdateRecruitmentSection(HexCell cell)
    {
        if (_recruitSection == null || _recruitGrid == null) return;

        bool isPlayerTerritory = cell.OwnerFactionId == 0;
        _recruitSection.Visible = isPlayerTerritory;

        if (!isPlayerTerritory) return;

        // Clear existing recruit buttons
        foreach (Node child in _recruitGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Dynamically instantiate recruit cards for Player Faction (Faction 0)
        var unitConfigs = GameConfigManager.GetAllUnitConfigs();
        int instantiated = 0;

        foreach (var (_, unit) in unitConfigs)
        {
            if (unit.FactionId != 0) continue;

            var card = CreateRecruitCard(unit);
            _recruitGrid.AddChild(card);
            instantiated++;
        }

        // Fallback if configs were not loaded
        if (instantiated == 0)
        {
            _recruitGrid.AddChild(CreateFallbackRecruitCard("cam_ve_quan", "Cấm Vệ", 30, 50, 25));
            _recruitGrid.AddChild(CreateFallbackRecruitCard("cung_thu", "Cung Thủ", 20, 45, 20));
        }
    }

    private Button CreateRecruitCard(UnitConfig unit)
    {
        var btn = new Button
        {
            CustomMinimumSize = new Vector2(100, 28),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ClipText = false
        };

        ApplyRecruitButtonStyle(btn);

        var content = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 3,
            OffsetTop = 1,
            OffsetRight = -3,
            OffsetBottom = -1
        };
        content.AddThemeConstantOverride("separation", 3);

        // Left: 16x16 px unit icon
        var unitTexture = UnitTextureManager.GetUnitTexture(unit.Id, 0)
            ?? IconManager.GetIcon("recruit");
        var sprite = new TextureRect
        {
            Texture = unitTexture,
            CustomMinimumSize = new Vector2(16, 16),
            Size = new Vector2(16, 16),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = TextureFilterEnum.Nearest,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        content.AddChild(sprite);

        // Right: Name and Cost Row
        var rightCol = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        rightCol.AddThemeConstantOverride("separation", 1);

        var nameLabel = new Label
        {
            Text = unit.Name,
            MouseFilter = MouseFilterEnum.Ignore,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 8);
        nameLabel.AddThemeColorOverride("font_color", OffWhiteColor);
        rightCol.AddChild(nameLabel);

        var costRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        costRow.AddThemeConstantOverride("separation", 2);

        if (unit.Cost.Food > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Food, unit.Cost.Food));
        }
        if (unit.Cost.Production > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Production, unit.Cost.Production));
        }
        if (unit.Cost.Gold > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Gold, unit.Cost.Gold));
        }

        rightCol.AddChild(costRow);
        content.AddChild(rightCol);
        btn.AddChild(content);

        // 1px upward offset on hover for tactile pixel feedback
        btn.MouseEntered += () => content.Position = new Vector2(0, -1);
        btn.MouseExited += () => content.Position = Vector2.Zero;

        string unitId = unit.Id;
        btn.Pressed += () =>
        {
            if (ActiveCell != null)
            {
                EmitSignal(SignalName.RecruitRequested, unitId, ActiveCell.Coords);
            }
        };

        return btn;
    }

    private Button CreateFallbackRecruitCard(string unitId, string unitName, int food, int prod, int gold)
    {
        var dummyCost = new ResourceBundle(food, prod, gold, 0, 0);
        var dummyConfig = new UnitConfig(
            unitId,
            unitName,
            "",
            0,
            20,
            5,
            3,
            3,
            dummyCost,
            ResourceBundle.Zero,
            "★",
            new Color("#2d241e"),
            GoldColor
        );
        return CreateRecruitCard(dummyConfig);
    }

    private static void ApplyRecruitButtonStyle(Button btn)
    {
        var normalStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.1176f, 0.0941f, 0.0784f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.3294f, 0.2353f, 0.0863f, 1f),
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 2,
            ContentMarginBottom = 2
        };

        var hoverStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.1804f, 0.1373f, 0.0863f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = GoldColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 1,
            ContentMarginBottom = 3
        };

        var pressedStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.0706f, 0.0549f, 0.0392f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = DimBronzeColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 3,
            ContentMarginBottom = 1
        };

        btn.AddThemeStyleboxOverride("normal", normalStyle);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        btn.AddThemeStyleboxOverride("pressed", pressedStyle);
        btn.AddThemeStyleboxOverride("focus", hoverStyle);
    }

    private void OnBuildImprovementPressed()
    {
        if (ActiveCell == null || string.IsNullOrEmpty(_pendingImprovementId)) return;
        EmitSignal(SignalName.UpgradeRequested, _pendingImprovementId, ActiveCell.Coords);
    }

    private static string FormatBundle(in ResourceBundle bundle)
    {
        var parts = new List<string>();
        if (bundle.Food > 0) parts.Add($"+{bundle.Food} Lương");
        if (bundle.Production > 0) parts.Add($"+{bundle.Production} Sản xuất");
        if (bundle.Gold > 0) parts.Add($"+{bundle.Gold} Vàng");
        if (bundle.Science > 0) parts.Add($"+{bundle.Science} Khoa học");
        if (bundle.Faith > 0) parts.Add($"+{bundle.Faith} Tín ngưỡng");
        return parts.Count > 0 ? string.Join(", ", parts) : "Không";
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
