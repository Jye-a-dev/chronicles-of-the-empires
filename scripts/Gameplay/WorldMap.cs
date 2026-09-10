using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.AI;
using ChroniclesOfTheEmpires.Gameplay.Economy;
using ChroniclesOfTheEmpires.Gameplay.Editor;
using ChroniclesOfTheEmpires.UI.Components;
using ChroniclesOfTheEmpires.UI.Controllers;
using ChroniclesOfTheEmpires.UI.Dev;
using ChroniclesOfTheEmpires.UI.Modals;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Master tactical world map coordinator and orchestrator.
/// Orchestrates GridMapManager, PathfindingManager, UnitRegistry, MapInputHandler, RTSCamera2D,
/// TurnManager, EconomyManager, FogOfWarManager, SimpleAITurnExecutor, and TacticalHUD.
/// Movement, combat, spawning, and turn routines are modularized into partial components.
/// </summary>
public partial class WorldMap : Node2D
{
    public MapInteractionState State { get; private set; } = MapInteractionState.Idle;

    private GridMapManager _gridMapManager = null!;
    private PathfindingManager _pathfindingManager = null!;
    private PathVisualizer _pathVisualizer = null!;
    private HexSelectionIndicator _hexIndicator = null!;
    private RTSCamera2D _camera = null!;
    private TurnManager _turnManager = null!;
    private Node2D _unitContainer = null!;
    private TacticalHUD _hud = null!;
    private EconomyManager _economyManager = null!;
    private EconomyHUDController _economyHUDController = null!;
    private FactionData _playerFaction = null!;
    private FogOfWarManager _fogOfWarManager = null!;
    public FogOfWarManager FogOfWarManager => _fogOfWarManager;
    private readonly SimpleAITurnExecutor _aiTurnExecutor = new();

    private BoardBackdrop? _boardBackdrop;
    private Line2D? _hoverIndicator;
    private EndGameModal? _endGameModal;
    private int _enemiesEliminated = 0;

    private readonly UnitRegistry _unitRegistry = new();
    private readonly AsyncPacer _asyncPacer = new();
    private MapInputHandler _inputHandler = null!;
    private UnitController? _selectedUnit;

    private MapEditorController? _mapEditorController;
    private MapEditorDock? _mapEditorDock;

    public UnitRegistry UnitRegistry => _unitRegistry;

    public override void _Ready()
    {
        // 1. Resolve Node References
        _gridMapManager = GetNode<GridMapManager>("WorldRoot/GridMapManager");
        _boardBackdrop = GetNodeOrNull<BoardBackdrop>("WorldRoot/BoardBackdrop");
        _hoverIndicator = GetNodeOrNull<Line2D>("WorldRoot/HexHoverIndicator");
        _pathVisualizer = GetNode<PathVisualizer>("WorldRoot/PathVisualizer");
        _hexIndicator = GetNode<HexSelectionIndicator>("WorldRoot/HexSelectionIndicator");
        _camera = GetNode<RTSCamera2D>("WorldRoot/RTSCamera2D");
        _turnManager = GetNode<TurnManager>("TurnManager");
        _unitContainer = GetNode<Node2D>("WorldRoot/UnitContainer");
        _hud = GetNode<TacticalHUD>("UILayer/HUD");

        // 2. Initialize Tactical Grid and Dimensions from GameSession
        var session = ChroniclesOfTheEmpires.UI.GameSession.ActiveConfig;
        _gridMapManager.InitializeFromSession(session.Mode, session.StageId, session.MapSize, session.Biome);

        // 3. Initialize Hexagonal AStar2D Pathfinding
        _pathfindingManager = new PathfindingManager();
        _pathfindingManager.Initialize(
            _gridMapManager.MapWidth,
            _gridMapManager.MapHeight,
            _gridMapManager
        );
        _pathfindingManager.SyncWithMap(_gridMapManager);

        // 4. Setup Camera Boundaries and Tactical Table Backdrop
        Vector2 maxCorner = GridMapManager.GridToWorldCenter(
            new Vector2I(_gridMapManager.MapWidth - 1, _gridMapManager.MapHeight - 1)
        );
        int mapPixelW = (int)maxCorner.X + GridMapManager.CellDimension * 2;
        int mapPixelH = (int)maxCorner.Y + GridMapManager.CellDimension * 2;
        _camera.SetBounds(mapPixelW, mapPixelH);
        _boardBackdrop?.SetDimensions(mapPixelW, mapPixelH);

        // 5. Setup Fog of War Manager directly inside WorldRoot
        _fogOfWarManager = new FogOfWarManager { Name = "FogOfWarManager" };
        GetNode<Node2D>("WorldRoot").AddChild(_fogOfWarManager);
        _fogOfWarManager.Initialize(_gridMapManager.MapWidth, _gridMapManager.MapHeight, _gridMapManager);

        // 6. Setup Economy Engine & Faction State
        _economyManager = new EconomyManager { Name = "EconomyManager" };
        AddChild(_economyManager);

        _economyHUDController = new EconomyHUDController { Name = "EconomyHUDController" };
        AddChild(_economyHUDController);

        _economyManager.SetTileDataProvider(pos => _gridMapManager.GetTileTerrainData(pos));

        var playerCfg = GameConfigManager.GetFactionConfig(0);
        _playerFaction = new FactionData
        {
            FactionId = 0,
            Name = playerCfg?.Name ?? "Đại Việt Hoàng Triều",
            CulturalSphere = playerCfg?.CulturalSphere ?? "EastAsian",
            Treasury = playerCfg?.StartingTreasury ?? new ResourceBundle(150, 80, 120, 20, 30)
        };

        for (int x = 0; x <= 4; x++)
        {
            for (int y = 0; y <= 4; y++)
            {
                var coord = new Vector2I(x, y);
                _playerFaction.ControlledTiles.Add(coord);
                var cell = _gridMapManager.GetCell(coord);
                if (cell != null) cell.OwnerFactionId = 0;
                _gridMapManager.RefreshTileOwnerVisual(coord, 0);
            }
        }
        _economyManager.RegisterFaction(_playerFaction);

        foreach (var (_, cfg) in GameConfigManager.GetAllFactionConfigs())
        {
            if (cfg.Id != 0)
            {
                var rival = new FactionData
                {
                    FactionId = cfg.Id,
                    Name = cfg.Name,
                    CulturalSphere = cfg.CulturalSphere,
                    Treasury = cfg.StartingTreasury
                };
                _economyManager.RegisterFaction(rival);
            }
        }

        _economyHUDController.Bind(_economyManager, _hud, _turnManager);
        _turnManager.Initialize(_playerFaction, _economyManager, _unitRegistry);

        // Verification: Audit unit sprite assets across all 18 factions (0..17)
        UnitTextureManager.ValidateFactionAssets(17);

        // 7. Initialize MapInputHandler
        _inputHandler = new MapInputHandler { Name = "MapInputHandler" };
        AddChild(_inputHandler);
        _inputHandler.Initialize(
            _gridMapManager,
            _unitRegistry,
            _pathfindingManager,
            _pathVisualizer,
            _hoverIndicator,
            () => State,
            () => _selectedUnit,
            () => _hud.IsSettingsOpen,
            () => _hud.CloseSettings()
        );

        _inputHandler.CellSelected += OnCellSelected;
        _inputHandler.UnitSelected += SelectUnit;
        _inputHandler.UnitMoveRequested += (u, grid, cell) => ExecuteSafeMove(u, grid, cell);
        _inputHandler.UnitAttackRequested += HandleCombatTarget;
        _inputHandler.UnitRecaptureRequested += ExecuteRecaptureOrder;
        _inputHandler.DeselectRequested += DeselectAll;
        _inputHandler.EndTurnRequested += OnEndTurnPressed;

        // 8. Initialize Map Editor Controller & Dev Dock
        _mapEditorController = new MapEditorController(this, _gridMapManager, _unitRegistry, _pathfindingManager);
        var dockScene = GD.Load<PackedScene>("res://scenes/dev/map_editor_dock.tscn");
        if (dockScene != null)
        {
            _mapEditorDock = dockScene.Instantiate<MapEditorDock>();
            _mapEditorDock.Controller = _mapEditorController;
            _mapEditorDock.SetMapDimensions(_gridMapManager.MapWidth, _gridMapManager.MapHeight);
            _mapEditorDock.Visible = false;

            var devLayer = new CanvasLayer { Name = "MapEditorLayer", Layer = 22 };
            AddChild(devLayer);
            devLayer.AddChild(_mapEditorDock);

            _mapEditorDock.ExportRequested += () =>
            {
                MapSerializer.ExportToJson(_gridMapManager, _unitRegistry, _mapEditorDock.MapWidth, _mapEditorDock.MapHeight, _gridMapManager.ActiveBiome, "Stage 1", "res://data/maps/custom_stage.json");
            };

            _mapEditorDock.ImportRequested += () =>
            {
                MapSerializer.LoadFromJson("res://data/maps/custom_stage.json", this, _gridMapManager, _unitRegistry, _pathfindingManager);
                _mapEditorDock.SetMapDimensions(_gridMapManager.MapWidth, _gridMapManager.MapHeight);
            };

            _mapEditorDock.ClearRequested += () =>
            {
                ClearAllUnits();
                _gridMapManager.ResetCells(_mapEditorDock.MapWidth, _mapEditorDock.MapHeight);
                _pathfindingManager.ResetGraph(_mapEditorDock.MapWidth, _mapEditorDock.MapHeight, _gridMapManager);
                _fogOfWarManager.Initialize(_mapEditorDock.MapWidth, _mapEditorDock.MapHeight, _gridMapManager);
                RefreshEconomyUI();
            };
        }

        _inputHandler.IsBlockingUIHovered = () => _hud.IsSettingsOpen || (_mapEditorDock != null && _mapEditorDock.IsMouseOverDock());
        _inputHandler.IsEditorActive = () => _mapEditorController?.IsEditorActive ?? false;
        _inputHandler.OnEditorPaint = (pos) => _mapEditorController?.ApplyPaint(pos);
        _inputHandler.OnEditorErase = (pos) => _mapEditorController?.ApplyErase(pos);
        _inputHandler.TileHovered += (pos) => _mapEditorDock?.UpdateHoverInfo(pos, _gridMapManager.GetCell(pos));

        // 9. Spawn Initial Units
        SpawnInitialUnits();

        // 10. Connect UI Events & Turn Manager
        _hud.Initialize(session.StageTitle);
        _hud.EndTurnRequested += OnEndTurnPressed;
        _hud.ExitToMenuRequested += OnExitToMenu;
        _hud.RecruitRequested += (unitId, coords) => ExecuteRecruitUnit(_playerFaction, unitId, coords);
        _hud.UpgradeRequested += OnTileUpgradeRequested;

        _turnManager.TurnChanged += OnTurnChanged;

        // 11. Initial UI Refresh & Vision illumination
        RefreshEconomyUI();
        _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
    }

    public override void _ExitTree()
    {
        _asyncPacer.Cancel();
        _asyncPacer.Dispose();
        _unitRegistry.Clear();
    }

    private void OnExitToMenu()
    {
        _asyncPacer.Cancel();
        GetTree().ChangeSceneToFile("res://scenes/ui/screens/main_menu.tscn");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.F1 || key.Keycode == Key.Quoteleft)
            {
                ToggleMapEditor();
            }
            else if (key.Keycode == Key.F5)
            {
                SetPlaytestMode(true);
            }
            else if (key.Keycode == Key.F6)
            {
                SetPlaytestMode(false);
            }
        }
    }

    private void ToggleMapEditor()
    {
        if (_mapEditorDock == null) return;
        _mapEditorDock.Visible = !_mapEditorDock.Visible;
        if (_mapEditorController != null)
        {
            _mapEditorController.IsEditorActive = _mapEditorDock.Visible;
            if (_mapEditorDock.Visible)
            {
                _fogOfWarManager.RevealAll(_unitRegistry.AllUnits);
            }
            else
            {
                _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
            }
        }
    }

    public void SetPlaytestMode(bool isPlaytest)
    {
        if (_mapEditorDock != null)
        {
            _mapEditorDock.Visible = !isPlaytest;
        }

        if (_mapEditorController != null)
        {
            _mapEditorController.IsEditorActive = !isPlaytest;
        }

        if (isPlaytest)
        {
            State = MapInteractionState.Idle;
            _fogOfWarManager.ResetFog(_gridMapManager);
            _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
        }
        else
        {
            _fogOfWarManager.RevealAll(_unitRegistry.AllUnits);
        }
    }

    private UnitController? FindUnitByNumericId(int unitId)
    {
        var all = _unitRegistry.AllUnits;
        for (int i = 0; i < all.Count; i++)
        {
            var u = all[i];
            if (IsInstanceValid(u) && u.Data.UnitId == unitId)
            {
                return u;
            }
        }
        return null;
    }

    public FactionData? FindFactionData(int factionId)
    {
        if (factionId == 0) return _playerFaction;
        var factions = _economyManager.Factions;
        for (int i = 0; i < factions.Count; i++)
        {
            if (factions[i].FactionId == factionId) return factions[i];
        }
        return null;
    }

    private Vector2I FindWalkableCell(Vector2I preferred)
    {
        if (_gridMapManager.IsWithinBounds(preferred) && !_pathfindingManager.IsPointSolid(preferred))
        {
            return preferred;
        }

        for (int r = 1; r < 10; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    var testPos = new Vector2I(preferred.X + dx, preferred.Y + dy);
                    if (_gridMapManager.IsWithinBounds(testPos) && !_pathfindingManager.IsPointSolid(testPos))
                    {
                        return testPos;
                    }
                }
            }
        }
        return new Vector2I(1, 1);
    }
}
