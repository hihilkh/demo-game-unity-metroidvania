using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using HIHIFramework.Asset;
using TMPro;

namespace HIHIFramework.Lang {
    public abstract class LangManagerBase {

        protected static bool IsInitialized = false;
        protected static event Action LangChangedEvent;

        private static LangType? _CurrentLang = null;
        protected static LangType CurrentLang {
            get {
                if (_CurrentLang == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentLangTypeKey)) {
                        _CurrentLang = (LangType)PlayerPrefs.GetInt (FrameworkVariable.CurrentLangTypeKey);
                    } else {
                        _CurrentLang = LangConfig.GetDeviceDefaultLang ();
                    }
                }

                return (LangType)_CurrentLang;
            }
            set {
                if (_CurrentLang == value) {
                    // No changes on language. Do not do anything
                    return;
                }

                PlayerPrefs.SetInt (FrameworkVariable.CurrentLangTypeKey, (int)value);
                _CurrentLang = value;

                var isSuccess = LoadLocalizationFileIfMissing (value);
                if (isSuccess) {
                    LangChangedEvent?.Invoke ();
                }
            }
        }

        protected static Dictionary<LangType, Dictionary<string, string>> LangKeyValueMappingDict = new Dictionary<LangType, Dictionary<string, string>> ();

        #region Initialization

        public static void Init (Action<bool> onFinished = null) {
            Log.Print ("Start init LangManager");
            Action<bool> onInitStreamingAssetsFinished = (isSuccess) => {
                if (isSuccess) {
                    var isLoadSuccess = LoadLocalizationFileIfMissing (CurrentLang);

                    if (isLoadSuccess) {
                        Log.Print ("Successfully init LangManager");
                        IsInitialized = true;
                        onFinished?.Invoke (true);
                    } else {
                        onFinished?.Invoke (false);
                    }
                } else {
                    onFinished?.Invoke (false);
                }
            };
            InitStreamingAssets (onInitStreamingAssetsFinished);
        }

        private static void InitStreamingAssets (Action<bool> onFinished = null) {
            Log.Print ("Start init language streaming assets.");
            var fileNameList = new List<string> ();

            foreach (LangType langType in Enum.GetValues (typeof (LangType))) {
                var fileName = LangConfig.GetLocalizationFileName (langType);
                if (!string.IsNullOrEmpty (fileName)) {
                    fileNameList.Add (fileName);
                }
            }

            Action<bool> onCopyFinished = (isSuccess) => {
                if (isSuccess) {
                    Log.Print ("Successfully init language streaming assets.");
                }

                onFinished?.Invoke (isSuccess);
            };

            CopySreamingAssetsFilesRecursive (fileNameList, onCopyFinished);
        }

        private static void CopySreamingAssetsFilesRecursive (List<string> fileNameList, Action<bool> onFinished = null) {
            if (fileNameList.Count <= 0) {
                onFinished?.Invoke (true);
                return;
            }

            Action<bool> onCopyOneFileFinished = (isSuccess) => {
                if (isSuccess) {
                    fileNameList.RemoveAt (0);
                    CopySreamingAssetsFilesRecursive (fileNameList, onFinished);
                } else {
                    Log.PrintError ("Update streaming assets failed. AssetType : " + AssetEnum.AssetType.Localization + " , fileName : " + fileNameList[0]);
                    onFinished?.Invoke (false);
                }
            };

            AssetHandler.Instance.CheckAndUpdateStreamingAssets (AssetEnum.AssetType.Localization, fileNameList[0], false, onCopyOneFileFinished);
        }

        #endregion

        #region Localization File

        private static bool LoadLocalizationFileIfMissing (LangType langType) {
            if (LangKeyValueMappingDict.ContainsKey (langType)) {
                return true;
            }

            Log.Print ("Start loading localization file. LangType : " + langType);

            var fileName = LangConfig.GetLocalizationFileName (langType);
            if (string.IsNullOrEmpty (fileName)) {
                Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Localization file do not exist.");
                return false;
            }

            (var lines, var errorMsg) = AssetHandler.Instance.ReadPersistentDataFileByLines (AssetEnum.AssetType.Localization, fileName);
            if (errorMsg != null) {
                Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : " + errorMsg);
                return false;
            }

            if (lines == null || lines.Length <= 0) {
                Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Localization file is empty");
                return false;
            }

            var keyValueMapping = new Dictionary<string, string> ();
            for (var i = 0; i < lines.Length; i++) {
                var pair = lines[i].Split (new string[] { FrameworkVariable.LocalizationFileDelimiter }, StringSplitOptions.None);
                if (pair == null || pair.Length != 2) {
                    Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Wrong format in line " + (i + 1) + " of the localization file.");
                    return false;
                }

                if (keyValueMapping.ContainsKey (pair[0])) {
                    Log.PrintError ("Load localization file Failed. langType : " + langType + " , Error : Duplicated localization key : " + pair[0]);
                    return false;
                }

                keyValueMapping.Add (pair[0], pair[1]);
            }

            LangKeyValueMappingDict.Add (langType, keyValueMapping);

            Log.Print ("Successfully load localization file. LangType : " + langType);

            return true;
        }

        public static void ReloadLocalizationFile () {
            // Clear the cache of all LangType, but not only CurrentLang
            LangKeyValueMappingDict.Clear ();

            // Reload only CurrentLang. Other lang's file would be loaded on demand
            var isSuccess = LoadLocalizationFileIfMissing (CurrentLang);

            // Force trigger LangChangedEvent
            if (isSuccess) {
                LangChangedEvent?.Invoke ();
            }
        }

        #endregion

        #region Event

        public static void AddLangChangedEventHandler (Action handler) {
            LangChangedEvent += handler;
        }

        public static void RemoveLangChangedEventHandler (Action handler) {
            LangChangedEvent -= handler;
        }

        #endregion

        #region CurrentLang

        public static LangType GetCurrentLang () {
            return CurrentLang;
        }

        public static void ChangeLang (LangType langType) {
            CurrentLang = langType;
        }

        #endregion

        #region Font

        // TODO
        public static TMP_FontAsset GetCurrentFont () {
            return new TMP_FontAsset ();
        }

        // TODO
        public static TMP_FontAsset GetFont (LangType langType) {
            return new TMP_FontAsset ();
        }
        #endregion

        #region GetWord / SetWord method

        /// <summary>
        /// Get localization of input localization key in <b>CurrentLang</b> 
        /// </summary>
        public static string GetWord (string key, bool isFallbackToRootLang = true) {
            return GetWord (CurrentLang, key, isFallbackToRootLang);
        }

        /// <summary>
        /// Get localization of input localization key in input LangType
        /// </summary>
        public static string GetWord (LangType langType, string key, bool isFallbackToRootLang = true) {
            Func<string> failedAction = () => {
                if (isFallbackToRootLang) {
                    var rootLang = LangConfig.GetRootLang ();
                    if (langType != rootLang) {
                        Log.Print ("Try to get word with root lang.");
                        return GetWord (rootLang, key, false);
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
                Log.PrintError ("Cannot get word. LangKeyValueMapping of LangType : " + langType + " is missing.");
                return failedAction ();
            }

            var mapping = LangKeyValueMappingDict[langType];
            if (!mapping.ContainsKey (key)) {
                Log.PrintError ("Cannot get word. The key value mapping is missing. key : " + key + " , LangType : " + langType);
                return failedAction ();
            }

            return mapping[key];
        }

        /// <summary>
        /// Set the Text components with corresponding BasicLocalizedTextDetails List
        /// </summary>
        //// Remarks : Use IEnumerable because it is covariance
        public static void SetWords (IEnumerable<BasicLocalizedTextDetails> detailsList, bool isFallbackToRootLang = true) {
            if (detailsList == null) {
                Log.PrintWarning ("The input detailsList is null. Cannot set words.");
                return;
            }

            // TODO
            //var font = GetCurrentFont ();
            foreach (var details in detailsList) {
                //details.text.font = font;
                details.text.text = GetWord (details.localizationKey, isFallbackToRootLang);
            }
        }

        #endregion
    }
}
