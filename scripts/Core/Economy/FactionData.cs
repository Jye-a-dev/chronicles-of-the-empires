using System.Collections.Generic;
using Godot;

namespace ChroniclesOfTheEmpires.Core.Economy;

/// <summary>
/// Level 1 Faction Entity: Strategic faction state holding treasury stockpiles,
/// cultural identity, controlled territorial cells, and collections of active buildings and military units.
/// </summary>
public class FactionData
{
    public int FactionId { get; init; }
    public string Name { get; set; } = "";
    public string CulturalSphere { get; set; } = "EastAsian"; // EastAsian, SoutheastAsian, Nomadic, etc.

    // Strategic Resource Stockpile
    public ResourceBundle Treasury { get; set; } = ResourceBundle.Zero;

    // Level 2 Entity Lists
    public List<UnitData> Units { get; } = new();
    public List<BuildingData> Buildings { get; } = new();

    // Territorial Control
    public HashSet<Vector2I> ControlledTiles { get; } = new();

    // Deficit & Sanction States
    public bool IsRecruitmentLocked { get; set; } = false;
    public int MoralePenalty { get; set; } = 0; // Negative offset applied to attack/defense
}

