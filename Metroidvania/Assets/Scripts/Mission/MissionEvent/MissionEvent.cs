using System.Collections.Generic;

public class MissionEvent {
    public MissionEventEnum.EventType EventType { get; }
    private List<SubEventBase> subEventList;

    public MissionEvent (MissionEventEnum.EventType eventType, params SubEventBase[] subEvents) {
        EventType = eventType;
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