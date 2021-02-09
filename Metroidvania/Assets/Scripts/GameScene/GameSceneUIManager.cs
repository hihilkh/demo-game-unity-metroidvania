using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using TMPro;
using UnityEngine;

public class GameSceneUIManager : MonoBehaviour {

    [Serializable]
    private class PanelControl {
        public GameObject panelBase;
        public GameObject clickIconObject;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI contentText;
        public Animator animator;
    }

    [SerializeField] private GameObject clickOnScreenBtnObject;
    [SerializeField] private PanelControl collectedPanelControl;
    [SerializeField] private PanelControl notePanelControl;

    private const float WaitPeriodBeforeAllowPanelClick = 1f;

    private bool isAllowPanelClick = false;
    private PanelControl currentShowingPanel = null;
    private Action currentShowPanelFinishedAction = null;

    private void Awake () {
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ClickOnScreen, OnClickOnScreenBtnClick);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Game_ClickOnScreen, OnClickOnScreenBtnClick);
    }

    #region Panel Control

    public void ShowCollectedPanel (Collectable collectable, Action onFinished = null) {
        var detail = new LocalizedTextDetails (collectedPanelControl.contentText, "YouGot");
        detail.AddReplaceString (collectable.displayNameKey, true);

        ShowPanel (collectedPanelControl, new List<LocalizedTextDetails> { detail }, onFinished);
    }

    public void ShowNotePanel (NoteCollectable noteCollectable, Action onFinished = null) {
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.titleText, noteCollectable.noteTitleKey));
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.contentText, noteCollectable.noteContentKey));

        ShowPanel (notePanelControl, localizedTextDetailsList, onFinished);
    }

    private void ShowPanel (PanelControl panelControl, List<LocalizedTextDetails> localizedTextDetailsList, Action onFinished = null) {
        currentShowingPanel = panelControl;
        currentShowPanelFinishedAction = onFinished;

        LangManager.SetTexts (localizedTextDetailsList);

        StartCoroutine (SetPanelClick (panelControl, WaitPeriodBeforeAllowPanelClick));

        panelControl.panelBase.SetActive (true);
        clickOnScreenBtnObject.SetActive (true);

    }

    private void HidePanel (PanelControl panelControl) {
        SetAllowPanelClick (panelControl, false);

        Action onFinished = () => {
            panelControl.panelBase.SetActive (false);
            clickOnScreenBtnObject.SetActive (false);

            currentShowingPanel = null;

            // Remarks : tempAction is to prevent the case that currentShowPanelFinishedAction set to some other action while invoke and then immediately set to null 
            var tempAction = currentShowPanelFinishedAction;
            currentShowPanelFinishedAction = null;
            tempAction?.Invoke ();
        };

        FrameworkUtils.Instance.StartSingleAnim (panelControl.animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    private IEnumerator SetPanelClick (PanelControl panelControl, float waitPeriodBeforeAllowClick) {
        SetAllowPanelClick (panelControl, false);

        yield return new WaitForSecondsRealtime (waitPeriodBeforeAllowClick);

        SetAllowPanelClick (panelControl, true);
    }

    private void SetAllowPanelClick (PanelControl panelControl, bool isAllow) {
        panelControl.clickIconObject.SetActive (isAllow);
        isAllowPanelClick = isAllow;
    }

    #endregion

    #region Event Handler

    private void OnClickOnScreenBtnClick () {
        if (!isAllowPanelClick) {
            return;
        }

        HidePanel (currentShowingPanel);
    }

    #endregion
}
