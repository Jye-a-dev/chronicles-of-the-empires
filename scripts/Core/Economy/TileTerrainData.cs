namespace ChroniclesOfTheEmpires.Core.Economy;

public enum BiomeType
{
    Plains = 0,
    Forest = 1,
    River = 2,
    Mountain = 3,
    Highlands = 4,
    Swamp = 5
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
}

