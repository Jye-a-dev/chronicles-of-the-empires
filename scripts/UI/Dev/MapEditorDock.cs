using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;
using ChroniclesOfTheEmpires.Gameplay.Editor;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Dev;

/// <summary>
/// Right-dock dev toolbar for in-game map editing, brush configuration, and JSON file exchange.
/// Root node uses Control.MouseFilterEnum.Stop to completely block through-clicks to the grid.
/// </summary>
public partial class MapEditorDock : PanelContainer
{
    public MapEditorController? Controller { get; set; }

    public event Action? ExportRequested;
    public event Action? ImportRequested;
    public event Action? ClearRequested;

    private Label? _hoverInfoLabel;
    private SpinBox? _widthSpinBox;
    private SpinBox? _heightSpinBox;
    private HBoxContainer? _dimRow;
    private Button? _btnToggleDim;
    private OptionButton? _factionOptionButton;
    private TabContainer? _tabContainer;

    private TextureRect? _activeBrushIcon;
    private Label? _activeBrushLabel;

    // Button references
    private Button? _btnPlains;
    private Button? _btnForest;
    private Button? _btnRiver;
    private Button? _btnMountain;

    private Button? _btnDepositCopper;
    private Button? _btnDepositIron;
    private Button? _btnDepositHerbs;
    private Button? _btnDepositJade;

    private Button? _btnImpFarm;
    private Button? _btnImpMine;
    private Button? _btnImpLumber;
    private Button? _btnImpWatchtower;

    private Button? _btnUnitGuard;
    private Button? _btnUnitArcher;
    private Button? _btnUnitVanguard;

    private Button? _btnPaintOwner;

    public int MapWidth => (int)(_widthSpinBox?.Value ?? 32);
    public int MapHeight => (int)(_heightSpinBox?.Value ?? 32);
    public int SelectedFaction => _factionOptionButton != null && _factionOptionButton.Selected >= 0
        ? _factionOptionButton.GetItemId(_factionOptionButton.Selected)
        : 0;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        CustomMinimumSize = new Vector2(165, 0);

        _hoverInfoLabel = GetNodeOrNull<Label>("%HoverInfoLabel");
        _widthSpinBox = GetNodeOrNull<SpinBox>("%WidthSpinBox");
        _heightSpinBox = GetNodeOrNull<SpinBox>("%HeightSpinBox");
        _dimRow = GetNodeOrNull<HBoxContainer>("%DimRow");
        _btnToggleDim = GetNodeOrNull<Button>("%BtnToggleDim");
        _factionOptionButton = GetNodeOrNull<OptionButton>("%FactionOptionButton");
        _tabContainer = GetNodeOrNull<TabContainer>("%TabContainer");

        _activeBrushIcon = GetNodeOrNull<TextureRect>("%ActiveBrushIcon");
        _activeBrushLabel = GetNodeOrNull<Label>("%ActiveBrushLabel");

        _btnPlains = GetNodeOrNull<Button>("%BtnPlains");
        _btnForest = GetNodeOrNull<Button>("%BtnForest");
        _btnRiver = GetNodeOrNull<Button>("%BtnRiver");
        _btnMountain = GetNodeOrNull<Button>("%BtnMountain");

        _btnDepositCopper = GetNodeOrNull<Button>("%BtnDepositCopper");
        _btnDepositIron = GetNodeOrNull<Button>("%BtnDepositIron");
        _btnDepositHerbs = GetNodeOrNull<Button>("%BtnDepositHerbs");
        _btnDepositJade = GetNodeOrNull<Button>("%BtnDepositJade");

        _btnImpFarm = GetNodeOrNull<Button>("%BtnImpFarm");
        _btnImpMine = GetNodeOrNull<Button>("%BtnImpMine");
        _btnImpLumber = GetNodeOrNull<Button>("%BtnImpLumber");
        _btnImpWatchtower = GetNodeOrNull<Button>("%BtnImpWatchtower");

        _btnUnitGuard = GetNodeOrNull<Button>("%BtnUnitGuard");
        _btnUnitArcher = GetNodeOrNull<Button>("%BtnUnitArcher");
        _btnUnitVanguard = GetNodeOrNull<Button>("%BtnUnitVanguard");

        _btnPaintOwner = GetNodeOrNull<Button>("%BtnPaintOwner");

        SetupDimToggle();
        PopulateFactions();
        SetupButtonIcons();
        SetupButtonHandlers();

        // Default initial brush
        SetActiveBrush(UnitTextureManager.GetTerrainTexture(TerrainType.Plains), "Đồng Bằng");
    }

    public void SetMapDimensions(int width, int height)
    {
        if (_widthSpinBox != null) _widthSpinBox.Value = width;
        if (_heightSpinBox != null) _heightSpinBox.Value = height;
    }

    public bool IsMouseOverDock()
    {
        if (!Visible) return false;
        var rect = GetGlobalRect();
        var mouse = GetViewport().GetMousePosition();
        return rect.HasPoint(mouse);
    }

    public void UpdateHoverInfo(Vector2I pos, HexCell? cell)
    {
        if (_hoverInfoLabel == null) return;

        if (cell == null)
        {
            _hoverInfoLabel.Text = $"Hex: ({pos.X}, {pos.Y}) | Ngoài biên";
            return;
        }

        string owner = cell.OwnerFactionId >= 0 ? $"Phe {cell.OwnerFactionId}" : "Vô chủ";
        string extra = cell.Deposit != null ? $" | {cell.Deposit.Name}" : (cell.TerrainData.Improvement != ImprovementType.None ? $" | {cell.TerrainData.Improvement}" : "");
        _hoverInfoLabel.Text = $"Hex: ({pos.X}, {pos.Y}) | {cell.Terrain} | {owner}{extra}";
    }

    public void SetActiveBrush(Texture2D? icon, string displayName)
    {
        if (_activeBrushIcon != null)
        {
            _activeBrushIcon.Texture = icon;
        }
        if (_activeBrushLabel != null)
        {
            _activeBrushLabel.Text = $"Cọ: {displayName}";
        }
    }

    private void SetupDimToggle()
    {
        if (_btnToggleDim != null && _dimRow != null)
        {
            _btnToggleDim.Connect("pressed", Callable.From(() =>
            {
                _dimRow.Visible = !_dimRow.Visible;
                _btnToggleDim.Text = _dimRow.Visible ? "Ẩn W-H" : "Hiện W-H";
            }));
        }
    }

    private void PopulateFactions()
    {
        if (_factionOptionButton == null) return;

        _factionOptionButton.Clear();
        var configs = GameConfigManager.GetAllFactionConfigs();
        var list = new List<FactionConfig>(configs.Values);
        list.Sort((a, b) => a.Id.CompareTo(b.Id));

        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var cfg = list[i];
                _factionOptionButton.AddItem($"[{cfg.Id}] {cfg.Name}", cfg.Id);
            }
        }
        else
        {
            for (int i = 0; i <= 17; i++)
            {
                string name = i == 0 ? "Đại Việt / Lạc Uyên" : $"Thế Lực {i}";
                _factionOptionButton.AddItem($"[{i}] {name}", i);
            }
        }

        _factionOptionButton.Select(0);
        _factionOptionButton.ItemSelected += OnFactionSelected;
    }

    private void OnFactionSelected(long index)
    {
        int factionId = _factionOptionButton != null && index >= 0 ? _factionOptionButton.GetItemId((int)index) : 0;
        if (Controller != null)
        {
            Controller.SelectedFactionId = factionId;
        }

        UpdateUnitButtonIcons();

        // If on Tab Units (Tab index 2) or ActiveCategory is Unit, reload unit sprite into active brush
        bool isUnitTab = _tabContainer != null && _tabContainer.CurrentTab == 2;
        if (isUnitTab || Controller?.ActiveCategory == EditorBrushCategory.Unit)
        {
            string unitType = Controller?.SelectedUnitType ?? "cam_ve_quan";
            var unitTex = UnitTextureManager.GetUnitTexture(unitType, factionId);
            string unitName = GetUnitDisplayName(unitType);
            string factionName = GetFactionName(factionId);
            SetActiveBrush(unitTex, $"{unitName} ({factionName})");
        }
        else if (Controller?.ActiveCategory == EditorBrushCategory.Ownership)
        {
            string factionName = GetFactionName(factionId);
            SetActiveBrush(IconManager.GetIcon("flag"), $"Chủ Quyền: {factionName}");
        }
    }

    private void SetupButtonIcons()
    {
        // Terrain icons
        if (_btnPlains != null) _btnPlains.Icon = UnitTextureManager.GetTerrainTexture(TerrainType.Plains);
        if (_btnForest != null) _btnForest.Icon = UnitTextureManager.GetTerrainTexture(TerrainType.Forest);
        if (_btnRiver != null) _btnRiver.Icon = UnitTextureManager.GetTerrainTexture(TerrainType.River);
        if (_btnMountain != null) _btnMountain.Icon = UnitTextureManager.GetTerrainTexture(TerrainType.Mountain) ?? IconManager.GetIcon("mountain");

        // Deposits icons
        if (_btnDepositCopper != null) _btnDepositCopper.Icon = IconManager.GetIcon("copper");
        if (_btnDepositIron != null) _btnDepositIron.Icon = IconManager.GetIcon("iron");
        if (_btnDepositHerbs != null) _btnDepositHerbs.Icon = IconManager.GetIcon("herbs");
        if (_btnDepositJade != null) _btnDepositJade.Icon = IconManager.GetIcon("jade");

        // Improvements icons
        if (_btnImpFarm != null) _btnImpFarm.Icon = IconManager.GetIcon("farm");
        if (_btnImpMine != null) _btnImpMine.Icon = IconManager.GetIcon("mine");
        if (_btnImpLumber != null) _btnImpLumber.Icon = IconManager.GetIcon("lumber");
        if (_btnImpWatchtower != null) _btnImpWatchtower.Icon = IconManager.GetIcon("watchtower");

        // Units icons
        UpdateUnitButtonIcons();

        // Ownership icon
        if (_btnPaintOwner != null) _btnPaintOwner.Icon = IconManager.GetIcon("flag");
    }

    private void UpdateUnitButtonIcons()
    {
        int factionId = SelectedFaction;
        var guardTex = UnitTextureManager.GetUnitTexture("cam_ve_quan", factionId);
        var archerTex = UnitTextureManager.GetUnitTexture("cung_thu", factionId);
        var vanguardTex = UnitTextureManager.GetUnitTexture("tien_phong_dich", factionId)
                          ?? UnitTextureManager.GetUnitTexture("tien_phong", factionId);

        if (_btnUnitGuard != null) _btnUnitGuard.Icon = guardTex;
        if (_btnUnitArcher != null) _btnUnitArcher.Icon = archerTex;
        if (_btnUnitVanguard != null) _btnUnitVanguard.Icon = vanguardTex;
    }

    private void SetupButtonHandlers()
    {
        // Terrain buttons
        _btnPlains?.Connect("pressed", Callable.From(() => SetTerrain(TerrainType.Plains, "Đồng Bằng")));
        _btnForest?.Connect("pressed", Callable.From(() => SetTerrain(TerrainType.Forest, "Rừng Rậm")));
        _btnRiver?.Connect("pressed", Callable.From(() => SetTerrain(TerrainType.River, "Sông Nước")));
        _btnMountain?.Connect("pressed", Callable.From(() => SetTerrain(TerrainType.Mountain, "Núi Đá")));

        // Brush Shapes
        GetNodeOrNull<Button>("%BtnShapeSingle")?.Connect("pressed", Callable.From(() => SetShape(BrushShape.Single)));
        GetNodeOrNull<Button>("%BtnShapeCluster")?.Connect("pressed", Callable.From(() => SetShape(BrushShape.Cluster)));
        GetNodeOrNull<Button>("%BtnShapeFlood")?.Connect("pressed", Callable.From(() => SetShape(BrushShape.FloodFill)));

        // Deposits
        _btnDepositCopper?.Connect("pressed", Callable.From(() => SetDeposit("copper", "Mỏ Đồng")));
        _btnDepositIron?.Connect("pressed", Callable.From(() => SetDeposit("iron", "Mỏ Sắt")));
        _btnDepositHerbs?.Connect("pressed", Callable.From(() => SetDeposit("herbs", "Thảo Dược")));
        _btnDepositJade?.Connect("pressed", Callable.From(() => SetDeposit("jade", "Ngọc Bích")));

        // Improvements
        _btnImpFarm?.Connect("pressed", Callable.From(() => SetImprovement(ImprovementType.Farm, "Nông Trại")));
        _btnImpMine?.Connect("pressed", Callable.From(() => SetImprovement(ImprovementType.Mine, "Mỏ Quặng")));
        _btnImpLumber?.Connect("pressed", Callable.From(() => SetImprovement(ImprovementType.LumberMill, "Trại Gỗ")));
        _btnImpWatchtower?.Connect("pressed", Callable.From(() => SetImprovement(ImprovementType.Watchtower, "Tiêu Đồn")));

        // Units
        _btnUnitGuard?.Connect("pressed", Callable.From(() => SetUnit("cam_ve_quan", "Cấm Vệ Quân")));
        _btnUnitArcher?.Connect("pressed", Callable.From(() => SetUnit("cung_thu", "Cung Thủ")));
        _btnUnitVanguard?.Connect("pressed", Callable.From(() => SetUnit("tien_phong_dich", "Tiên Phong Địch")));

        // Ownership
        _btnPaintOwner?.Connect("pressed", Callable.From(SetOwnership));

        // Top controls
        GetNodeOrNull<Button>("%BtnClearMap")?.Connect("pressed", Callable.From(() => ClearRequested?.Invoke()));
        GetNodeOrNull<Button>("%BtnExportJson")?.Connect("pressed", Callable.From(() => ExportRequested?.Invoke()));
        GetNodeOrNull<Button>("%BtnImportJson")?.Connect("pressed", Callable.From(() => ImportRequested?.Invoke()));
    }

    private void SetTerrain(TerrainType terrain, string displayName)
    {
        if (Controller == null) return;
        Controller.ActiveCategory = EditorBrushCategory.Terrain;
        Controller.SelectedTerrain = terrain;
        var icon = terrain == TerrainType.Mountain
            ? (UnitTextureManager.GetTerrainTexture(terrain) ?? IconManager.GetIcon("mountain"))
            : UnitTextureManager.GetTerrainTexture(terrain);
        SetActiveBrush(icon, displayName);
    }

    private void SetShape(BrushShape shape)
    {
        if (Controller == null) return;
        Controller.ActiveShape = shape;
    }

    private void SetDeposit(string depId, string displayName)
    {
        if (Controller == null) return;
        Controller.ActiveCategory = EditorBrushCategory.Deposit;
        Controller.SelectedDepositId = depId;
        SetActiveBrush(IconManager.GetIcon(depId), displayName);
    }

    private void SetImprovement(ImprovementType imp, string displayName)
    {
        if (Controller == null) return;
        Controller.ActiveCategory = EditorBrushCategory.Improvement;
        Controller.SelectedImprovement = imp;
        string key = imp switch
        {
            ImprovementType.Farm => "farm",
            ImprovementType.Mine => "mine",
            ImprovementType.LumberMill => "lumber",
            ImprovementType.Watchtower => "watchtower",
            _ => "improvement"
        };
        SetActiveBrush(IconManager.GetIcon(key), displayName);
    }

    private void SetUnit(string unitType, string displayName)
    {
        if (Controller == null) return;
        Controller.ActiveCategory = EditorBrushCategory.Unit;
        Controller.SelectedUnitType = unitType;
        Controller.SelectedFactionId = SelectedFaction;
        var texture = UnitTextureManager.GetUnitTexture(unitType, SelectedFaction);
        string factionName = GetFactionName(SelectedFaction);
        SetActiveBrush(texture, $"{displayName} ({factionName})");
    }

    private void SetOwnership()
    {
        if (Controller == null) return;
        Controller.ActiveCategory = EditorBrushCategory.Ownership;
        Controller.SelectedFactionId = SelectedFaction;
        string factionName = GetFactionName(SelectedFaction);
        SetActiveBrush(IconManager.GetIcon("flag"), $"Chủ Quyền ({factionName})");
    }

    private static string GetUnitDisplayName(string unitType) => unitType switch
    {
        "cam_ve_quan" => "Cấm Vệ Quân",
        "cung_thu" => "Cung Thủ",
        "tien_phong_dich" or "tien_phong" => "Tiên Phong Địch",
        _ => unitType
    };

    private static string GetFactionName(int factionId)
    {
        var cfg = GameConfigManager.GetFactionConfig(factionId);
        return cfg?.Name ?? $"Phe {factionId}";
    }
}
