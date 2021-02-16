using System.Collections.Generic;

public class CommandPanelSubEvent : InstructionSubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CommandPanel;

    public CharEnum.Command Command { get; }
    public CharEnum.InputSituation InputSituation { get; }


    public CommandPanelSubEvent (CharEnum.Command command, CharEnum.InputSituation inputSituation, string instructionLocalizationKeyBase, int setCommandInputIndex, int confirmInputIndex) : base (instructionLocalizationKeyBase, setCommandInputIndex, confirmInputIndex) {
        Command = command;
        InputSituation = inputSituation;
    }
}