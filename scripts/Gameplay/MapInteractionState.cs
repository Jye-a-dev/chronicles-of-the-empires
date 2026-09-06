namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Dictates interactive board authority. Used to atomically block clicks and hotkeys
/// while movements, combat resolutions, or AI turns are executing.
/// </summary>
public enum MapInteractionState
{
    Idle,
    UnitMoving,
    CombatResolving,
    AITurnProcessing,
    Disabled
}

