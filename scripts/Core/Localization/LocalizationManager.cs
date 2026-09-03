using Godot;
using System;
using System.Collections.Generic;

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

/// <summary>
/// Centralized localization service managing dynamic English and Vietnamese translations
/// for Chronicles of the Old Empires: Mythic Ages (Vạn Quốc Phong Vân Ký).
/// </summary>
public static class LocalizationManager
{
    public const string LangEnglish = "en";
    public const string LangVietnamese = "vi";

    public static event Action? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = LangEnglish;

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

    private static readonly Dictionary<string, string> EnTranslations = new()
    {
        // Core Game Title
        ["MENU_TITLE"] = "CHRONICLES OF THE OLD EMPIRES",
        ["MENU_SUBTITLE"] = "◆ MYTHIC AGES ◆",
        ["MENU_NEW_CAMPAIGN"] = "New Campaign ▸",
        ["MENU_LOAD_GAME"] = "Load Game",
        ["MENU_SETTINGS"] = "Settings",
        ["MENU_EXIT"] = "Exit to Desktop",
        ["MENU_ARCHIVES"] = "📖 Archives",
        ["MENU_CREDITS"] = "👥 Credits",
        ["MENU_DISCORD"] = "💬 Discord",
        ["MENU_ARCHIVES_TOOLTIP"] = "Civilopedia & Chronicles Archives",
        ["MENU_CREDITS_TOOLTIP"] = "Development Credits",
        ["MENU_DISCORD_TOOLTIP"] = "Official Community Discord",

        // Campaign Mode Selector
        ["CAMPAIGN_MODAL_TITLE"] = "SELECT CAMPAIGN MODE",
        ["CAMPAIGN_HEADER"] = "SELECT CAMPAIGN MODE",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "⚔ Campaign (Stages)",
        ["CAMPAIGN_TAB_SKIRMISH"] = "⚡ Skirmish (Map)",
        ["CAMPAIGN_TAB_TRAINING"] = "📜 Practice & Sandbox",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ Campaign",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ Skirmish",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Tutorial",
        ["CAMPAIGN_DEPLOY"] = "⚔ DEPLOY CAMPAIGN",
        ["CAMPAIGN_START_SKIRMISH"] = "⚡ START SKIRMISH",
        ["CAMPAIGN_START_TRAINING"] = "📜 START TRAINING",
        ["SPEC_MAP_PREFIX"] = "Map: ",
        ["SPEC_RIVALS_PREFIX"] = "Rivals: ",
        ["SPEC_VICTORY_PREFIX"] = "Objective: ",
        ["SPEC_REWARD_PREFIX"] = "Reward: ",

        // Mode Details for CampaignModePanel
        ["MODE_GRAND_NAME"] = "Grand Campaign",
        ["MODE_GRAND_ERA"] = "Epoch of Mythic Empires (4X Macro & Pausable RTS)",
        ["MODE_GRAND_DESC"] = "Lead a legendary civilization across ancient scroll provinces. Construct wonders, harvest alluvial basins, and resolve tactical regiment battles in pausable real-time.",
        ["MODE_GRAND_MAP"] = "Continental Scroll Provinces",
        ["MODE_GRAND_RIVALS"] = "16 Rival Dynasties across 4 Spheres",
        ["MODE_GRAND_VICTORY"] = "Total Imperial Hegemony or Wonder Dominance",

        ["MODE_SKIRMISH_NAME"] = "Custom Skirmish",
        ["MODE_SKIRMISH_ERA"] = "Tactical Sandbox (Custom Parameters)",
        ["MODE_SKIRMISH_DESC"] = "Deploy instant tactical warfare across isometric grids or province territories with customizable AI factions, terrain biomes, and victory conditions.",
        ["MODE_SKIRMISH_MAP"] = "Configurable (Small, Standard, Continental)",
        ["MODE_SKIRMISH_RIVALS"] = "2 to 8 Factions",
        ["MODE_SKIRMISH_VICTORY"] = "Conquest, Wonder Hegemony, Regicide",

        ["MODE_TUTORIAL_NAME"] = "Tactical Training & Sandbox",
        ["MODE_TUTORIAL_ERA"] = "Command Drills & Unit Proving Grounds",
        ["MODE_TUTORIAL_DESC"] = "Master regiment maneuvers, Active Pause (Spacebar) command chains, terrain elevation bonuses, and unit counter-mechanics.",
        ["MODE_TUTORIAL_MAP"] = "Training Grounds & Isometric Testing Arena",
        ["MODE_TUTORIAL_RIVALS"] = "1 Sparring Chieftain",
        ["MODE_TUTORIAL_VICTORY"] = "Complete Command Objectives",

        // Cultural Blocks
        ["BLOCK_ALLUVIUM_NAME"] = "Pantheons & Alluvium",
        ["BLOCK_ALLUVIUM_REGION"] = "South & Southwest Asian River Basins",
        ["BLOCK_ALLUVIUM_DESC"] = "Fertile floodplains, ancient river redoubts, sacred temples, and mystical rituals.",

        ["BLOCK_MANDATE_NAME"] = "Iron & Mandate",
        ["BLOCK_MANDATE_REGION"] = "East Asia & The Great Steppe",
        ["BLOCK_MANDATE_DESC"] = "Rigid iron discipline, siege artillery, nomad horsemen, and divine imperial decrees.",

        ["BLOCK_STEEL_NAME"] = "Sacred Crests & Heavy Steel",
        ["BLOCK_STEEL_REGION"] = "Mediterranean Shores & Western Europe",
        ["BLOCK_STEEL_DESC"] = "Impenetrable phalanxes, knightly shock charges, massed longbows, and steel plate armaments.",

        ["BLOCK_FROST_NAME"] = "Wild Gods & Frost",
        ["BLOCK_FROST_REGION"] = "Eastern Europe, Balkans & Ancient Woodlands",
        ["BLOCK_FROST_DESC"] = "Pagan pantheons, blizzard resilience, fearsome cavalry charges, and dense primeval warfare.",

        // 18 Civilizations
        // 1. Vietnam
        ["CIV_VN_NAME"] = "Vietnam (Au Lac / Baiyue)",
        ["CIV_VN_EPITHET"] = "Lords of the Alluvial Floodplains",
        ["CIV_VN_COLORS"] = "Verdigris (#43B3AE) & Rice Gold (#E5A93C)",
        ["CIV_VN_ATTIRE"] = "Tall feathered Lac headdress, round Dong Son bronze pectoral, woven loincloth",
        ["CIV_VN_UNIT"] = "Lien Chau Crossbowmen & Rattan-Armored War Elephants",
        ["CIV_VN_TRAIT"] = "Stealth in wetlands, armor-piercing volleys, submerged river stakes, permanent flood buffs",

        // 2. Khmer
        ["CIV_KHMER_NAME"] = "Khmer (Angkor / Funan)",
        ["CIV_KHMER_EPITHET"] = "Architects of the Golden Reservoir",
        ["CIV_KHMER_COLORS"] = "Placer Gold (#D4AF37) & Terracotta (#8B4513)",
        ["CIV_KHMER_ATTIRE"] = "Gold-woven Sampot, terracotta Apsara cuirass, body-covered Yantra ritual tattoos",
        ["CIV_KHMER_UNIT"] = "Gilded-Tusk War Elephants & Temple Guardians",
        ["CIV_KHMER_TRAIT"] = "Baray reservoir network provides immense sustenance in arid biomes",

        // 3. India
        ["CIV_INDIA_NAME"] = "India (Ancient Maurya)",
        ["CIV_INDIA_EPITHET"] = "Keepers of the Solar Chakra",
        ["CIV_INDIA_COLORS"] = "Saffron (#FF9933) & Silk White (#F8F8FF)",
        ["CIV_INDIA_ATTIRE"] = "Broad jewel-crested turban, translucent silk tunic trimmed with silver filigree",
        ["CIV_INDIA_UNIT"] = "Chakram Throwers & Luminescent Ceremonial War Elephants",
        ["CIV_INDIA_TRAIT"] = "Ricocheting circular disc projectiles and aura of radiant morale inspiration",

        // 4. Persia
        ["CIV_PERSIA_NAME"] = "Persia (Sassanid Arcana)",
        ["CIV_PERSIA_EPITHET"] = "Heirs of the Immortals",
        ["CIV_PERSIA_COLORS"] = "Tyrian Purple (#66023C) & Turmeric Gold (#DDAA00)",
        ["CIV_PERSIA_ATTIRE"] = "Half-face purple silk veil, gold-spun tunic over jewel-scale armor, woven wicker shield",
        ["CIV_PERSIA_UNIT"] = "Cataphract Cavalry & The Immortals Legion",
        ["CIV_PERSIA_TRAIT"] = "The Immortals resurrect upon falling in battle; heavy shock cavalry charges",

        // 5. China
        ["CIV_CHINA_NAME"] = "China (Ancient Huaxia)",
        ["CIV_CHINA_EPITHET"] = "Masters of the Iron Bastion",
        ["CIV_CHINA_COLORS"] = "Imperial Crimson (#8B0000) & Jet Black (#1C1C1C)",
        ["CIV_CHINA_ATTIRE"] = "Square Qin-Han iron lamellar plates, iron helm with long red pheasant plumes",
        ["CIV_CHINA_UNIT"] = "Rocket Cart Artillery & Heavy Siege Crossbow Batteries",
        ["CIV_CHINA_TRAIT"] = "Rapid fortress construction and high-speed imperial cultural assimilation",

        // 6. Japan
        ["CIV_JAPAN_NAME"] = "Japan (Yamato / Shinto)",
        ["CIV_JAPAN_EPITHET"] = "Blades of the Kami Sun",
        ["CIV_JAPAN_COLORS"] = "Snow White (#FFFAFA) & Torii Vermilion (#C8382B)",
        ["CIV_JAPAN_ATTIRE"] = "Lacquered wood O-Yoroi armor, Kabuto helm with grand crescent moon, back banner Sashimono",
        ["CIV_JAPAN_UNIT"] = "Blood-Pact Samurai & Shinto Onmyoji Diviners",
        ["CIV_JAPAN_TRAIT"] = "Samurai gain exponential attack when low on HP; Onmyoji invoke obscuring battlefield mists",

        // 7. Korea
        ["CIV_KOREA_NAME"] = "Korea (Goguryeo / Joseon)",
        ["CIV_KOREA_EPITHET"] = "Sentinels of the Pine Ridges",
        ["CIV_KOREA_COLORS"] = "Cobalt Blue (#0047AB) & Leather Brown (#7B3F00)",
        ["CIV_KOREA_ATTIRE"] = "Felt peaked hat adorned with dual raptor feathers, flexible fish-scale armor",
        ["CIV_KOREA_UNIT"] = "Mounted Hwarang Archers & Hwacha Rocket Fire-Carts",
        ["CIV_KOREA_TRAIT"] = "Uphill archery damage bonus and relentless fire-arrow barrages",

        // 8. Mongolia
        ["CIV_MONGOLIA_NAME"] = "Mongolia (Great Khan Steppe)",
        ["CIV_MONGOLIA_EPITHET"] = "Riders of the Eternal Blue Sky",
        ["CIV_MONGOLIA_COLORS"] = "Steppe Azure (#4682B4) & Wolf Fur Brown (#5C4033)",
        ["CIV_MONGOLIA_ATTIRE"] = "Flapped sheepskin fur cap, fur-lined leather Deel robe, composite recurve bow",
        ["CIV_MONGOLIA_UNIT"] = "Keshik Horse Archers & Nomad Heavy Lancers",
        ["CIV_MONGOLIA_TRAIT"] = "All-cavalry roster; Parting shot mechanics with instantaneous post-attack hit-and-run",

        // 9. Greece
        ["CIV_GREECE_NAME"] = "Greece (Hellenic / Sparta & Athens)",
        ["CIV_GREECE_EPITHET"] = "Shieldwall of the Aegean",
        ["CIV_GREECE_COLORS"] = "Antique Bronze (#CD7F32) & Spartan Red (#990000)",
        ["CIV_GREECE_ATTIRE"] = "Corinthian bronze helm with curved horsehair crest, Lambda Aspis shield, anatomical muscle cuirass",
        ["CIV_GREECE_UNIT"] = "Hoplite Phalanx & Ramming Trireme Galleys",
        ["CIV_GREECE_TRAIT"] = "Phalanx wall reflects 100% of direct frontal melee attacks",

        // 10. England
        ["CIV_ENGLAND_NAME"] = "England (Anglo-Saxon / Arthurian)",
        ["CIV_ENGLAND_EPITHET"] = "Yeomen of the Green Boughs",
        ["CIV_ENGLAND_COLORS"] = "Forest Green (#228B22) & Mud Brown (#6E5334)",
        ["CIV_ENGLAND_ATTIRE"] = "Tall yew longbow exceeding warrior height, coarse wool doublet, round iron kettle hat",
        ["CIV_ENGLAND_UNIT"] = "Arthurian Longbowmen & Billmen",
        ["CIV_ENGLAND_TRAIT"] = "Longest projectile range in the game; can deploy anti-cavalry sharpened stakes",

        // 11. France
        ["CIV_FRANCE_NAME"] = "France (Royal Knights)",
        ["CIV_FRANCE_EPITHET"] = "Champions of the Fleur-de-lis",
        ["CIV_FRANCE_COLORS"] = "Royal Blue (#4169E1) & Silver White (#E0E0E0)",
        ["CIV_FRANCE_ATTIRE"] = "Warhorse caparisoned in gold-embroidered lilies, knight in great helm with cross visor slits, couched lance",
        ["CIV_FRANCE_UNIT"] = "Chevalier Paladins & Royal Crossbow Garrisons",
        ["CIV_FRANCE_TRAIT"] = "Devastating couched lance charge fractures enemy formations and shields nearby allies",

        // 12. Germany
        ["CIV_GERMANY_NAME"] = "Germany (Holy Roman / Teutonic)",
        ["CIV_GERMANY_EPITHET"] = "Iron Vanguard of the Cross",
        ["CIV_GERMANY_COLORS"] = "Teutonic Black (#111111) & Pure White (#F5F5F5)",
        ["CIV_GERMANY_ATTIRE"] = "White surcoat bearing black chest cross, horned/winged Great Helm, full Gothic plate",
        ["CIV_GERMANY_UNIT"] = "Zweihander Greatswordsmen & Teutonic Knights",
        ["CIV_GERMANY_TRAIT"] = "Zweihander sweeps 3 tiles wide, specializing in sundering enemy pike and spear lines",

        // 13. Spain
        ["CIV_SPAIN_NAME"] = "Spain (Reconquista Kingdom)",
        ["CIV_SPAIN_EPITHET"] = "Bastion of the Silver Pike",
        ["CIV_SPAIN_COLORS"] = "Castilian Gold (#DAA520) & Crimson Scarlet (#C70039)",
        ["CIV_SPAIN_ATTIRE"] = "Dual-pointed Morion comb helm, burnished plate cuirass, Toledo steel blade",
        ["CIV_SPAIN_UNIT"] = "Tercio Pike-and-Shot Regiments & Conquistadors",
        ["CIV_SPAIN_TRAIT"] = "Tercio square formation is completely immune to frontal cavalry charges",

        // 14. Ancient Slavs
        ["CIV_SLAVS_NAME"] = "Ancient Slavs (Pagan / Perun's Host)",
        ["CIV_SLAVS_EPITHET"] = "Sons of the Thunder Oak",
        ["CIV_SLAVS_COLORS"] = "Embroidery Red (#C8102E) & Wolf Grey (#696969)",
        ["CIV_SLAVS_ATTIRE"] = "White linen shirt with red folk embroidery (Vyshyvanka), wolf pelt mantle, beaded beard braids",
        ["CIV_SLAVS_UNIT"] = "Vityaz Woodsmen & Perun Thunder Priests",
        ["CIV_SLAVS_TRAIT"] = "Vityaz regenerates health near woodland borders; completely immune to frost movement slows",

        // 15. Ancient Bulgars
        ["CIV_BULGARS_NAME"] = "Ancient Bulgars (Khan Krum Khaganate)",
        ["CIV_BULGARS_EPITHET"] = "Riders of the Tangra Tug",
        ["CIV_BULGARS_COLORS"] = "Blood Red (#8A0303) & Charcoal Black (#232B2B)",
        ["CIV_BULGARS_ATTIRE"] = "Curved conical helm with black horsehair tail, sacred Tangra standard (Tug), suede over scale armor",
        ["CIV_BULGARS_UNIT"] = "Boyar Heavy Cavalry & Frenzied Clan Footmen",
        ["CIV_BULGARS_TRAIT"] = "Boyars loot bonus resources from fallen commanders; infantry enter frenzied Blood Oath near death",

        // 16. Kievan Rus
        ["CIV_RUS_NAME"] = "Kievan Rus (Mythic Dnieper)",
        ["CIV_RUS_EPITHET"] = "Sentinels of the Boreal Ice",
        ["CIV_RUS_COLORS"] = "Burgundy Red (#800020) & Snow White (#FFFAFA)",
        ["CIV_RUS_ATTIRE"] = "Thick bear-fur mantle, pointed Spangenhelm with chainmail aventail, two-handed Bardiche axe",
        ["CIV_RUS_UNIT"] = "Druzhina Hearthguard & Bardiche Berserkers",
        ["CIV_RUS_TRAIT"] = "Druzhina gains maximum armor and defense when stationed in wetlands or snowstorms",

        // 17. Poland
        ["CIV_POLAND_NAME"] = "Poland (Mythic Husaria)",
        ["CIV_POLAND_EPITHET"] = "Sovereigns of the Eagle Wings",
        ["CIV_POLAND_COLORS"] = "Polish Crimson (#DC143C) & Feather White (#FFFFFF)",
        ["CIV_POLAND_ATTIRE"] = "Arching eagle-feather frame strapped across polished steel breastplate, pennoned lance",
        ["CIV_POLAND_UNIT"] = "Winged Husaria & Pancerni Cavalry",
        ["CIV_POLAND_TRAIT"] = "Full-speed charge generates terrifying wind screech, causing enemy regiments to waver and flee",

        // 18. Mesoamerica
        ["CIV_MESO_NAME"] = "Ancient Mesoamerica (Maya / Mississippian)",
        ["CIV_MESO_EPITHET"] = "Warriors of the Feathered Serpent",
        ["CIV_MESO_COLORS"] = "Clay Ochre (#B22222) & Quetzal Green (#00A86B)",
        ["CIV_MESO_ATTIRE"] = "Spreading harpy eagle feathered headdress, painted body glyphs, spotted jaguar pelt armor",
        ["CIV_MESO_UNIT"] = "Jaguar Shock Warriors & Atlatl Dart Throwers",
        ["CIV_MESO_TRAIT"] = "Jaguar warriors run at full cavalry speed; zero terrain movement penalty over mountains or jungle",

        // Stages
        ["STAGE_1_TITLE"] = "Stage 1: Dawn of Van Lang",
        ["STAGE_1_DESC"] = "Rally tribal peasants, master the Red River basin, and establish the earliest bronze-age clan.",
        ["STAGE_1_OBJ"] = "Cultivate 3 wet-rice paddies & train 15 militiamen",
        ["STAGE_1_ENEMY"] = "Black River Barbarians",
        ["STAGE_1_REWARD"] = "Unlock: Early Bronze Metallurgy & Fire Crossbow",

        ["STAGE_2_TITLE"] = "Stage 2: Bronze Drums of Dong Son",
        ["STAGE_2_DESC"] = "Mine tin and copper veins, forge sacred Dong Son bronze drums, and inspire clan warriors.",
        ["STAGE_2_OBJ"] = "Cast 1 Sacred Bronze Drum & upgrade forge to Tier 2",
        ["STAGE_2_ENEMY"] = "Yelang Tribe",
        ["STAGE_2_REWARD"] = "Unlock: Bronze Axemen & Defensive Moats",

        ["STAGE_3_TITLE"] = "Stage 3: Magic Crossbow of Co Loa",
        ["STAGE_3_DESC"] = "Construct the spiral ramparts of Co Loa and deploy repeating crossbows against invaders.",
        ["STAGE_3_OBJ"] = "Defend Citadel & repel 5 vanguard assault waves",
        ["STAGE_3_ENEMY"] = "Trieu Da Invaders",
        ["STAGE_3_REWARD"] = "Unlock: Co Loa Repeating Crossbowmen",

        ["STAGE_4_TITLE"] = "Stage 4: War Elephants of Me Linh",
        ["STAGE_4_DESC"] = "Tame majestic war elephants and lead an uprising against hostile occupying garrisons.",
        ["STAGE_4_OBJ"] = "Tame 4 War Elephants & destroy hostile fortress",
        ["STAGE_4_ENEMY"] = "Garrison of Prefect To Dinh",
        ["STAGE_4_REWARD"] = "Unlock: Armored War Elephants",

        ["STAGE_5_TITLE"] = "Stage 5: Unification of Baiyue",
        ["STAGE_5_DESC"] = "Unite the sixteen tribal confederacies atop Nghia Linh Mountain and forge a thousand-year dynasty.",
        ["STAGE_5_OBJ"] = "Subjugate 3 rival factions or construct Van Lang Monument",
        ["STAGE_5_ENEMY"] = "Alliance of 3 Northern Chieftains",
        ["STAGE_5_REWARD"] = "Title: First Hung King & Unifier of Realm",

        // Skirmish Settings
        ["SKIRMISH_MAP_SIZE"] = "Map Size",
        ["SKIRMISH_BIOME"] = "Battlefield Biome",
        ["SKIRMISH_RIVALS"] = "Number of Rivals",
        ["SKIRMISH_VICTORY"] = "Victory Condition",
        ["SKIRMISH_DIFFICULTY"] = "AI Difficulty",
        ["MAP_SMALL"] = "Small (32x32) - Fast Skirmish",
        ["MAP_MEDIUM"] = "Standard (64x64) - Tactical",
        ["MAP_LARGE"] = "Continental (128x128) - Grand War",
        ["BIOME_RED_RIVER"] = "Red River Basin (Alluvial paddies, stakes)",
        ["BIOME_JUNGLE"] = "Tropical Rainforest (Mist & swamps)",
        ["BIOME_HIGHLANDS"] = "Karst Highlands (Mines & passes)",
        ["BIOME_STEPPE"] = "Eurasian Steppe (Plains & cavalry)",
        ["BIOME_TAIGA"] = "Frost Taiga (Snow & marshes)",
        ["BIOME_MEDITERRANEAN"] = "Aegean Coast (Harbors & naval)",
        ["RIVALS_2"] = "2 Factions (Duel)",
        ["RIVALS_4"] = "4 Factions (Standard FFA)",
        ["RIVALS_6"] = "6 Factions (Large Battle)",
        ["RIVALS_8"] = "8 Factions (All-out War)",
        ["VICTORY_CONQUEST"] = "Military Conquest",
        ["VICTORY_CULTURE"] = "Cultural Wonder Hegemony",
        ["VICTORY_REGICIDE"] = "Regicide (Assassination)",
        ["DIFF_EASY"] = "Easy (Apprentice)",
        ["DIFF_NORMAL"] = "Standard (Commander)",
        ["DIFF_HARD"] = "Hard (Chieftain)",
        ["DIFF_LEGEND"] = "Legendary (Hung King)",

        // Training
        ["TRAINING_SELECT"] = "Select Mode",
        ["TRAINING_SELECTED"] = "Selected",
        ["TRAINING_TUTORIAL_NAME"] = "Core Tutorial",
        ["TRAINING_SANDBOX_NAME"] = "Unit Testing Sandbox",

        // Settings
        ["SETTINGS_TITLE"] = "SETTINGS",
        ["SETTINGS_TAB_AUDIO"] = "🔊 Audio",
        ["SETTINGS_TAB_VIDEO"] = "🖥 Display",
        ["SETTINGS_TAB_GAMEPLAY"] = "🌐 Language",
        ["SETTINGS_MASTER_VOL"] = "Master Volume",
        ["SETTINGS_SFX_VOL"] = "Sound Effects",
        ["SETTINGS_MUSIC_VOL"] = "Music Volume",
        ["SETTINGS_FULLSCREEN"] = "Fullscreen Mode",
        ["SETTINGS_VSYNC"] = "Vertical Sync (VSync)",
        ["SETTINGS_LANGUAGE"] = "Game Language",
        ["SETTINGS_BTN_CLOSE"] = "✕ Close",

        // Display Settings
        ["SETTINGS_WINDOW_MODE"] = "Window Mode",
        ["SETTINGS_RESOLUTION"] = "Resolution",
        ["SETTINGS_VSYNC_LABEL"] = "Vertical Sync (VSync)",
        ["SETTINGS_MAX_FPS"] = "Max Frame Rate",
        ["SETTINGS_MODE_WINDOWED"] = "Windowed",
        ["SETTINGS_MODE_BORDERLESS"] = "Borderless Window",
        ["SETTINGS_MODE_FULLSCREEN"] = "Fullscreen",
        ["SETTINGS_MODE_EXCLUSIVE"] = "Exclusive Fullscreen",
        ["SETTINGS_VSYNC_DISABLED"] = "Disabled",
        ["SETTINGS_VSYNC_ENABLED"] = "Enabled",
        ["SETTINGS_VSYNC_ADAPTIVE"] = "Adaptive",
        ["SETTINGS_FPS_UNLIMITED"] = "Unlimited",
        ["SETTINGS_UI_SCALE"] = "UI Scale",
        ["SETTINGS_BTN_SAVE"] = "💾 Save & Apply",
        ["SETTINGS_STATUS_SAVED"] = "Settings saved & applied!",
        ["SETTINGS_STATUS_SAVED_VI"] = "Đã lưu & áp dụng cài đặt!"
    };

    private static readonly Dictionary<string, string> ViTranslations = new()
    {
        // Core Game Title
        ["MENU_TITLE"] = "VẠN QUỐC PHONG VÂN KÝ",
        ["MENU_SUBTITLE"] = "◆ THỜI KỲ HUYỀN SỬ ◆",
        ["MENU_NEW_CAMPAIGN"] = "Chiến Dịch Mới ▸",
        ["MENU_LOAD_GAME"] = "Tải Bản Lưu",
        ["MENU_SETTINGS"] = "Cài Đặt",
        ["MENU_EXIT"] = "Thoát Trò Chơi",
        ["MENU_ARCHIVES"] = "📖 Thư Khố",
        ["MENU_CREDITS"] = "👥 Đội Ngũ",
        ["MENU_DISCORD"] = "💬 Cộng Đồng",
        ["MENU_ARCHIVES_TOOLTIP"] = "Thư Khố & Bách Khoa Toàn Thư Vạn Quốc",
        ["MENU_CREDITS_TOOLTIP"] = "Đội Ngũ Phát Triển & Vinh Danh",
        ["MENU_DISCORD_TOOLTIP"] = "Kênh Discord Cộng Đồng Chính Thức",

        // Campaign Mode Selector
        ["CAMPAIGN_MODAL_TITLE"] = "CHỌN CHẾ ĐỘ CHIẾN DỊCH",
        ["CAMPAIGN_HEADER"] = "CHỌN CHẾ ĐỘ CHIẾN DỊCH",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "⚔ Chiến Dịch (Ải)",
        ["CAMPAIGN_TAB_SKIRMISH"] = "⚡ Thư Hùng (Map)",
        ["CAMPAIGN_TAB_TRAINING"] = "📜 Tập Trận & Sandbox",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ Chiến Dịch",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ Thư Hùng",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Tập Trận",
        ["CAMPAIGN_DEPLOY"] = "⚔ VÀO TRẬN (DEPLOY)",
        ["CAMPAIGN_START_SKIRMISH"] = "⚡ VÀO THƯ HÙNG",
        ["CAMPAIGN_START_TRAINING"] = "📜 VÀO TẬP TRẬN",
        ["SPEC_MAP_PREFIX"] = "Sa bàn: ",
        ["SPEC_RIVALS_PREFIX"] = "Đối thủ: ",
        ["SPEC_VICTORY_PREFIX"] = "Mục tiêu: ",
        ["SPEC_REWARD_PREFIX"] = "Phần thưởng: ",

        // Mode Details for CampaignModePanel
        ["MODE_GRAND_NAME"] = "Đại Chiến Dịch",
        ["MODE_GRAND_ERA"] = "Kỷ Nguyên Vạn Quốc (4X Vĩ Mô & RTS Tạm Dừng)",
        ["MODE_GRAND_DESC"] = "Lãnh đạo nền văn minh huyền sử qua các phân vùng lãnh thổ tranh cuộn. Xây dựng kỳ quan, khai phá lưu vực sông và trực tiếp chỉ huy các trung đoàn thời gian thực có tạm dừng.",
        ["MODE_GRAND_MAP"] = "Phân Vùng Lãnh Thổ Tranh Cuộn",
        ["MODE_GRAND_RIVALS"] = "16 Đế Bang thuộc 4 Khối Văn Hóa",
        ["MODE_GRAND_VICTORY"] = "Bá Quyền Đế Chế hoặc Thần Khí Kỳ Quan",

        ["MODE_SKIRMISH_NAME"] = "Thư Hùng Tự Do",
        ["MODE_SKIRMISH_ERA"] = "Sa Bàn Chiến Thuật (Tùy Chỉnh)",
        ["MODE_SKIRMISH_DESC"] = "Khai màn giao tranh tức thì trên lưới thoi isometric hoặc phân vùng lãnh thổ với tùy chọn số phe AI, địa hình sinh thái và điều kiện thắng.",
        ["MODE_SKIRMISH_MAP"] = "Tùy Biến (Nhỏ, Tiêu Chuẩn, Đại Lục)",
        ["MODE_SKIRMISH_RIVALS"] = "2 đến 8 Phe Phái",
        ["MODE_SKIRMISH_VICTORY"] = "Chinh Phục, Kỳ Quan, Trảm Tướng",

        ["MODE_TUTORIAL_NAME"] = "Tập Trận & Sa Bàn Thực Nghiệm",
        ["MODE_TUTORIAL_ERA"] = "Binh Pháp Cơ Bản & Thao Trường Thử Nghiệm",
        ["MODE_TUTORIAL_DESC"] = "Luyện tập thao tác điều khiển trung đoàn, lệnh Tạm Dừng Tác Chiến (Space), khắc chế binh chủng và lợi thế địa hình.",
        ["MODE_TUTORIAL_MAP"] = "Thao Trường & Đấu Trường Isometric",
        ["MODE_TUTORIAL_RIVALS"] = "1 Quân Huấn Luyện",
        ["MODE_TUTORIAL_VICTORY"] = "Hoàn Thành Mục Tiêu Binh Pháp",

        // Cultural Blocks
        ["BLOCK_ALLUVIUM_NAME"] = "Bách Thần & Phù Sa",
        ["BLOCK_ALLUVIUM_REGION"] = "Lưu Vực Sông Nam & Tây Nam Á",
        ["BLOCK_ALLUVIUM_DESC"] = "Đồng bằng châu thổ phì nhiêu, phòng tuyến lòng sông cổ kính, đền tháp linh thiêng và nghi lễ huyền bí.",

        ["BLOCK_MANDATE_NAME"] = "Thiết Kỷ & Thiên Mệnh",
        ["BLOCK_MANDATE_REGION"] = "Đông Bắc Á & Thảo Nguyên Bao La",
        ["BLOCK_MANDATE_DESC"] = "Kỷ luật giáp sắt nghiêm ngặt, hỏa xa công thành, kỵ binh du mục và thiên mệnh hoàng gia.",

        ["BLOCK_STEEL_NAME"] = "Thánh Huy & Trọng Thép",
        ["BLOCK_STEEL_REGION"] = "Bờ Biển Địa Trung Hải & Tây Âu",
        ["BLOCK_STEEL_DESC"] = "Trận đồ giáo khiên bất khả xâm phạm, cú húc kỵ sĩ hoàng gia, bão tên trường cung và thiết giáp kiên cố.",

        ["BLOCK_FROST_NAME"] = "Hoang Thần & Băng Lãnh",
        ["BLOCK_FROST_REGION"] = "Đông Âu, Balkan & Rừng Già Cổ Đại",
        ["BLOCK_FROST_DESC"] = "Tín ngưỡng đa thần nguyên thủy, chiến binh băng giá, kỵ binh cánh đại bàng và dũng khí rừng thiêng.",

        // 18 Civilizations
        // 1. Vietnam
        ["CIV_VN_NAME"] = "Việt Nam (Âu Lạc / Bách Việt)",
        ["CIV_VN_EPITHET"] = "Chúa Tể Lưu Vực Phù Sa",
        ["CIV_VN_COLORS"] = "Xanh Rêu Đồng Cổ (#43B3AE) & Vàng Lúa (#E5A93C)",
        ["CIV_VN_ATTIRE"] = "Nón lông chim Lạc nhọn cao vút, hộ tâm phiến bằng đồng tròn chạm hoa văn Đông Sơn, khố dệt ngắn",
        ["CIV_VN_UNIT"] = "Nỏ Liên Châu & Tượng Binh Giáp Mây",
        ["CIV_VN_TRAIT"] = "Tàng hình trong rừng/đầm lầy, bắn xuyên giáp, bẫy cọc ngầm lòng sông, buff vĩnh viễn sau lũ lụt",

        // 2. Khmer
        ["CIV_KHMER_NAME"] = "Khmer (Đế Chế Angkor / Phù Nam)",
        ["CIV_KHMER_EPITHET"] = "Kiến Trúc Gia Hồ Thần Thủy",
        ["CIV_KHMER_COLORS"] = "Vàng Sa Khoáng (#D4AF37) & Nâu Đất Nung (#8B4513)",
        ["CIV_KHMER_ATTIRE"] = "Vải quấn Sampot dệt vàng, giáp ngực nung khắc tượng Apsara, lính xăm bùa Yantra toàn thân",
        ["CIV_KHMER_UNIT"] = "Voi Bọc Giáp Ngà Vàng & Vệ Binh Đền Tháp",
        ["CIV_KHMER_TRAIT"] = "Hồ Baray cấp lương cực hạn vùng khô cằn, đền tháp tăng cường sinh lực và phòng thủ",

        // 3. India
        ["CIV_INDIA_NAME"] = "Ấn Độ (Vương Triều Maurya Cổ)",
        ["CIV_INDIA_EPITHET"] = "Người Nắm Giữ Vòng Tròn Thần Nhật",
        ["CIV_INDIA_COLORS"] = "Cam Nghệ (#FF9933) & Trắng Lụa (#F8F8FF)",
        ["CIV_INDIA_ATTIRE"] = "Khăn Turban to bản đính ngọc, áo lụa mỏng viền kim loại bạc",
        ["CIV_INDIA_UNIT"] = "Xạ Thủ Phóng Đĩa Chakram & Chiến Tượng Nghi Lễ",
        ["CIV_INDIA_TRAIT"] = "Đĩa Chakram nảy mục tiêu liên hoàn, Chiến tượng nghi lễ phát quang tăng vọt sĩ khí",

        // 4. Persia
        ["CIV_PERSIA_NAME"] = "Ba Tư (Kỳ Thuật Sassanid)",
        ["CIV_PERSIA_EPITHET"] = "Hậu Duệ Quân Đoàn Bất Tử",
        ["CIV_PERSIA_COLORS"] = "Tím Hoàng Gia (#66023C) & Vàng Nghệ (#DDAA00)",
        ["CIV_PERSIA_ATTIRE"] = "Khăn lụa tím bịt nửa mặt, áo dệt sợi vàng khoác ngoài giáp vảy kim hoàn, khiên đan liễu gai",
        ["CIV_PERSIA_UNIT"] = "Kỵ Binh Thiết Giáp Cataphract & Quân Đoàn Bất Tử (Immortals)",
        ["CIV_PERSIA_TRAIT"] = "Quân đoàn Bất Tử tự hồi sinh khi tử trận; kỵ binh thiết giáp húc sập phòng tuyến",

        // 5. China
        ["CIV_CHINA_NAME"] = "Trung Hoa (Hoa Hạ Cổ Triều)",
        ["CIV_CHINA_EPITHET"] = "Bậc Thầy Thành Lũy & Hỏa Lực",
        ["CIV_CHINA_COLORS"] = "Đỏ Thẫm Hoàng Gia (#8B0000) & Đen Then (#1C1C1C)",
        ["CIV_CHINA_ATTIRE"] = "Giáp phiến sắt vuông vức thời Tần - Hán, mũ sắt cắm lông trĩ đỏ dài",
        ["CIV_CHINA_UNIT"] = "Máy Bắn Hỏa Tiễn & Nỏ Cơ Giới Diện Rộng",
        ["CIV_CHINA_TRAIT"] = "Đẩy máy bắn hỏa tiễn, xây thành lũy kiên cố và đồng hóa thành phố siêu tốc",

        // 6. Japan
        ["CIV_JAPAN_NAME"] = "Nhật Bản (Thời Kỳ Yamato / Thần Đạo)",
        ["CIV_JAPAN_EPITHET"] = "Thần Kiếm Mặt Trời Thái Dương",
        ["CIV_JAPAN_COLORS"] = "Trắng Tuyết (#FFFAFA) & Đỏ Son Torii (#C8382B)",
        ["CIV_JAPAN_ATTIRE"] = "Giáp gỗ sơn mài O-Yoroi, mũ Kabuto gắn sừng trăng khuyết lớn, cờ lệnh chữ nhật sau lưng",
        ["CIV_JAPAN_UNIT"] = "Samurai Thí Huyết & Phù Thủy Onmyoji",
        ["CIV_JAPAN_TRAIT"] = "Samurai Thí Huyết tăng sát thương khi cạn máu, phù thủy Onmyoji gọi sương mù che mắt đối phương",

        // 7. Korea
        ["CIV_KOREA_NAME"] = "Hàn Quốc (Tam Quốc Cổ: Goguryeo / Joseon)",
        ["CIV_KOREA_EPITHET"] = "Vệ Tướng Đồi Núi Rìa Đông",
        ["CIV_KOREA_COLORS"] = "Xanh Lam Cobalt (#0047AB) & Nâu Da Thuộc (#7B3F00)",
        ["CIV_KOREA_ATTIRE"] = "Mũ chóp nỉ cắm hai lông chim săn mồi vểnh ngang, giáp vảy cá bó sát linh hoạt",
        ["CIV_KOREA_UNIT"] = "Hwarang Cưỡi Ngựa Bắn Tỉa & Chiến Xa Bắn Bão Tên Hwacha",
        ["CIV_KOREA_TRAIT"] = "Hwarang cưỡi ngựa leo dốc núi bắn tỉa và chiến xa Hwacha phóng bão tên hủy diệt",

        // 8. Mongolia
        ["CIV_MONGOLIA_NAME"] = "Mông Cổ (Thảo Nguyên Đại Hãn)",
        ["CIV_MONGOLIA_EPITHET"] = "Kỵ Sĩ Trời Xanh Vĩnh Hằng",
        ["CIV_MONGOLIA_COLORS"] = "Xanh Da Trời Thảo Nguyên (#4682B4) & Nâu Lông Sói (#5C4033)",
        ["CIV_MONGOLIA_ATTIRE"] = "Mũ lông thú tai cừu rủ, áo choàng da Deel lót lông, cung tên phức hợp phản khúc",
        ["CIV_MONGOLIA_UNIT"] = "Xạ Thủ Keshik & Thiết Kỵ Du Mục",
        ["CIV_MONGOLIA_TRAIT"] = "Toàn quân là kỵ binh; xạ thủ Keshik vừa phi ngựa vừa bắn, tấn công xong rút lui ngay trong lượt",

        // 9. Greece
        ["CIV_GREECE_NAME"] = "Hi Lạp (Hellenic / Sparta & Athens)",
        ["CIV_GREECE_EPITHET"] = "Phòng Tuyến Khiên Đồng Biển Aegean",
        ["CIV_GREECE_COLORS"] = "Đồng Thau (#CD7F32) & Đỏ Cờ Sparta (#990000)",
        ["CIV_GREECE_ATTIRE"] = "Mũ Corinthian đồng thau với mào lông ngựa đỏ uốn cong, khiên tròn Aspis chạm chữ Lambda, giáp ngực đúc khối cơ bụng",
        ["CIV_GREECE_UNIT"] = "Trận Đồ Giáo Khiên Hoplite Phalanx & Chiến Thuyền Đâm Trireme",
        ["CIV_GREECE_TRAIT"] = "Trận đồ giáo khiên Hoplite Phalanx phản đòn trực diện 100% cận chiến",

        // 10. England
        ["CIV_ENGLAND_NAME"] = "Anh (Anglo-Saxon / Arthurian)",
        ["CIV_ENGLAND_EPITHET"] = "Cung Thủ Rừng Già Arthur",
        ["CIV_ENGLAND_COLORS"] = "Xanh Lá Cây Rừng (#228B22) & Nâu Bùn (#6E5334)",
        ["CIV_ENGLAND_ATTIRE"] = "Cung dài Longbow cao vượt đầu người, áo chẽn nỉ gai, mũ chảo sắt tròn Kettle hat",
        ["CIV_ENGLAND_UNIT"] = "Xạ Thủ Trường Cung Longbow & Trảm Binh",
        ["CIV_ENGLAND_TRAIT"] = "Xạ thủ tầm bắn xa nhất game, dựng cọc nhọn bẫy kỵ binh",

        // 11. France
        ["CIV_FRANCE_NAME"] = "Pháp (Hiệp Sĩ Hoàng Gia)",
        ["CIV_FRANCE_EPITHET"] = "Kỵ Sĩ Hoa Bách Hợp Hoàng Gia",
        ["CIV_FRANCE_COLORS"] = "Xanh Dương Hoàng Gia (#4169E1) & Trắng Bạc (#E0E0E0)",
        ["CIV_FRANCE_ATTIRE"] = "Ngựa chiến bọc vải phủ hoa bách hợp thêu chỉ vàng, kỵ sĩ mũ kín mít khe thở chữ thập, vác thương dài Lance",
        ["CIV_FRANCE_UNIT"] = "Kỵ Sĩ Thiết Giáp Hoàng Gia & Nỏ Thủ Thành Lũy",
        ["CIV_FRANCE_TRAIT"] = "Cú húc (Charge) làm vỡ tan hàng ngũ địch và tạo khiên giảm sát thương cho quân nhà",

        // 12. Germany
        ["CIV_GERMANY_NAME"] = "Đức (Thần Thánh La Mã / Teutonic)",
        ["CIV_GERMANY_EPITHET"] = "Thiết Giáp Thập Tự Quân",
        ["CIV_GERMANY_COLORS"] = "Đen Tuyền (#111111) & Trắng Tinh Khôi (#F5F5F5)",
        ["CIV_GERMANY_ATTIRE"] = "Áo choàng trắng in chữ thập đen ngực, mũ sắt gắn cánh/sừng hươu đồ sộ, giáp phiến Gothic",
        ["CIV_GERMANY_UNIT"] = "Hiệp Sĩ Đại Kiếm Zweihander & Kỵ Sĩ Teutonic",
        ["CIV_GERMANY_TRAIT"] = "Hiệp sĩ vác đại kiếm Zweihander chém quét lan 3 ô, chuyên phá tan hàng ngũ giáo khiên",

        // 13. Spain
        ["CIV_SPAIN_NAME"] = "Tây Ban Nha (Vương Quốc Reconquista)",
        ["CIV_SPAIN_EPITHET"] = "Phương Trận Trường Giáo Bất Bại",
        ["CIV_SPAIN_COLORS"] = "Vàng Kim (#DAA520) & Đỏ Tươi (#C70039)",
        ["CIV_SPAIN_ATTIRE"] = "Mũ vành thuyền nhọn hai đầu Morion, giáp ngực thép phản quang, kiếm thép Toledo",
        ["CIV_SPAIN_UNIT"] = "Phương Trận Tercio & Kỵ Binh Chinh Phục",
        ["CIV_SPAIN_TRAIT"] = "Đội hình Tercio phối hợp trường giáo kiên cố miễn nhiễm hoàn toàn các đòn húc kỵ binh",

        // 14. Ancient Slavs
        ["CIV_SLAVS_NAME"] = "Slav Cổ (Pagan Slavic / Thần Sấm Perun)",
        ["CIV_SLAVS_EPITHET"] = "Chiến Binh Cổ Thụ Thần Sấm",
        ["CIV_SLAVS_COLORS"] = "Đỏ Thêu Dân Gian (#C8102E) & Xám Da Sói (#696969)",
        ["CIV_SLAVS_ATTIRE"] = "Áo lanh trắng thêu hoa văn chỉ đỏ (Vyshyvanka), khoác áo da sói, râu tết hạt gỗ",
        ["CIV_SLAVS_UNIT"] = "Chiến Binh Vityaz & Tế Lễ Thần Sấm",
        ["CIV_SLAVS_TRAIT"] = "Chiến binh Vityaz hồi máu gần bìa rừng, miễn nhiễm làm chậm của băng giá",

        // 15. Ancient Bulgars
        ["CIV_BULGARS_NAME"] = "Bulgar Cổ (Hãn Quốc Dã Sử / Thời Khan Krum)",
        ["CIV_BULGARS_EPITHET"] = "Kỵ Sĩ Ngọn Cờ Thiêng Tangra",
        ["CIV_BULGARS_COLORS"] = "Đỏ Huyết Dụ (#8A0303) & Đen Than Củi (#232B2B)",
        ["CIV_BULGARS_ATTIRE"] = "Mũ sắt chóp vuốt cong đính đuôi ngựa đen, cọc cờ hiệu thiêng thần Tangra (Tug), áo da lộn trùm ngoài giáp vảy",
        ["CIV_BULGARS_UNIT"] = "Kỵ Binh Boyar & Dân Binh Cuồng Nộ",
        ["CIV_BULGARS_TRAIT"] = "Kỵ binh Boyar cướp tài nguyên trên xác tướng địch, bộ binh kích hoạt trạng thái Huyết Thệ cuồng bạo khi sắp chết",

        // 16. Kievan Rus
        ["CIV_RUS_NAME"] = "Nga (Kievan Rus Dã Sử)",
        ["CIV_RUS_EPITHET"] = "Vệ Quân Băng Giá Dnieper",
        ["CIV_RUS_COLORS"] = "Đỏ Rượu Chát (#800020) & Trắng Tuyết (#FFFAFA)",
        ["CIV_RUS_ATTIRE"] = "Áo khoác da gấu dày sụ viền lông thú lớn, mũ Spangenhelm vuốt nhọn có lưới xích che kín cằm, vác rìu hai lưỡi Bardiche",
        ["CIV_RUS_UNIT"] = "Vệ Binh Druzhina & Rìu Binh Bardiche",
        ["CIV_RUS_TRAIT"] = "Vệ binh Druzhina nhận lượng phòng ngự cực đại trong đầm lầy hoặc bão tuyết",

        // 17. Poland
        ["CIV_POLAND_NAME"] = "Ba Lan (Husaria Thần Thoại)",
        ["CIV_POLAND_EPITHET"] = "Đại Bàng Cánh Thép Thần Tốc",
        ["CIV_POLAND_COLORS"] = "Đỏ Cờ Thắm (#DC143C) & Trắng Lông Vũ (#FFFFFF)",
        ["CIV_POLAND_ATTIRE"] = "Khung cánh lông vũ đại bàng cong vút sau lưng áo giáp thép bóng loáng, thương dài gắn lụa hai màu",
        ["CIV_POLAND_UNIT"] = "Kỵ Binh Winged Husaria & Thiết Kỵ Pancerni",
        ["CIV_POLAND_TRAIT"] = "Kỵ binh Husaria phi nước đại tạo âm thanh rít gió làm giảm sĩ khí khiến quân địch tự tháo chạy",

        // 18. Mesoamerica
        ["CIV_MESO_NAME"] = "Mỹ Bản Địa Cổ (Maya / Mississippian Tiền Trung Cổ)",
        ["CIV_MESO_EPITHET"] = "Dũng Sĩ Báo Đốm Rừng Già",
        ["CIV_MESO_COLORS"] = "Đỏ Đất Nung (#B22222) & Xanh Lông Vẹt (#00A86B)",
        ["CIV_MESO_ATTIRE"] = "Mũ lông chim ưng xòe tròn lớn, mình trần sơn vằn vện, áo giáp da báo đốm",
        ["CIV_MESO_UNIT"] = "Dũng Sĩ Báo Đốm Jaguar & Xạ Thủ Lao Atlatl",
        ["CIV_MESO_TRAIT"] = "Dũng sĩ Báo Đốm chạy bộ nhanh ngang kỵ binh, không bị trừ tốc độ khi leo núi đá hay băng rừng rậm",

        // Stages
        ["STAGE_1_TITLE"] = "Ải 1: Khởi Nguồn Văn Lang",
        ["STAGE_1_DESC"] = "Chiêu mộ nông binh, thuần hóa thủy vực sông Hồng, đặt nền móng thị tộc đầu tiên.",
        ["STAGE_1_OBJ"] = "Khai phá 3 khoảnh lúa nước & chiêu mộ 15 dân binh",
        ["STAGE_1_ENEMY"] = "Man Di sông Đà (Thị tộc Hắc Thủy)",
        ["STAGE_1_REWARD"] = "Mở khóa: Kỹ nghệ đúc đồng sơ kỳ & Nỏ nung lửa",

        ["STAGE_2_TITLE"] = "Ải 2: Đúc Trống Đồng",
        ["STAGE_2_DESC"] = "Khai thác mỏ thiếc & mỏ đồng, đúc thần khí trống đồng linh thiêng, chấn hưng binh sĩ.",
        ["STAGE_2_OBJ"] = "Đúc 1 Trống Đồng Thần Khí & nâng cấp lò rèn cấp 2",
        ["STAGE_2_ENEMY"] = "Thị tộc Dạ Lang",
        ["STAGE_2_REWARD"] = "Mở khóa: Chiến binh Rìu Đồng & Hào thành lũy",

        ["STAGE_3_TITLE"] = "Ải 3: Nỏ Thần Cổ Loa",
        ["STAGE_3_DESC"] = "Xây dựng phòng tuyến lũy thành xoáy ốc, dàn trận nỏ liên châu đẩy lui ngoại xâm.",
        ["STAGE_3_OBJ"] = "Bảo vệ Hoàng Thành & tiêu diệt 5 đợt quân cảm tử",
        ["STAGE_3_ENEMY"] = "Quân viễn chinh Triệu Đà",
        ["STAGE_3_REWARD"] = "Mở khóa: Nỏ Thủ Cổ Loa Liên Châu",

        ["STAGE_4_TITLE"] = "Ải 4: Voi Chiến Mê Linh",
        ["STAGE_4_DESC"] = "Thuần phục thớt voi chiến hung hãn, lãnh đạo cuộc khởi nghĩa trừng phạt quân thù.",
        ["STAGE_4_OBJ"] = "Thuần phục 4 tượng binh & hạ đồn trú địch",
        ["STAGE_4_ENEMY"] = "Quân đô hộ Thái thú Tô Định",
        ["STAGE_4_REWARD"] = "Mở khóa: Tượng Binh Giáp Đồng Mê Linh",

        ["STAGE_5_TITLE"] = "Ải 5: Thống Nhất Bách Việt",
        ["STAGE_5_DESC"] = "Hội quân mười sáu lãnh bang, quyết chiến tại đỉnh Nghĩa Lĩnh, khai sinh vương triều ngàn năm.",
        ["STAGE_5_OBJ"] = "Thôn tính 3 bộ tộc thù địch hoặc xây Kỳ Đài Văn Lang",
        ["STAGE_5_ENEMY"] = "Liên minh 3 Đại Lãnh Bang Bắc Bộ",
        ["STAGE_5_REWARD"] = "Danh hiệu: HÙNG VƯƠNG ĐỆ NHẤT & Thống Nhất Sơn Hà",

        // Skirmish Settings
        ["SKIRMISH_MAP_SIZE"] = "Kích thước bản đồ",
        ["SKIRMISH_BIOME"] = "Địa hình sinh thái",
        ["SKIRMISH_RIVALS"] = "Số lượng đối thủ AI",
        ["SKIRMISH_VICTORY"] = "Điều kiện thắng",
        ["SKIRMISH_DIFFICULTY"] = "Độ khó AI",
        ["MAP_SMALL"] = "Nhỏ (32x32) - Giao tranh nhanh",
        ["MAP_MEDIUM"] = "Tiêu chuẩn (64x64) - Sa bàn chiến thuật",
        ["MAP_LARGE"] = "Đại lục (128x128) - Đại chiến đế chế",
        ["BIOME_RED_RIVER"] = "Lưu vực sông Hồng (Phù sa, bẫy cọc ngầm)",
        ["BIOME_JUNGLE"] = "Rừng rậm nhiệt đới (Ẩn nấp, đầm lầy, sương mù)",
        ["BIOME_HIGHLANDS"] = "Cao nguyên đá vôi (Khoáng sản, hiểm trở)",
        ["BIOME_STEPPE"] = "Đại thảo nguyên Á-Âu (Bình nguyên, kỵ binh cơ động)",
        ["BIOME_TAIGA"] = "Rừng lá kim băng tuyết (Đầm lầy tuyết, hành quân chậm)",
        ["BIOME_MEDITERRANEAN"] = "Duyên hải Địa Trung Hải (Hải cảng, pháo đài)",
        ["RIVALS_2"] = "2 Phe (Song hùng quyết đấu)",
        ["RIVALS_4"] = "4 Phe (Quần hùng tranh phong)",
        ["RIVALS_6"] = "6 Phe (Bát ngát sa bàn)",
        ["RIVALS_8"] = "8 Phe (Hỗn chiến đế chế)",
        ["VICTORY_CONQUEST"] = "Chinh phục quân sự (Conquest)",
        ["VICTORY_CULTURE"] = "Bá quyền văn hóa & Kỳ quan (Wonder)",
        ["VICTORY_REGICIDE"] = "Trảm tướng đoạt cờ (Regicide)",
        ["DIFF_EASY"] = "Dễ (Tập sự)",
        ["DIFF_NORMAL"] = "Tiêu chuẩn (Chỉ huy)",
        ["DIFF_HARD"] = "Khó (Thủ lĩnh)",
        ["DIFF_LEGEND"] = "Huyền thoại (Hùng Vương)",

        // Training
        ["TRAINING_SELECT"] = "Chọn Chế Độ",
        ["TRAINING_SELECTED"] = "Đã Chọn",
        ["TRAINING_TUTORIAL_NAME"] = "Tập Huấn Binh Pháp Cơ Bản",
        ["TRAINING_SANDBOX_NAME"] = "Sa Bàn Sandbox Test Đơn Vị",

        // Settings
        ["SETTINGS_TITLE"] = "CÀI ĐẶT HỆ THỐNG",
        ["SETTINGS_TAB_AUDIO"] = "🔊 Âm Thanh",
        ["SETTINGS_TAB_VIDEO"] = "🖥 Hiển Thị",
        ["SETTINGS_TAB_GAMEPLAY"] = "🌐 Ngôn Ngữ",
        ["SETTINGS_MASTER_VOL"] = "Âm Lượng Tổng",
        ["SETTINGS_SFX_VOL"] = "Hiệu Ứng Âm (SFX)",
        ["SETTINGS_MUSIC_VOL"] = "Nhạc Nền (BGM)",
        ["SETTINGS_FULLSCREEN"] = "Toàn Màn Hình",
        ["SETTINGS_VSYNC"] = "Đồng Bộ Khung Hình",
        ["SETTINGS_LANGUAGE"] = "Ngôn Ngữ Trò Chơi",
        ["SETTINGS_BTN_CLOSE"] = "✕ Đóng",

        // Display Settings
        ["SETTINGS_WINDOW_MODE"] = "Chế Độ Cửa Sổ",
        ["SETTINGS_RESOLUTION"] = "Độ Phân Giải",
        ["SETTINGS_VSYNC_LABEL"] = "Đồng Bộ Khung Hình (VSync)",
        ["SETTINGS_MAX_FPS"] = "Giới Hạn Khung Hình (FPS)",
        ["SETTINGS_MODE_WINDOWED"] = "Cửa Sổ (Windowed)",
        ["SETTINGS_MODE_BORDERLESS"] = "Không Viền (Borderless)",
        ["SETTINGS_MODE_FULLSCREEN"] = "Toàn Màn Hình (Fullscreen)",
        ["SETTINGS_MODE_EXCLUSIVE"] = "Toàn Màn Hình Độc Quyền",
        ["SETTINGS_VSYNC_DISABLED"] = "Tắt",
        ["SETTINGS_VSYNC_ENABLED"] = "Bật",
        ["SETTINGS_VSYNC_ADAPTIVE"] = "Thích Ứng (Adaptive)",
        ["SETTINGS_FPS_UNLIMITED"] = "Không Giới Hạn",
        ["SETTINGS_UI_SCALE"] = "Tỉ Lệ Giao Diện (UI Scale)",
        ["SETTINGS_BTN_SAVE"] = "💾 Lưu & Áp Dụng",
        ["SETTINGS_STATUS_SAVED"] = "Đã lưu & áp dụng cài đặt!",
        ["SETTINGS_STATUS_SAVED_VI"] = "Đã lưu & áp dụng cài đặt!"
    };

    public const string LocalizationDir = "res://data/localization";

    private static Translation? _enTrans;
    private static Translation? _viTrans;

    static LocalizationManager()
    {
        LoadAllLocalizationFiles();
        RegisterTranslations();
    }

    public static void Initialize()
    {
        LoadAllLocalizationFiles();
        RegisterTranslations();
    }

    public static void ReloadAll()
    {
        LoadAllLocalizationFiles();
        RegisterTranslations();
        TranslationServer.SetLocale(CurrentLanguage);
        LanguageChanged?.Invoke();
    }

    public static void LoadAllLocalizationFiles()
    {
        if (!DirAccess.DirExistsAbsolute(LocalizationDir))
        {
            return;
        }

        using var dir = DirAccess.Open(LocalizationDir);
        if (dir == null) return;

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                string fullPath = $"{LocalizationDir}/{fileName}";
                bool isVi = fileName.EndsWith("_vi.txt", StringComparison.OrdinalIgnoreCase);
                var dict = isVi ? ViTranslations : EnTranslations;
                ParseLocalizationFile(fullPath, dict);
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();
    }

    private static void ParseLocalizationFile(string filePath, Dictionary<string, string> dict)
    {
        if (!FileAccess.FileExists(filePath)) return;
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

            int sepIdx = line.IndexOf('=');
            if (sepIdx > 0)
            {
                string key = line[..sepIdx].Trim();
                string val = line[(sepIdx + 1)..].Trim();
                val = val.Replace("\\n", "\n");
                dict[key] = val;
            }
        }
    }

    private static void RegisterTranslations()
    {
        if (_enTrans != null) TranslationServer.RemoveTranslation(_enTrans);
        if (_viTrans != null) TranslationServer.RemoveTranslation(_viTrans);

        _enTrans = new Translation { Locale = LangEnglish };
        foreach (var (k, v) in EnTranslations)
        {
            _enTrans.AddMessage(k, v);
        }
        TranslationServer.AddTranslation(_enTrans);

        _viTrans = new Translation { Locale = LangVietnamese };
        foreach (var (k, v) in ViTranslations)
        {
            _viTrans.AddMessage(k, v);
        }
        TranslationServer.AddTranslation(_viTrans);
    }

    public static void SetLanguage(string langCode)
    {
        if (langCode != LangEnglish && langCode != LangVietnamese)
        {
            langCode = LangEnglish;
        }

        CurrentLanguage = langCode;
        TranslationServer.SetLocale(langCode);
        LanguageChanged?.Invoke();
    }

    public static string Get(string key)
    {
        var dict = CurrentLanguage == LangVietnamese ? ViTranslations : EnTranslations;
        if (dict.TryGetValue(key, out string? value))
        {
            return value;
        }
        return TranslationServer.Translate(key);
    }
}


