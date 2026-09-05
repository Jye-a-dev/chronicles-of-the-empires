using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.UI.Components;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Master tactical world map coordinator.
/// Orchestrates GridMapManager, PathfindingManager, UnitController, RTSCamera2D, TurnManager,
/// HexSelectionIndicator, and decoupled TacticalHUD UI components.
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

        // 5. Spawn Initial Units from customizable txt settings
        SpawnInitialUnits();

        // 6. Connect UI Events & HUD
        _hud.Initialize(session.StageTitle);
        _hud.EndTurnRequested += OnEndTurnPressed;
        _hud.ExitToMenuRequested += () => GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");

        _turnManager.TurnChanged += OnTurnChanged;

        // 7. Initial UI Refresh
        RefreshEconomyUI();
    }

    private void SpawnInitialUnits()
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/unit.tscn");
        if (unitScene == null) return;

        Vector2I p1Pos = FindWalkableCell(new Vector2I(2, 2));
        Vector2I p2Pos = FindWalkableCell(new Vector2I(3, 3));
        Vector2I rivalPos = FindWalkableCell(new Vector2I(_gridMapManager.MapWidth - 4, _gridMapManager.MapHeight - 4));

        // Spawn Player Unit 1 (Cấm Vệ Quân)
        var player1 = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(player1);
        player1.ApplyConfig("cam_ve_quan");
        player1.SnapToGrid(p1Pos);
        RegisterUnit(player1);

        // Spawn Player Unit 2 (Cung Thủ Rừng Rậm)
        var player2 = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(player2);
        player2.ApplyConfig("cung_thu");
        player2.SnapToGrid(p2Pos);
        RegisterUnit(player2);

        // Spawn Rival Unit (Tiên Phong Địch Quốc)
        var rival = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(rival);
        rival.ApplyConfig("tien_phong_dich");
        rival.SnapToGrid(rivalPos);
        RegisterUnit(rival);

        // Focus camera on first player     
        _camera.Position = GridMapManager.GridToWorldCenter(p1Pos);
    }

    private void RegisterUnit(UnitController unit)
    {
        _allUnits.Add(unit);
        _pathfindingManager.SetPointSolid(unit.GridPosition, true);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell != null) cell.OccupyingUnit = unit;

        if (unit.FactionId == 0)
        {
            _turnManager.RegisterPlayerUnit(unit);
        }

        unit.UnitSelected += OnUnitSelected;
        unit.UnitMoved += (u, oldPos, newPos) =>
        {
            _pathfindingManager.SetPointSolid(oldPos, false);
            _pathfindingManager.SetPointSolid(newPos, true);

            var oldCell = _gridMapManager.GetCell(oldPos);
            if (oldCell != null) oldCell.OccupyingUnit = null;

            var newCell = _gridMapManager.GetCell(newPos);
            if (newCell != null) newCell.OccupyingUnit = u;
        };
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

    private void OnUnitSelected(UnitController unit)
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.Right)
            {
                HandleRightClickMove();
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
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I gridPos = GridMapManager.WorldToGrid(mouseWorld);
        var hexCell = _gridMapManager.GetCell(gridPos);

        if (hexCell == null)
        {
            DeselectAll();
            return;
        }

        // Clear active unit selection when clicking on an empty hex
        if (_selectedUnit != null)
        {
            _selectedUnit.SetSelected(false);
            _selectedUnit = null;
        }

        _pathVisualizer.ClearPath();

        // Highlight selected hex and display tile details in TacticalHUD
        _hexIndicator.SelectHex(hexCell.WorldPosition);
        _hud.DisplayTile(hexCell);
    }

    private void HandleRightClickMove()
    {
        if (_selectedUnit == null || _selectedUnit.IsMoving || _selectedUnit.FactionId != 0) return;

        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        if (!_gridMapManager.IsWithinBounds(targetGrid) || targetGrid == _selectedUnit.GridPosition) return;

        _pathfindingManager.SetPointSolid(_selectedUnit.GridPosition, false);
        var path = _pathfindingManager.FindPath(_selectedUnit.GridPosition, targetGrid);

        if (path.Length > 1)
        {
            int cost = _pathfindingManager.CalculatePathCost(path, _gridMapManager);
            if (cost <= _selectedUnit.MovementRangeRemaining)
            {
                _selectedUnit.MoveAlongPath(path, cost, () =>
                {
                    _hexIndicator.SelectHex(_selectedUnit.Position);
                    _hud.RefreshUnitInfo();
                });
                _pathVisualizer.ClearPath();
                return;
            }
        }

        _pathfindingManager.SetPointSolid(_selectedUnit.GridPosition, true);
    }

    private void HandleMouseHoverPreview()
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        // Update subtle hex hover outline
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
        if (_selectedUnit != null)
        {
            _selectedUnit.SetSelected(false);
            _selectedUnit = null;
        }
        _hexIndicator.ClearSelection();
        _pathVisualizer.ClearPath();
        _hud.DeselectAll();
    }

    private void OnEndTurnPressed()
    {
        _turnManager.EndTurn();
        DeselectAll();
    }

    private void OnTurnChanged(int turn, int food, int prod, int gold, int dFood, int dProd, int dGold)
    {
        RefreshEconomyUI();
        _hud.RefreshUnitInfo();
    }

    private void RefreshEconomyUI()
    {
        _hud.UpdateEconomy(
            _turnManager.Food,
            _turnManager.FoodYield,
            _turnManager.Production,
            _turnManager.ProductionYield,
            _turnManager.Gold,
            _turnManager.GoldYield,
            _turnManager.TurnCount
        );
    }
}
