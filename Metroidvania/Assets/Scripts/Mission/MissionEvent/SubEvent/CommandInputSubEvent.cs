public class CommandInputSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CommandInput;

    public CharEnum.InputSituation InputSituation { get; }
    public string BeforeInputLocalizationKeyBase { get; }
    public string AfterInputLocalizationKeyBase { get; }

    public CommandInputSubEvent (CharEnum.InputSituation inputSituation, string beforeInputLocalizationKeyBase, string afterInputLocalizationKeyBase) : base () {
        InputSituation = inputSituation;
        BeforeInputLocalizationKeyBase = beforeInputLocalizationKeyBase;
        AfterInputLocalizationKeyBase = afterInputLocalizationKeyBase;
    }
}