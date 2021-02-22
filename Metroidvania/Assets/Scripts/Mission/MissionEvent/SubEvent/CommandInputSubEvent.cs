public class CommandInputSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CommandInput;

    public CharEnum.InputSituation InputSituation { get; }
    public string BeforeInputLocalizationKeyBase { get; }

    public CommandInputSubEvent (CharEnum.InputSituation inputSituation, string beforeInputLocalizationKeyBase) : base () {
        InputSituation = inputSituation;
        BeforeInputLocalizationKeyBase = beforeInputLocalizationKeyBase;
    }
}