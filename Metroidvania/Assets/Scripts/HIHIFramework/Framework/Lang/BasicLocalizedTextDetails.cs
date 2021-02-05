using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HIHIFramework.Lang {
    public class BasicLocalizedTextDetails {
        public TextMeshProUGUI text { get; protected set; }
        public string localizationKey { get; protected set; }
        public bool isNeedLocalization { get; protected set; }

        /// <summary>
        /// It use FrameworkUtils.StringReplace() to replace a specific format of string,
        /// e.g., replace "{0}" from "Hello {0}"<br />
        /// string : replace string or coresponding localization key<br />
        /// bool : isNeedLocalization
        /// </summary>
        public Dictionary<string, bool> replaceStringDict { get; protected set; }

        public BasicLocalizedTextDetails (TextMeshProUGUI text, string localizationKey, bool isNeedLocalization = true) {
            this.text = text;
            this.localizationKey = localizationKey;
            this.isNeedLocalization = isNeedLocalization;
            this.replaceStringDict = null;
        }

        public void AddReplaceString (string replaceString, bool isNeedLocalization) {
            if (replaceStringDict == null) {
                replaceStringDict = new Dictionary<string, bool> ();
            }

            replaceStringDict.Add (replaceString, isNeedLocalization);
        }
    }
}


