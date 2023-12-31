﻿using System;
using System.Collections.Generic;
using System.IO;
using HihiFramework.Asset;
using HihiFramework.Core;
using TMPro;
using UnityEngine;

namespace HihiFramework.Lang {
    public abstract class LangManagerBase {

        protected static bool IsInitialized { get; private set; } = false;
        public static event Action LangChanged;

        private static LangType? _CurrentLang = null;
        protected static LangType CurrentLang {
            get {
                if (_CurrentLang == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentLangTypeKey)) {
                        _CurrentLang = (LangType)PlayerPrefs.GetInt (FrameworkVariable.CurrentLangTypeKey);
                    } else {
                        _CurrentLang = LangConfig.GetDeviceDefaultLang ();
                        // Set player prefs to save the device default lang
                        PlayerPrefs.SetInt (FrameworkVariable.CurrentLangTypeKey, (int)_CurrentLang);
                    }
                }

                return (LangType)_CurrentLang;
            }
            private set {
                PlayerPrefs.SetInt (FrameworkVariable.CurrentLangTypeKey, (int)value);
                _CurrentLang = value;
            }
        }

        protected static Dictionary<LangType, Dictionary<string, string>> LangKeyValueMappingDict { get; } = new Dictionary<LangType, Dictionary<string, string>> ();
        protected static Dictionary<LangType, TMP_FontAsset> FontDict { get; } = new Dictionary<LangType, TMP_FontAsset> ();
        protected static Dictionary<LangType, string> LangNameDict { get; } = new Dictionary<LangType, string> ();

        #region Initialization

        public static void Init (Action<bool> onFinished = null) {
            Log.Print ("Start init LangManager", LogTypes.Lang);
            Action<bool> onInitStreamingAssetsFinished = (isSuccess) => {
                if (isSuccess) {
                    var isLoadSuccess = LoadLocalizationFileIfMissing (CurrentLang);
                    if (isLoadSuccess) {
                        isLoadSuccess = ReloadAllLangNames ();
                    }

                    if (isLoadSuccess) {
                        Log.Print ("Successfully init LangManager", LogTypes.Lang);
                        IsInitialized = true;
                        onFinished?.Invoke (true);
                    } else {
                        Log.PrintError ("LangManager init failed. Please check.", LogTypes.Lang);
                        onFinished?.Invoke (false);
                    }
                } else {
                    Log.PrintError ("LangManager init failed. Please check.", LogTypes.Lang);
                    onFinished?.Invoke (false);
                }
            };
            InitStreamingAssets (onInitStreamingAssetsFinished);
        }

        private static void InitStreamingAssets (Action<bool> onFinished = null) {
            Log.Print ("Start init language streaming assets.", LogTypes.Lang | LogTypes.Asset);

            Action<bool> onCopyFinished = (isSuccess) => {
                if (isSuccess) {
                    Log.Print ("Successfully init language streaming assets.", LogTypes.Lang | LogTypes.Asset);
                }

                onFinished?.Invoke (isSuccess);
            };

            AssetHandler.Instance.CheckAndUpdateStreamingAssets (AssetEnum.AssetType.Localization, onCopyFinished);
        }

        #endregion

        #region Localization File

        /// <summary>
        /// If the contents is null or empty, view as failed. If failed, <paramref name="lines"/> will be assigned to null.<br />
        /// So if success, <paramref name="lines"/> must not be null and lines.Length must be more than 0
        /// </summary>
        /// <returns>Is success</returns>
        private static bool TryReadLocalizationFileContents (LangType langType, out string[] lines) {
            lines = null;

            var fileName = LangConfig.GetLocalizationFileName (langType);
            if (string.IsNullOrEmpty (fileName)) {
                Log.PrintError ("Read localization file content Failed. langType : " + langType + " , Error : Localization file do not exist.", LogTypes.Lang | LogTypes.Asset);
                return false;
            }

            var isSuccess = AssetHandler.Instance.TryReadPersistentDataFileByLines (AssetEnum.AssetType.Localization, fileName, out lines);
            if (!isSuccess) {
                Log.PrintError ("Read localization file content Failed. langType : " + langType, LogTypes.Lang | LogTypes.Asset);
                return false;
            }

            if (lines == null || lines.Length <= 0) {
                Log.PrintError ("Read localization file content Failed. langType : " + langType + " , Error : Localization file is empty", LogTypes.Lang | LogTypes.Asset);
                return false;
            }

            return true;
        }

        private static bool ReloadAllLangNames () {
            Log.Print ("Start reloading all lang name.", LogTypes.Lang | LogTypes.Asset);

            LangNameDict.Clear ();

            var langTypeList = LangConfig.GetSelectableLangTypeList ();

            if (langTypeList == null || langTypeList.Count <= 0) {
                Log.PrintWarning ("SelectableLangTypeList is null or empty.", LogTypes.Lang | LogTypes.Asset);
                // Do not return false because maybe some games really do not have language
            } else {
                foreach (var langType in langTypeList) {
                    string[] lines;
                    var isSuccess = TryReadLocalizationFileContents (langType, out lines);

                    if (!isSuccess) {
                        Log.PrintError ("Reload lang name Failed. langType : " + langType + " , Error : Cannot read localization file content", LogTypes.Lang | LogTypes.Asset);
                        return false;
                    }

                    for (var i = 0; i < lines.Length; i++) {
                        var pair = lines[i].Split (new string[] { FrameworkVariable.LocalizationFileDelimiter }, StringSplitOptions.None);
                        if (pair == null || pair.Length != 2) {
                            // Line with wrong format. Ignore it.
                            continue;
                        }

                        if (pair[0] == FrameworkVariable.LangNameLocalizationKey) {
                            LangNameDict.Add (langType, pair[1]);
                            break;
                        }
                    }
                }
            }

            Log.Print ("Successfully reloading all lang name.", LogTypes.Lang | LogTypes.Asset);
            return true;
        }

        private static bool LoadLocalizationFileIfMissing (LangType langType) {
            if (LangKeyValueMappingDict.ContainsKey (langType)) {
                return true;
            }

            Log.Print ("Start loading localization file. LangType : " + langType, LogTypes.Lang | LogTypes.Asset);

            string[] lines;
            var isSuccess = TryReadLocalizationFileContents (langType, out lines);

            if (!isSuccess) {
                Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Cannot read localization file content", LogTypes.Lang | LogTypes.Asset);
                return false;
            }

            var keyValueMapping = new Dictionary<string, string> ();
            for (var i = 0; i < lines.Length; i++) {
                var pair = lines[i].Split (new string[] { FrameworkVariable.LocalizationFileDelimiter }, StringSplitOptions.None);
                if (pair == null || pair.Length != 2) {
                    // Line with wrong format. Ignore it.
                    continue;
                }

                if (keyValueMapping.ContainsKey (pair[0])) {
                    Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Duplicated localization key : " + pair[0], LogTypes.Lang | LogTypes.Asset);
                    return false;
                }

                keyValueMapping.Add (pair[0], pair[1]);
            }

            LangKeyValueMappingDict.Add (langType, keyValueMapping);

            Log.Print ("Successfully load localization file. LangType : " + langType, LogTypes.Lang | LogTypes.Asset);

            return true;
        }

        public static void ReloadLocalizationFiles () {
            // Clear the cache of all LangType, but not only CurrentLang
            LangKeyValueMappingDict.Clear ();

            // Reload only CurrentLang. Other lang's file would be loaded on demand
            var isSuccess = LoadLocalizationFileIfMissing (CurrentLang);

            if (isSuccess) {
                isSuccess = ReloadAllLangNames ();
            }

            // Force trigger LangChangedEvent
            if (isSuccess) {
                LangChanged?.Invoke ();
            }
        }

        #endregion

        #region CurrentLang

        public static LangType GetCurrentLang () {
            return CurrentLang;
        }

        public static void ChangeLang (LangType langType) {
            if (CurrentLang == langType) {
                // No changes on language. Do not do anything
                return;
            }

            CurrentLang = langType;

            var isSuccess = LoadLocalizationFileIfMissing (langType);
            if (isSuccess) {
                LangChanged?.Invoke ();
            }
        }

        #endregion

        #region Font

        public static TMP_FontAsset GetCurrentFont () {
            return GetFont (CurrentLang);
        }

        public static TMP_FontAsset GetFont (LangType langType) {
            if (!FontDict.ContainsKey (langType)) {
                var resourcesName = LangConfig.GetFontResourcesName (langType);
                var font = Resources.Load<TMP_FontAsset> (Path.Combine (FrameworkVariable.FontResourcesFolder, resourcesName));
                if (font == null) {
                    Log.PrintError ("Get font failed. LangType : " + langType, LogTypes.Lang | LogTypes.Asset);
                } else {
                    FontDict[langType] = font;
                }
            }

            return FontDict[langType];
        }

        #endregion

        #region GetLocalizedStr / SetTexts method

        /// <summary>
        /// Get localization of input localization key in <b>CurrentLang</b> 
        /// </summary>
        public static string GetLocalizedStr (string key, bool isFallbackToRootLang = true, bool isShowWarningIfNull = true) {
            return GetLocalizedStr (CurrentLang, key, isFallbackToRootLang, isShowWarningIfNull);
        }

        /// <summary>
        /// Get localization of input localization key in input LangType
        /// </summary>
        public static string GetLocalizedStr (LangType langType, string key, bool isFallbackToRootLang = true, bool isShowErrorIfNull = true) {
            Func<string> failedAction = () => {
                if (isFallbackToRootLang) {
                    var rootLang = LangConfig.GetRootLang ();
                    if (langType != rootLang) {
                        Log.Print ("Try to get word with root lang.", LogTypes.Lang);
                        return GetLocalizedStr (rootLang, key, false);
                    }
                }

                return "";
            };

            // For the case that do not init the LangManager properly (From GameConfig scene)
            // In this case, I assume the streaming assets has been copied to persistent data path. So only load the LocalizationFile
            if (!IsInitialized) {
                LoadLocalizationFileIfMissing (langType);
                IsInitialized = true;
            }

            // TODO : Think of the bug that if inputting a langType that not yet loaded LocalizationFile, it will fail
            if (!LangKeyValueMappingDict.ContainsKey (langType)) {
                if (isShowErrorIfNull) {
                    Log.PrintError ("Cannot get word. LangKeyValueMapping of LangType : " + langType + " is missing.", LogTypes.Lang);
                }

                return failedAction ();
            }

            var mapping = LangKeyValueMappingDict[langType];
            if (!mapping.ContainsKey (key)) {
                if (isShowErrorIfNull) {
                    Log.PrintError ("Cannot get word. The key value mapping is missing. key : " + key + " , LangType : " + langType, LogTypes.Lang);
                }

                return failedAction ();
            }

            return mapping[key];
        }

        private static void SetText (TextMeshProUGUI text, TMP_FontAsset font, string str) {
            text.font = font;
            text.text = str;
        }

        private static void SetText (BasicLocalizedTextDetails details, TMP_FontAsset font, bool isFallbackToRootLang = true) {
            var str = details.IsNeedLocalization ? GetLocalizedStr (details.LocalizationKey, isFallbackToRootLang) : details.LocalizationKey;
            if (details.ReplaceStringDict != null && details.ReplaceStringDict.Count > 0) {
                List<string> replaceStringList = new List<string> ();
                foreach (var pair in details.ReplaceStringDict) {
                    if (pair.Value) {
                        replaceStringList.Add (GetLocalizedStr (pair.Key, isFallbackToRootLang));
                    } else {
                        replaceStringList.Add (pair.Key);
                    }
                }

                str = FrameworkUtils.StringReplace (str, replaceStringList.ToArray ());
            }

            SetText (details.Text, font, str);
        }

        /// <summary>
        /// Set the Text component with corresponding BasicLocalizedTextDetails List
        /// </summary>
        public static void SetText (BasicLocalizedTextDetails details, bool isFallbackToRootLang = true) {
            var font = GetCurrentFont ();

            SetText (details, font, isFallbackToRootLang);
        }

        /// <summary>
        /// Set the Text components with corresponding BasicLocalizedTextDetails List
        /// </summary>
        //// Remarks : Use IEnumerable because it is covariance
        public static void SetTexts (IEnumerable<BasicLocalizedTextDetails> detailsList, bool isFallbackToRootLang = true) {
            if (detailsList == null) {
                Log.PrintWarning ("The input detailsList is null. Cannot set words.", LogTypes.Lang);
                return;
            }

            var font = GetCurrentFont ();
            foreach (var details in detailsList) {
                SetText (details, font, isFallbackToRootLang);
            }
        }

        public static void SetLangNameText (TextMeshProUGUI text, LangType langType) {
            if (!LangNameDict.ContainsKey (langType)) {
                Log.PrintError ("LangNameDict do not contain lang name of langType : " + langType + ". Please check. (Maybe the langType is not a selectable lang type?)", LogTypes.Lang);
                return;
            }

            var langName = LangNameDict[langType];
            var font = GetFont (langType);

            SetText (text, font, langName);
        }

        #endregion

        #region LocalizationKeyBase

        public static List<string> GetLocalizedStrList (string localizationKeyBase) {
            var result = new List<string> ();

            var counter = 0;
            var localizationKey = localizationKeyBase + "_" + counter.ToString ();
            // Remarks : Do not fallback to root language because different language may have different length of string list
            var localizedStr = GetLocalizedStr (localizationKey, false, false);

            while (!string.IsNullOrEmpty (localizedStr)) {
                result.Add (localizedStr);
                counter++;
                localizationKey = localizationKeyBase + "_" + counter.ToString ();
                localizedStr = GetLocalizedStr (localizationKey, false, false);
            }

            return result;
        }

        #endregion

    }
}
