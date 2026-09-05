using Godot;

namespace ChroniclesOfTheEmpires.Core.Economy;

/// <summary>
/// Level 2 Faction Entity: Tactical military unit representation with stats, coordinates, and Level 3 resource costs.
/// </summary>
public record UnitData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int FactionId { get; init; }
    public int HpMax { get; init; } = 20;
    public int CurrentHp { get; set; } = 20;
    public int Attack { get; set; } = 5;
    public int Defense { get; set; } = 2;
    public int MovementMax { get; init; } = 4;
    public int MovementRemaining { get; set; } = 4;
    public Vector2I GridPosition { get; set; }
    public bool IsActive { get; set; } = true;

    // Level 3 Economy Contracts
    public ResourceBundle ProductionCost { get; init; } = ResourceBundle.Zero;
    public ResourceBundle Upkeep { get; init; } = ResourceBundle.Zero;
}

