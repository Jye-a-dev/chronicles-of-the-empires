using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;
using ChroniclesOfTheEmpires.Core.Combat;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.Entities;
using ChroniclesOfTheEmpires.Core.Game;
using ChroniclesOfTheEmpires.Core.Mathematics;
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
/// Strictly decoupled via domain services, zero-allocation enumerators, and MapInteractionState authority.
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

    public BattlePayload? RequestCombatAnalysis(UnitController attacker, UnitController defender)
    {
        int distance = _gridMapManager.GetNeighbors(attacker.GridPosition).Contains(defender.GridPosition) ? 1 : 2;
        if (distance == 1)
        {
            return BuildBattlePayload(attacker, defender);
        }
        return null;
    }

    public async void ExecuteSafeMove(UnitController unit, Vector2I targetGrid, Action? onComplete = null)
    {
        var targetCell = _gridMapManager.GetCell(targetGrid);
        if (targetCell != null)
        {
            await ExecuteSafeMoveAsync(unit, targetGrid, targetCell);
            onComplete?.Invoke();
        }
    }

    private void OnCellSelected(Vector2I gridPos, HexCell? hexCell)
    {
        DeselectUnitOnly();
        _pathVisualizer.ClearPath();
        if (hexCell != null)
        {
            _hexIndicator.SelectHex(hexCell.WorldPosition);
            _economyHUDController.OnTileSelected(hexCell);
        }
    }

    public void SelectUnit(UnitController unit)
    {
        if (_selectedUnit != null && _selectedUnit != unit)
        {
            _selectedUnit.SetSelected(false);
        }

        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);

        _hexIndicator.SelectHex(unit.GlobalPosition);
        _hud.DisplayUnit(unit);
    }

    private void DeselectUnitOnly()
    {
        if (_selectedUnit != null)
        {
            _selectedUnit.SetSelected(false);
            _selectedUnit = null;
        }
    }

    public void DeselectAll()
    {
        DeselectUnitOnly();
        _hexIndicator.ClearSelection();
        _pathVisualizer.ClearPath();
        _hud.DeselectAll();
    }

    public async void ExecuteSafeMove(UnitController unit, Vector2I targetGrid, HexCell targetCell, Action? onComplete = null)
    {
        await ExecuteSafeMoveAsync(unit, targetGrid, targetCell);
        onComplete?.Invoke();
    }

    public async Task<bool> ExecuteSafeMoveAsync(UnitController unit, Vector2I targetGrid, HexCell targetCell)
    {
        if (State != MapInteractionState.Idle && State != MapInteractionState.AITurnProcessing) return false;
        if (unit.IsMoving || unit.Data.IsMoving) return false;

        _pathfindingManager.SetPointSolid(unit.GridPosition, false);
        var path = _pathfindingManager.FindPath(unit.GridPosition, targetGrid);

        if (path.Length <= 1)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            return false;
        }

        int cost = _pathfindingManager.CalculatePathCost(path, _gridMapManager);
        if (cost > unit.MovementRangeRemaining)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            return false;
        }

        // Re-lock origin until transactional commit
        _pathfindingManager.SetPointSolid(unit.GridPosition, true);
        _pathVisualizer.ClearPath();

        return await ExecuteSafeMoveTransactionalAsync(unit, targetGrid, path, cost);
    }

    /// <summary>
    /// Executes transactional movement: Pre-locks destination, preserves origin obstacle until arrival,
    /// and uses a 5-second safety timeout to eliminate softlocks.
    /// </summary>
    public async Task<bool> ExecuteSafeMoveTransactionalAsync(
        UnitController unit,
        Vector2I targetGrid,
        Vector2I[] path,
        int totalCost)
    {
        if (State != MapInteractionState.Idle && State != MapInteractionState.AITurnProcessing)
            return false;
        if (unit.IsMoving || unit.Data.IsMoving)
            return false;

        Vector2I originGrid = unit.Data.GridPosition;

        // 1. Transactional Pre-Lock: Lock target destination, keep origin solid during transit
        _pathfindingManager.SetPointSolid(targetGrid, true);
        unit.Data.IsMoving = true;
        unit.Data.MovementRemaining = Math.Max(0, unit.Data.MovementRemaining - totalCost);

        bool wasIdle = State == MapInteractionState.Idle;
        if (wasIdle) State = MapInteractionState.UnitMoving;

        var tcs = new TaskCompletionSource<bool>();
        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5.0)); // Anti-softlock guard

        // 2. Trigger step-by-step movement animation
        unit.MoveAlongPath(path.ToArray(), () => tcs.TrySetResult(true));

        using (cts.Token.Register(() => tcs.TrySetResult(false)))
        {
            bool success = await tcs.Task;

            // 3. Commit Phase: Release origin cell obstacle, transfer occupancy and logic coordinates
            _pathfindingManager.SetPointSolid(originGrid, false);

            var originCell = _gridMapManager.GetCell(originGrid);
            if (originCell != null && originCell.OccupyingUnit == unit)
            {
                originCell.OccupyingUnit = null;
            }

            var targetCell = _gridMapManager.GetCell(targetGrid);
            if (targetCell != null)
            {
                targetCell.OccupyingUnit = unit;
            }

            unit.Data.GridPosition = targetGrid;
            unit.Data.IsMoving = false;
            _unitRegistry.UpdatePosition(unit, originGrid, targetGrid);

            if (!success)
            {
                // Fallback emergency snap if tween was aborted or timed out
                unit.Position = GridMapManager.GridToWorldCenter(targetGrid);
            }

            _hexIndicator.SelectHex(unit.GlobalPosition);
            _hud.RefreshUnitInfo();

            ExecuteClaimTile(unit.FactionId, targetGrid);

            if (unit.FactionId == 0)
            {
                _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
            }

            if (wasIdle) State = MapInteractionState.Idle;
            return success;
        }
    }

    private void HandleCombatTarget(UnitController attacker, UnitController defender, Vector2I targetGrid)
    {
        if (attacker.MovementRangeRemaining <= 0) return;

        bool inRange = CombatResolver.IsWithinAttackRange(
            attacker.GridPosition,
            targetGrid,
            attacker.Data.IsRanged ? attacker.Data.AttackRange : 1,
            _gridMapManager
        );

        if (inRange)
        {
            ExecuteCombatResolution(attacker, defender);
            return;
        }

        if (!attacker.Data.IsRanged)
        {
            var neighbors = _gridMapManager.GetNeighbors(targetGrid);
            Vector2I[] bestPath = Array.Empty<Vector2I>();
            int lowestCost = int.MaxValue;

            _pathfindingManager.SetPointSolid(attacker.GridPosition, false);
            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (!_gridMapManager.IsWithinBounds(neighbor) || _pathfindingManager.IsPointSolid(neighbor)) continue;

                var testPath = _pathfindingManager.FindPath(attacker.GridPosition, neighbor);
                if (testPath.Length > 1)
                {
                    int cost = _pathfindingManager.CalculatePathCost(testPath, _gridMapManager);
                    if (cost < lowestCost && cost <= attacker.MovementRangeRemaining)
                    {
                        lowestCost = cost;
                        bestPath = testPath;
                    }
                }
            }
            _pathfindingManager.SetPointSolid(attacker.GridPosition, true);

            if (bestPath.Length > 1)
            {
                Vector2I moveDest = bestPath[^1];
                var destCell = _gridMapManager.GetCell(moveDest);
                if (destCell != null)
                {
                    ExecuteSafeMove(attacker, moveDest, destCell, () =>
                    {
                        if (IsInstanceValid(defender) && defender.HpCurrent > 0)
                        {
                            ExecuteCombatResolution(attacker, defender);
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Executes sequential combat resolution via CombatResolver domain service.
    /// WorldMap triggers visual feedback and refreshes UI.
    /// </summary>
    public void ExecuteCombatResolution(UnitController attacker, UnitController defender)
    {
        if (attacker.IsMoving || defender.IsMoving) return;
        bool wasIdle = State == MapInteractionState.Idle;
        if (wasIdle) State = MapInteractionState.CombatResolving;

        int distance = _gridMapManager.GetNeighbors(attacker.GridPosition).Contains(defender.GridPosition) ? 1 : 2;
        var result = CombatResolver.ResolveCombat(attacker.Data, defender.Data, distance);

        if (result.Success)
        {
            _hud.RefreshUnitInfo();
            RefreshEconomyUI();

            if (result.DefenderDied && !attacker.Data.IsRanged)
            {
                ExecuteClaimTile(attacker.FactionId, defender.GridPosition);
            }
        }

        if (wasIdle) State = MapInteractionState.Idle;
        EvaluateAndTriggerGameOver();
    }

    public void ExecuteRecaptureOrder(UnitController actor, UnitController target)
    {
        if (actor.IsMoving || target.IsMoving || !target.IsSurrendered) return;

        bool isOriginalOwner = _playerFaction.FactionId == target.OriginalFactionId;
        var cost = isOriginalOwner
            ? new ResourceBundle(15, 0, 10, 0, 0)
            : new ResourceBundle(0, 0, 10, 0, 0);

        if (!_playerFaction.Treasury.HasEnough(cost)) return;

        _playerFaction.Treasury -= cost;

        var oldOwner = FindFactionData(target.FactionId);
        target.Data.Recapture(_playerFaction.FactionId, isOriginalOwner ? 40 : 30);
        _unitRegistry.ChangeFaction(target, target.FactionId, _playerFaction.FactionId, oldOwner, _playerFaction);

        actor.Data.MovementRemaining = Math.Max(0, actor.Data.MovementRemaining - 1);

        RefreshEconomyUI();
        _hud.DisplayUnit(target);
    }

    private void SpawnInitialUnits()
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return;

        Vector2I p1Pos = FindWalkableCell(new Vector2I(2, 2));
        Vector2I p2Pos = FindWalkableCell(new Vector2I(3, 3));
        Vector2I rivalPos = FindWalkableCell(new Vector2I(_gridMapManager.MapWidth - 4, _gridMapManager.MapHeight - 4));

        SpawnUnit(unitScene, "cam_ve_quan", 0, p1Pos);
        SpawnUnit(unitScene, "cung_thu", 0, p2Pos);
        SpawnUnit(unitScene, "tien_phong_dich", 1, rivalPos);

        _camera.Position = GridMapManager.GridToWorldCenter(p1Pos);
    }

    private void SpawnUnit(PackedScene unitScene, string configId, int factionId, Vector2I gridPos)
    {
        var uCfg = GameConfigManager.GetUnitConfig(configId);
        bool isRanged = configId.Contains("cung_thu", StringComparison.OrdinalIgnoreCase);

        var data = new UnitData(
            id: configId,
            name: uCfg?.Name ?? "Chiến Binh",
            factionId: factionId,
            hpMax: uCfg?.HpMax ?? 20,
            attack: uCfg?.Attack ?? 6,
            defense: uCfg?.Defense ?? 3,
            movementMax: uCfg?.MovementMax ?? 4,
            gridPosition: gridPos,
            upkeep: uCfg?.Upkeep ?? ResourceBundle.Zero,
            cost: uCfg?.Cost ?? ResourceBundle.Zero,
            description: uCfg?.Description ?? "",
            isRanged: isRanged,
            attackRange: isRanged ? 2 : 1
        );

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);
        RegisterUnit(controller, FindFactionData(factionId));
    }

    public UnitController? ExecuteRecruitUnit(FactionData faction, string unitTypeId, Vector2I targetTile)
    {
        if (!RecruitmentManager.CanRecruitUnit(faction, unitTypeId, targetTile, _gridMapManager, _pathfindingManager, out Vector2I spawnTile))
        {
            return null;
        }

        var data = RecruitmentManager.CreateRecruitData(faction, unitTypeId, spawnTile);
        if (data == null) return null;

        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return null;

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);

        RegisterUnit(controller, faction);
        RefreshEconomyUI();

        if (faction.FactionId == 0)
        {
            _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
        }

        return controller;
    }

    private void RegisterUnit(UnitController unit, FactionData? faction)
    {
        _unitRegistry.Register(unit, faction);
        _pathfindingManager.SetPointSolid(unit.GridPosition, true);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell != null) cell.OccupyingUnit = unit;

        unit.UnitDestroyed += HandleUnitDestroyed;
        unit.UnitClicked += SelectUnit;
        unit.UnitMoved += HandleUnitMovedVisualSync;
    }

    private void HandleUnitMovedVisualSync(UnitController u, Vector2I oldPos, Vector2I newPos)
    {
        _hexIndicator.SelectHex(u.GlobalPosition);
        _hud.RefreshUnitInfo();
    }

    private void HandleUnitDestroyed(UnitController unit)
    {
        unit.UnitDestroyed -= HandleUnitDestroyed;
        unit.UnitClicked -= SelectUnit;
        unit.UnitMoved -= HandleUnitMovedVisualSync;

        if (unit.FactionId != 0)
        {
            _enemiesEliminated++;
        }

        UnitLifecycleManager.TerminateUnit(unit, _unitRegistry, _pathfindingManager, _gridMapManager, _economyManager);

        if (_selectedUnit == unit)
        {
            DeselectAll();
        }

        EvaluateAndTriggerGameOver();
    }

    public void ClearAllUnits()
    {
        var units = _unitRegistry.AllUnits.ToArray();
        for (int i = 0; i < units.Length; i++)
        {
            var u = units[i];
            if (GodotObject.IsInstanceValid(u))
            {
                u.UnitDestroyed -= HandleUnitDestroyed;
                u.UnitClicked -= SelectUnit;
                u.UnitMoved -= HandleUnitMovedVisualSync;
                UnitLifecycleManager.TerminateUnit(u, _unitRegistry, _pathfindingManager, _gridMapManager, _economyManager);
            }
        }
        _unitRegistry.Clear();
    }

    public UnitController? SpawnCustomUnit(string configId, int factionId, Vector2I gridPos, int hp = -1)
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return null;

        var uCfg = GameConfigManager.GetUnitConfig(configId);
        bool isRanged = configId.Contains("cung_thu", StringComparison.OrdinalIgnoreCase);

        int maxHp = uCfg?.HpMax ?? 20;
        var data = new UnitData(
            id: configId,
            name: uCfg?.Name ?? "Chiến Binh",
            factionId: factionId,
            hpMax: maxHp,
            attack: uCfg?.Attack ?? 6,
            defense: uCfg?.Defense ?? 3,
            movementMax: uCfg?.MovementMax ?? 4,
            gridPosition: gridPos,
            upkeep: uCfg?.Upkeep ?? ResourceBundle.Zero,
            cost: uCfg?.Cost ?? ResourceBundle.Zero,
            description: uCfg?.Description ?? "",
            isRanged: isRanged,
            attackRange: isRanged ? 2 : 1
        );

        if (hp > 0 && hp < maxHp)
        {
            data.ApplyDamage(maxHp - hp);
        }

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);
        RegisterUnit(controller, FindFactionData(factionId));
        return controller;
    }

    private void OnTileUpgradeRequested(string improvementId, Vector2I coords)
    {
        var cell = _gridMapManager.GetCell(coords);
        if (cell == null || cell.OwnerFactionId != 0) return;

        ResourceBundle cost;
        ImprovementType impType;
        ResourceBundle bonusYield;
        int buildTurns = 2;

        if (cell.Deposit != null && !cell.Deposit.IsExploited)
        {
            var bldCfg = GameConfigManager.GetBuildingConfig(cell.Deposit.RequiredImprovement);
            cost = bldCfg?.Cost ?? new ResourceBundle(0, 30, 20, 0, 0);
            impType = cell.Deposit.Category switch
            {
                DepositCategory.Mineral => ImprovementType.Mine,
                DepositCategory.Agricultural => ImprovementType.Farm,
                _ => ImprovementType.Mine
            };
            bonusYield = cell.Deposit.BonusYield;
        }
        else
        {
            var (defaultImp, _, impCost) = TileInfoModal.GetDefaultImprovementForTile(cell.TerrainData.Biome);
            impType = defaultImp;
            cost = impCost;
            bonusYield = impType switch
            {
                ImprovementType.Farm => new ResourceBundle(2, 0, 0, 0, 0),
                ImprovementType.LumberMill => new ResourceBundle(0, 2, 0, 0, 0),
                ImprovementType.Mine => new ResourceBundle(0, 2, 1, 0, 0),
                ImprovementType.Watchtower => new ResourceBundle(0, 0, 0, 0, 1),
                _ => ResourceBundle.Zero
            };
        }

        if (!_playerFaction.Treasury.HasEnough(cost))
        {
            return;
        }

        _playerFaction.Treasury -= cost;
        cell.TerrainData.Improvement = impType;
        cell.TerrainData.ImprovementBonusYield = bonusYield;
        cell.TerrainData.ConstructionTurnsRemaining = buildTurns;
        cell.TerrainData.IsConstructed = false;

        if (cell.Deposit != null)
        {
            cell.Deposit.IsExploited = true;
        }

        RefreshEconomyUI();
        _hud.DisplayTile(cell);
    }

    private async void OnEndTurnPressed()
    {
        if (State != MapInteractionState.Idle) return;

        State = MapInteractionState.AITurnProcessing;
        _hud.SetButtonsDisabled(true);

        try
        {
            // 1. Process Surrendered Units: 0-allocation hex scan for rival defections
            var allUnits = _unitRegistry.AllUnits;
            for (int i = allUnits.Count - 1; i >= 0; i--)
            {
                var unit = allUnits[i];
                if (!IsInstanceValid(unit) || !unit.IsSurrendered) continue;

                unit.Data.SurrenderTurnsRemaining--;
                if (unit.Data.SurrenderTurnsRemaining <= 0)
                {
                    UnitController? rivalNeighbor = null;
                    var neighbors = _gridMapManager.GetNeighbors(unit.GridPosition);
                    for (int n = 0; n < neighbors.Count; n++)
                    {
                        var cell = _gridMapManager.GetCell(neighbors[n]);
                        if (cell?.OccupyingUnit is UnitController other && IsInstanceValid(other) &&
                            !other.IsSurrendered && other.FactionId != unit.OriginalFactionId)
                        {
                            rivalNeighbor = other;
                            break;
                        }
                    }

                    if (rivalNeighbor != null)
                    {
                        var currentOwner = FindFactionData(unit.FactionId);
                        var recipientFaction = FindFactionData(rivalNeighbor.FactionId);

                        if (currentOwner != null && recipientFaction != null)
                        {
                            _unitRegistry.ResolveDefection(unit, currentOwner, recipientFaction);
                        }
                        else
                        {
                            unit.Data.Recapture(rivalNeighbor.FactionId, 25);
                        }
                    }
                }
            }

            // 2. Execute AI turns sequentially on Main Thread
            await _aiTurnExecutor.ExecuteAllAITurnsAsync(
                this,
                _gridMapManager,
                _pathfindingManager,
                _economyManager,
                _unitRegistry,
                _asyncPacer,
                _asyncPacer.Token
            );

            // 3. Reset movement points for non-player units
            for (int i = 0; i < allUnits.Count; i++)
            {
                var u = allUnits[i];
                if (IsInstanceValid(u) && u.FactionId != 0)
                {
                    u.ResetTurnMovement();
                }
            }

            // 4. Advance turn counter, calculate economy for all factions, and refresh player military points
            _turnManager.EndTurn();
            DeselectAll();

            // 5. Update player fog of war vision for the new turn
            _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
            RefreshEconomyUI();
        }
        catch (OperationCanceledException)
        {
            // Clean abort when game is exited to menu during turn processing
        }
        finally
        {
            if (State != MapInteractionState.Disabled)
            {
                _hud.SetButtonsDisabled(false);
                State = MapInteractionState.Idle;
                EvaluateAndTriggerGameOver();
            }
        }
    }

    private void OnTurnChanged(int turn, int food, int prod, int gold, int dFood, int dProd, int dGold)
    {
        RefreshEconomyUI();
        _hud.RefreshUnitInfo();
    }

    public void RefreshEconomyUI()
    {
        var (_, _, netIncome) = _economyManager.CalculateTurnIncome(_playerFaction);
        _hud.UpdateEconomy(_playerFaction.Treasury, netIncome, _turnManager.TurnCount);
    }

    // ======================================================================
    // TERRITORY & EXPAND PIPELINE
    // ======================================================================
    public bool CanClaimTile(int factionId, Vector2I coord)
    {
        if (!_gridMapManager.IsWithinBounds(coord)) return false;
        var cell = _gridMapManager.GetCell(coord);
        if (cell == null || cell.IsSolid) return false;

        // Cannot claim if occupied by active, non-surrendered hostile unit
        if (cell.OccupyingUnit is UnitController occupier && IsInstanceValid(occupier) &&
            occupier.FactionId != factionId && !occupier.IsSurrendered && occupier.Data.IsActive)
        {
            return false;
        }

        return true;
    }

    public void ExecuteClaimTile(int factionId, Vector2I coord)
    {
        if (!CanClaimTile(factionId, coord)) return;

        var cell = _gridMapManager.GetCell(coord);
        if (cell == null) return;

        int oldOwner = cell.OwnerFactionId;
        if (oldOwner == factionId) return;

        if (oldOwner >= 0)
        {
            var oldFaction = FindFactionData(oldOwner);
            oldFaction?.ControlledTiles.Remove(coord);
        }

        var newFaction = FindFactionData(factionId);
        if (newFaction != null)
        {
            newFaction.ControlledTiles.Add(coord);
            cell.OwnerFactionId = factionId;

            _gridMapManager.RefreshTileOwnerVisual(coord, factionId);
            _economyManager.NotifyTerritoryChanged(newFaction);
            RefreshEconomyUI();
        }
    }

    // ======================================================================
    // VICTORY / DEFEAT PIPELINE
    // ======================================================================
    public GameResult CheckGameOverConditions(out VictoryType victoryType)
    {
        // 1. Defeat Condition: Player has 0 active unsurrendered units OR lost all territory
        var playerUnits = _unitRegistry.GetUnitsForFaction(0);
        int activePlayerUnits = 0;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            var u = playerUnits[i];
            if (IsInstanceValid(u) && u.Data.IsActive && !u.IsSurrendered && u.HpCurrent > 0)
            {
                activePlayerUnits++;
            }
        }

        if (activePlayerUnits == 0)
        {
            victoryType = VictoryType.Conquest;
            return GameResult.Defeat;
        }

        if (_playerFaction.ControlledTiles.Count == 0)
        {
            victoryType = VictoryType.Domination;
            return GameResult.Defeat;
        }

        // 2. Victory Condition (Conquest): All rival factions have 0 active units
        bool anyRivalAlive = false;
        var factions = _economyManager.Factions;
        for (int f = 0; f < factions.Count; f++)
        {
            var faction = factions[f];
            if (faction.FactionId == 0) continue;

            var rivalUnits = _unitRegistry.GetUnitsForFaction(faction.FactionId);
            for (int u = 0; u < rivalUnits.Count; u++)
            {
                var ru = rivalUnits[u];
                if (IsInstanceValid(ru) && ru.Data.IsActive && !ru.IsSurrendered && ru.HpCurrent > 0)
                {
                    anyRivalAlive = true;
                    break;
                }
            }
            if (anyRivalAlive) break;
        }

        if (!anyRivalAlive && factions.Count > 1)
        {
            victoryType = VictoryType.Conquest;
            return GameResult.Victory;
        }

        victoryType = VictoryType.None;
        return GameResult.Undecided;
    }

    public bool EvaluateAndTriggerGameOver()
    {
        if (State == MapInteractionState.Disabled) return true;

        var result = CheckGameOverConditions(out var victoryType);
        if (result == GameResult.Undecided) return false;

        State = MapInteractionState.Disabled;
        _hud.SetButtonsDisabled(true);
        DeselectAll();

        if (_endGameModal == null)
        {
            var modalScene = GD.Load<PackedScene>("res://scenes/ui/end_game_modal.tscn");
            if (modalScene != null)
            {
                _endGameModal = modalScene.Instantiate<EndGameModal>();
                AddChild(_endGameModal);
            }
        }

        _endGameModal?.ShowResult(result, victoryType, _turnManager.TurnCount, _enemiesEliminated);
        return true;
    }

    // ======================================================================
    // BATTLEPAYLOAD & SUSPEND / RESUME CONTRACT
    // ======================================================================
    public BattlePayload BuildBattlePayload(UnitController attacker, UnitController defender)
    {
        var cell = _gridMapManager.GetCell(defender.GridPosition);
        var biome = cell?.TerrainData.Biome ?? BiomeType.Plains;

        var atkSnapshot = new UnitCombatSnapshot(
            unitId: attacker.Data.UnitId,
            factionId: attacker.FactionId,
            unitType: attacker.Data.UnitType,
            hpCurrent: attacker.HpCurrent,
            hpMax: attacker.HpMax,
            attack: attacker.Attack,
            defense: attacker.Defense,
            moraleCurrent: attacker.MoraleCurrent,
            moraleMax: attacker.MoraleMax,
            isRanged: attacker.Data.IsRanged
        );

        var defSnapshot = new UnitCombatSnapshot(
            unitId: defender.Data.UnitId,
            factionId: defender.FactionId,
            unitType: defender.Data.UnitType,
            hpCurrent: defender.HpCurrent,
            hpMax: defender.HpMax,
            attack: defender.Attack,
            defense: defender.Defense,
            moraleCurrent: defender.MoraleCurrent,
            moraleMax: defender.MoraleMax,
            isRanged: defender.Data.IsRanged
        );

        return new BattlePayload(
            battleId: Guid.NewGuid().ToString("N"),
            hexPosition: defender.GridPosition,
            terrainType: biome,
            attackers: new List<UnitCombatSnapshot> { atkSnapshot },
            defenders: new List<UnitCombatSnapshot> { defSnapshot }
        );
    }

    public void ApplyBattleResolution(BattleResolutionResult result)
    {
        // 1. Update surviving units state
        for (int i = 0; i < result.SurvivingUnits.Count; i++)
        {
            var snapshot = result.SurvivingUnits[i];
            var unit = FindUnitByNumericId(snapshot.UnitId);
            if (unit != null && IsInstanceValid(unit))
            {
                int hpDiff = unit.Data.CurrentHp - snapshot.HpCurrent;
                if (hpDiff > 0)
                {
                    unit.Data.ApplyDamage(hpDiff);
                }
                int moraleDiff = snapshot.MoraleCurrent - unit.Data.MoraleCurrent;
                if (moraleDiff != 0)
                {
                    unit.Data.ModifyMorale(moraleDiff);
                }
            }
        }

        // 2. Eliminate destroyed units
        for (int i = 0; i < result.DestroyedUnitIds.Count; i++)
        {
            int destroyedId = result.DestroyedUnitIds[i];
            var unit = FindUnitByNumericId(destroyedId);
            if (unit != null && IsInstanceValid(unit))
            {
                unit.Data.Disband();
            }
        }

        _hud.RefreshUnitInfo();
        RefreshEconomyUI();
        _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);

        State = MapInteractionState.Idle;
        EvaluateAndTriggerGameOver();
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
