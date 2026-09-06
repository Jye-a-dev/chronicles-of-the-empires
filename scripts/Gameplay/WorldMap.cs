using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.AI;
using ChroniclesOfTheEmpires.Gameplay.Economy;
using ChroniclesOfTheEmpires.UI.Components;
using ChroniclesOfTheEmpires.UI.Controllers;

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
    private readonly SimpleAITurnExecutor _aiTurnExecutor = new();

    private BoardBackdrop? _boardBackdrop;
    private Line2D? _hoverIndicator;

    private readonly UnitRegistry _unitRegistry = new();
    private readonly AsyncPacer _asyncPacer = new();
    private MapInputHandler _inputHandler = null!;
    private UnitController? _selectedUnit;

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

        // 8. Spawn Initial Units
        SpawnInitialUnits();

        // 9. Connect UI Events & Turn Manager
        _hud.Initialize(session.StageTitle);
        _hud.EndTurnRequested += OnEndTurnPressed;
        _hud.ExitToMenuRequested += OnExitToMenu;
        _hud.RecruitRequested += (unitId, coords) => ExecuteRecruitUnit(_playerFaction, unitId, coords);

        _turnManager.TurnChanged += OnTurnChanged;

        // 10. Initial UI Refresh & Vision illumination
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

        _hexIndicator.SelectHex(unit.Position);
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

    /// <summary>
    /// Executes movement and pre-locks target cell in A* and HexCell occupancy BEFORE the Tween begins,
    /// eliminating A* solid race conditions. Includes transactional fallback if aborted.
    /// </summary>
    public void ExecuteSafeMove(UnitController unit, Vector2I targetGrid, HexCell targetCell, Action? onComplete = null)
    {
        if (State != MapInteractionState.Idle && State != MapInteractionState.AITurnProcessing) return;
        bool wasIdle = State == MapInteractionState.Idle;
        if (wasIdle) State = MapInteractionState.UnitMoving;

        _pathfindingManager.SetPointSolid(unit.GridPosition, false);
        var pathSpan = _pathfindingManager.FindPathSpan(unit.GridPosition, targetGrid);

        if (pathSpan.Length <= 1)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            if (wasIdle) State = MapInteractionState.Idle;
            return;
        }

        int cost = _pathfindingManager.CalculatePathCost(pathSpan, _gridMapManager);
        if (cost > unit.MovementRangeRemaining)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            if (wasIdle) State = MapInteractionState.Idle;
            return;
        }

        // PRE-LOCK OCCUPANCY (Transactional state sync)
        Vector2I oldPos = unit.GridPosition;
        _pathfindingManager.SetPointSolid(oldPos, false);
        _pathfindingManager.SetPointSolid(targetGrid, true);

        var oldCell = _gridMapManager.GetCell(oldPos);
        if (oldCell != null) oldCell.OccupyingUnit = null;

        targetCell.OccupyingUnit = unit;
        unit.Data.GridPosition = targetGrid;
        _unitRegistry.UpdatePosition(unit, oldPos, targetGrid);

        _pathVisualizer.ClearPath();

        unit.MoveAlongPath(pathSpan, cost, () =>
        {
            _hexIndicator.SelectHex(unit.Position);
            _hud.RefreshUnitInfo();

            if (unit.FactionId == 0)
            {
                _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
            }

            if (wasIdle) State = MapInteractionState.Idle;
            onComplete?.Invoke();
        });
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
        }

        if (wasIdle) State = MapInteractionState.Idle;
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
        unit.UnitMoved += (u, oldPos, newPos) =>
        {
            _hexIndicator.SelectHex(u.Position);
            _hud.RefreshUnitInfo();
        };
    }

    private void HandleUnitDestroyed(UnitController unit)
    {
        _pathfindingManager.SetPointSolid(unit.GridPosition, false);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell?.OccupyingUnit == unit)
        {
            cell.OccupyingUnit = null;
        }

        if (_selectedUnit == unit)
        {
            DeselectAll();
        }
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
            _hud.SetButtonsDisabled(false);
            State = MapInteractionState.Idle;
        }
    }

    private void OnTurnChanged(int turn, int food, int prod, int gold, int dFood, int dProd, int dGold)
    {
        RefreshEconomyUI();
        _hud.RefreshUnitInfo();
    }

    private void RefreshEconomyUI()
    {
        var (_, _, netIncome) = _economyManager.CalculateTurnIncome(_playerFaction);
        _hud.UpdateEconomy(_playerFaction.Treasury, netIncome, _turnManager.TurnCount);
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
