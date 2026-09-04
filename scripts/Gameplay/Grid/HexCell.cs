using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// First-class object representation of an individual hexagonal grid cell.
/// Encapsulates geometric positioning, terrain attributes, resource yields, and occupancy state.
/// </summary>
public partial class HexCell : GodotObject
{
    public Vector2I Coords { get; set; }
    public Vector2 WorldPosition { get; set; }
    public TerrainType Terrain { get; set; } = TerrainType.Plains;
    public string Name { get; set; } = "Đồng Bằng";
    public string Description { get; set; } = string.Empty;
    public int MoveCost { get; set; } = 1;
    public int FoodYield { get; set; } = 2;
    public int ProdYield { get; set; } = 0;
    public int GoldYield { get; set; } = 1;
    public bool IsSolid { get; set; } = false;
    public UnitController? OccupyingUnit { get; set; }
    public int OwnerFactionId { get; set; } = -1; // -1: Neutral / Unclaimed territory

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
        var config = ChroniclesOfTheEmpires.Core.Config.GameConfigManager.GetHexConfig(terrain);
        if (config != null)
        {
            Name = config.Name;
            Description = config.Description;
            MoveCost = config.MoveCost;
            FoodYield = config.FoodYield;
            ProdYield = config.ProdYield;
            GoldYield = config.GoldYield;
            IsSolid = config.IsSolid;
            return;
        }

        switch (terrain)
        {
            case TerrainType.Plains:
                Name = "Đồng Bằng Phù Sa";
                Description = "Đất đai màu mỡ thích hợp canh tác lúa nước.";
                MoveCost = 1;
                FoodYield = 2;
                ProdYield = 0;
                GoldYield = 1;
                IsSolid = false;
                break;

            case TerrainType.Forest:
                Name = "Rừng Rậm Nhiệt Đới";
                Description = "Cung cấp dồi dào gỗ và tài nguyên chế tác.";
                MoveCost = 2;
                FoodYield = 1;
                ProdYield = 2;
                GoldYield = 0;
                IsSolid = false;
                break;

            case TerrainType.River:
                Name = "Sông Hồng";
                Description = "Dòng sông lớn chia cắt địa hình, cung cấp nguồn nước và giao thương.";
                MoveCost = 3;
                FoodYield = 1;
                ProdYield = 0;
                GoldYield = 2;
                IsSolid = true; // Deep river acts as natural boundary
                break;

            case TerrainType.Mountain:
                Name = "Dãy Hoàng Liên Sơn";
                Description = "Núi cao hiểm trở, bất khả xâm phạm nhưng giàu quặng mỏ.";
                MoveCost = 99;
                FoodYield = 0;
                ProdYield = 3;
                GoldYield = 1;
                IsSolid = true;
                break;
        }
    }
}

