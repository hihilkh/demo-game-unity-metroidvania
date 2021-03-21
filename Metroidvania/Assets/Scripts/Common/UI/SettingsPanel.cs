using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using TMPro;
using UnityEngine;

public class SettingsPanel : GeneralPanelBase {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI langTitleText;
    [SerializeField] private TextMeshProUGUI langContentText;
    [SerializeField] private TextMeshProUGUI applyBtnText;

    private bool isInitialized = false;

    private List<LocalizedTextDetails> localizedTextDetailsList;
    private LangType currentSelectingLangType;

    private void Init () {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "SettingsPanel_Title"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (langTitleText, "SettingsPanel_LangTitle"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (applyBtnText, "SettingsPanel_Apply"));

        UIEventManager.AddEventHandler (BtnOnClickType.Settings_LangLeft, LangLeftBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_LangRight, LangRightBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.Settings_Apply, ApplyBtnClickedHandler);
        LangManager.LangChanged += LangChangedHandler;
    }

    protected override void OnDestroy () {
        if (isInitialized) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_LangLeft, LangLeftBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_LangRight, LangRightBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Settings_Apply, ApplyBtnClickedHandler);
            LangManager.LangChanged -= LangChangedHandler;
        }

        base.OnDestroy ();
    }

    new public void Show () {
        Init ();
        SetTexts ();
        SetSelectingLang (LangManager.GetCurrentLang ());

        base.Show ();
    }

    private void SetTexts () {
        LangManager.SetTexts (localizedTextDetailsList);
    }

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

    #region Events

    private void LangLeftBtnClickedHandler (HihiButton sender) {
        ChangeSelectingLang (false);
    }

    private void LangRightBtnClickedHandler (HihiButton sender) {
        ChangeSelectingLang (true);
    }

    private void ApplyBtnClickedHandler (HihiButton sender) {
        LangManager.ChangeLang (currentSelectingLangType);
    }

    private void LangChangedHandler () {
        SetTexts ();
    }

    #endregion
}