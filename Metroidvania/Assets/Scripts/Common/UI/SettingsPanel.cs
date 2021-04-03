using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : GeneralPanelBase {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI langTitleText;
    [SerializeField] private TextMeshProUGUI langContentText;
    [SerializeField] private TextMeshProUGUI bgmTitleText;
    [SerializeField] private TextMeshProUGUI bgmContentText;
    [SerializeField] private TextMeshProUGUI sfxTitleText;
    [SerializeField] private TextMeshProUGUI sfxContentText;
    [SerializeField] private TextMeshProUGUI applyBtnText;
    [SerializeField] private TextMeshProUGUI viewCreditsBtnText;

    [Header("Credits Panel")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Animator creditsPanelAnimator;
    [SerializeField] private TextMeshProUGUI creditsTitleText;
    [SerializeField] private TextMeshProUGUI creditsContentText;
    [SerializeField] private ScrollRect creditsContentScrollRect;

    private bool isInitialized = false;

    private List<LocalizedTextDetails> localizedTextDetailsList;
    private LocalizedTextDetails bgmContentLocalizedTextDetails;
    private LocalizedTextDetails sfxContentLocalizedTextDetails;

    private LangType currentSelectingLangType;
    private bool currentSelectingBgmOnOffStatus;
    private bool currentSelectingSfxOnOffStatus;

    private void Init () {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "SettingsPanel_Title"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (langTitleText, "SettingsPanel_LangTitle"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (applyBtnText, "SettingsPanel_Apply"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (viewCreditsBtnText, "SettingsPanel_ViewCredits"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (creditsTitleText, "SettingsPanel_CreditsTitle"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (creditsContentText, "SettingsPanel_CreditsContent"));


        localizedTextDetailsList.Add (new LocalizedTextDetails (bgmTitleText, "SettingsPanel_BGMTitle"));
        bgmContentLocalizedTextDetails = new LocalizedTextDetails (bgmContentText, ""); // Remarks : The details would be set by SetSelectingBgmOnOff ()
        localizedTextDetailsList.Add (bgmContentLocalizedTextDetails);

        localizedTextDetailsList.Add (new LocalizedTextDetails (sfxTitleText, "SettingsPanel_SFXTitle"));
        sfxContentLocalizedTextDetails = new LocalizedTextDetails (sfxContentText, ""); // Remarks : The details would be set by SetSelectingSfxOnOff ()
        localizedTextDetailsList.Add (sfxContentLocalizedTextDetails);

        UIEventManager.AddEventHandler (BtnOnClickType.Settings_LangLeft, LangLeftBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_LangRight, LangRightBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_BgmLeft, BgmBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_BgmRight, BgmBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_SfxLeft, SfxBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_SfxRight, SfxBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_Apply, ApplyBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_ViewCredits, ViewCreditsBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_CloseCredits, CloseCreditsBtnClickedHandler);
        LangManager.LangChanged += LangChangedHandler;
    }

    protected override void OnDestroy () {
        if (isInitialized) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_LangLeft, LangLeftBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_LangRight, LangRightBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_BgmLeft, BgmBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_BgmRight, BgmBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_SfxLeft, SfxBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_SfxRight, SfxBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_Apply, ApplyBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_ViewCredits, ViewCreditsBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_CloseCredits, CloseCreditsBtnClickedHandler);
            LangManager.LangChanged -= LangChangedHandler;
        }

        base.OnDestroy ();
    }

    new public void Show () {
        Init ();

        SetSelectingLang (LangManager.GetCurrentLang ());
        SetSelectingBgmOnOff (AudioManager.Instance.CurrentBgmOnOffFlag);
        SetSelectingSfxOnOff (AudioManager.Instance.CurrentSfxOnOffFlag);

        SetTexts ();

        creditsPanel.SetActive (false);

        base.Show ();
    }

    private void SetTexts () {
        LangManager.SetTexts (localizedTextDetailsList);
    }

    private string GetOnOffLocalizationKey (bool isOn) {
        return isOn ? "On" : "Off";
    }

    #region select Lang

    private void SetSelectingLang (LangType langType) {
        LangManager.SetLangNameText (langContentText, langType);
        currentSelectingLangType = langType;
    }

    /// <param name="toNextLang"><b>false</b> means to previous lang</param>
    private void ChangeSelectingLang (bool toNextLang) {
        var langTypeList = LangConfig.GetSelectableLangTypeList ();

        if (langTypeList == null || langTypeList.Count <= 0) {
            Log.PrintError ("SelectableLangTypeList is null or empty. Cannot change selecting lang type.", LogTypes.Lang | LogTypes.UI);
            return;
        }

        var targetIndex = langTypeList.IndexOf (currentSelectingLangType);
        if (targetIndex < 0) {
            Log.PrintWarning ("currentSelectingLangType is not in SelectableLangTypeList. Please check.", LogTypes.Lang | LogTypes.UI);
            targetIndex = 0;
        } else {
            if (toNextLang) {
                targetIndex++;
                if (targetIndex >= langTypeList.Count) {
                    targetIndex = 0;
                }
            } else {
                targetIndex--;
                if (targetIndex < 0) {
                    targetIndex = langTypeList.Count - 1;
                }
            }
        }

        var targetLangType = langTypeList[targetIndex];
        SetSelectingLang (targetLangType);
    }

    #endregion

    #region select Bgm / Sfx On Off

    private void SetSelectingBgmOnOff (bool isOn) {
        bgmContentLocalizedTextDetails.ChangeLocalizationKey (GetOnOffLocalizationKey (isOn));
        LangManager.SetText (bgmContentLocalizedTextDetails);
        currentSelectingBgmOnOffStatus = isOn;
    }

    private void SetSelectingSfxOnOff (bool isOn) {
        sfxContentLocalizedTextDetails.ChangeLocalizationKey (GetOnOffLocalizationKey (isOn));
        LangManager.SetText (sfxContentLocalizedTextDetails);
        currentSelectingSfxOnOffStatus = isOn;
    }

    #endregion

    #region Events

    private void LangLeftBtnClickedHandler (HihiButton sender) {
        ChangeSelectingLang (false);
    }

    private void LangRightBtnClickedHandler (HihiButton sender) {
        ChangeSelectingLang (true);
    }

    private void BgmBtnClickedHandler (HihiButton sender) {
        SetSelectingBgmOnOff (!currentSelectingBgmOnOffStatus);
    }

    private void SfxBtnClickedHandler (HihiButton sender) {
        SetSelectingSfxOnOff (!currentSelectingSfxOnOffStatus);
    }

    private void ApplyBtnClickedHandler (HihiButton sender) {
        LangManager.ChangeLang (currentSelectingLangType);

        AudioManager.Instance.SetBgmOnOff (currentSelectingBgmOnOffStatus);
        AudioManager.Instance.SaveBgmOnOffSetting (currentSelectingBgmOnOffStatus);

        AudioManager.Instance.SetSfxOnOff (currentSelectingSfxOnOffStatus);
        AudioManager.Instance.SaveSfxOnOffSetting (currentSelectingSfxOnOffStatus);
    }

    private void ViewCreditsBtnClickedHandler (HihiButton sender) {
        Action onClosePanelFinished = () => {
            creditsPanel.SetActive (true);
            StartCoroutine (DelaySetCreditsContentScrollRect ());
        };
        FrameworkUtils.Instance.StartSingleAnim (Animator, GameVariable.HidePanelAnimStateName, onClosePanelFinished);
    }

    private IEnumerator DelaySetCreditsContentScrollRect () {
        yield return null;

        creditsContentScrollRect.verticalNormalizedPosition = 1;
    }

    private void CloseCreditsBtnClickedHandler (HihiButton sender) {
        Action onClosePanelFinished = () => {
            creditsPanel.SetActive (false);
            FrameworkUtils.Instance.StartSingleAnim (Animator, GameVariable.ShowPanelAnimStateName);
        };
        FrameworkUtils.Instance.StartSingleAnim (creditsPanelAnimator, GameVariable.HidePanelAnimStateName, onClosePanelFinished);
    }

    private void LangChangedHandler () {
        SetTexts ();
    }

    #endregion
}