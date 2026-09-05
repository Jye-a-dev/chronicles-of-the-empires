using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.Economy;
using ChroniclesOfTheEmpires.UI.Components;
using ChroniclesOfTheEmpires.UI.Controllers;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Master tactical world map coordinator.
/// Orchestrates GridMapManager, PathfindingManager, UnitController, RTSCamera2D, TurnManager,
/// EconomyManager, EconomyHUDController, HexSelectionIndicator, and decoupled TacticalHUD UI components.
/// Uses 100% Grid Mouse Picking (no physics raycast) and strict Single Source of Truth architecture.
/// </summary>
public partial class WorldMap : Node2D
{
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

    private BoardBackdrop? _boardBackdrop;
    private Line2D? _hoverIndicator;

    private UnitController? _selectedUnit;
    private readonly List<UnitController> _allUnits = new();

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

        // 5. Setup Economy Engine & Faction State from TXT
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
        _turnManager.Initialize(_playerFaction, _economyManager);

        // 6. Spawn Initial Units using decoupled Controller-Model binding
        SpawnInitialUnits();

        // 7. Connect UI Events & HUD
        _hud.Initialize(session.StageTitle);
        _hud.EndTurnRequested += OnEndTurnPressed;
        _hud.ExitToMenuRequested += () => GetTree().ChangeSceneToFile("res://scenes/ui/screens/main_menu.tscn");

        _turnManager.TurnChanged += OnTurnChanged;

        // 8. Initial UI Refresh
        RefreshEconomyUI();
    }

    private void SpawnInitialUnits()
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return;

        Vector2I p1Pos = FindWalkableCell(new Vector2I(2, 2));
        Vector2I p2Pos = FindWalkableCell(new Vector2I(3, 3));
        Vector2I rivalPos = FindWalkableCell(new Vector2I(_gridMapManager.MapWidth - 4, _gridMapManager.MapHeight - 4));

        // Player Unit 1: Cấm Vệ Quân
        SpawnUnit(unitScene, "cam_ve_quan", 0, p1Pos);

        // Player Unit 2: Cung Thủ Rừng Rậm
        SpawnUnit(unitScene, "cung_thu", 0, p2Pos);

        // Rival Unit: Tiên Phong Địch Quốc
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
        RegisterUnit(controller);
    }

    private void RegisterUnit(UnitController unit)
    {
        _allUnits.Add(unit);
        _pathfindingManager.SetPointSolid(unit.GridPosition, true);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell != null) cell.OccupyingUnit = unit;

        var faction = FindFactionData(unit.FactionId);
        if (faction != null && !faction.Units.Contains(unit.Data))
        {
            faction.Units.Add(unit.Data);
        }

        if (unit.FactionId == 0)
        {
            _turnManager.RegisterPlayerUnit(unit);
        }

        unit.UnitDestroyed += HandleUnitDestroyed;
        unit.UnitMoved += (u, oldPos, newPos) =>
        {
            _hexIndicator.SelectHex(u.Position);
            _hud.RefreshUnitInfo();
        };
    }

    private void HandleUnitDestroyed(UnitController unit)
    {
        _allUnits.Remove(unit);
        _pathfindingManager.SetPointSolid(unit.GridPosition, false);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell?.OccupyingUnit == unit)
        {
            cell.OccupyingUnit = null;
        }

        var faction = FindFactionData(unit.FactionId);
        faction?.Units.Remove(unit.Data);

        if (unit.FactionId == 0)
        {
            _turnManager.UnregisterPlayerUnit(unit);
        }

        if (_selectedUnit == unit)
        {
            DeselectAll();
        }
    }

    private FactionData? FindFactionData(int factionId)
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.Right)
            {
                HandleRightClickAction();
            }
            else if (mb.ButtonIndex == MouseButton.Left)
            {
                HandleLeftClickInspect();
            }
        }
        else if (@event is InputEventMouseMotion)
        {
            HandleMouseHoverPreview();
        }
        else if (@event is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.Space)
            {
                if (!_hud.IsSettingsOpen)
                {
                    OnEndTurnPressed();
                }
            }
            else if (key.Keycode == Key.Escape)
            {
                if (_hud.IsSettingsOpen)
                {
                    _hud.CloseSettings();
                }
                else
                {
                    DeselectAll();
                }
            }
        }
    }

    private void HandleLeftClickInspect()
    {
        // Block new selection clicks while any unit is currently animating movement
        if (_selectedUnit != null && _selectedUnit.IsMoving) return;

        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I gridPos = GridMapManager.WorldToGrid(mouseWorld);

        if (!_gridMapManager.IsWithinBounds(gridPos))
        {
            DeselectAll();
            return;
        }

        var hexCell = _gridMapManager.GetCell(gridPos);
        if (hexCell == null)
        {
            DeselectAll();
            return;
        }

        // Pure Grid Mouse Picking: check if this hex contains an occupying military unit
        if (hexCell.OccupyingUnit is UnitController clickedUnit && IsInstanceValid(clickedUnit))
        {
            SelectUnit(clickedUnit);
            return;
        }

        // Empty hex clicked: clear unit selection and display tile economic data
        DeselectUnitOnly();
        _pathVisualizer.ClearPath();
        _hexIndicator.SelectHex(hexCell.WorldPosition);
        _economyHUDController.OnTileSelected(hexCell);
    }

    private void SelectUnit(UnitController unit)
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

    private void HandleRightClickAction()
    {
        if (_selectedUnit == null || _selectedUnit.IsMoving || _selectedUnit.FactionId != 0 || _selectedUnit.IsSurrendered) return;

        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        if (!_gridMapManager.IsWithinBounds(targetGrid) || targetGrid == _selectedUnit.GridPosition) return;

        var targetCell = _gridMapManager.GetCell(targetGrid);
        if (targetCell == null) return;

        // 1. Target cell contains a Unit: determine Recapture or Combat
        if (targetCell.OccupyingUnit is UnitController targetUnit && IsInstanceValid(targetUnit))
        {
            if (targetUnit.IsSurrendered)
            {
                HandleSurrenderedInteraction(_selectedUnit, targetUnit, targetGrid);
                return;
            }

            if (targetUnit.FactionId != _selectedUnit.FactionId)
            {
                HandleCombatTarget(_selectedUnit, targetUnit, targetGrid);
                return;
            }

            // Clicked friendly unit: switch selection
            SelectUnit(targetUnit);
            return;
        }

        // 2. Target cell is empty: execute safe movement
        ExecuteSafeMove(_selectedUnit, targetGrid, targetCell);
    }

    private void HandleSurrenderedInteraction(UnitController actor, UnitController target, Vector2I targetGrid)
    {
        var neighbors = _gridMapManager.GetSurroundingCells(targetGrid);
        bool isAdjacent = Array.IndexOf(neighbors, actor.GridPosition) >= 0;

        if (isAdjacent)
        {
            ExecuteRecaptureOrder(actor, target);
            return;
        }

        // Move adjacent then recapture
        Vector2I[] bestPath = Array.Empty<Vector2I>();
        int lowestCost = int.MaxValue;

        _pathfindingManager.SetPointSolid(actor.GridPosition, false);
        for (int i = 0; i < neighbors.Length; i++)
        {
            var neighbor = neighbors[i];
            if (!_gridMapManager.IsWithinBounds(neighbor) || _pathfindingManager.IsPointSolid(neighbor)) continue;

            var testPath = _pathfindingManager.FindPath(actor.GridPosition, neighbor);
            if (testPath.Length > 1)
            {
                int cost = _pathfindingManager.CalculatePathCost(testPath, _gridMapManager);
                if (cost < lowestCost && cost <= actor.MovementRangeRemaining)
                {
                    lowestCost = cost;
                    bestPath = testPath;
                }
            }
        }
        _pathfindingManager.SetPointSolid(actor.GridPosition, true);

        if (bestPath.Length > 1)
        {
            Vector2I moveDest = bestPath[^1];
            var destCell = _gridMapManager.GetCell(moveDest);
            if (destCell != null)
            {
                ExecuteSafeMove(actor, moveDest, destCell, () =>
                {
                    ExecuteRecaptureOrder(actor, target);
                });
            }
        }
    }

    private void HandleCombatTarget(UnitController attacker, UnitController defender, Vector2I targetGrid)
    {
        if (attacker.MovementRangeRemaining <= 0) return;

        bool inRange = IsWithinAttackRange(attacker.GridPosition, targetGrid, attacker.Data.IsRanged ? attacker.Data.AttackRange : 1);
        if (inRange)
        {
            ExecuteCombatResolution(attacker, defender);
            return;
        }

        // If not in range, attempt to move adjacent (for melee units)
        if (!attacker.Data.IsRanged)
        {
            var neighbors = _gridMapManager.GetSurroundingCells(targetGrid);
            Vector2I[] bestPath = Array.Empty<Vector2I>();
            int lowestCost = int.MaxValue;

            _pathfindingManager.SetPointSolid(attacker.GridPosition, false);
            for (int i = 0; i < neighbors.Length; i++)
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

    private bool IsWithinAttackRange(Vector2I start, Vector2I target, int range)
    {
        if (start == target) return true;
        if (range <= 0) return false;

        var neighbors = _gridMapManager.GetSurroundingCells(start);
        if (Array.IndexOf(neighbors, target) >= 0) return true;
        if (range == 1) return false;

        // Range 2: check 2nd-degree surrounding cells
        for (int i = 0; i < neighbors.Length; i++)
        {
            var n2 = _gridMapManager.GetSurroundingCells(neighbors[i]);
            if (Array.IndexOf(n2, target) >= 0) return true;
        }

        return false;
    }

    /// <summary>
    /// Executes movement and pre-locks target cell in A* and HexCell occupancy BEFORE the Tween begins,
    /// eliminating A* solid race condition and preventing stacking collisions.
    /// </summary>
    private void ExecuteSafeMove(UnitController unit, Vector2I targetGrid, HexCell targetCell, Action? onComplete = null)
    {
        _pathfindingManager.SetPointSolid(unit.GridPosition, false);
        var path = _pathfindingManager.FindPath(unit.GridPosition, targetGrid);

        if (path.Length <= 1)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            return;
        }

        int cost = _pathfindingManager.CalculatePathCost(path, _gridMapManager);
        if (cost > unit.MovementRangeRemaining)
        {
            _pathfindingManager.SetPointSolid(unit.GridPosition, true);
            return;
        }

        // PRE-LOCK OCCUPANCY: Lock destination cell immediately before Tween starts
        Vector2I oldPos = unit.GridPosition;
        _pathfindingManager.SetPointSolid(oldPos, false);
        _pathfindingManager.SetPointSolid(targetGrid, true);

        var oldCell = _gridMapManager.GetCell(oldPos);
        if (oldCell != null) oldCell.OccupyingUnit = null;

        targetCell.OccupyingUnit = unit;
        unit.Data.GridPosition = targetGrid;

        _pathVisualizer.ClearPath();

        unit.MoveAlongPath(path, cost, () =>
        {
            _hexIndicator.SelectHex(unit.Position);
            _hud.RefreshUnitInfo();
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Executes sequential combat resolution with strict guards against double suicides and ghost references.
    /// Counter-attacks only occur if Defender remains alive, unsurrendered, and Attacker is not ranged.
    /// </summary>
    private void ExecuteCombatResolution(UnitController attacker, UnitController defender)
    {
        if (attacker.IsMoving || defender.IsMoving) return;

        bool isRanged = attacker.Data.IsRanged;
        int distance = Array.IndexOf(_gridMapManager.GetSurroundingCells(attacker.GridPosition), defender.GridPosition) >= 0 ? 1 : 2;

        // Phase 1: Attacker strikes Defender
        float atkMoraleMod = attacker.MoraleCurrent > 80 ? 1.15f : (attacker.MoraleCurrent < 40 ? 0.75f : 1.0f);
        int rawDmg = Math.Max(1, (int)(attacker.Attack * atkMoraleMod) - defender.Defense);

        defender.Data.ApplyDamage(rawDmg);
        defender.Data.ModifyMorale(-20);

        // Attacker expends tactical action budget (2 movement points or remaining)
        attacker.Data.MovementRemaining = Math.Max(0, attacker.Data.MovementRemaining - 2);

        // Phase 2: Counter-attack with strict sequential safety guard
        if (defender.Data.CurrentHp > 0 && !defender.Data.IsSurrendered && !isRanged && distance == 1)
        {
            int counterDmg = Math.Max(1, defender.Defense - (attacker.Defense / 2));
            attacker.Data.ApplyDamage(counterDmg);
            attacker.Data.ModifyMorale(-10);
        }

        _hud.RefreshUnitInfo();
        RefreshEconomyUI();
    }

    private void ExecuteRecaptureOrder(UnitController actor, UnitController target)
    {
        if (actor.IsMoving || target.IsMoving || !target.IsSurrendered) return;

        bool isOriginalOwner = _playerFaction.FactionId == target.OriginalFactionId;
        var cost = isOriginalOwner
            ? new ResourceBundle(15, 0, 10, 0, 0)
            : new ResourceBundle(0, 0, 10, 0, 0);

        if (!_playerFaction.Treasury.HasEnough(cost)) return;

        _playerFaction.Treasury -= cost;

        target.Data.Recapture(_playerFaction.FactionId, isOriginalOwner ? 40 : 30);

        if (!_playerFaction.Units.Contains(target.Data))
        {
            _playerFaction.Units.Add(target.Data);
        }
        _turnManager.RegisterPlayerUnit(target);

        actor.Data.MovementRemaining = Math.Max(0, actor.Data.MovementRemaining - 1);

        RefreshEconomyUI();
        _hud.DisplayUnit(target);
    }

    private void HandleMouseHoverPreview()
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        if (_hoverIndicator != null)
        {
            if (_gridMapManager.IsWithinBounds(targetGrid))
            {
                _hoverIndicator.Position = GridMapManager.GridToWorldCenter(targetGrid);
                _hoverIndicator.Visible = true;
            }
            else
            {
                _hoverIndicator.Visible = false;
            }
        }

        if (_selectedUnit == null || _selectedUnit.IsMoving || _selectedUnit.FactionId != 0)
        {
            _pathVisualizer.ClearPath();
            return;
        }

        if (!_gridMapManager.IsWithinBounds(targetGrid) || targetGrid == _selectedUnit.GridPosition)
        {
            _pathVisualizer.ClearPath();
            return;
        }

        var targetCell = _gridMapManager.GetCell(targetGrid);
        if (targetCell?.OccupyingUnit != null)
        {
            _pathVisualizer.ClearPath();
            return;
        }

        _pathfindingManager.SetPointSolid(_selectedUnit.GridPosition, false);
        var path = _pathfindingManager.FindPath(_selectedUnit.GridPosition, targetGrid);
        _pathfindingManager.SetPointSolid(_selectedUnit.GridPosition, true);

        if (path.Length > 1)
        {
            int cost = _pathfindingManager.CalculatePathCost(path, _gridMapManager);
            bool reachable = cost <= _selectedUnit.MovementRangeRemaining;
            _pathVisualizer.ShowPath(path, reachable);
        }
        else
        {
            _pathVisualizer.ClearPath();
        }
    }

    private void DeselectAll()
    {
        DeselectUnitOnly();
        _hexIndicator.ClearSelection();
        _pathVisualizer.ClearPath();
        _hud.DeselectAll();
    }

    private void OnEndTurnPressed()
    {
        // Guard: lock End Turn while any unit is currently animating movement
        for (int i = 0; i < _allUnits.Count; i++)
        {
            if (IsInstanceValid(_allUnits[i]) && _allUnits[i].IsMoving) return;
        }

        // Process Surrendered Units: local O(1) hex neighborhood scan for rival defections
        for (int i = _allUnits.Count - 1; i >= 0; i--)
        {
            var unit = _allUnits[i];
            if (!IsInstanceValid(unit) || !unit.IsSurrendered) continue;

            unit.Data.SurrenderTurnsRemaining--;
            if (unit.Data.SurrenderTurnsRemaining <= 0)
            {
                UnitController? rivalNeighbor = null;
                var neighbors = _gridMapManager.GetSurroundingCells(unit.GridPosition);
                for (int n = 0; n < neighbors.Length; n++)
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
                        ResolveDefection(unit.Data, currentOwner, recipientFaction);
                    }
                    else
                    {
                        unit.Data.Recapture(rivalNeighbor.FactionId, 25);
                    }

                    if (unit.FactionId == 0)
                    {
                        _turnManager.RegisterPlayerUnit(unit);
                    }
                    else
                    {
                        _turnManager.UnregisterPlayerUnit(unit);
                    }
                }
            }
        }

        _turnManager.EndTurn();
        DeselectAll();
    }

    public void ResolveDefection(UnitData unit, FactionData currentOwner, FactionData recipientFaction)
    {
        currentOwner.Units.Remove(unit);
        recipientFaction.Units.Add(unit);
        unit.Recapture(recipientFaction.FactionId, restoredMorale: 25);
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
}
