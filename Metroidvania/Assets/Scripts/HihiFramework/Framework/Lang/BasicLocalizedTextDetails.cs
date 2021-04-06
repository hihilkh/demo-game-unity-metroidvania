using System.Collections.Generic;
using TMPro;

namespace HihiFramework.Lang {
    public class BasicLocalizedTextDetails {
        public TextMeshProUGUI Text { get; protected set; }
        public string LocalizationKey { get; protected set; }
        public bool IsNeedLocalization { get; protected set; }

        /// <summary>
        /// It use FrameworkUtils.StringReplace() to replace a specific format of string,
        /// e.g., replace "{0}" from "Hello {0}"<br />
        /// string : replace string or coresponding localization key<br />
        /// bool : isNeedLocalization
        /// </summary>
        public Dictionary<string, bool> ReplaceStringDict { get; protected set; }

        public BasicLocalizedTextDetails (TextMeshProUGUI text, string localizationKey, bool isNeedLocalization = true) {
            Text = text;
            ChangeLocalizationKey (localizationKey, isNeedLocalization, true);
        }

        public void AddReplaceString (string replaceString, bool isNeedLocalization) {
            if (ReplaceStringDict == null) {
                ReplaceStringDict = new Dictionary<string, bool> ();
            }

            ReplaceStringDict.Add (replaceString, isNeedLocalization);
        }

        public void ChangeLocalizationKey (string localizationKey, bool isNeedLocalization = true, bool isClearReplaceStringDict = true) {
            LocalizationKey = localizationKey;
            IsNeedLocalization = isNeedLocalization;
            if (isClearReplaceStringDict) {
                ReplaceStringDict = null;
            }
        }
    }
}


