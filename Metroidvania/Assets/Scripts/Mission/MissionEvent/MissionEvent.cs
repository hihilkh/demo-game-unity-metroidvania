public class MissionEvent : MissionEventBase {
    public MissionEventEnum.EventType EventType { get; }
    public bool IsNeedToStopChar { get; }
    public bool IsOneTimeEvent { get; }

    public MissionEvent (MissionEventEnum.EventType eventType, bool isNeedToStopChar, bool isOneTimeEvent, params SubEventBase[] subEvents) : base (subEvents) {
        EventType = eventType;
        IsNeedToStopChar = isNeedToStopChar;
        IsOneTimeEvent = isOneTimeEvent;
    }
}