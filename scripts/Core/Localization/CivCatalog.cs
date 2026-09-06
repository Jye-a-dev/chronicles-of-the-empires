using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public record CivRecord(
    string Id,
    string BlockId,
    string NameKey,
    string EpithetKey,
    string ColorKey,
    string AttireKey,
    string UnitKey,
    string TraitKey
);

public record CulturalBlockRecord(
    string Id,
    string NameKey,
    string RegionKey,
    string DescriptionKey,
    string[] CivIds
);

public static partial class LocalizationManager
{
    public static readonly CulturalBlockRecord[] CulturalBlocks =
    [
        new("block_alluvium", "BLOCK_ALLUVIUM_NAME", "BLOCK_ALLUVIUM_REGION", "BLOCK_ALLUVIUM_DESC", ["vn", "khmer", "india", "persia"]),
        new("block_mandate", "BLOCK_MANDATE_NAME", "BLOCK_MANDATE_REGION", "BLOCK_MANDATE_DESC", ["china", "japan", "korea", "mongolia"]),
        new("block_steel", "BLOCK_STEEL_NAME", "BLOCK_STEEL_REGION", "BLOCK_STEEL_DESC", ["greece", "england", "france", "germany", "spain"]),
        new("block_frost", "BLOCK_FROST_NAME", "BLOCK_FROST_REGION", "BLOCK_FROST_DESC", ["slavs", "bulgars", "rus", "poland", "mesoamerica"])
    ];

    public static readonly CivRecord[] Civilizations =
    [
        // Block 1: Pantheons & Alluvium
        new("vn", "block_alluvium", "CIV_VN_NAME", "CIV_VN_EPITHET", "CIV_VN_COLORS", "CIV_VN_ATTIRE", "CIV_VN_UNIT", "CIV_VN_TRAIT"),
        new("khmer", "block_alluvium", "CIV_KHMER_NAME", "CIV_KHMER_EPITHET", "CIV_KHMER_COLORS", "CIV_KHMER_ATTIRE", "CIV_KHMER_UNIT", "CIV_KHMER_TRAIT"),
        new("india", "block_alluvium", "CIV_INDIA_NAME", "CIV_INDIA_EPITHET", "CIV_INDIA_COLORS", "CIV_INDIA_ATTIRE", "CIV_INDIA_UNIT", "CIV_INDIA_TRAIT"),
        new("persia", "block_alluvium", "CIV_PERSIA_NAME", "CIV_PERSIA_EPITHET", "CIV_PERSIA_COLORS", "CIV_PERSIA_ATTIRE", "CIV_PERSIA_UNIT", "CIV_PERSIA_TRAIT"),

        // Block 2: Iron & Mandate
        new("china", "block_mandate", "CIV_CHINA_NAME", "CIV_CHINA_EPITHET", "CIV_CHINA_COLORS", "CIV_CHINA_ATTIRE", "CIV_CHINA_UNIT", "CIV_CHINA_TRAIT"),
        new("japan", "block_mandate", "CIV_JAPAN_NAME", "CIV_JAPAN_EPITHET", "CIV_JAPAN_COLORS", "CIV_JAPAN_ATTIRE", "CIV_JAPAN_UNIT", "CIV_JAPAN_TRAIT"),
        new("korea", "block_mandate", "CIV_KOREA_NAME", "CIV_KOREA_EPITHET", "CIV_KOREA_COLORS", "CIV_KOREA_ATTIRE", "CIV_KOREA_UNIT", "CIV_KOREA_TRAIT"),
        new("mongolia", "block_mandate", "CIV_MONGOLIA_NAME", "CIV_MONGOLIA_EPITHET", "CIV_MONGOLIA_COLORS", "CIV_MONGOLIA_ATTIRE", "CIV_MONGOLIA_UNIT", "CIV_MONGOLIA_TRAIT"),

        // Block 3: Sacred Crests & Heavy Steel
        new("greece", "block_steel", "CIV_GREECE_NAME", "CIV_GREECE_EPITHET", "CIV_GREECE_COLORS", "CIV_GREECE_ATTIRE", "CIV_GREECE_UNIT", "CIV_GREECE_TRAIT"),
        new("england", "block_steel", "CIV_ENGLAND_NAME", "CIV_ENGLAND_EPITHET", "CIV_ENGLAND_COLORS", "CIV_ENGLAND_ATTIRE", "CIV_ENGLAND_UNIT", "CIV_ENGLAND_TRAIT"),
        new("france", "block_steel", "CIV_FRANCE_NAME", "CIV_FRANCE_EPITHET", "CIV_FRANCE_COLORS", "CIV_FRANCE_ATTIRE", "CIV_FRANCE_UNIT", "CIV_FRANCE_TRAIT"),
        new("germany", "block_steel", "CIV_GERMANY_NAME", "CIV_GERMANY_EPITHET", "CIV_GERMANY_COLORS", "CIV_GERMANY_ATTIRE", "CIV_GERMANY_UNIT", "CIV_GERMANY_TRAIT"),
        new("spain", "block_steel", "CIV_SPAIN_NAME", "CIV_SPAIN_EPITHET", "CIV_SPAIN_COLORS", "CIV_SPAIN_ATTIRE", "CIV_SPAIN_UNIT", "CIV_SPAIN_TRAIT"),

        // Block 4: Wild Gods & Frost
        new("slavs", "block_frost", "CIV_SLAVS_NAME", "CIV_SLAVS_EPITHET", "CIV_SLAVS_COLORS", "CIV_SLAVS_ATTIRE", "CIV_SLAVS_UNIT", "CIV_SLAVS_TRAIT"),
        new("bulgars", "block_frost", "CIV_BULGARS_NAME", "CIV_BULGARS_EPITHET", "CIV_BULGARS_COLORS", "CIV_BULGARS_ATTIRE", "CIV_BULGARS_UNIT", "CIV_BULGARS_TRAIT"),
        new("rus", "block_frost", "CIV_RUS_NAME", "CIV_RUS_EPITHET", "CIV_RUS_COLORS", "CIV_RUS_ATTIRE", "CIV_RUS_UNIT", "CIV_RUS_TRAIT"),
        new("poland", "block_frost", "CIV_POLAND_NAME", "CIV_POLAND_EPITHET", "CIV_POLAND_COLORS", "CIV_POLAND_ATTIRE", "CIV_POLAND_UNIT", "CIV_POLAND_TRAIT"),
        new("mesoamerica", "block_frost", "CIV_MESO_NAME", "CIV_MESO_EPITHET", "CIV_MESO_COLORS", "CIV_MESO_ATTIRE", "CIV_MESO_UNIT", "CIV_MESO_TRAIT")
    ];
}
