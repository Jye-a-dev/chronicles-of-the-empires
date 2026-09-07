namespace ChroniclesOfTheEmpires.Core.Game;

/// <summary>
/// Dictates terminal state outcome for the current campaign or skirmish match.
/// </summary>
public enum GameResult
{
    Undecided = 0,
    Victory = 1,
    Defeat = 2
}

/// <summary>
/// Specific tactical or strategic cause triggering match victory or defeat.
/// </summary>
public enum VictoryType
{
    None = 0,
    Conquest = 1,          // All rival units eliminated
    Regicide = 2,          // Enemy leader / warlord destroyed
    Domination = 3,        // Reached required territorial control threshold
    DeficitCollapse = 4    // Economic bankruptcy or catastrophic famine
}

