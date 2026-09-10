namespace ChroniclesOfTheEmpires.Core.Economy;

public enum BiomeType
{
    Plains = 0,
    Forest = 1,
    River = 2,
    Mountain = 3,
    Highlands = 4,
    Swamp = 5,
    Ocean = 6,
    Hill = 7
}

public enum ImprovementType
{
    None = 0,
    Farm = 1,
    Mine = 2,
    LumberMill = 3,
    Watchtower = 4
}

/// <summary>
/// Level 1 Map Entity: Pure data representation of terrain cell properties,
/// movement costs, barrier collision, and baseline periodic yield bundles.
/// </summary>
public record TileTerrainData
{
    public BiomeType Biome { get; init; } = BiomeType.Plains;
    public string Name { get; init; } = "Đồng Bằng";
    public string Description { get; init; } = "";
    public int MoveCost { get; init; } = 1;
    public bool IsBlocked { get; init; } = false;
    public ResourceBundle BaseYield { get; init; } = ResourceBundle.Zero;

    // Reference to Level 2 Deposit
    public ResourceDepositData? Deposit { get; set; }

    // Level 3 Tile Improvement / Exploitation
    public ImprovementType Improvement { get; set; } = ImprovementType.None;
    public bool IsConstructed { get; set; } = false;
    public int ConstructionTurnsRemaining { get; set; } = 0;
    public ResourceBundle ImprovementBonusYield { get; set; } = ResourceBundle.Zero;
}

