using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionEvent {
    public MissionEventEnum.EventType EventType { get; private set; }
    public List<SubEventBase> SubEventList { get; private set; }

    public MissionEvent (MissionEventEnum.EventType eventType, params SubEventBase[] subEvents) {
        EventType = eventType;
        if (subEvents == null) {
            SubEventList = new List<SubEventBase> ();
        } else {
            SubEventList = new List<SubEventBase> (subEvents);
        }
    }
}