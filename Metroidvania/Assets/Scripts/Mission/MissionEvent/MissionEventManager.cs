using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

// TODO : Think of getting mission events by JSON
public class MissionEventManager {
    private static readonly List<MissionEvent> AllMissionEvents = new List<MissionEvent> () {
        new MissionEvent (
            MissionEventEnum.EventType.Command_Hit,
            new DialogSubEvent (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Hit"),
            new CommandPanelSubEvent (CharEnum.Command.Hit, CharEnum.InputSituation.GroundTap, "Event_Command_Hit_Panel", 0, 1)
        ),
    };

    public static MissionEvent GetMissionEvent (MissionEventEnum.EventType eventType) {
        foreach (var missionEvent in AllMissionEvents) {
            if (missionEvent.EventType == eventType) {
                return missionEvent;
            }
        }

        Log.PrintWarning ("Cannot find MissionEvent of type : " + eventType, LogTypes.General);
        return null;
    }
}