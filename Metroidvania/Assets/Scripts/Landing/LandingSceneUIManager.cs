using TMPro;
using UnityEngine;
using HihiFramework.UI;
using HihiFramework.Core;
using System.Collections.Generic;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private GameObject baseUI;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private TextMeshProUGUI versionText;

    private List<LocalizedTextDetails> localizedTextDetailsList;

    private void Start () {
        localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (startText, "PressToStart"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (versionText, FrameworkUtils.GetVersionNoStr (), false));
        SetTexts ();

        LangManager.LangChanged += LangChangedHandler;
    }

    private void OnDestroy () {
        LangManager.LangChanged -= LangChangedHandler;
    }

    private void SetTexts () {
        LangManager.SetTexts (localizedTextDetailsList);
    }

    public void HideUI () {
        baseUI.SetActive (false);
    }

    #region Events

    private void LangChangedHandler () {
        SetTexts ();
    }

    #endregion
}