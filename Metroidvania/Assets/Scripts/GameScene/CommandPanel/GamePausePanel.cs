using System.Collections.Generic;
using HihiFramework.UI;
using TMPro;
using UnityEngine;

public class GamePausePanel : CommandMatrixPanel {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI backToMMBtnText;
    [SerializeField] private TextMeshProUGUI restartBtnText;
    [SerializeField] private TextMeshProUGUI viewEnvBtnText;

    [SerializeField] private HihiButton backToMMBtn;
    [SerializeField] private HihiButton viewEnvBtn;

    private bool isHideResetTimeScale = true;

    #region CommandMatrixPanel

    private void OnEnable () {
        GameUtils.StopTime ();
        isHideResetTimeScale = true;
    }

    private void OnDisable () {
        if (isHideResetTimeScale) {
            GameUtils.ResumeTime ();
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
        localizedTextDetailsList.Add (new LocalizedTextDetails (viewEnvBtnText, "ViewEnv"));
        LangManager.SetTexts (localizedTextDetailsList);

        return true;
    }

    #endregion

    new public void Show (Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        // btn interactable
        backToMMBtn.SetInteractable (UserManager.CheckIsFirstMissionCleared ());
        viewEnvBtn.SetInteractable (UserManager.GetIsAllowUserInput ());

        base.Show (defaultCommandSettings);
    }

    public void Hide (bool isResetTimeScale) {
        isHideResetTimeScale = isResetTimeScale;
        base.Hide ();
    }
}