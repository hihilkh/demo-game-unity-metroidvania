using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HIHIFramework.Lang {
    public class BasicLocalizedTextDetails {
        public TextMeshProUGUI text { get; protected set; }
        public string localizationKey { get; protected set; }
        public bool isNeedLocalization { get; protected set; }

        public BasicLocalizedTextDetails (TextMeshProUGUI text, string localizationKey, bool isNeedLocalization = true) {
            this.text = text;
            this.localizationKey = localizationKey;
            this.isNeedLocalization = isNeedLocalization;
        }
    }
}


