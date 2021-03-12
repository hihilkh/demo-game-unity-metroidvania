using System.Collections.Generic;

public class MissionEventBase {
    protected List<SubEventBase> subEventList;

    public MissionEventBase (params SubEventBase[] subEvents) {
        if (subEvents == null) {
            subEventList = new List<SubEventBase> ();
        } else {
            subEventList = new List<SubEventBase> (subEvents);
        }
    }

    public List<SubEventBase> GetSubEventListClone () {
        return new List<SubEventBase> (subEventList);
    }

    public bool CheckHasSubEventType (MissionEventEnum.SubEventType subEventType) {
        foreach (var subEvent in subEventList) {
            if (subEvent.SubEventType == subEventType) {
                return true;
            }
        }

        return false;
    }
}
