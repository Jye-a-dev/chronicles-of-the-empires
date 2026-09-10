using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// First-class object representation of an individual hexagonal grid cell.
/// Encapsulates geometric positioning, TileTerrainData, resource deposits, improvements, and occupancy state.
/// </summary>
public partial class HexCell : GodotObject
{
    public Vector2I Coords { get; set; }
    public Vector2 WorldPosition { get; set; }
    public TerrainType Terrain { get; set; } = TerrainType.Plains;

    public TileTerrainData TerrainData { get; set; } = new();
    public BuildingData? ConstructedBuilding { get; set; }
    public UnitController? OccupyingUnit { get; set; }
    public int OwnerFactionId { get; set; } = -1; // -1: Neutral / Unclaimed territory
    public FogState Fog { get; set; } = FogState.Unexplored;

    // Backward compatibility delegates to TerrainData
    public string Name => TerrainData.Name;
    public string Description => TerrainData.Description;
    public int MoveCost => TerrainData.MoveCost;
    public int FoodYield => TerrainData.BaseYield.Food + (TerrainData.Deposit is { IsExploited: true } d ? d.BonusYield.Food : 0);
    public int ProdYield => TerrainData.BaseYield.Production + (TerrainData.Deposit is { IsExploited: true } d ? d.BonusYield.Production : 0);
    public int GoldYield => TerrainData.BaseYield.Gold + (TerrainData.Deposit is { IsExploited: true } d ? d.BonusYield.Gold : 0);
    public int SciYield => TerrainData.BaseYield.Science + (TerrainData.Deposit is { IsExploited: true } d ? d.BonusYield.Science : 0);
    public int FaithYield => TerrainData.BaseYield.Faith + (TerrainData.Deposit is { IsExploited: true } d ? d.BonusYield.Faith : 0);

    public bool IsSolid => TerrainData.IsBlocked || (ConstructedBuilding is { IsDefenseBarrier: true });

    public ResourceDepositData? Deposit
    {
        get => TerrainData.Deposit;
        set => TerrainData.Deposit = value;
    }

    public HexCell() { }

    public HexCell(Vector2I coords, Vector2 worldPos, TerrainType terrain)
    {
        Coords = coords;
        WorldPosition = worldPos;
        Terrain = terrain;
        ConfigureTerrainDefaults(terrain);
    }

    public void ConfigureTerrainDefaults(TerrainType terrain)
    {
        Terrain = terrain;
        var config = GameConfigManager.GetHexConfig(terrain);

        BiomeType biome = terrain switch
        {
            TerrainType.Plains => BiomeType.Plains,
            TerrainType.Forest => BiomeType.Forest,
            TerrainType.River => BiomeType.River,
            TerrainType.Mountain => BiomeType.Mountain,
            TerrainType.Ocean => BiomeType.Ocean,
            TerrainType.Hill => BiomeType.Hill,
            _ => BiomeType.Plains
        };

        if (config != null)
        {
            TerrainData = new TileTerrainData
            {
                Biome = biome,
                Name = config.Name,
                Description = config.Description,
                MoveCost = config.MoveCost,
                IsBlocked = config.IsBlocked,
                BaseYield = config.BaseYield,
                Deposit = null
            };
            return;
        }

        // Hardcoded Fallback if config is missing
        TerrainData = terrain switch
        {
            TerrainType.Plains => new TileTerrainData
            {
                Biome = BiomeType.Plains,
                Name = "Đồng Bằng Phù Sa",
                Description = "Đất đai màu mỡ thích hợp canh tác lúa nước.",
                MoveCost = 1,
                IsBlocked = false,
                BaseYield = new ResourceBundle(2, 0, 1, 0, 0)
            },
            TerrainType.Forest => new TileTerrainData
            {
                Biome = BiomeType.Forest,
                Name = "Rừng Rậm Nhiệt Đới",
                Description = "Cung cấp dồi dào gỗ và tài nguyên chế tác.",
                MoveCost = 2,
                IsBlocked = false,
                BaseYield = new ResourceBundle(1, 2, 0, 0, 1)
            },
            TerrainType.River => new TileTerrainData
            {
                Biome = BiomeType.River,
                Name = "Đại Hà Xích Giang",
                Description = "Dòng sông lớn chia cắt địa hình, cung cấp nguồn nước và giao thương.",
                MoveCost = 3,
                IsBlocked = true,
                BaseYield = new ResourceBundle(1, 0, 2, 1, 0)
            },
            TerrainType.Mountain => new TileTerrainData
            {
                Biome = BiomeType.Mountain,
                Name = "Dãy Thần Phong Sơn",
                Description = "Núi cao hiểm trở, bất khả xâm phạm nhưng giàu quặng mỏ.",
                MoveCost = 99,
                IsBlocked = true,
                BaseYield = new ResourceBundle(0, 3, 1, 1, 2)
            },
            TerrainType.Ocean => new TileTerrainData
            {
                Biome = BiomeType.Ocean,
                Name = "Biển Khơi",
                Description = "Vùng biển sâu mênh mông bão táp, ranh giới hải phận tự nhiên giàu nguồn lợi thủy hải sản và muối biển.",
                MoveCost = 99,
                IsBlocked = true,
                BaseYield = new ResourceBundle(1, 0, 1, 0, 0)
            },
            TerrainType.Hill => new TileTerrainData
            {
                Biome = BiomeType.Hill,
                Name = "Gò Đồi",
                Description = "Địa hình đồi thoải chiến lược dễ thủ khó công, dồi dào khoáng sản và thổ nhưỡng thuận lợi dựng tiêu đồn.",
                MoveCost = 2,
                IsBlocked = false,
                BaseYield = new ResourceBundle(1, 1, 0, 0, 0)
            },
            _ => new TileTerrainData()
        };
    }

    /// <summary>
    /// Constructs a building on this hex. If it matches the required improvement of the deposit,
    /// marks the deposit as exploited to activate bonus yields.
    /// </summary>
    public void BuildImprovement(BuildingData building)
    {
        ConstructedBuilding = building;
        building.GridPosition = Coords;

        if (TerrainData.Deposit != null &&
            string.Equals(building.Id, TerrainData.Deposit.RequiredImprovement, System.StringComparison.OrdinalIgnoreCase))
        {
            TerrainData.Deposit.IsExploited = true;
        }
    }
}
