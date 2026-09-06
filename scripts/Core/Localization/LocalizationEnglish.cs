using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public static partial class LocalizationManager
{
    private static readonly Dictionary<string, string> EnTranslations = new()
    {
        // Core Game Title
        ["MENU_TITLE"] = "CHRONICLES OF THE OLD EMPIRES",
        ["MENU_SUBTITLE"] = "â—† MYTHIC AGES â—†",
        ["MENU_NEW_CAMPAIGN"] = "New Campaign â–¸",
        ["MENU_LOAD_GAME"] = "Load Game",
        ["MENU_SETTINGS"] = "Settings",
        ["MENU_EXIT"] = "Exit to Desktop",
        ["MENU_ARCHIVES"] = "đŸ“– Archives",
        ["MENU_CREDITS"] = "đŸ‘¥ Credits",
        ["MENU_DISCORD"] = "đŸ’¬ Discord",
        ["MENU_ARCHIVES_TOOLTIP"] = "Civilopedia & Chronicles Archives",
        ["MENU_CREDITS_TOOLTIP"] = "Development Credits",
        ["MENU_DISCORD_TOOLTIP"] = "Official Community Discord",

        // Campaign Mode Selector
        ["CAMPAIGN_MODAL_TITLE"] = "SELECT CAMPAIGN MODE",
        ["CAMPAIGN_HEADER"] = "SELECT CAMPAIGN MODE",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "â” Campaign (Stages)",
        ["CAMPAIGN_TAB_SKIRMISH"] = "â¡ Skirmish (Map)",
        ["CAMPAIGN_TAB_TRAINING"] = "đŸ“œ Practice & Sandbox",
        ["CAMPAIGN_BTN_GRAND"] = "â” Campaign",
        ["CAMPAIGN_BTN_SKIRMISH"] = "â¡ Skirmish",
        ["CAMPAIGN_BTN_TUTORIAL"] = "đŸ“œ Tutorial",
        ["CAMPAIGN_DEPLOY"] = "â” DEPLOY CAMPAIGN",
        ["CAMPAIGN_START_SKIRMISH"] = "â¡ START SKIRMISH",
        ["CAMPAIGN_START_TRAINING"] = "đŸ“œ START TRAINING",
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
        ["SETTINGS_TAB_AUDIO"] = "đŸ” Audio",
        ["SETTINGS_TAB_VIDEO"] = "đŸ–¥ Display",
        ["SETTINGS_TAB_GAMEPLAY"] = "đŸŒ Language",
        ["SETTINGS_MASTER_VOL"] = "Master Volume",
        ["SETTINGS_SFX_VOL"] = "Sound Effects",
        ["SETTINGS_MUSIC_VOL"] = "Music Volume",
        ["SETTINGS_FULLSCREEN"] = "Fullscreen Mode",
        ["SETTINGS_VSYNC"] = "Vertical Sync (VSync)",
        ["SETTINGS_LANGUAGE"] = "Game Language",
        ["SETTINGS_BTN_CLOSE"] = "âœ• Close",

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
        ["SETTINGS_BTN_SAVE"] = "đŸ’¾ Save & Apply",
        ["SETTINGS_STATUS_SAVED"] = "Settings saved & applied!",
        ["SETTINGS_STATUS_SAVED_VI"] = "ÄĂ£ lÆ°u & Ă¡p dá»¥ng cĂ i Ä‘áº·t!"
    };
}
