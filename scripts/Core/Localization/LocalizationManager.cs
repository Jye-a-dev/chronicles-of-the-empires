using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Centralized localization service managing dynamic English and Vietnamese translations
/// for Chronicles of the Old Empires: Mythic Ages (Vạn Quốc Phong Vân Ký).
/// </summary>
public static partial class LocalizationManager
{
    public const string LangEnglish = "en";
    public const string LangVietnamese = "vi";
    public const string LocalizationDir = "res://data/localization";

    public static event Action? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = LangEnglish;

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
