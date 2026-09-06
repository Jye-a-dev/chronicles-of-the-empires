using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Handles 100% raycast-free grid mouse picking, camera-aware input translation,
/// and tactical action dispatching based on MapInteractionState authority.
/// </summary>
public partial class MapInputHandler : Node2D
{
    public event Action<Vector2I, HexCell>? CellSelected;
    public event Action<UnitController>? UnitSelected;
    public event Action<UnitController, Vector2I, HexCell>? UnitMoveRequested;
    public event Action<UnitController, UnitController, Vector2I>? UnitAttackRequested;
    public event Action<UnitController, UnitController>? UnitRecaptureRequested;
    public event Action<Vector2I>? TileHovered;
    public event Action? TileHoverCleared;
    public event Action? DeselectRequested;
    public event Action? EndTurnRequested;

    private GridMapManager _gridMap = null!;
    private UnitRegistry _unitRegistry = null!;
    private PathfindingManager _pathfinding = null!;
    private PathVisualizer _pathVisualizer = null!;
    private Line2D? _hoverIndicator;

    private Func<MapInteractionState> _getState = () => MapInteractionState.Idle;
    private Func<UnitController?> _getSelectedUnit = () => null;
    private Func<bool> _isSettingsOpen = () => false;
    private Action _closeSettings = () => { };

    public void Initialize(
        GridMapManager gridMap,
        UnitRegistry unitRegistry,
        PathfindingManager pathfinding,
        PathVisualizer pathVisualizer,
        Line2D? hoverIndicator,
        Func<MapInteractionState> getState,
        Func<UnitController?> getSelectedUnit,
        Func<bool> isSettingsOpen,
        Action closeSettings)
    {
        _gridMap = gridMap;
        _unitRegistry = unitRegistry;
        _pathfinding = pathfinding;
        _pathVisualizer = pathVisualizer;
        _hoverIndicator = hoverIndicator;
        _getState = getState;
        _getSelectedUnit = getSelectedUnit;
        _isSettingsOpen = isSettingsOpen;
        _closeSettings = closeSettings;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_getState() != MapInteractionState.Idle) return;

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
                if (!_isSettingsOpen())
                {
                    EndTurnRequested?.Invoke();
                }
            }
            else if (key.Keycode == Key.Escape)
            {
                if (_isSettingsOpen())
                {
                    _closeSettings();
                }
                else
                {
                    DeselectRequested?.Invoke();
                }
            }
        }
    }

    private void HandleLeftClickInspect()
    {
        var selected = _getSelectedUnit();
        if (selected != null && selected.IsMoving) return;

        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I gridPos = GridMapManager.WorldToGrid(mouseWorld);

        if (!_gridMap.IsWithinBounds(gridPos))
        {
            DeselectRequested?.Invoke();
            return;
        }

        var hexCell = _gridMap.GetCell(gridPos);
        if (hexCell == null)
        {
            DeselectRequested?.Invoke();
            return;
        }

        // Pure Grid Mouse Picking: check registry or cell occupant
        var clickedUnit = _unitRegistry.GetUnitAt(gridPos) ?? hexCell.OccupyingUnit as UnitController;
        if (clickedUnit != null && GodotObject.IsInstanceValid(clickedUnit) && clickedUnit.Visible)
        {
            UnitSelected?.Invoke(clickedUnit);
            return;
        }

        // Empty hex clicked: clear unit selection and inspect cell
        CellSelected?.Invoke(gridPos, hexCell);
    }

    private void HandleRightClickAction()
    {
        var selected = _getSelectedUnit();
        if (selected == null || selected.IsMoving || selected.FactionId != 0 || selected.IsSurrendered) return;

        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        if (!_gridMap.IsWithinBounds(targetGrid) || targetGrid == selected.GridPosition) return;

        var targetCell = _gridMap.GetCell(targetGrid);
        if (targetCell == null) return;

        var targetUnit = _unitRegistry.GetUnitAt(targetGrid) ?? targetCell.OccupyingUnit as UnitController;
        if (targetUnit != null && GodotObject.IsInstanceValid(targetUnit) && targetUnit.Visible)
        {
            if (targetUnit.IsSurrendered)
            {
                UnitRecaptureRequested?.Invoke(selected, targetUnit);
                return;
            }

            if (targetUnit.FactionId != selected.FactionId)
            {
                UnitAttackRequested?.Invoke(selected, targetUnit, targetGrid);
                return;
            }

            UnitSelected?.Invoke(targetUnit);
            return;
        }

        // Target cell is empty: request movement
        UnitMoveRequested?.Invoke(selected, targetGrid, targetCell);
    }

    private void HandleMouseHoverPreview()
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I targetGrid = GridMapManager.WorldToGrid(mouseWorld);

        if (_hoverIndicator != null)
        {
            if (_gridMap.IsWithinBounds(targetGrid))
            {
                _hoverIndicator.GlobalPosition = GridMapManager.GridToWorldCenter(targetGrid);
                _hoverIndicator.Visible = true;
            }
            else
            {
                _hoverIndicator.Visible = false;
            }
        }

        var selected = _getSelectedUnit();
        if (selected == null || selected.IsMoving || selected.FactionId != 0)
        {
            _pathVisualizer.ClearPath();
            TileHoverCleared?.Invoke();
            return;
        }

        if (!_gridMap.IsWithinBounds(targetGrid) || targetGrid == selected.GridPosition)
        {
            _pathVisualizer.ClearPath();
            TileHoverCleared?.Invoke();
            return;
        }

        var targetCell = _gridMap.GetCell(targetGrid);
        if (targetCell?.OccupyingUnit != null)
        {
            _pathVisualizer.ClearPath();
            TileHoverCleared?.Invoke();
            return;
        }

        _pathfinding.SetPointSolid(selected.GridPosition, false);
        var pathSpan = _pathfinding.FindPathSpan(selected.GridPosition, targetGrid);
        _pathfinding.SetPointSolid(selected.GridPosition, true);

        if (pathSpan.Length > 1)
        {
            int cost = _pathfinding.CalculatePathCost(pathSpan, _gridMap);
            bool reachable = cost <= selected.MovementRangeRemaining;
            _pathVisualizer.ShowPath(pathSpan, reachable);
            TileHovered?.Invoke(targetGrid);
        }
        else
        {
            _pathVisualizer.ClearPath();
            TileHoverCleared?.Invoke();
        }
    }
}

