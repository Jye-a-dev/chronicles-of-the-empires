using Godot;

namespace ChroniclesOfTheEmpires.Core.Economy;

/// <summary>
/// Level 2 Faction Entity: Building representation holding static bonuses and Level 3 resource costs.
/// </summary>
public record BuildingData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "infrastructure"; // infrastructure, military, religious, commercial
    public Vector2I GridPosition { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefenseBarrier { get; init; } = false;

    // Level 3 Economy Contracts
    public ResourceBundle ProductionCost { get; init; } = ResourceBundle.Zero;
    public ResourceBundle Upkeep { get; init; } = ResourceBundle.Zero;
    public ResourceBundle YieldBonus { get; init; } = ResourceBundle.Zero;
}

