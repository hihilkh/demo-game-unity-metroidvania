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
    [SerializeField] private PanelControl dialogPanelControl;

    [Header("Dialog Panel")]
    [SerializeField] private Transform dialogLeftSideBaseTransform;
    [SerializeField] private Transform dialogRightSideBaseTransform;
    [SerializeField] private List<DialogCharacterBase> dialogCharacterTemplateList;

    private const float WaitPeriodBeforeAllowPanelClick = 1f;
    private readonly List<DialogCharacterBase> dialogCharacterCacheList = new List<DialogCharacterBase> ();

    // General Panel
    private bool isAllowPanelClick = false;
    private bool hasShownAllTexts = false;
    private PanelControl currentShowingPanel = null;
    private Action currentShowPanelFinishedAction = null;

    private void Awake () {
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ClickOnScreen, ClickOnScreenBtnClickedHandler);
        MissionEventManager.MissionEventStarted += MissionEventStartedHandler;
        MissionEventManager.MissionEventFinished += MissionEventFinishedHandler;
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Game_ClickOnScreen, ClickOnScreenBtnClickedHandler);
        MissionEventManager.MissionEventStarted -= MissionEventStartedHandler;
        MissionEventManager.MissionEventFinished -= MissionEventFinishedHandler;
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

        ShowPanel (collectedPanelControl, new List<LocalizedTextDetails> { detail }, true, onFinished);
    }

    public void ShowNotePanel (NoteCollectable noteCollectable, Action onFinished = null) {
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.TitleText, noteCollectable.DisplayNameKey));
        localizedTextDetailsList.Add (new LocalizedTextDetails (notePanelControl.ContentText, noteCollectable.NoteContentKey));

        ShowPanel (notePanelControl, localizedTextDetailsList, true, onFinished);
    }

    public void ShowInstructionPanel (string localizationKeyBase, Action onFinished = null) {
        PrepareInstructionPanel ();
        var localizedStrList = LangManager.GetLocalizedStrList (localizationKeyBase);

        ShowContentRecursive (dialogPanelControl, localizedStrList, true, onFinished);
    }

    public void ShowDialogPanel (DialogSubEvent dialogSubEvent, Action onFinished = null) {
        ShowContentRecursive (dialogPanelControl, dialogSubEvent.GetDialogDetailsListClone (), onFinished);
    }

    private void ShowContentRecursive (PanelControl panelControl, List<DialogSubEvent.DialogDetails> dialogDetailsList, Action onAllFinished = null) {
        if (dialogDetailsList != null && dialogDetailsList.Count > 0) {
            var dialogDetails = dialogDetailsList[0];
            dialogDetailsList.RemoveAt (0);

            PrepareDialogPanel (dialogDetails);
            var localizedStrList = LangManager.GetLocalizedStrList (dialogDetails.DialogLocalizationKeyBase);

            var isContainLastText = dialogDetailsList.Count == 0;

            Action onShowOneFinished = () => {
                ShowContentRecursive (panelControl, dialogDetailsList, onAllFinished);
            };

            ShowContentRecursive (panelControl, localizedStrList, isContainLastText, onShowOneFinished);

        } else {
            onAllFinished?.Invoke ();
        }
    }

    private void ShowContentRecursive (PanelControl panelControl, List<string> localizedStrList, bool isContainLastText, Action onAllFinished = null) {
        if (localizedStrList != null && localizedStrList.Count > 0) {
            var localizedTextDetailsList = new List<LocalizedTextDetails> ();
            localizedTextDetailsList.Add (new LocalizedTextDetails (panelControl.ContentText, localizedStrList[0], false));

            localizedStrList.RemoveAt (0);

            var isLastText = isContainLastText ? localizedStrList.Count == 0 : false;

            Action onShowOneFinished = () => {
                ShowContentRecursive (panelControl, localizedStrList, isContainLastText, onAllFinished);
            };

            ShowPanel (panelControl, localizedTextDetailsList, isLastText, onShowOneFinished);
        } else {
            onAllFinished?.Invoke ();
        }
    }

    private void ShowPanel (PanelControl panelControl, List<LocalizedTextDetails> localizedTextDetailsList, bool isLastText, Action onFinished = null) {
        currentShowingPanel = panelControl;
        currentShowPanelFinishedAction = onFinished;
        this.hasShownAllTexts = isLastText;

        LangManager.SetTexts (localizedTextDetailsList);

        StartCoroutine (SetPanelClick (panelControl, WaitPeriodBeforeAllowPanelClick));

        panelControl.PanelBase.SetActive (true);
        clickOnScreenBtnObject.SetActive (true);

    }

    private void OnShowPanelFinished () {
        // Remarks : tempAction is to prevent the case that currentShowPanelFinishedAction set to some other action while invoke and then immediately set to null 
        var tempAction = currentShowPanelFinishedAction;
        currentShowPanelFinishedAction = null;
        tempAction?.Invoke ();
    }

    private void HidePanel (PanelControl panelControl) {
        SetAllowPanelClick (panelControl, false);

        Action onFinished = () => {
            panelControl.PanelBase.SetActive (false);
            clickOnScreenBtnObject.SetActive (false);

            currentShowingPanel = null;

            OnShowPanelFinished ();
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

    #region Dialog Panel

    private void PrepareInstructionPanel () {
        foreach (var cache in dialogCharacterCacheList) {
            cache.Hide ();
        }

        SetCharacter (dialogLeftSideBaseTransform, MissionEventEnum.Character.None, MissionEventEnum.Expression.Normal, false);
        SetCharacter (dialogRightSideBaseTransform, MissionEventEnum.Character.None, MissionEventEnum.Expression.Normal, false);
    }

    private void PrepareDialogPanel (DialogSubEvent.DialogDetails dialogDetails) {
        foreach (var cache in dialogCharacterCacheList) {
            cache.Hide ();
        }

        SetCharacter (dialogLeftSideBaseTransform, dialogDetails.LeftSide, dialogDetails.LeftSideExpression, dialogDetails.IsLeftSideTalking);
        SetCharacter (dialogRightSideBaseTransform, dialogDetails.RightSide, dialogDetails.RightSideExpression, !dialogDetails.IsLeftSideTalking);
    }

    private void SetCharacter (Transform baseTransform, MissionEventEnum.Character character, MissionEventEnum.Expression expression, bool isTalking) {
        if (character == MissionEventEnum.Character.None) {
            baseTransform.gameObject.SetActive (false);
            return;
        }

        var dialogCharacter = GetDialogCharacter (character);
        if (dialogCharacter == null) {
            Log.PrintWarning ("Cannot get dialog character for character : " + character, LogTypes.MissionEvent | LogTypes.UI);
            baseTransform.gameObject.SetActive (false);
            return;
        }

        dialogCharacter.Show (expression, !isTalking);

        FrameworkUtils.InsertChildrenToParent (baseTransform, dialogCharacter, true, false, -1, false);
        baseTransform.gameObject.SetActive (true);
    }

    private DialogCharacterBase GetDialogCharacter (MissionEventEnum.Character character) {
        if (character == MissionEventEnum.Character.None) {
            return null;
        }

        foreach (var cache in dialogCharacterCacheList) {
            if (cache.Character == character) {
                return cache;
            }
        }

        foreach (var template in dialogCharacterTemplateList) {
            if (template.Character == character) {
                var cache = Instantiate (template);
                dialogCharacterCacheList.Add (cache);

                return cache;
            }
        }

        return null;
    }

    #endregion

    #region Event Handler

    private void ClickOnScreenBtnClickedHandler (HIHIButton sender) {
        if (!isAllowPanelClick) {
            return;
        }

        if (hasShownAllTexts) {
            HidePanel (currentShowingPanel);
        } else {
            OnShowPanelFinished ();
        }
    }

    #region Mission Event

    private bool isPauseBtnInteractableBeforeMissionEvent;

    private void MissionEventStartedHandler () {
        isPauseBtnInteractableBeforeMissionEvent = pauseBtn.interactable;
        pauseBtn.SetInteractable (false);
    }

    private void MissionEventFinishedHandler () {
        pauseBtn.SetInteractable (isPauseBtnInteractableBeforeMissionEvent);
    }

    #endregion

    #endregion
}
