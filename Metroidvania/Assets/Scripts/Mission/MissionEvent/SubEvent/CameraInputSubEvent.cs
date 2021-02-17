public class CameraInputSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CameraInput;

    public CharEnum.LookDirections LookDirections { get; }
    public string BeforeInputLocalizationKeyBase { get; }
    public string AfterInputLocalizationKeyBase { get; }

    public CameraInputSubEvent (CharEnum.LookDirections lookDirections, string beforeInputLocalizationKeyBase, string afterInputLocalizationKeyBase) : base () {
        LookDirections = lookDirections;
        BeforeInputLocalizationKeyBase = beforeInputLocalizationKeyBase;
        AfterInputLocalizationKeyBase = afterInputLocalizationKeyBase;
    }
}