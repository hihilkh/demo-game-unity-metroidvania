using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public partial class LangConfig {
    /// <summary>
    /// Device default language is the language to use after app installed
    /// </summary>
    public static LangType GetDeviceDefaultLang () {
        // Just sample. Please rewrite it to fit your project
        switch (Application.systemLanguage) {
            case SystemLanguage.ChineseTraditional:
                return LangType.TraditionalChinese;
            default:
                return LangType.English;
        }
    }

    /// <summary>
    /// Root language is the rollback language to use for any exceptional cases
    /// </summary>
    public static LangType GetRootLang () {
        return LangType.English;
    }

    /// <summary>
    /// Selectable Lang Type List is the LangType list user can select in Settings page
    /// </summary>
    public static List<LangType> GetSelectableLangTypeList () {
        return new List<LangType> {
                LangType.TraditionalChinese,
                LangType.English
            };
    }

    /// <summary>
    /// Get the lang code (string) of corresponding LangType
    /// </summary>
    public static string GetLangCode (LangType langType) {
        switch (langType) {
            case LangType.TraditionalChinese: return "zh-hk";
            case LangType.English: return "en-us";
        }

        Log.PrintError ("LangType : " + langType + " has not been assigned lang code. Return null.");
        return null;
    }

    /// <summary>
    /// Get the localization file name (without extension) of corresponding LangType
    /// </summary>
    public static string GetLocalizationFileName (LangType langType) {
        var langCode = GetLangCode (langType);
        if (string.IsNullOrEmpty(langCode)) {
            Log.PrintError ("GetLocalizationFileName failed. LangType : " + langType);
            return null;
        } else {
            return langCode + "." + FrameworkVariable.LocalizationFileExtension;
        }
    }
}
