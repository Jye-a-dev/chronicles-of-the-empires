using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public static partial class LocalizationManager
{
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
        ["CAMPAIGN_MODAL_TITLE"] = "WAR COUNCIL & THEATERS OF CAMPAIGN",
        ["CAMPAIGN_HEADER"] = "WAR COUNCIL & STRATEGIC MOBILIZATION",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "⚔ World Chronicles (Stages)",
        ["CAMPAIGN_TAB_SKIRMISH"] = "⚡ War of Dynasties (Skirmish)",
        ["CAMPAIGN_TAB_TRAINING"] = "📜 Martial Drills & Arena",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ World Chronicles",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ War of Dynasties",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Martial Drills",
        ["CAMPAIGN_DEPLOY"] = "⚔ MARCH TO WAR",
        ["CAMPAIGN_START_SKIRMISH"] = "⚡ COMMENCE BATTLE",
        ["CAMPAIGN_START_TRAINING"] = "📜 COMMENCE DRILLS",
        ["SPEC_MAP_PREFIX"] = "Theater: ",
        ["SPEC_RIVALS_PREFIX"] = "Contenders: ",
        ["SPEC_VICTORY_PREFIX"] = "Mandate: ",
        ["SPEC_REWARD_PREFIX"] = "Spoils of War: ",

        // Mode Details for CampaignModePanel
        ["MODE_GRAND_NAME"] = "World Chronicles",
        ["MODE_GRAND_ERA"] = "Epoch of the Four Realms (Grand Strategy & Pausable Tactical Combat)",
        ["MODE_GRAND_DESC"] = "Forge a mythic empire across painted silk scroll provinces, journeying across the four ancient cultural spheres. Tame sacred river basins, withstand mechanized siege engines, breach heavy plate phalanxes, and conquer frozen northern taigas to claim dominion over all known realms.",
        ["MODE_GRAND_MAP"] = "Continental Silk Scroll Provinces",
        ["MODE_GRAND_RIVALS"] = "Eighteen Sovereign Dynasties across Four Spheres",
        ["MODE_GRAND_VICTORY"] = "Imperial Hegemony or Sacred Wonder Dominance",

        ["MODE_SKIRMISH_NAME"] = "War of Dynasties",
        ["MODE_SKIRMISH_ERA"] = "Custom Theater of War (Tactical Sandbox)",
        ["MODE_SKIRMISH_DESC"] = "Ignite immediate tactical clashes across isometric grid battlefields or provincial scroll territories. Freely configure rival sovereign empires, geographical boundaries, and mandates of victory.",
        ["MODE_SKIRMISH_MAP"] = "Theater Scale (Skirmish, Provincial, Continental)",
        ["MODE_SKIRMISH_RIVALS"] = "2 to 8 Contending Empires",
        ["MODE_SKIRMISH_VICTORY"] = "Total Conquest, Monumental Hegemony, Regicide Strike",

        ["MODE_TUTORIAL_NAME"] = "Martial Academy & Proving Grounds",
        ["MODE_TUTORIAL_ERA"] = "Tactical Doctrine & Cohort Exercises",
        ["MODE_TUTORIAL_DESC"] = "Master the maneuvering of armored regiments, the command chain of Active Pause (Spacebar), elevation tactical advantages, and the intricate counter-doctrines among unit classes.",
        ["MODE_TUTORIAL_MAP"] = "Imperial Proving Grounds & Isometric Arena",
        ["MODE_TUTORIAL_RIVALS"] = "1 Sparring Cohort",
        ["MODE_TUTORIAL_VICTORY"] = "Master All Martial Doctrines",

        // Cultural Spheres
        ["BLOCK_ALLUVIUM_NAME"] = "Pantheons & Alluvium",
        ["BLOCK_ALLUVIUM_REGION"] = "Great River Basins & Mythic Deserts",
        ["BLOCK_ALLUVIUM_DESC"] = "Fertile floodplains, ancient river redoubts, sacred temples, and mystical rituals.",

        ["BLOCK_MANDATE_NAME"] = "Iron & Mandate",
        ["BLOCK_MANDATE_REGION"] = "Celestial Provinces & The Vast Steppe",
        ["BLOCK_MANDATE_DESC"] = "Rigid iron discipline, siege artillery, nomad horsemen, and divine imperial decrees.",

        ["BLOCK_STEEL_NAME"] = "Sacred Crests & Heavy Steel",
        ["BLOCK_STEEL_REGION"] = "Pelagion Shores & Western Strongholds",
        ["BLOCK_STEEL_DESC"] = "Impenetrable phalanxes, knightly shock charges, massed longbows, and steel plate armaments.",

        ["BLOCK_FROST_NAME"] = "Wild Gods & Frost",
        ["BLOCK_FROST_REGION"] = "Boreal Tundra & Primeval Woodlands",
        ["BLOCK_FROST_DESC"] = "Pagan pantheons, blizzard resilience, fearsome cavalry charges, and dense primeval warfare.",

        // 18 Civilizations
        // 1. Lac Uyen
        ["CIV_VN_NAME"] = "Sovereign Realm of Lac Uyen (Alluvial Dynasty)",
        ["CIV_VN_EPITHET"] = "Lords of the Crimson Waters",
        ["CIV_VN_COLORS"] = "Verdigris (#43B3AE) & Rice Gold (#E5A93C)",
        ["CIV_VN_ATTIRE"] = "Tall feathered Lac headdress, round storm-cast bronze pectoral, hand-woven brocade tunic",
        ["CIV_VN_UNIT"] = "Uyen-Chau Repeating Crossbow & Tide-Rattan War Elephants",
        ["CIV_VN_TRAIT"] = "Stealth in wetlands and mangrove bogs, armor-piercing volleys, submerged river stakes, permanent flood buffs",

        // 2. Baray-Nagar
        ["CIV_KHMER_NAME"] = "Sacred Dominion of Sovannagar (Sunken Realm of Nagar)",
        ["CIV_KHMER_EPITHET"] = "Architects of the Golden Basin",
        ["CIV_KHMER_COLORS"] = "Placer Gold (#D4AF37) & Terracotta (#8B4513)",
        ["CIV_KHMER_ATTIRE"] = "Gold-woven Sampot, terracotta sacred Apsara cuirass, body-covered talismanic ward tattoos",
        ["CIV_KHMER_UNIT"] = "Gilded-Tusk War Elephants & Temple Guardians of Baray",
        ["CIV_KHMER_TRAIT"] = "Baray hydrological network provides immense sustenance in arid biomes; holy spires fortify vitality",

        // 3. Solarika
        ["CIV_INDIA_NAME"] = "Solarika Empire (Radiant Realm of Mauraka)",
        ["CIV_INDIA_EPITHET"] = "Keepers of the Solar Chakra",
        ["CIV_INDIA_COLORS"] = "Saffron (#FF9933) & Silk White (#F8F8FF)",
        ["CIV_INDIA_ATTIRE"] = "Broad jewel-crested solar turban, translucent silk tunic trimmed with silver filigree",
        ["CIV_INDIA_UNIT"] = "Solar Chakram Whirlers & Luminescent Ritual War Elephants",
        ["CIV_INDIA_TRAIT"] = "Ricocheting circular chakram projectiles and radiant aura of supreme morale inspiration",

        // 4. Zafaran
        ["CIV_PERSIA_NAME"] = "Grand Imperium of Zafaran (Sassanor Empire)",
        ["CIV_PERSIA_EPITHET"] = "Heirs of the Everliving",
        ["CIV_PERSIA_COLORS"] = "Tyrian Purple (#66023C) & Turmeric Gold (#DDAA00)",
        ["CIV_PERSIA_ATTIRE"] = "Half-face purple silk veil, gold-spun tunic over jewel-scale armor, woven wicker tower shield",
        ["CIV_PERSIA_UNIT"] = "Gilded Cataphract Cavalry & The Everliving Cohort",
        ["CIV_PERSIA_TRAIT"] = "The Everliving cohort resurrects upon falling in battle; heavy shock cataphracts shatter enemy formations",

        // 5. Yan-Hao
        ["CIV_CHINA_NAME"] = "Celestial Realm of Yan-Hao (Mandate Empire of Haozhou)",
        ["CIV_CHINA_EPITHET"] = "Masters of the Iron Bastion",
        ["CIV_CHINA_COLORS"] = "Imperial Crimson (#8B0000) & Jet Black (#1C1C1C)",
        ["CIV_CHINA_ATTIRE"] = "Square classical iron lamellar plates, iron helm with long red pheasant plumes",
        ["CIV_CHINA_UNIT"] = "Celestial Fire-Arrow Batteries & Ironwork Siege Ballistas",
        ["CIV_CHINA_TRAIT"] = "Deploy massive rocket artillery batteries, erect impenetrable fortresses, and rapidly assimilate conquered cities",

        // 6. Hinokuni
        ["CIV_JAPAN_NAME"] = "Sacred Shogunate of Hinokuni (Blades of the Radiant Sun)",
        ["CIV_JAPAN_EPITHET"] = "Blades of the Sunken Sun",
        ["CIV_JAPAN_COLORS"] = "Snow White (#FFFAFA) & Torii Vermilion (#C8382B)",
        ["CIV_JAPAN_ATTIRE"] = "Traditional lacquered wood armor, Kabuto war helm with grand crescent crest, rear banner Sashimono",
        ["CIV_JAPAN_UNIT"] = "Blood-Pact Swordmasters & Spirit Mist Diviners",
        ["CIV_JAPAN_TRAIT"] = "Blood-Pact Swordmasters gain exponential combat power when critical; Diviners summon battlefield shroud mists",

        // 7. Gaorun
        ["CIV_KOREA_NAME"] = "Mountain Kingdom of Gaorun (Ridge Confederacy of Gaorun)",
        ["CIV_KOREA_EPITHET"] = "Sentinels of the Frost Peaks",
        ["CIV_KOREA_COLORS"] = "Cobalt Blue (#0047AB) & Leather Brown (#7B3F00)",
        ["CIV_KOREA_ATTIRE"] = "Felt peaked hat adorned with dual raptor feathers, flexible fish-scale armor",
        ["CIV_KOREA_UNIT"] = "Gale Ridge Mounted Snipers & Thunder-Arrow Siege Carts",
        ["CIV_KOREA_TRAIT"] = "High-elevation sniper range bonus and devastating salvos of fire-arrow barrages",

        // 8. Turgai
        ["CIV_MONGOLIA_NAME"] = "Grand Khaganate of Turgai (Eternal Sky Horde)",
        ["CIV_MONGOLIA_EPITHET"] = "Riders of the Eternal Blue Sky",
        ["CIV_MONGOLIA_COLORS"] = "Steppe Azure (#4682B4) & Wolf Fur Brown (#5C4033)",
        ["CIV_MONGOLIA_ATTIRE"] = "Flapped sheepskin fur cap, fur-lined leather robe, composite recurve hornbow",
        ["CIV_MONGOLIA_UNIT"] = "Night-Falcon Horse Archers & Steppe Iron Lancers",
        ["CIV_MONGOLIA_TRAIT"] = "Complete all-cavalry roster; horse archers shoot on the gallop and reposition immediately after attack",

        // 9. Aethelon
        ["CIV_GREECE_NAME"] = "Maritime League of Aethelon (Archon Polis of Aethelon)",
        ["CIV_GREECE_EPITHET"] = "Shieldwall of the Pelagion",
        ["CIV_GREECE_COLORS"] = "Antique Bronze (#CD7F32) & War Banner Red (#990000)",
        ["CIV_GREECE_ATTIRE"] = "Crested bronze helm with horsehair plumed ridge, embossed round Hoplon shield, anatomical muscle cuirass",
        ["CIV_GREECE_UNIT"] = "Bronzecrest Hoplite Phalanx & Ramming Trireme Galleys",
        ["CIV_GREECE_TRAIT"] = "Phalanx shieldwall reflects 100% of direct frontal melee combat damage",

        // 10. Eldoria
        ["CIV_ENGLAND_NAME"] = "High Kingdom of Eldoria (Duchy of Albreshire)",
        ["CIV_ENGLAND_EPITHET"] = "Yeomen of the Greenwood Boughs",
        ["CIV_ENGLAND_COLORS"] = "Forest Green (#228B22) & Mud Brown (#6E5334)",
        ["CIV_ENGLAND_ATTIRE"] = "Tall yew longbow exceeding warrior height, coarse wool doublet, round iron kettle hat",
        ["CIV_ENGLAND_UNIT"] = "Eldorian Greenwood Longbowmen & Heavy Billmen",
        ["CIV_ENGLAND_TRAIT"] = "Longest projectile range in the theater; can deploy anti-cavalry defensive stakes",

        // 11. Valoisia
        ["CIV_FRANCE_NAME"] = "Imperial Realm of Valoisia (Kingdom of Lysgard)",
        ["CIV_FRANCE_EPITHET"] = "Champions of the Golden Lily",
        ["CIV_FRANCE_COLORS"] = "Royal Blue (#4169E1) & Silver White (#E0E0E0)",
        ["CIV_FRANCE_ATTIRE"] = "Caparisoned warhorse in gold-embroidered lilies, knight in great helm with cross visor slits, couched lance",
        ["CIV_FRANCE_UNIT"] = "Sovereign Fleur Chevaliers & Bastion Crossbow Garrisons",
        ["CIV_FRANCE_TRAIT"] = "Devastating couched lance charge fractures enemy formations and shields nearby allied units",

        // 12. Eisenreich
        ["CIV_GERMANY_NAME"] = "Iron Imperium of Eisenreich (Grand Duchy of Teutonia)",
        ["CIV_GERMANY_EPITHET"] = "Vanguard of the Iron Cross",
        ["CIV_GERMANY_COLORS"] = "Teutonic Black (#111111) & Pure White (#F5F5F5)",
        ["CIV_GERMANY_ATTIRE"] = "White surcoat bearing black chest cross, horned great helm, heavy articulated Gothic plate",
        ["CIV_GERMANY_UNIT"] = "Zweihander Greatswordsmen & Iron Cross Knights",
        ["CIV_GERMANY_TRAIT"] = "Zweihander greatswordsmen sweep 3 tiles wide, specializing in sundering enemy shield and pike lines",

        // 13. Castilia
        ["CIV_SPAIN_NAME"] = "Royal Realm of Castilia (Crown of Aragonis)",
        ["CIV_SPAIN_EPITHET"] = "Bastion of the Gilded Pike",
        ["CIV_SPAIN_COLORS"] = "Castilian Gold (#DAA520) & Crimson Scarlet (#C70039)",
        ["CIV_SPAIN_ATTIRE"] = "Dual-pointed comb Morion helm, burnished steel plate cuirass, oil-tempered rapier blade",
        ["CIV_SPAIN_UNIT"] = "Castilian Tercio Regiments & Conquistador Lancers",
        ["CIV_SPAIN_TRAIT"] = "Tercio square formation is completely immune to frontal cavalry shock charges",

        // 14. Borislav
        ["CIV_SLAVS_NAME"] = "Primeval Realm of Borislav (Woodland Dominion of Perunika)",
        ["CIV_SLAVS_EPITHET"] = "Sons of the Thunder Oak",
        ["CIV_SLAVS_COLORS"] = "Embroidery Red (#C8102E) & Wolf Grey (#696969)",
        ["CIV_SLAVS_ATTIRE"] = "Linen shirt with ritual red folk embroidery, wolf pelt mantle, carved wooden talisman beard beads",
        ["CIV_SLAVS_UNIT"] = "Vityaz Woodland Sentinels & Perun Thunder Shamans",
        ["CIV_SLAVS_TRAIT"] = "Vityaz sentinels regenerate health near woodland borders; completely immune to frost movement penalties",

        // 15. Krumak
        ["CIV_BULGARS_NAME"] = "Blood-Banner Khanate of Krumak (Dominion of Tangrad)",
        ["CIV_BULGARS_EPITHET"] = "Riders of the Sacred Tangrad Standard",
        ["CIV_BULGARS_COLORS"] = "Blood Red (#8A0303) & Charcoal Black (#232B2B)",
        ["CIV_BULGARS_ATTIRE"] = "Curved conical iron helm with black horsetail crest, sacred Tangrad standard pole, suede scale armor",
        ["CIV_BULGARS_UNIT"] = "Boyar Heavy Cavalry & Blood-Oath Berserk Footmen",
        ["CIV_BULGARS_TRAIT"] = "Boyars plunder bonus resources from fallen commanders; infantry enter frenzied Blood Oath when endangered",

        // 16. Volskya
        ["CIV_RUS_NAME"] = "Boreal Principality of Volskya (Grand Duchy of Frostgarde)",
        ["CIV_RUS_EPITHET"] = "Sentinels of the Boreal Ice",
        ["CIV_RUS_COLORS"] = "Burgundy Red (#800020) & Snow White (#FFFAFA)",
        ["CIV_RUS_ATTIRE"] = "Heavy bear-fur mantle, pointed Spangenhelm with chainmail aventail, two-handed Bardiche battle axe",
        ["CIV_RUS_UNIT"] = "Druzhina Boreal Hearthguard & Bardiche Heavy Axemen",
        ["CIV_RUS_TRAIT"] = "Druzhina gains maximum armor and defense when stationed in wetlands, marshes, or snowstorms",

        // 17. Sarmatia
        ["CIV_POLAND_NAME"] = "Winged Sovereignty of Sarmatia (Kingdom of Lechia)",
        ["CIV_POLAND_EPITHET"] = "Sovereigns of the Steel Wings",
        ["CIV_POLAND_COLORS"] = "Polish Crimson (#DC143C) & Feather White (#FFFFFF)",
        ["CIV_POLAND_ATTIRE"] = "Arching eagle-feather frame strapped across burnished steel breastplate, pennoned lance",
        ["CIV_POLAND_UNIT"] = "Sarmatian Winged Husaria & Pancerni Armored Cavalry",
        ["CIV_POLAND_TRAIT"] = "Full-speed charge generates terrifying wind screech, causing enemy regiments to waver and break morale",

        // 18. Tlalokan
        ["CIV_MESO_NAME"] = "Sun-Pact Empire of Tlalokan (Divine Realm of Quetzala)",
        ["CIV_MESO_EPITHET"] = "Warriors of the Feathered Serpent",
        ["CIV_MESO_COLORS"] = "Clay Ochre (#B22222) & Quetzal Green (#00A86B)",
        ["CIV_MESO_ATTIRE"] = "Spreading harpy eagle feathered headdress, sacred painted glyphs, spotted jaguar pelt armor",
        ["CIV_MESO_UNIT"] = "Jaguar Shock Guardians & Atlatl Dart Throwers",
        ["CIV_MESO_TRAIT"] = "Jaguar shock guardians move at cavalry speed; zero terrain movement penalty over mountains or jungle",

        // Stages
        ["STAGE_1_TITLE"] = "Stage 1: Alluvial Genesis (Pantheons & Alluvium)",
        ["STAGE_1_DESC"] = "Establish your dynasty along the sacred floodplains of the ancient mother rivers. Tame monsoon deluges, plant river stakes, revere river deities, and cast early bronze weaponry to carve out an ancestral homeland.",
        ["STAGE_1_OBJ"] = "Cultivate 3 fertile alluvial riverbanks, fortify river palisades & muster 15 vanguard militia",
        ["STAGE_1_ENEMY"] = "Riverbank Raider Clans",
        ["STAGE_1_REWARD"] = "Unlock: Sacred Bronze Metallurgy & Uyen-Chau Repeating Crossbow",

        ["STAGE_2_TITLE"] = "Stage 2: Steppe Hooves & Siege Engines (Iron & Mandate)",
        ["STAGE_2_DESC"] = "March into the vast windswept steppes and fortified frontiers of the Northern Provinces. Repel lightning cavalry hit-and-run raids and storm the entrenched rocket artillery and heavy crossbow batteries of iron-clad dynasties.",
        ["STAGE_2_OBJ"] = "Destroy 3 Siege Battery Carts, shatter steppe horse-archer charges & seize the mountain pass",
        ["STAGE_2_ENEMY"] = "Steppe Khaganate & Iron Legion Batteries",
        ["STAGE_2_REWARD"] = "Unlock: Fire-Arrow War Carts & Steppe Horse-Archer Doctrines",

        ["STAGE_3_TITLE"] = "Stage 3: Heavy Steel & The Phalanx Wall (Sacred Crests & Heavy Steel)",
        ["STAGE_3_DESC"] = "Cross into the Pelagion Seas and Western Strongholds. Confront the impenetrable shieldwall of the long-pike Hoplite Phalanx and shatter the devastating, earth-quaking lance charge of heavy plate royal paladins.",
        ["STAGE_3_OBJ"] = "Rupture the frontal Hoplite Phalanx wall, withstand royal lance charges & breach the stone fortress",
        ["STAGE_3_ENEMY"] = "Holy Crest Legions & Royal Armored Knights",
        ["STAGE_3_REWARD"] = "Unlock: Gothic Full Plate Armor & Pike-Square Formations",

        ["STAGE_4_TITLE"] = "Stage 4: Taiga of the Thunder Deities (Wild Gods & Frost)",
        ["STAGE_4_DESC"] = "Advance deep into the frozen northern taiga and howling snow bogs. Endure blinding blizzards, defeat pagan woodsmen bound to thunder gods, and break the terrifying screeching charge of eagle-winged warriors.",
        ["STAGE_4_OBJ"] = "Survive frozen tundra storms, overcome Vityaz woodland guardians & defeat the winged shock cavalry",
        ["STAGE_4_ENEMY"] = "Frostland Clans & Ancient Pagan Shamanate",
        ["STAGE_4_REWARD"] = "Unlock: Blizzard Resilience Doctrines & Blood Oath Berserk Frenzy",

        ["STAGE_5_TITLE"] = "Stage 5: War of the Four Spheres (The Grand Climax)",
        ["STAGE_5_DESC"] = "The supreme war council gathers at the navel of the ancient world. Eighteen dynasties from all four continents deploy in total clash; overcome allied emperors, erect the World Wonder Monument, and claim eternal hegemony.",
        ["STAGE_5_OBJ"] = "Vanquish the Triad Imperial Coalition or complete the Monument of World Hegemony",
        ["STAGE_5_ENEMY"] = "Grand Coalition of the Four Cultural Spheres",
        ["STAGE_5_REWARD"] = "Imperial Title: EMPEROR OF THE KNOWN REALMS & Sovereign of the Four Spheres",

        // Skirmish Settings
        ["SKIRMISH_MAP_SIZE"] = "Theater Dimensions",
        ["SKIRMISH_BIOME"] = "Terrain & Landform Formation",
        ["SKIRMISH_RIVALS"] = "Contending Empires",
        ["SKIRMISH_VICTORY"] = "Mandate of Triumph",
        ["SKIRMISH_DIFFICULTY"] = "Adversary Strategic Cunning",
        ["MAP_SMALL"] = "Skirmish Theater (32x32) - Rapid Engagement",
        ["MAP_MEDIUM"] = "Provincial Arena (64x64) - Grand Tactical",
        ["MAP_LARGE"] = "Continental Expanse (128x128) - World War",
        ["BIOME_RED_RIVER"] = "Great River Alluvium (Fertile floodplains, submerged river stakes)",
        ["BIOME_JUNGLE"] = "Primeval Canopy (Ambush concealment & treacherous quagmires)",
        ["BIOME_HIGHLANDS"] = "Karst Crags & Defiles (Rich minerals & formidable chokepoints)",
        ["BIOME_STEPPE"] = "Vast Steppe Plains (Sweeping plains & rapid cavalry maneuvers)",
        ["BIOME_TAIGA"] = "Boreal Frost Taiga (Blizzards, snowdrifts & grueling marches)",
        ["BIOME_MEDITERRANEAN"] = "Pelagion Maritime Coastline (Coastal strongholds & naval ramming straits)",
        ["RIVALS_2"] = "Duel of Emperors (2 Factions)",
        ["RIVALS_4"] = "Four Winds War (4 Factions)",
        ["RIVALS_6"] = "War of the Six Realms (6 Factions)",
        ["RIVALS_8"] = "Clash of Eight Dynasties (8 Factions)",
        ["VICTORY_CONQUEST"] = "Total Conquest (Military Annihilation)",
        ["VICTORY_CULTURE"] = "Monumental Hegemony (Sacred Wonder Dominance)",
        ["VICTORY_REGICIDE"] = "Regicide Strike (Slay the Sovereign)",
        ["DIFF_EASY"] = "Novice Strategist (Apprentice)",
        ["DIFF_NORMAL"] = "Seasoned General (Commander)",
        ["DIFF_HARD"] = "Iron Warlord (Warlord)",
        ["DIFF_LEGEND"] = "Mythic Sovereign (Emperor)",

        // Training
        ["TRAINING_SELECT"] = "Select Doctrine",
        ["TRAINING_SELECTED"] = "Doctrine Selected",
        ["TRAINING_TUTORIAL_NAME"] = "Martial Doctrine & Tactical Exercises",
        ["TRAINING_SANDBOX_NAME"] = "Imperial Proving Grounds Sandbox",

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
        ["SETTINGS_BTN_CLOSE"] = "✖ Close",

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
}
