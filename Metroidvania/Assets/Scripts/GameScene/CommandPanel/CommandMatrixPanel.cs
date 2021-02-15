using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandMatrixPanel : GeneralPanelBase {
    [SerializeField] private TextMeshProUGUI airText;
    [SerializeField] private TextMeshProUGUI groundText;
    [SerializeField] private TextMeshProUGUI tapText;
    [SerializeField] private TextMeshProUGUI holdText;
    [SerializeField] private TextMeshProUGUI releaseText;

    [SerializeField] private CommandDisplay _commandMatrix_AirTap;
    protected CommandDisplay CommandMatrix_AirTap => _commandMatrix_AirTap;
    [SerializeField] private CommandDisplay _commandMatrix_AirHold;
    protected CommandDisplay CommandMatrix_AirHold => _commandMatrix_AirHold;
    [SerializeField] private CommandDisplay _commandMatrix_AirRelease;
    protected CommandDisplay CommandMatrix_AirRelease => _commandMatrix_AirRelease;
    [SerializeField] private CommandDisplay _commandMatrix_GroundTap;
    protected CommandDisplay CommandMatrix_GroundTap => _commandMatrix_GroundTap;
    [SerializeField] private CommandDisplay _commandMatrix_GroundHold;
    protected CommandDisplay CommandMatrix_GroundHold => _commandMatrix_GroundHold;
    [SerializeField] private CommandDisplay _commandMatrix_GroundRelease;
    protected CommandDisplay CommandMatrix_GroundRelease => _commandMatrix_GroundRelease;

    protected Dictionary<CommandDisplay, CharEnum.InputSituation> CommandMatrixToSituationDict { get; } = new Dictionary<CommandDisplay, CharEnum.InputSituation> ();

    protected bool IsInitialized { get; private set; } = false;
    
    protected bool IsGroundHoldBinding { get; set; } = false;
    protected bool IsAirHoldBinding { get; set; } = false;

    protected virtual bool IsCommandMatrixDisplayOnly => true;

    /// <returns><b>false</b> means skip initialization because initialized before</returns>
    protected virtual bool Init () {
        if (IsInitialized) {
            return false;
        }

        IsInitialized = true;

        // CommandMatrix
        CommandMatrixToSituationDict.Clear ();
        CommandMatrixToSituationDict.Add (CommandMatrix_AirTap, CharEnum.InputSituation.AirTap);
        CommandMatrixToSituationDict.Add (CommandMatrix_AirHold, CharEnum.InputSituation.AirHold);
        CommandMatrixToSituationDict.Add (CommandMatrix_AirRelease, CharEnum.InputSituation.AirRelease);
        CommandMatrixToSituationDict.Add (CommandMatrix_GroundTap, CharEnum.InputSituation.GroundTap);
        CommandMatrixToSituationDict.Add (CommandMatrix_GroundHold, CharEnum.InputSituation.GroundHold);
        CommandMatrixToSituationDict.Add (CommandMatrix_GroundRelease, CharEnum.InputSituation.GroundRelease);

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
        IsGroundHoldBinding = false;
        IsAirHoldBinding = false;

        CharEnum.Command? groundHoldCommand = null;
        CharEnum.Command? airHoldCommand = null;

        if (commandSettings.ContainsKey (CharEnum.InputSituation.GroundHold)) {
            groundHoldCommand = commandSettings[CharEnum.InputSituation.GroundHold];
        }

        if (commandSettings.ContainsKey (CharEnum.InputSituation.AirHold)) {
            airHoldCommand = commandSettings[CharEnum.InputSituation.AirHold];
        }

        foreach (var pair in CommandMatrixToSituationDict) {
            var type = IsCommandMatrixDisplayOnly ? CommandDisplay.DisplayType.Display : CommandDisplay.DisplayType.Container;
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
                                IsGroundHoldBinding = true;
                            }
                        }
                        break;
                    case CharEnum.InputSituation.AirRelease:
                        var isSame2 = command == airHoldCommand;
                        pair.Key.SetReleaseCommand (command, true, isSame2);
                        if (isSame2) {
                            if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, true)) {
                                IsAirHoldBinding = true;
                            }
                        }
                        break;
                }
            }
        }
    }
}