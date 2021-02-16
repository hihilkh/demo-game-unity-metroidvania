using System.Collections.Generic;

public class CameraInputSubEvent : InstructionSubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CameraInput;

    public CharEnum.LookDirections LookDirections { get; }

    public CameraInputSubEvent (CharEnum.LookDirections lookDirections, string instructionLocalizationKeyBase, int inputIndex) : base (instructionLocalizationKeyBase, inputIndex) {
        LookDirections = lookDirections;
    }
}