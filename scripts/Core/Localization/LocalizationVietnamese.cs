using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

public static partial class LocalizationManager
{
    private static readonly Dictionary<string, string> ViTranslations = new()
    {
        // Core Game Title
        ["MENU_TITLE"] = "Váº N QUá»C PHONG VĂ‚N KĂ",
        ["MENU_SUBTITLE"] = "â—† THá»œI Ká»² HUYá»€N Sá»¬ â—†",
        ["MENU_NEW_CAMPAIGN"] = "Chiáº¿n Dá»‹ch Má»›i â–¸",
        ["MENU_LOAD_GAME"] = "Táº£i Báº£n LÆ°u",
        ["MENU_SETTINGS"] = "CĂ i Äáº·t",
        ["MENU_EXIT"] = "ThoĂ¡t TrĂ² ChÆ¡i",
        ["MENU_ARCHIVES"] = "đŸ“– ThÆ° Khá»‘",
        ["MENU_CREDITS"] = "đŸ‘¥ Äá»™i NgÅ©",
        ["MENU_DISCORD"] = "đŸ’¬ Cá»™ng Äá»“ng",
        ["MENU_ARCHIVES_TOOLTIP"] = "ThÆ° Khá»‘ & BĂ¡ch Khoa ToĂ n ThÆ° Váº¡n Quá»‘c",
        ["MENU_CREDITS_TOOLTIP"] = "Äá»™i NgÅ© PhĂ¡t Triá»ƒn & Vinh Danh",
        ["MENU_DISCORD_TOOLTIP"] = "KĂªnh Discord Cá»™ng Äá»“ng ChĂ­nh Thá»©c",

        // Campaign Mode Selector
        ["CAMPAIGN_MODAL_TITLE"] = "CHá»ŒN CHáº¾ Äá»˜ CHIáº¾N Dá»CH",
        ["CAMPAIGN_HEADER"] = "CHá»ŒN CHáº¾ Äá»˜ CHIáº¾N Dá»CH",
        ["CAMPAIGN_TAB_CAMPAIGN"] = "â” Chiáº¿n Dá»‹ch (áº¢i)",
        ["CAMPAIGN_TAB_SKIRMISH"] = "â¡ ThÆ° HĂ¹ng (Map)",
        ["CAMPAIGN_TAB_TRAINING"] = "đŸ“œ Táº­p Tráº­n & Sandbox",
        ["CAMPAIGN_BTN_GRAND"] = "â” Chiáº¿n Dá»‹ch",
        ["CAMPAIGN_BTN_SKIRMISH"] = "â¡ ThÆ° HĂ¹ng",
        ["CAMPAIGN_BTN_TUTORIAL"] = "đŸ“œ Táº­p Tráº­n",
        ["CAMPAIGN_DEPLOY"] = "â” VĂ€O TRáº¬N (DEPLOY)",
        ["CAMPAIGN_START_SKIRMISH"] = "â¡ VĂ€O THÆ¯ HĂ™NG",
        ["CAMPAIGN_START_TRAINING"] = "đŸ“œ VĂ€O Táº¬P TRáº¬N",
        ["SPEC_MAP_PREFIX"] = "Sa bĂ n: ",
        ["SPEC_RIVALS_PREFIX"] = "Äá»‘i thá»§: ",
        ["SPEC_VICTORY_PREFIX"] = "Má»¥c tiĂªu: ",
        ["SPEC_REWARD_PREFIX"] = "Pháº§n thÆ°á»Ÿng: ",

        // Mode Details for CampaignModePanel
        ["MODE_GRAND_NAME"] = "Äáº¡i Chiáº¿n Dá»‹ch",
        ["MODE_GRAND_ERA"] = "Ká»· NguyĂªn Váº¡n Quá»‘c (4X VÄ© MĂ´ & RTS Táº¡m Dá»«ng)",
        ["MODE_GRAND_DESC"] = "LĂ£nh Ä‘áº¡o ná»n vÄƒn minh huyá»n sá»­ qua cĂ¡c phĂ¢n vĂ¹ng lĂ£nh thá»• tranh cuá»™n. XĂ¢y dá»±ng ká»³ quan, khai phĂ¡ lÆ°u vá»±c sĂ´ng vĂ  trá»±c tiáº¿p chá»‰ huy cĂ¡c trung Ä‘oĂ n thá»i gian thá»±c cĂ³ táº¡m dá»«ng.",
        ["MODE_GRAND_MAP"] = "PhĂ¢n VĂ¹ng LĂ£nh Thá»• Tranh Cuá»™n",
        ["MODE_GRAND_RIVALS"] = "16 Äáº¿ Bang thuá»™c 4 Khá»‘i VÄƒn HĂ³a",
        ["MODE_GRAND_VICTORY"] = "BĂ¡ Quyá»n Äáº¿ Cháº¿ hoáº·c Tháº§n KhĂ­ Ká»³ Quan",

        ["MODE_SKIRMISH_NAME"] = "ThÆ° HĂ¹ng Tá»± Do",
        ["MODE_SKIRMISH_ERA"] = "Sa BĂ n Chiáº¿n Thuáº­t (TĂ¹y Chá»‰nh)",
        ["MODE_SKIRMISH_DESC"] = "Khai mĂ n giao tranh tá»©c thĂ¬ trĂªn lÆ°á»›i thoi isometric hoáº·c phĂ¢n vĂ¹ng lĂ£nh thá»• vá»›i tĂ¹y chá»n sá»‘ phe AI, Ä‘á»‹a hĂ¬nh sinh thĂ¡i vĂ  Ä‘iá»u kiá»‡n tháº¯ng.",
        ["MODE_SKIRMISH_MAP"] = "TĂ¹y Biáº¿n (Nhá», TiĂªu Chuáº©n, Äáº¡i Lá»¥c)",
        ["MODE_SKIRMISH_RIVALS"] = "2 Ä‘áº¿n 8 Phe PhĂ¡i",
        ["MODE_SKIRMISH_VICTORY"] = "Chinh Phá»¥c, Ká»³ Quan, Tráº£m TÆ°á»›ng",

        ["MODE_TUTORIAL_NAME"] = "Táº­p Tráº­n & Sa BĂ n Thá»±c Nghiá»‡m",
        ["MODE_TUTORIAL_ERA"] = "Binh PhĂ¡p CÆ¡ Báº£n & Thao TrÆ°á»ng Thá»­ Nghiá»‡m",
        ["MODE_TUTORIAL_DESC"] = "Luyá»‡n táº­p thao tĂ¡c Ä‘iá»u khiá»ƒn trung Ä‘oĂ n, lá»‡nh Táº¡m Dá»«ng TĂ¡c Chiáº¿n (Space), kháº¯c cháº¿ binh chá»§ng vĂ  lá»£i tháº¿ Ä‘á»‹a hĂ¬nh.",
        ["MODE_TUTORIAL_MAP"] = "Thao TrÆ°á»ng & Äáº¥u TrÆ°á»ng Isometric",
        ["MODE_TUTORIAL_RIVALS"] = "1 QuĂ¢n Huáº¥n Luyá»‡n",
        ["MODE_TUTORIAL_VICTORY"] = "HoĂ n ThĂ nh Má»¥c TiĂªu Binh PhĂ¡p",

        // Cultural Blocks
        ["BLOCK_ALLUVIUM_NAME"] = "BĂ¡ch Tháº§n & PhĂ¹ Sa",
        ["BLOCK_ALLUVIUM_REGION"] = "LÆ°u Vá»±c SĂ´ng Nam & TĂ¢y Nam Ă",
        ["BLOCK_ALLUVIUM_DESC"] = "Äá»“ng báº±ng chĂ¢u thá»• phĂ¬ nhiĂªu, phĂ²ng tuyáº¿n lĂ²ng sĂ´ng cá»• kĂ­nh, Ä‘á»n thĂ¡p linh thiĂªng vĂ  nghi lá»… huyá»n bĂ­.",

        ["BLOCK_MANDATE_NAME"] = "Thiáº¿t Ká»· & ThiĂªn Má»‡nh",
        ["BLOCK_MANDATE_REGION"] = "ÄĂ´ng Báº¯c Ă & Tháº£o NguyĂªn Bao La",
        ["BLOCK_MANDATE_DESC"] = "Ká»· luáº­t giĂ¡p sáº¯t nghiĂªm ngáº·t, há»a xa cĂ´ng thĂ nh, ká»µ binh du má»¥c vĂ  thiĂªn má»‡nh hoĂ ng gia.",

        ["BLOCK_STEEL_NAME"] = "ThĂ¡nh Huy & Trá»ng ThĂ©p",
        ["BLOCK_STEEL_REGION"] = "Bá» Biá»ƒn Äá»‹a Trung Háº£i & TĂ¢y Ă‚u",
        ["BLOCK_STEEL_DESC"] = "Tráº­n Ä‘á»“ giĂ¡o khiĂªn báº¥t kháº£ xĂ¢m pháº¡m, cĂº hĂºc ká»µ sÄ© hoĂ ng gia, bĂ£o tĂªn trÆ°á»ng cung vĂ  thiáº¿t giĂ¡p kiĂªn cá»‘.",

        ["BLOCK_FROST_NAME"] = "Hoang Tháº§n & BÄƒng LĂ£nh",
        ["BLOCK_FROST_REGION"] = "ÄĂ´ng Ă‚u, Balkan & Rá»«ng GiĂ  Cá»• Äáº¡i",
        ["BLOCK_FROST_DESC"] = "TĂ­n ngÆ°á»¡ng Ä‘a tháº§n nguyĂªn thá»§y, chiáº¿n binh bÄƒng giĂ¡, ká»µ binh cĂ¡nh Ä‘áº¡i bĂ ng vĂ  dÅ©ng khĂ­ rá»«ng thiĂªng.",

        // 18 Civilizations
        // 1. Vietnam
        ["CIV_VN_NAME"] = "Viá»‡t Nam (Ă‚u Láº¡c / BĂ¡ch Viá»‡t)",
        ["CIV_VN_EPITHET"] = "ChĂºa Tá»ƒ LÆ°u Vá»±c PhĂ¹ Sa",
        ["CIV_VN_COLORS"] = "Xanh RĂªu Äá»“ng Cá»• (#43B3AE) & VĂ ng LĂºa (#E5A93C)",
        ["CIV_VN_ATTIRE"] = "NĂ³n lĂ´ng chim Láº¡c nhá»n cao vĂºt, há»™ tĂ¢m phiáº¿n báº±ng Ä‘á»“ng trĂ²n cháº¡m hoa vÄƒn ÄĂ´ng SÆ¡n, khá»‘ dá»‡t ngáº¯n",
        ["CIV_VN_UNIT"] = "Ná» LiĂªn ChĂ¢u & TÆ°á»£ng Binh GiĂ¡p MĂ¢y",
        ["CIV_VN_TRAIT"] = "TĂ ng hĂ¬nh trong rá»«ng/Ä‘áº§m láº§y, báº¯n xuyĂªn giĂ¡p, báº«y cá»c ngáº§m lĂ²ng sĂ´ng, buff vÄ©nh viá»…n sau lÅ© lá»¥t",

        // 2. Khmer
        ["CIV_KHMER_NAME"] = "Khmer (Äáº¿ Cháº¿ Angkor / PhĂ¹ Nam)",
        ["CIV_KHMER_EPITHET"] = "Kiáº¿n TrĂºc Gia Há»“ Tháº§n Thá»§y",
        ["CIV_KHMER_COLORS"] = "VĂ ng Sa KhoĂ¡ng (#D4AF37) & NĂ¢u Äáº¥t Nung (#8B4513)",
        ["CIV_KHMER_ATTIRE"] = "Váº£i quáº¥n Sampot dá»‡t vĂ ng, giĂ¡p ngá»±c nung kháº¯c tÆ°á»£ng Apsara, lĂ­nh xÄƒm bĂ¹a Yantra toĂ n thĂ¢n",
        ["CIV_KHMER_UNIT"] = "Voi Bá»c GiĂ¡p NgĂ  VĂ ng & Vá»‡ Binh Äá»n ThĂ¡p",
        ["CIV_KHMER_TRAIT"] = "Há»“ Baray cáº¥p lÆ°Æ¡ng cá»±c háº¡n vĂ¹ng khĂ´ cáº±n, Ä‘á»n thĂ¡p tÄƒng cÆ°á»ng sinh lá»±c vĂ  phĂ²ng thá»§",

        // 3. India
        ["CIV_INDIA_NAME"] = "áº¤n Äá»™ (VÆ°Æ¡ng Triá»u Maurya Cá»•)",
        ["CIV_INDIA_EPITHET"] = "NgÆ°á»i Náº¯m Giá»¯ VĂ²ng TrĂ²n Tháº§n Nháº­t",
        ["CIV_INDIA_COLORS"] = "Cam Nghá»‡ (#FF9933) & Tráº¯ng Lá»¥a (#F8F8FF)",
        ["CIV_INDIA_ATTIRE"] = "KhÄƒn Turban to báº£n Ä‘Ă­nh ngá»c, Ă¡o lá»¥a má»ng viá»n kim loáº¡i báº¡c",
        ["CIV_INDIA_UNIT"] = "Xáº¡ Thá»§ PhĂ³ng ÄÄ©a Chakram & Chiáº¿n TÆ°á»£ng Nghi Lá»…",
        ["CIV_INDIA_TRAIT"] = "ÄÄ©a Chakram náº£y má»¥c tiĂªu liĂªn hoĂ n, Chiáº¿n tÆ°á»£ng nghi lá»… phĂ¡t quang tÄƒng vá»t sÄ© khĂ­",

        // 4. Persia
        ["CIV_PERSIA_NAME"] = "Ba TÆ° (Ká»³ Thuáº­t Sassanid)",
        ["CIV_PERSIA_EPITHET"] = "Háº­u Duá»‡ QuĂ¢n ÄoĂ n Báº¥t Tá»­",
        ["CIV_PERSIA_COLORS"] = "TĂ­m HoĂ ng Gia (#66023C) & VĂ ng Nghá»‡ (#DDAA00)",
        ["CIV_PERSIA_ATTIRE"] = "KhÄƒn lá»¥a tĂ­m bá»‹t ná»­a máº·t, Ă¡o dá»‡t sá»£i vĂ ng khoĂ¡c ngoĂ i giĂ¡p váº£y kim hoĂ n, khiĂªn Ä‘an liá»…u gai",
        ["CIV_PERSIA_UNIT"] = "Ká»µ Binh Thiáº¿t GiĂ¡p Cataphract & QuĂ¢n ÄoĂ n Báº¥t Tá»­ (Immortals)",
        ["CIV_PERSIA_TRAIT"] = "QuĂ¢n Ä‘oĂ n Báº¥t Tá»­ tá»± há»“i sinh khi tá»­ tráº­n; ká»µ binh thiáº¿t giĂ¡p hĂºc sáº­p phĂ²ng tuyáº¿n",

        // 5. China
        ["CIV_CHINA_NAME"] = "Trung Hoa (Hoa Háº¡ Cá»• Triá»u)",
        ["CIV_CHINA_EPITHET"] = "Báº­c Tháº§y ThĂ nh LÅ©y & Há»a Lá»±c",
        ["CIV_CHINA_COLORS"] = "Äá» Tháº«m HoĂ ng Gia (#8B0000) & Äen Then (#1C1C1C)",
        ["CIV_CHINA_ATTIRE"] = "GiĂ¡p phiáº¿n sáº¯t vuĂ´ng vá»©c thá»i Táº§n - HĂ¡n, mÅ© sáº¯t cáº¯m lĂ´ng trÄ© Ä‘á» dĂ i",
        ["CIV_CHINA_UNIT"] = "MĂ¡y Báº¯n Há»a Tiá»…n & Ná» CÆ¡ Giá»›i Diá»‡n Rá»™ng",
        ["CIV_CHINA_TRAIT"] = "Äáº©y mĂ¡y báº¯n há»a tiá»…n, xĂ¢y thĂ nh lÅ©y kiĂªn cá»‘ vĂ  Ä‘á»“ng hĂ³a thĂ nh phá»‘ siĂªu tá»‘c",

        // 6. Japan
        ["CIV_JAPAN_NAME"] = "Nháº­t Báº£n (Thá»i Ká»³ Yamato / Tháº§n Äáº¡o)",
        ["CIV_JAPAN_EPITHET"] = "Tháº§n Kiáº¿m Máº·t Trá»i ThĂ¡i DÆ°Æ¡ng",
        ["CIV_JAPAN_COLORS"] = "Tráº¯ng Tuyáº¿t (#FFFAFA) & Äá» Son Torii (#C8382B)",
        ["CIV_JAPAN_ATTIRE"] = "GiĂ¡p gá»— sÆ¡n mĂ i O-Yoroi, mÅ© Kabuto gáº¯n sá»«ng trÄƒng khuyáº¿t lá»›n, cá» lá»‡nh chá»¯ nháº­t sau lÆ°ng",
        ["CIV_JAPAN_UNIT"] = "Samurai ThĂ­ Huyáº¿t & PhĂ¹ Thá»§y Onmyoji",
        ["CIV_JAPAN_TRAIT"] = "Samurai ThĂ­ Huyáº¿t tÄƒng sĂ¡t thÆ°Æ¡ng khi cáº¡n mĂ¡u, phĂ¹ thá»§y Onmyoji gá»i sÆ°Æ¡ng mĂ¹ che máº¯t Ä‘á»‘i phÆ°Æ¡ng",

        // 7. Korea
        ["CIV_KOREA_NAME"] = "HĂ n Quá»‘c (Tam Quá»‘c Cá»•: Goguryeo / Joseon)",
        ["CIV_KOREA_EPITHET"] = "Vá»‡ TÆ°á»›ng Äá»“i NĂºi RĂ¬a ÄĂ´ng",
        ["CIV_KOREA_COLORS"] = "Xanh Lam Cobalt (#0047AB) & NĂ¢u Da Thuá»™c (#7B3F00)",
        ["CIV_KOREA_ATTIRE"] = "MÅ© chĂ³p ná»‰ cáº¯m hai lĂ´ng chim sÄƒn má»“i vá»ƒnh ngang, giĂ¡p váº£y cĂ¡ bĂ³ sĂ¡t linh hoáº¡t",
        ["CIV_KOREA_UNIT"] = "Hwarang CÆ°á»¡i Ngá»±a Báº¯n Tá»‰a & Chiáº¿n Xa Báº¯n BĂ£o TĂªn Hwacha",
        ["CIV_KOREA_TRAIT"] = "Hwarang cÆ°á»¡i ngá»±a leo dá»‘c nĂºi báº¯n tá»‰a vĂ  chiáº¿n xa Hwacha phĂ³ng bĂ£o tĂªn há»§y diá»‡t",

        // 8. Mongolia
        ["CIV_MONGOLIA_NAME"] = "MĂ´ng Cá»• (Tháº£o NguyĂªn Äáº¡i HĂ£n)",
        ["CIV_MONGOLIA_EPITHET"] = "Ká»µ SÄ© Trá»i Xanh VÄ©nh Háº±ng",
        ["CIV_MONGOLIA_COLORS"] = "Xanh Da Trá»i Tháº£o NguyĂªn (#4682B4) & NĂ¢u LĂ´ng SĂ³i (#5C4033)",
        ["CIV_MONGOLIA_ATTIRE"] = "MÅ© lĂ´ng thĂº tai cá»«u rá»§, Ă¡o choĂ ng da Deel lĂ³t lĂ´ng, cung tĂªn phá»©c há»£p pháº£n khĂºc",
        ["CIV_MONGOLIA_UNIT"] = "Xáº¡ Thá»§ Keshik & Thiáº¿t Ká»µ Du Má»¥c",
        ["CIV_MONGOLIA_TRAIT"] = "ToĂ n quĂ¢n lĂ  ká»µ binh; xáº¡ thá»§ Keshik vá»«a phi ngá»±a vá»«a báº¯n, táº¥n cĂ´ng xong rĂºt lui ngay trong lÆ°á»£t",

        // 9. Greece
        ["CIV_GREECE_NAME"] = "Hi Láº¡p (Hellenic / Sparta & Athens)",
        ["CIV_GREECE_EPITHET"] = "PhĂ²ng Tuyáº¿n KhiĂªn Äá»“ng Biá»ƒn Aegean",
        ["CIV_GREECE_COLORS"] = "Äá»“ng Thau (#CD7F32) & Äá» Cá» Sparta (#990000)",
        ["CIV_GREECE_ATTIRE"] = "MÅ© Corinthian Ä‘á»“ng thau vá»›i mĂ o lĂ´ng ngá»±a Ä‘á» uá»‘n cong, khiĂªn trĂ²n Aspis cháº¡m chá»¯ Lambda, giĂ¡p ngá»±c Ä‘Ăºc khá»‘i cÆ¡ bá»¥ng",
        ["CIV_GREECE_UNIT"] = "Tráº­n Äá»“ GiĂ¡o KhiĂªn Hoplite Phalanx & Chiáº¿n Thuyá»n ÄĂ¢m Trireme",
        ["CIV_GREECE_TRAIT"] = "Tráº­n Ä‘á»“ giĂ¡o khiĂªn Hoplite Phalanx pháº£n Ä‘Ă²n trá»±c diá»‡n 100% cáº­n chiáº¿n",

        // 10. England
        ["CIV_ENGLAND_NAME"] = "Anh (Anglo-Saxon / Arthurian)",
        ["CIV_ENGLAND_EPITHET"] = "Cung Thá»§ Rá»«ng GiĂ  Arthur",
        ["CIV_ENGLAND_COLORS"] = "Xanh LĂ¡ CĂ¢y Rá»«ng (#228B22) & NĂ¢u BĂ¹n (#6E5334)",
        ["CIV_ENGLAND_ATTIRE"] = "Cung dĂ i Longbow cao vÆ°á»£t Ä‘áº§u ngÆ°á»i, Ă¡o cháº½n ná»‰ gai, mÅ© cháº£o sáº¯t trĂ²n Kettle hat",
        ["CIV_ENGLAND_UNIT"] = "Xáº¡ Thá»§ TrÆ°á»ng Cung Longbow & Tráº£m Binh",
        ["CIV_ENGLAND_TRAIT"] = "Xáº¡ thá»§ táº§m báº¯n xa nháº¥t game, dá»±ng cá»c nhá»n báº«y ká»µ binh",

        // 11. France
        ["CIV_FRANCE_NAME"] = "PhĂ¡p (Hiá»‡p SÄ© HoĂ ng Gia)",
        ["CIV_FRANCE_EPITHET"] = "Ká»µ SÄ© Hoa BĂ¡ch Há»£p HoĂ ng Gia",
        ["CIV_FRANCE_COLORS"] = "Xanh DÆ°Æ¡ng HoĂ ng Gia (#4169E1) & Tráº¯ng Báº¡c (#E0E0E0)",
        ["CIV_FRANCE_ATTIRE"] = "Ngá»±a chiáº¿n bá»c váº£i phá»§ hoa bĂ¡ch há»£p thĂªu chá»‰ vĂ ng, ká»µ sÄ© mÅ© kĂ­n mĂ­t khe thá»Ÿ chá»¯ tháº­p, vĂ¡c thÆ°Æ¡ng dĂ i Lance",
        ["CIV_FRANCE_UNIT"] = "Ká»µ SÄ© Thiáº¿t GiĂ¡p HoĂ ng Gia & Ná» Thá»§ ThĂ nh LÅ©y",
        ["CIV_FRANCE_TRAIT"] = "CĂº hĂºc (Charge) lĂ m vá»¡ tan hĂ ng ngÅ© Ä‘á»‹ch vĂ  táº¡o khiĂªn giáº£m sĂ¡t thÆ°Æ¡ng cho quĂ¢n nhĂ ",

        // 12. Germany
        ["CIV_GERMANY_NAME"] = "Äá»©c (Tháº§n ThĂ¡nh La MĂ£ / Teutonic)",
        ["CIV_GERMANY_EPITHET"] = "Thiáº¿t GiĂ¡p Tháº­p Tá»± QuĂ¢n",
        ["CIV_GERMANY_COLORS"] = "Äen Tuyá»n (#111111) & Tráº¯ng Tinh KhĂ´i (#F5F5F5)",
        ["CIV_GERMANY_ATTIRE"] = "Ăo choĂ ng tráº¯ng in chá»¯ tháº­p Ä‘en ngá»±c, mÅ© sáº¯t gáº¯n cĂ¡nh/sá»«ng hÆ°Æ¡u Ä‘á»“ sá»™, giĂ¡p phiáº¿n Gothic",
        ["CIV_GERMANY_UNIT"] = "Hiá»‡p SÄ© Äáº¡i Kiáº¿m Zweihander & Ká»µ SÄ© Teutonic",
        ["CIV_GERMANY_TRAIT"] = "Hiá»‡p sÄ© vĂ¡c Ä‘áº¡i kiáº¿m Zweihander chĂ©m quĂ©t lan 3 Ă´, chuyĂªn phĂ¡ tan hĂ ng ngÅ© giĂ¡o khiĂªn",

        // 13. Spain
        ["CIV_SPAIN_NAME"] = "TĂ¢y Ban Nha (VÆ°Æ¡ng Quá»‘c Reconquista)",
        ["CIV_SPAIN_EPITHET"] = "PhÆ°Æ¡ng Tráº­n TrÆ°á»ng GiĂ¡o Báº¥t Báº¡i",
        ["CIV_SPAIN_COLORS"] = "VĂ ng Kim (#DAA520) & Äá» TÆ°Æ¡i (#C70039)",
        ["CIV_SPAIN_ATTIRE"] = "MÅ© vĂ nh thuyá»n nhá»n hai Ä‘áº§u Morion, giĂ¡p ngá»±c thĂ©p pháº£n quang, kiáº¿m thĂ©p Toledo",
        ["CIV_SPAIN_UNIT"] = "PhÆ°Æ¡ng Tráº­n Tercio & Ká»µ Binh Chinh Phá»¥c",
        ["CIV_SPAIN_TRAIT"] = "Äá»™i hĂ¬nh Tercio phá»‘i há»£p trÆ°á»ng giĂ¡o kiĂªn cá»‘ miá»…n nhiá»…m hoĂ n toĂ n cĂ¡c Ä‘Ă²n hĂºc ká»µ binh",

        // 14. Ancient Slavs
        ["CIV_SLAVS_NAME"] = "Slav Cá»• (Pagan Slavic / Tháº§n Sáº¥m Perun)",
        ["CIV_SLAVS_EPITHET"] = "Chiáº¿n Binh Cá»• Thá»¥ Tháº§n Sáº¥m",
        ["CIV_SLAVS_COLORS"] = "Äá» ThĂªu DĂ¢n Gian (#C8102E) & XĂ¡m Da SĂ³i (#696969)",
        ["CIV_SLAVS_ATTIRE"] = "Ăo lanh tráº¯ng thĂªu hoa vÄƒn chá»‰ Ä‘á» (Vyshyvanka), khoĂ¡c Ă¡o da sĂ³i, rĂ¢u táº¿t háº¡t gá»—",
        ["CIV_SLAVS_UNIT"] = "Chiáº¿n Binh Vityaz & Táº¿ Lá»… Tháº§n Sáº¥m",
        ["CIV_SLAVS_TRAIT"] = "Chiáº¿n binh Vityaz há»“i mĂ¡u gáº§n bĂ¬a rá»«ng, miá»…n nhiá»…m lĂ m cháº­m cá»§a bÄƒng giĂ¡",

        // 15. Ancient Bulgars
        ["CIV_BULGARS_NAME"] = "Bulgar Cá»• (HĂ£n Quá»‘c DĂ£ Sá»­ / Thá»i Khan Krum)",
        ["CIV_BULGARS_EPITHET"] = "Ká»µ SÄ© Ngá»n Cá» ThiĂªng Tangra",
        ["CIV_BULGARS_COLORS"] = "Äá» Huyáº¿t Dá»¥ (#8A0303) & Äen Than Cá»§i (#232B2B)",
        ["CIV_BULGARS_ATTIRE"] = "MÅ© sáº¯t chĂ³p vuá»‘t cong Ä‘Ă­nh Ä‘uĂ´i ngá»±a Ä‘en, cá»c cá» hiá»‡u thiĂªng tháº§n Tangra (Tug), Ă¡o da lá»™n trĂ¹m ngoĂ i giĂ¡p váº£y",
        ["CIV_BULGARS_UNIT"] = "Ká»µ Binh Boyar & DĂ¢n Binh Cuá»“ng Ná»™",
        ["CIV_BULGARS_TRAIT"] = "Ká»µ binh Boyar cÆ°á»›p tĂ i nguyĂªn trĂªn xĂ¡c tÆ°á»›ng Ä‘á»‹ch, bá»™ binh kĂ­ch hoáº¡t tráº¡ng thĂ¡i Huyáº¿t Thá»‡ cuá»“ng báº¡o khi sáº¯p cháº¿t",

        // 16. Kievan Rus
        ["CIV_RUS_NAME"] = "Nga (Kievan Rus DĂ£ Sá»­)",
        ["CIV_RUS_EPITHET"] = "Vá»‡ QuĂ¢n BÄƒng GiĂ¡ Dnieper",
        ["CIV_RUS_COLORS"] = "Äá» RÆ°á»£u ChĂ¡t (#800020) & Tráº¯ng Tuyáº¿t (#FFFAFA)",
        ["CIV_RUS_ATTIRE"] = "Ăo khoĂ¡c da gáº¥u dĂ y sá»¥ viá»n lĂ´ng thĂº lá»›n, mÅ© Spangenhelm vuá»‘t nhá»n cĂ³ lÆ°á»›i xĂ­ch che kĂ­n cáº±m, vĂ¡c rĂ¬u hai lÆ°á»¡i Bardiche",
        ["CIV_RUS_UNIT"] = "Vá»‡ Binh Druzhina & RĂ¬u Binh Bardiche",
        ["CIV_RUS_TRAIT"] = "Vá»‡ binh Druzhina nháº­n lÆ°á»£ng phĂ²ng ngá»± cá»±c Ä‘áº¡i trong Ä‘áº§m láº§y hoáº·c bĂ£o tuyáº¿t",

        // 17. Poland
        ["CIV_POLAND_NAME"] = "Ba Lan (Husaria Tháº§n Thoáº¡i)",
        ["CIV_POLAND_EPITHET"] = "Äáº¡i BĂ ng CĂ¡nh ThĂ©p Tháº§n Tá»‘c",
        ["CIV_POLAND_COLORS"] = "Äá» Cá» Tháº¯m (#DC143C) & Tráº¯ng LĂ´ng VÅ© (#FFFFFF)",
        ["CIV_POLAND_ATTIRE"] = "Khung cĂ¡nh lĂ´ng vÅ© Ä‘áº¡i bĂ ng cong vĂºt sau lÆ°ng Ă¡o giĂ¡p thĂ©p bĂ³ng loĂ¡ng, thÆ°Æ¡ng dĂ i gáº¯n lá»¥a hai mĂ u",
        ["CIV_POLAND_UNIT"] = "Ká»µ Binh Winged Husaria & Thiáº¿t Ká»µ Pancerni",
        ["CIV_POLAND_TRAIT"] = "Ká»µ binh Husaria phi nÆ°á»›c Ä‘áº¡i táº¡o Ă¢m thanh rĂ­t giĂ³ lĂ m giáº£m sÄ© khĂ­ khiáº¿n quĂ¢n Ä‘á»‹ch tá»± thĂ¡o cháº¡y",

        // 18. Mesoamerica
        ["CIV_MESO_NAME"] = "Má»¹ Báº£n Äá»‹a Cá»• (Maya / Mississippian Tiá»n Trung Cá»•)",
        ["CIV_MESO_EPITHET"] = "DÅ©ng SÄ© BĂ¡o Äá»‘m Rá»«ng GiĂ ",
        ["CIV_MESO_COLORS"] = "Äá» Äáº¥t Nung (#B22222) & Xanh LĂ´ng Váº¹t (#00A86B)",
        ["CIV_MESO_ATTIRE"] = "MÅ© lĂ´ng chim Æ°ng xĂ²e trĂ²n lá»›n, mĂ¬nh tráº§n sÆ¡n váº±n vá»‡n, Ă¡o giĂ¡p da bĂ¡o Ä‘á»‘m",
        ["CIV_MESO_UNIT"] = "DÅ©ng SÄ© BĂ¡o Äá»‘m Jaguar & Xáº¡ Thá»§ Lao Atlatl",
        ["CIV_MESO_TRAIT"] = "DÅ©ng sÄ© BĂ¡o Äá»‘m cháº¡y bá»™ nhanh ngang ká»µ binh, khĂ´ng bá»‹ trá»« tá»‘c Ä‘á»™ khi leo nĂºi Ä‘Ă¡ hay bÄƒng rá»«ng ráº­m",

        // Stages
        ["STAGE_1_TITLE"] = "áº¢i 1: Khá»Ÿi Nguá»“n VÄƒn Lang",
        ["STAGE_1_DESC"] = "ChiĂªu má»™ nĂ´ng binh, thuáº§n hĂ³a thá»§y vá»±c sĂ´ng Há»“ng, Ä‘áº·t ná»n mĂ³ng thá»‹ tá»™c Ä‘áº§u tiĂªn.",
        ["STAGE_1_OBJ"] = "Khai phĂ¡ 3 khoáº£nh lĂºa nÆ°á»›c & chiĂªu má»™ 15 dĂ¢n binh",
        ["STAGE_1_ENEMY"] = "Man Di sĂ´ng ÄĂ  (Thá»‹ tá»™c Háº¯c Thá»§y)",
        ["STAGE_1_REWARD"] = "Má»Ÿ khĂ³a: Ká»¹ nghá»‡ Ä‘Ăºc Ä‘á»“ng sÆ¡ ká»³ & Ná» nung lá»­a",

        ["STAGE_2_TITLE"] = "áº¢i 2: ÄĂºc Trá»‘ng Äá»“ng",
        ["STAGE_2_DESC"] = "Khai thĂ¡c má» thiáº¿c & má» Ä‘á»“ng, Ä‘Ăºc tháº§n khĂ­ trá»‘ng Ä‘á»“ng linh thiĂªng, cháº¥n hÆ°ng binh sÄ©.",
        ["STAGE_2_OBJ"] = "ÄĂºc 1 Trá»‘ng Äá»“ng Tháº§n KhĂ­ & nĂ¢ng cáº¥p lĂ² rĂ¨n cáº¥p 2",
        ["STAGE_2_ENEMY"] = "Thá»‹ tá»™c Dáº¡ Lang",
        ["STAGE_2_REWARD"] = "Má»Ÿ khĂ³a: Chiáº¿n binh RĂ¬u Äá»“ng & HĂ o thĂ nh lÅ©y",

        ["STAGE_3_TITLE"] = "áº¢i 3: Ná» Tháº§n Cá»• Loa",
        ["STAGE_3_DESC"] = "XĂ¢y dá»±ng phĂ²ng tuyáº¿n lÅ©y thĂ nh xoĂ¡y á»‘c, dĂ n tráº­n ná» liĂªn chĂ¢u Ä‘áº©y lui ngoáº¡i xĂ¢m.",
        ["STAGE_3_OBJ"] = "Báº£o vá»‡ HoĂ ng ThĂ nh & tiĂªu diá»‡t 5 Ä‘á»£t quĂ¢n cáº£m tá»­",
        ["STAGE_3_ENEMY"] = "QuĂ¢n viá»…n chinh Triá»‡u ÄĂ ",
        ["STAGE_3_REWARD"] = "Má»Ÿ khĂ³a: Ná» Thá»§ Cá»• Loa LiĂªn ChĂ¢u",

        ["STAGE_4_TITLE"] = "áº¢i 4: Voi Chiáº¿n MĂª Linh",
        ["STAGE_4_DESC"] = "Thuáº§n phá»¥c thá»›t voi chiáº¿n hung hĂ£n, lĂ£nh Ä‘áº¡o cuá»™c khá»Ÿi nghÄ©a trá»«ng pháº¡t quĂ¢n thĂ¹.",
        ["STAGE_4_OBJ"] = "Thuáº§n phá»¥c 4 tÆ°á»£ng binh & háº¡ Ä‘á»“n trĂº Ä‘á»‹ch",
        ["STAGE_4_ENEMY"] = "QuĂ¢n Ä‘Ă´ há»™ ThĂ¡i thĂº TĂ´ Äá»‹nh",
        ["STAGE_4_REWARD"] = "Má»Ÿ khĂ³a: TÆ°á»£ng Binh GiĂ¡p Äá»“ng MĂª Linh",

        ["STAGE_5_TITLE"] = "áº¢i 5: Thá»‘ng Nháº¥t BĂ¡ch Viá»‡t",
        ["STAGE_5_DESC"] = "Há»™i quĂ¢n mÆ°á»i sĂ¡u lĂ£nh bang, quyáº¿t chiáº¿n táº¡i Ä‘á»‰nh NghÄ©a LÄ©nh, khai sinh vÆ°Æ¡ng triá»u ngĂ n nÄƒm.",
        ["STAGE_5_OBJ"] = "ThĂ´n tĂ­nh 3 bá»™ tá»™c thĂ¹ Ä‘á»‹ch hoáº·c xĂ¢y Ká»³ ÄĂ i VÄƒn Lang",
        ["STAGE_5_ENEMY"] = "LiĂªn minh 3 Äáº¡i LĂ£nh Bang Báº¯c Bá»™",
        ["STAGE_5_REWARD"] = "Danh hiá»‡u: HĂ™NG VÆ¯Æ NG Äá»† NHáº¤T & Thá»‘ng Nháº¥t SÆ¡n HĂ ",

        // Skirmish Settings
        ["SKIRMISH_MAP_SIZE"] = "KĂ­ch thÆ°á»›c báº£n Ä‘á»“",
        ["SKIRMISH_BIOME"] = "Äá»‹a hĂ¬nh sinh thĂ¡i",
        ["SKIRMISH_RIVALS"] = "Sá»‘ lÆ°á»£ng Ä‘á»‘i thá»§ AI",
        ["SKIRMISH_VICTORY"] = "Äiá»u kiá»‡n tháº¯ng",
        ["SKIRMISH_DIFFICULTY"] = "Äá»™ khĂ³ AI",
        ["MAP_SMALL"] = "Nhá» (32x32) - Giao tranh nhanh",
        ["MAP_MEDIUM"] = "TiĂªu chuáº©n (64x64) - Sa bĂ n chiáº¿n thuáº­t",
        ["MAP_LARGE"] = "Äáº¡i lá»¥c (128x128) - Äáº¡i chiáº¿n Ä‘áº¿ cháº¿",
        ["BIOME_RED_RIVER"] = "LÆ°u vá»±c sĂ´ng Há»“ng (PhĂ¹ sa, báº«y cá»c ngáº§m)",
        ["BIOME_JUNGLE"] = "Rá»«ng ráº­m nhiá»‡t Ä‘á»›i (áº¨n náº¥p, Ä‘áº§m láº§y, sÆ°Æ¡ng mĂ¹)",
        ["BIOME_HIGHLANDS"] = "Cao nguyĂªn Ä‘Ă¡ vĂ´i (KhoĂ¡ng sáº£n, hiá»ƒm trá»Ÿ)",
        ["BIOME_STEPPE"] = "Äáº¡i tháº£o nguyĂªn Ă-Ă‚u (BĂ¬nh nguyĂªn, ká»µ binh cÆ¡ Ä‘á»™ng)",
        ["BIOME_TAIGA"] = "Rá»«ng lĂ¡ kim bÄƒng tuyáº¿t (Äáº§m láº§y tuyáº¿t, hĂ nh quĂ¢n cháº­m)",
        ["BIOME_MEDITERRANEAN"] = "DuyĂªn háº£i Äá»‹a Trung Háº£i (Háº£i cáº£ng, phĂ¡o Ä‘Ă i)",
        ["RIVALS_2"] = "2 Phe (Song hĂ¹ng quyáº¿t Ä‘áº¥u)",
        ["RIVALS_4"] = "4 Phe (Quáº§n hĂ¹ng tranh phong)",
        ["RIVALS_6"] = "6 Phe (BĂ¡t ngĂ¡t sa bĂ n)",
        ["RIVALS_8"] = "8 Phe (Há»—n chiáº¿n Ä‘áº¿ cháº¿)",
        ["VICTORY_CONQUEST"] = "Chinh phá»¥c quĂ¢n sá»± (Conquest)",
        ["VICTORY_CULTURE"] = "BĂ¡ quyá»n vÄƒn hĂ³a & Ká»³ quan (Wonder)",
        ["VICTORY_REGICIDE"] = "Tráº£m tÆ°á»›ng Ä‘oáº¡t cá» (Regicide)",
        ["DIFF_EASY"] = "Dá»… (Táº­p sá»±)",
        ["DIFF_NORMAL"] = "TiĂªu chuáº©n (Chá»‰ huy)",
        ["DIFF_HARD"] = "KhĂ³ (Thá»§ lÄ©nh)",
        ["DIFF_LEGEND"] = "Huyá»n thoáº¡i (HĂ¹ng VÆ°Æ¡ng)",

        // Training
        ["TRAINING_SELECT"] = "Chá»n Cháº¿ Äá»™",
        ["TRAINING_SELECTED"] = "ÄĂ£ Chá»n",
        ["TRAINING_TUTORIAL_NAME"] = "Táº­p Huáº¥n Binh PhĂ¡p CÆ¡ Báº£n",
        ["TRAINING_SANDBOX_NAME"] = "Sa BĂ n Sandbox Test ÄÆ¡n Vá»‹",

        // Settings
        ["SETTINGS_TITLE"] = "CĂ€I Äáº¶T Há»† THá»NG",
        ["SETTINGS_TAB_AUDIO"] = "đŸ” Ă‚m Thanh",
        ["SETTINGS_TAB_VIDEO"] = "đŸ–¥ Hiá»ƒn Thá»‹",
        ["SETTINGS_TAB_GAMEPLAY"] = "đŸŒ NgĂ´n Ngá»¯",
        ["SETTINGS_MASTER_VOL"] = "Ă‚m LÆ°á»£ng Tá»•ng",
        ["SETTINGS_SFX_VOL"] = "Hiá»‡u á»¨ng Ă‚m (SFX)",
        ["SETTINGS_MUSIC_VOL"] = "Nháº¡c Ná»n (BGM)",
        ["SETTINGS_FULLSCREEN"] = "ToĂ n MĂ n HĂ¬nh",
        ["SETTINGS_VSYNC"] = "Äá»“ng Bá»™ Khung HĂ¬nh",
        ["SETTINGS_LANGUAGE"] = "NgĂ´n Ngá»¯ TrĂ² ChÆ¡i",
        ["SETTINGS_BTN_CLOSE"] = "âœ• ÄĂ³ng",

        // Display Settings
        ["SETTINGS_WINDOW_MODE"] = "Cháº¿ Äá»™ Cá»­a Sá»•",
        ["SETTINGS_RESOLUTION"] = "Äá»™ PhĂ¢n Giáº£i",
        ["SETTINGS_VSYNC_LABEL"] = "Äá»“ng Bá»™ Khung HĂ¬nh (VSync)",
        ["SETTINGS_MAX_FPS"] = "Giá»›i Háº¡n Khung HĂ¬nh (FPS)",
        ["SETTINGS_MODE_WINDOWED"] = "Cá»­a Sá»• (Windowed)",
        ["SETTINGS_MODE_BORDERLESS"] = "KhĂ´ng Viá»n (Borderless)",
        ["SETTINGS_MODE_FULLSCREEN"] = "ToĂ n MĂ n HĂ¬nh (Fullscreen)",
        ["SETTINGS_MODE_EXCLUSIVE"] = "ToĂ n MĂ n HĂ¬nh Äá»™c Quyá»n",
        ["SETTINGS_VSYNC_DISABLED"] = "Táº¯t",
        ["SETTINGS_VSYNC_ENABLED"] = "Báº­t",
        ["SETTINGS_VSYNC_ADAPTIVE"] = "ThĂ­ch á»¨ng (Adaptive)",
        ["SETTINGS_FPS_UNLIMITED"] = "KhĂ´ng Giá»›i Háº¡n",
        ["SETTINGS_UI_SCALE"] = "Tá»‰ Lá»‡ Giao Diá»‡n (UI Scale)",
        ["SETTINGS_BTN_SAVE"] = "đŸ’¾ LÆ°u & Ăp Dá»¥ng",
        ["SETTINGS_STATUS_SAVED"] = "ÄĂ£ lÆ°u & Ă¡p dá»¥ng cĂ i Ä‘áº·t!",
        ["SETTINGS_STATUS_SAVED_VI"] = "ÄĂ£ lÆ°u & Ă¡p dá»¥ng cĂ i Ä‘áº·t!"
    };
}
