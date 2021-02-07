using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using TMPro;

public class MainMenuSceneUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI selectAreaText;

    private List<LocalizedTextDetails> localizedTextDetailsList;

    private void Start () {
        localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (selectAreaText, "SelectArea"));
        SetTexts ();

        LangManager.LangChangedEvent += SetTexts;
    }

    private void OnDestroy () {
        LangManager.LangChangedEvent -= SetTexts;
    }

    private void SetTexts () {
        LangManager.SetTexts (localizedTextDetailsList);
    }
}
