namespace ChroniclesOfTheEmpires.Core.Economy;

public enum DepositCategory
{
    Mineral = 0,      // Khoáng sản (Iron, Gold, Copper, Stone)
    Agricultural = 1, // Nông sản (Rice, Livestock, Timber, Spices)
    Cultural = 2      // Văn hóa / Tín ngưỡng (Relics, Sacred Grove)
}

/// <summary>
/// Level 2 Map Entity: Specialized resource deposit overlaying a hex cell.
/// Provides extra yields when the required improvement has been constructed and active.
/// </summary>
public record ResourceDepositData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public DepositCategory Category { get; init; } = DepositCategory.Mineral;
    public string RequiredImprovement { get; init; } = ""; // e.g., "mine", "granary", "sawmill"
    public ResourceBundle BonusYield { get; init; } = ResourceBundle.Zero;
    public bool IsTradeable { get; init; } = true;
    public bool IsExploited { get; set; } = false;
}

