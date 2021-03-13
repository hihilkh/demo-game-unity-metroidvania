using System.Collections.Generic;
using HihiFramework.UI;
using TMPro;
using UnityEngine;

public class GamePausePanel : CommandMatrixPanel {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI backToMMBtnText;
    [SerializeField] private TextMeshProUGUI restartBtnText;

    [SerializeField] private HIHIButton backToMMBtn;

    private bool isHideResetTimeScale = true;

    #region CommandMatrixPanel

    private void OnEnable () {
        Time.timeScale = 0;
        isHideResetTimeScale = true;
    }

    private void OnDisable () {
        if (isHideResetTimeScale) {
            Time.timeScale = 1;
        }
    }

    protected override bool Init () {
        if (!base.Init ()) {
            return false;
        }

        // Localization
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "GamePausePanel_Title"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (backToMMBtnText, "BackToMM"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (restartBtnText, "Restart"));
        LangManager.SetTexts (localizedTextDetailsList);

        backToMMBtn.SetInteractable (UserManager.CheckIsFirstMissionCleared ());

        return true;
    }

    #endregion

    new public void Show (Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        base.Show (defaultCommandSettings);
    }

    public void Hide (bool isResetTimeScale) {
        isHideResetTimeScale = isResetTimeScale;
        base.Hide ();
    }
}