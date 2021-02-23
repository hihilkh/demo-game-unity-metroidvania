using System.Collections.Generic;

public class MissionEvent {
    public MissionEventEnum.EventType EventType { get; }
    public bool IsNeedToStopChar { get; }
    public bool IsOneTimeEvent { get; }
    private List<SubEventBase> subEventList;

    public MissionEvent (MissionEventEnum.EventType eventType, bool isNeedToStopChar, bool isOneTimeEvent, params SubEventBase[] subEvents) {
        EventType = eventType;
        IsNeedToStopChar = isNeedToStopChar;
        IsOneTimeEvent = isOneTimeEvent;
        if (subEvents == null) {
            subEventList = new List<SubEventBase> ();
        } else {
            subEventList = new List<SubEventBase> (subEvents);
        }
    }

    public List<SubEventBase> GetSubEventListClone () {
        return new List<SubEventBase> (subEventList);
    }
}