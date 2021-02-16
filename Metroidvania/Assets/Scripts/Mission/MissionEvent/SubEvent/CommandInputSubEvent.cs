using System.Collections.Generic;

public class CommandInputSubEvent : InstructionSubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CommandInput;

    public CharEnum.InputSituation InputSituation { get; }

    public CommandInputSubEvent (CharEnum.InputSituation inputSituation, string instructionLocalizationKeyBase, int inputIndex) : base (instructionLocalizationKeyBase, inputIndex) {
        InputSituation = inputSituation;
    }
}