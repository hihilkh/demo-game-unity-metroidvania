using System.Collections;
using System.Collections.Generic;
using HIHIFramework.UI;
using TMPro;
using UnityEngine;

public class GamePausePanel : CommandMatrixPanel {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI backToMMBtnText;
    [SerializeField] private TextMeshProUGUI restartBtnText;

    #region CommandMatrixPanel

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

        return true;
    }

    #endregion

    new public void Show (Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        base.Show (defaultCommandSettings);
    }
}