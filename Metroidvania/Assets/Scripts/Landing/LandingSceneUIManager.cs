using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HIHIFramework.Core;
using System;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI startText;

    private void Start () {
        SetTexts ();
    }
    
    private void SetTexts () {
        var detailsList = new List<LocalizedTextDetails> ();
        detailsList.Add (new LocalizedTextDetails (startText, "PressToStart"));

        LangManager.SetWords (detailsList);
    }

}