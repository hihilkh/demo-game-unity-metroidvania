using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuSceneUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI selectAreaText;

    private List<LocalizedTextDetails> localizedTextDetailsList;

    private void Start () {
        localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (selectAreaText, "SelectArea"));
        SetTexts ();

        LangManager.LangChanged += LangChangedHandler;
    }

    private void OnDestroy () {
        LangManager.LangChanged -= LangChangedHandler;
    }

    private void SetTexts () {
        LangManager.SetTexts (localizedTextDetailsList);
    }

    #region Events

    private void LangChangedHandler () {
        SetTexts ();
    }

    #endregion
}
