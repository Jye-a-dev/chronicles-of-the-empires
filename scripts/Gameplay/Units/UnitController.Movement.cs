using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class UnitController
{
    /// <summary>
    /// Cancels active movement tween and immediately snaps visual transform to the current Data.GridPosition center.
    /// </summary>
    public void CancelMovement()
    {
        if (_movementTween != null && _movementTween.IsRunning())
        {
            _movementTween.Kill();
            _movementTween = null;
        }

        if (Data != null)
        {
            Data.IsMoving = false;
            Position = GridMapManager.GridToWorldCenter(Data.GridPosition);
        }
        QueueRedraw();
    }

    public void MoveAlongPath(ReadOnlySpan<Vector2I> path, Action? onComplete = null)
    {
        if (Data == null || IsSurrendered)
        {
            onComplete?.Invoke();
            return;
        }

        if (path.Length <= 1)
        {
            Vector2I fallbackPos = path.Length == 1 ? path[0] : Data.GridPosition;
            Position = GridMapManager.GridToWorldCenter(fallbackPos);
            onComplete?.Invoke();
            return;
        }

        // Cancel previous running movement tween safely with fallback snap
        if (_movementTween != null && _movementTween.IsRunning())
        {
            _movementTween.Kill();
            _movementTween = null;
            Position = GridMapManager.GridToWorldCenter(Data.GridPosition);
        }

        Data.IsMoving = true;
        _movementTween = CreateTween().SetTrans(Tween.TransitionType.Linear);

        Vector2I originPos = path[0];
        Vector2I destinationPos = path[^1];
        Vector2 destinationWorldPos = GridMapManager.GridToWorldCenter(destinationPos);

        // Ensure starting position is aligned to origin tile center
        Position = GridMapManager.GridToWorldCenter(originPos);

        // Step-by-step tile movement animation starting from first next step
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 targetWorld = (i == path.Length - 1) ? destinationWorldPos : GridMapManager.GridToWorldCenter(path[i]);
            _movementTween.TweenProperty(this, "position", targetWorld, 0.12f);
        }

        _movementTween.TweenCallback(Callable.From(() =>
        {
            Data.IsMoving = false;
            Position = destinationWorldPos; // Presentation View anchor only; WorldMap orchestrates Data.GridPosition
            QueueRedraw();

            EmitSignal(SignalName.UnitMoved, this, originPos, destinationPos);
            if (Data.MovementRemaining == 0)
            {
                EmitSignal(SignalName.MovementDepleted, this);
            }
            onComplete?.Invoke();
        }));
    }

    public void MoveAlongPath(Vector2I[] path, Action? onComplete = null) =>
        MoveAlongPath(new ReadOnlySpan<Vector2I>(path), onComplete);

    public void MoveAlongPath(ReadOnlySpan<Vector2I> path, int totalCost, Action? onComplete = null)
    {
        if (Data != null)
        {
            Data.MovementRemaining = Math.Max(0, Data.MovementRemaining - totalCost);
        }
        MoveAlongPath(path, onComplete);
    }

    public void MoveAlongPath(Vector2I[] path, int totalCost, Action? onComplete = null) =>
        MoveAlongPath(new ReadOnlySpan<Vector2I>(path), totalCost, onComplete);

    public void ResetTurnMovement()
    {
        Data?.ResetTurnMovement();
        QueueRedraw();
    }

    public void SnapToGrid(Vector2I gridPos)
    {
        if (Data != null)
        {
            Data.GridPosition = gridPos;
        }
        Position = GridMapManager.GridToWorldCenter(gridPos);
    }
}

