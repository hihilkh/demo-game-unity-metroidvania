using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandMatrixPanel : GeneralPanel {
    [SerializeField] private TextMeshProUGUI airText;
    [SerializeField] private TextMeshProUGUI groundText;
    [SerializeField] private TextMeshProUGUI tapText;
    [SerializeField] private TextMeshProUGUI holdText;
    [SerializeField] private TextMeshProUGUI releaseText;

    [SerializeField] protected CommandDisplay commandMatrix_AirTap;
    [SerializeField] protected CommandDisplay commandMatrix_AirHold;
    [SerializeField] protected CommandDisplay commandMatrix_AirRelease;
    [SerializeField] protected CommandDisplay commandMatrix_GroundTap;
    [SerializeField] protected CommandDisplay commandMatrix_GroundHold;
    [SerializeField] protected CommandDisplay commandMatrix_GroundRelease;

    protected Dictionary<CommandDisplay, CharEnum.InputSituation> commandMatrixToSituationDict = new Dictionary<CommandDisplay, CharEnum.InputSituation> ();

    protected bool isInitialized = false;

    protected virtual bool isCommandMatrixDisplayOnly => true;

    protected bool isGroundHoldBinding = false;
    protected bool isAirHoldBinding = false;

    /// <returns><b>false</b> means skip initialization because initialized before</returns>
    protected virtual bool Init () {
        if (isInitialized) {
            return false;
        }

        isInitialized = true;

        // CommandMatrix
        commandMatrixToSituationDict.Clear ();
        commandMatrixToSituationDict.Add (commandMatrix_AirTap, CharEnum.InputSituation.AirTap);
        commandMatrixToSituationDict.Add (commandMatrix_AirHold, CharEnum.InputSituation.AirHold);
        commandMatrixToSituationDict.Add (commandMatrix_AirRelease, CharEnum.InputSituation.AirRelease);
        commandMatrixToSituationDict.Add (commandMatrix_GroundTap, CharEnum.InputSituation.GroundTap);
        commandMatrixToSituationDict.Add (commandMatrix_GroundHold, CharEnum.InputSituation.GroundHold);
        commandMatrixToSituationDict.Add (commandMatrix_GroundRelease, CharEnum.InputSituation.GroundRelease);

        // Localization
        var baseLocalizedTextDetailsList = new List<LocalizedTextDetails> ();
        baseLocalizedTextDetailsList.Add (new LocalizedTextDetails (airText, "InAir"));
        baseLocalizedTextDetailsList.Add (new LocalizedTextDetails (groundText, "OnGround"));
        baseLocalizedTextDetailsList.Add (new LocalizedTextDetails (tapText, "Tag"));
        baseLocalizedTextDetailsList.Add (new LocalizedTextDetails (holdText, "Hold"));
        baseLocalizedTextDetailsList.Add (new LocalizedTextDetails (releaseText, "Release"));
        LangManager.SetTexts (baseLocalizedTextDetailsList);

        return true;
    }

    protected virtual void Show (Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        Init ();

        base.Show ();

        ResetCommandMatrix (defaultCommandSettings);
    }

    protected virtual void ResetCommandMatrix (Dictionary<CharEnum.InputSituation, CharEnum.Command> commandSettings) {
        isGroundHoldBinding = false;
        isAirHoldBinding = false;

        CharEnum.Command? groundHoldCommand = null;
        CharEnum.Command? airHoldCommand = null;

        if (commandSettings.ContainsKey (CharEnum.InputSituation.GroundHold)) {
            groundHoldCommand = commandSettings[CharEnum.InputSituation.GroundHold];
        }

        if (commandSettings.ContainsKey (CharEnum.InputSituation.AirHold)) {
            airHoldCommand = commandSettings[CharEnum.InputSituation.AirHold];
        }

        foreach (var pair in commandMatrixToSituationDict) {
            var type = isCommandMatrixDisplayOnly ? CommandDisplay.Type.Display : CommandDisplay.Type.Container;
            pair.Key.Reset (type);

            if (commandSettings.ContainsKey (pair.Value)) {
                var command = commandSettings[pair.Value];

                switch (pair.Value) {
                    case CharEnum.InputSituation.GroundTap:
                        pair.Key.SetTapCommand (command, false);
                        break;
                    case CharEnum.InputSituation.AirTap:
                        pair.Key.SetTapCommand (command, true);
                        break;
                    case CharEnum.InputSituation.GroundHold:
                        pair.Key.SetHoldCommand (command, false);
                        break;
                    case CharEnum.InputSituation.AirHold:
                        pair.Key.SetHoldCommand (command, true);
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                        var isSame = command == groundHoldCommand;
                        pair.Key.SetReleaseCommand (command, false, isSame);
                        if (isSame) {
                            if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, false)) {
                                isGroundHoldBinding = true;
                            }
                        }
                        break;
                    case CharEnum.InputSituation.AirRelease:
                        var isSame2 = command == airHoldCommand;
                        pair.Key.SetReleaseCommand (command, true, isSame2);
                        if (isSame2) {
                            if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, true)) {
                                isAirHoldBinding = true;
                            }
                        }
                        break;
                }
            }
        }
    }
}