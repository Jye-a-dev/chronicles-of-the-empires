using System;
using System.Threading.Tasks;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class WorldMap
{
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

    public async void ExecuteSafeMove(UnitController unit, Vector2I targetGrid, Action? onComplete = null)
    {
        var targetCell = _gridMapManager.GetCell(targetGrid);
        if (targetCell != null)
        {
            await ExecuteSafeMoveAsync(unit, targetGrid, targetCell);
            onComplete?.Invoke();
        }
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
        unit.MoveAlongPath(path, () => tcs.TrySetResult(true));

        using (cts.Token.Register(() => tcs.TrySetResult(false)))
        {
            bool success = await tcs.Task;

            if (success)
            {
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
                unit.Position = GridMapManager.GridToWorldCenter(targetGrid);
                _unitRegistry.UpdatePosition(unit, originGrid, targetGrid);

                _hexIndicator.SelectHex(unit.GlobalPosition);
                _hud.RefreshUnitInfo();

                ExecuteClaimTile(unit.FactionId, targetGrid);

                if (unit.FactionId == 0)
                {
                    _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
                }
            }
            else
            {
                // Fallback / Rollback Phase: Movement tween was aborted, timed out, or interrupted
                _pathfindingManager.SetPointSolid(targetGrid, false);
                _pathfindingManager.SetPointSolid(originGrid, true);

                unit.CancelMovement();
                unit.Data.MovementRemaining = Math.Min(unit.Data.MovementMax, unit.Data.MovementRemaining + totalCost);
                _unitRegistry.UpdatePosition(unit, originGrid, unit.Data.GridPosition);
            }

            if (wasIdle) State = MapInteractionState.Idle;
            return success;
        }
    }
}

