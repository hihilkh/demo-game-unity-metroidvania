using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI startText;

    private void Start () {
        SetTexts ();
    }
    
    private void SetTexts () {
        Dictionary<TextMeshProUGUI, string> textLocalizationKeyDict = new Dictionary<TextMeshProUGUI, string> {
            { startText, "PressToStart" }
        };

        LangManager.SetWords (textLocalizationKeyDict);
    }
}