public class InstructionSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.Instructrion;

    public string LocalizationKeyBase { get; }

    public InstructionSubEvent (string localizationKeyBase) : base () {
        LocalizationKeyBase = localizationKeyBase;
    }
}