using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using TMPro;
using UnityEngine;

public class GameSceneUIManager : MonoBehaviour {

    [Serializable]
    private class PanelControl {
        public GameObject PanelBase;
        public GameObject ClickIconObject;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI ContentText;
        public Animator Animator;
    }

    [SerializeField] private HIHIButton pauseBtn;
    [SerializeField] private GameObject clickOnScreenBtnObject;
    [SerializeField] private PanelControl collectedPanelControl;
    [SerializeField] private PanelControl notePanelControl;

    private const float WaitPeriodBeforeAllowPanelClick = 1f;

    // General Panel
    private bool isAllowPanelClick = false;
    private PanelControl currentShowingPanel = null;
    private Action currentShowPanelFinishedAction = null;

    private void Awake () {
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ClickOnScreen, ClickOnScreenBtnClickedHandler);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Game_ClickOnScreen, ClickOnScreenBtnClickedHandler);
    }

    public void ResetGame () {
        pauseBtn.SetInteractable (false);
    }

    public void StartGame () {
        pauseBtn.SetInteractable (true);
    }

    #region General Panel Control

    public void ShowCollectedPanel (Collectable collectable, Action onFinished = null) {
        var detail = new LocalizedTextDetails (collectedPanelControl.ContentText, "YouGot");
        detail.AddReplaceString (collectable.DisplayNameKey, true);

        ShowPanel (collectedPanelControl, new List<LocalizedTextDetails> { detail }, onFinished);
    }

    public void ShowNotePanel (NoteCollectable noteCollectable, Action onFinished = null) {
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.TitleText, noteCollectable.DisplayNameKey));
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.ContentText, noteCollectable.NoteContentKey));

        ShowPanel (notePanelControl, localizedTextDetailsList, onFinished);
    }

    private void ShowPanel (PanelControl panelControl, List<LocalizedTextDetails> localizedTextDetailsList, Action onFinished = null) {
        currentShowingPanel = panelControl;
        currentShowPanelFinishedAction = onFinished;

        LangManager.SetTexts (localizedTextDetailsList);

        StartCoroutine (SetPanelClick (panelControl, WaitPeriodBeforeAllowPanelClick));

        panelControl.PanelBase.SetActive (true);
        clickOnScreenBtnObject.SetActive (true);

    }

    private void HidePanel (PanelControl panelControl) {
        SetAllowPanelClick (panelControl, false);

        Action onFinished = () => {
            panelControl.PanelBase.SetActive (false);
            clickOnScreenBtnObject.SetActive (false);

            currentShowingPanel = null;

            // Remarks : tempAction is to prevent the case that currentShowPanelFinishedAction set to some other action while invoke and then immediately set to null 
            var tempAction = currentShowPanelFinishedAction;
            currentShowPanelFinishedAction = null;
            tempAction?.Invoke ();
        };

        FrameworkUtils.Instance.StartSingleAnim (panelControl.Animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    private IEnumerator SetPanelClick (PanelControl panelControl, float waitPeriodBeforeAllowClick) {
        SetAllowPanelClick (panelControl, false);

        yield return new WaitForSecondsRealtime (waitPeriodBeforeAllowClick);

        SetAllowPanelClick (panelControl, true);
    }

    private void SetAllowPanelClick (PanelControl panelControl, bool isAllow) {
        panelControl.ClickIconObject.SetActive (isAllow);
        isAllowPanelClick = isAllow;
    }

    #endregion

    #region Event Handler

    private void ClickOnScreenBtnClickedHandler (HIHIButton sender) {
        if (!isAllowPanelClick) {
            return;
        }

        HidePanel (currentShowingPanel);
    }

    #endregion
}
