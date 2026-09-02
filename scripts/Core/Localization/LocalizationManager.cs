using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Centralized localization service managing dynamic English and Vietnamese translations
/// via Godot's TranslationServer and reactive change events.
/// </summary>
public static class LocalizationManager
{
    public const string LangEnglish = "en";
    public const string LangVietnamese = "vi";

    public static event Action? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = LangEnglish;

    private static readonly Dictionary<string, string> EnTranslations = new()
    {
        // Main Menu Core
        ["MENU_TITLE"] = "CHRONICLES OF THE EMPIRES",
        ["MENU_SUBTITLE"] = "◆ MYTHIC AGES ◆",
        ["MENU_NEW_CAMPAIGN"] = "New Campaign ▸",
        ["MENU_LOAD_GAME"] = "Load Game",
        ["MENU_SETTINGS"] = "Settings",
        ["MENU_EXIT"] = "Exit to Desktop",
        ["MENU_ARCHIVES"] = "📖 Archives",
        ["MENU_CREDITS"] = "👥 Credits",
        ["MENU_DISCORD"] = "💬 Discord",
        ["MENU_ARCHIVES_TOOLTIP"] = "Civilopedia & Archives",
        ["MENU_CREDITS_TOOLTIP"] = "Development Credits",
        ["MENU_DISCORD_TOOLTIP"] = "Official Community Discord",

        // Campaign Mode Selector
        ["CAMPAIGN_HEADER"] = "SELECT CAMPAIGN MODE",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ Campaign",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ Skirmish",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Tutorial",
        ["CAMPAIGN_DEPLOY"] = "⚔ DEPLOY CAMPAIGN",
        ["SPEC_MAP_PREFIX"] = "• Map: ",
        ["SPEC_RIVALS_PREFIX"] = "• Rivals: ",
        ["SPEC_VICTORY_PREFIX"] = "• Victory: ",

        ["MODE_GRAND_NAME"] = "GRAND CAMPAIGN",
        ["MODE_GRAND_ERA"] = "◆ BRONZE AGE ERA ◆",
        ["MODE_GRAND_DESC"] = "Lead your clan from the dawn of bronze working. Tame war elephants, expand across fertile river valleys, and unite 16 rival civilizations.",
        ["MODE_GRAND_MAP"] = "Continental (128x128 Hex)",
        ["MODE_GRAND_RIVALS"] = "16 Ancient Civilizations",
        ["MODE_GRAND_VICTORY"] = "Conquest / Cultural Hegemony / Wonders",

        ["MODE_SKIRMISH_NAME"] = "SKIRMISH BATTLE",
        ["MODE_SKIRMISH_ERA"] = "◆ CUSTOM BATTLEFIELD ◆",
        ["MODE_SKIRMISH_DESC"] = "Engage rival warlords in fast-paced skirmishes on procedural maps with customizable army compositions and tactical terrain.",
        ["MODE_SKIRMISH_MAP"] = "Custom (64x64 Hex)",
        ["MODE_SKIRMISH_RIVALS"] = "2 - 8 Warlords",
        ["MODE_SKIRMISH_VICTORY"] = "Military Domination / Regicide",

        ["MODE_TUTORIAL_NAME"] = "IMPERIAL TUTORIAL",
        ["MODE_TUTORIAL_ERA"] = "◆ TACTICAL ACADEMY ◆",
        ["MODE_TUTORIAL_DESC"] = "Step-by-step primer on hexagonal maneuvering, bronze metallurgy, hydraulic irrigation, and combined-arms archery tactics.",
        ["MODE_TUTORIAL_MAP"] = "Guided (32x32 Hex)",
        ["MODE_TUTORIAL_RIVALS"] = "1 Instructor vs 1 Practice AI",
        ["MODE_TUTORIAL_VICTORY"] = "Objective Mastery",

        // Settings
        ["SETTINGS_TITLE"] = "SETTINGS",
        ["SETTINGS_TAB_AUDIO"] = "Audio",
        ["SETTINGS_TAB_VIDEO"] = "Display",
        ["SETTINGS_TAB_GAMEPLAY"] = "Language",
        ["SETTINGS_MASTER_VOL"] = "Master Volume",
        ["SETTINGS_SFX_VOL"] = "Sound Effects",
        ["SETTINGS_MUSIC_VOL"] = "Music Volume",
        ["SETTINGS_FULLSCREEN"] = "Fullscreen Mode",
        ["SETTINGS_VSYNC"] = "Vertical Sync (VSync)",
        ["SETTINGS_LANGUAGE"] = "Game Language",
        ["SETTINGS_BTN_CLOSE"] = "✕ Close"
    };

    private static readonly Dictionary<string, string> ViTranslations = new()
    {
        // Main Menu Core
        ["MENU_TITLE"] = "KỶ NGUYÊN ĐẾ CHẾ",
        ["MENU_SUBTITLE"] = "◆ THỜI KỲ HUYỀN SỬ ◆",
        ["MENU_NEW_CAMPAIGN"] = "Chiến Dịch Mới ▸",
        ["MENU_LOAD_GAME"] = "Tải Bản Lưu",
        ["MENU_SETTINGS"] = "Cài Đặt",
        ["MENU_EXIT"] = "Thoát Trò Chơi",
        ["MENU_ARCHIVES"] = "📖 Thư Khố",
        ["MENU_CREDITS"] = "👥 Đội Ngũ",
        ["MENU_DISCORD"] = "💬 Cộng Đồng",
        ["MENU_ARCHIVES_TOOLTIP"] = "Thư Khố & Bách Khoa Toàn Thư",
        ["MENU_CREDITS_TOOLTIP"] = "Đội Ngũ Phát Triển & Vinh Danh",
        ["MENU_DISCORD_TOOLTIP"] = "Kênh Discord Cộng Đồng Chính Thức",

        // Campaign Mode Selector
        ["CAMPAIGN_HEADER"] = "CHỌN CHẾ ĐỘ CHIẾN DỊCH",
        ["CAMPAIGN_BTN_GRAND"] = "⚔ Chiến Dịch",
        ["CAMPAIGN_BTN_SKIRMISH"] = "⚡ Thư Hùng",
        ["CAMPAIGN_BTN_TUTORIAL"] = "📜 Tập Trận",
        ["CAMPAIGN_DEPLOY"] = "⚔ VÀO TRẬN (DEPLOY)",
        ["SPEC_MAP_PREFIX"] = "• Quy mô: ",
        ["SPEC_RIVALS_PREFIX"] = "• Đối thủ: ",
        ["SPEC_VICTORY_PREFIX"] = "• Điều kiện: ",

        ["MODE_GRAND_NAME"] = "ĐẠI CHIẾN DỊCH",
        ["MODE_GRAND_ERA"] = "◆ THỜI ĐẠI ĐỒ ĐỒNG ĐÔNG SƠN ◆",
        ["MODE_GRAND_DESC"] = "Lãnh đạo thị tộc từ thuở đồ đồng, khai phá lưu vực sông Hồng, thuần dưỡng voi chiến và thôn tính 16 lãnh bang để thống nhất giang sơn.",
        ["MODE_GRAND_MAP"] = "Lục địa Mở Rộng (128x128 Ô)",
        ["MODE_GRAND_RIVALS"] = "16 Lãnh Bang Cổ",
        ["MODE_GRAND_VICTORY"] = "Chinh Phục / Văn Hóa Bá Quyền / Kỳ Quan",

        ["MODE_SKIRMISH_NAME"] = "THƯ HÙNG TÙY BIẾN",
        ["MODE_SKIRMISH_ERA"] = "◆ CHIẾN TRƯỜNG SA BÀN ◆",
        ["MODE_SKIRMISH_DESC"] = "Trận chiến tác chiến nhanh trên bản đồ ngẫu nhiên. Tự do định đoạt binh lực, địa hình hiểm trở và quy tắc giao chiến.",
        ["MODE_SKIRMISH_MAP"] = "Bản đồ Tùy Biến (64x64 Ô)",
        ["MODE_SKIRMISH_RIVALS"] = "2 - 8 Chúa Đất AI",
        ["MODE_SKIRMISH_VICTORY"] = "Quân Sự Hủy Diệt / Trảm Tướng",

        ["MODE_TUTORIAL_NAME"] = "BINH PHÁP TẬP TRẬN",
        ["MODE_TUTORIAL_ERA"] = "◆ HƯỚNG DẪN CHỈ HUY ◆",
        ["MODE_TUTORIAL_DESC"] = "Chỉ dẫn từng bước thao tác sa bàn lục giác, kỹ nghệ đúc đồng thiếc, quản lý thủy nông lúa nước và điều phối cung thủ chiến trường.",
        ["MODE_TUTORIAL_MAP"] = "Sa Bàn Giới Hạn (32x32 Ô)",
        ["MODE_TUTORIAL_RIVALS"] = "1 Giáo Đầu vs 1 AI Tập Trận",
        ["MODE_TUTORIAL_VICTORY"] = "Hoàn Thành Mục Tiêu Sa Bàn",

        // Settings
        ["SETTINGS_TITLE"] = "CÀI ĐẶT HỆ THỐNG",
        ["SETTINGS_TAB_AUDIO"] = "Âm Thanh",
        ["SETTINGS_TAB_VIDEO"] = "Hiển Thị",
        ["SETTINGS_TAB_GAMEPLAY"] = "Ngôn Ngữ",
        ["SETTINGS_MASTER_VOL"] = "Âm Lượng Tổng",
        ["SETTINGS_SFX_VOL"] = "Hiệu Ứng Âm",
        ["SETTINGS_MUSIC_VOL"] = "Nhạc Nền",
        ["SETTINGS_FULLSCREEN"] = "Toàn Màn Hình",
        ["SETTINGS_VSYNC"] = "Đồng Bộ Khung Hình (VSync)",
        ["SETTINGS_LANGUAGE"] = "Ngôn Ngữ Hiển Thị",
        ["SETTINGS_BTN_CLOSE"] = "✕ Đóng"
    };

    static LocalizationManager()
    {
        RegisterTranslations();
    }

    public static void Initialize()
    {
        RegisterTranslations();
    }

    private static void RegisterTranslations()
    {
        var enTrans = new Translation { Locale = LangEnglish };
        foreach (var (k, v) in EnTranslations)
        {
            enTrans.AddMessage(k, v);
        }
        TranslationServer.AddTranslation(enTrans);

        var viTrans = new Translation { Locale = LangVietnamese };
        foreach (var (k, v) in ViTranslations)
        {
            viTrans.AddMessage(k, v);
        }
        TranslationServer.AddTranslation(viTrans);
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

