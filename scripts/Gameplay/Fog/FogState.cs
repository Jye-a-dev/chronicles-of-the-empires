namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Fog of War visibility state for individual hex cells.
/// </summary>
public enum FogState
{
    /// <summary>Completely pitch black, hides static terrain and units.</summary>
    Unexplored = 0,

    /// <summary>Previously explored terrain is visible, but enemy units remain hidden.</summary>
    Fogged = 1,

    /// <summary>Actively illuminated by allied units; full terrain and unit visibility.</summary>
    Visible = 2
}

