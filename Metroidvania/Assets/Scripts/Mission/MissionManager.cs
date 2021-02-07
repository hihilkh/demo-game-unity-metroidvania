using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

// TODO : Thin of getting mission and collectable details by map export to JSON
public static class MissionManager {
    public const int FirstMissionId = 1;

    private static readonly Mission Mission_1 = new Mission (1, "Mission_1");
    private static readonly Mission Mission_2 = new Mission (2, "Mission_2");
    private static readonly Mission Mission_3 = new Mission (3, "Mission_3");
    private static readonly Mission Mission_4 = new Mission (4, "Mission_4");
    private static readonly Mission Mission_5 = new Mission (5, "Mission_5");

    public static List<Mission> MissionListInOrder { get; private set; } = new List<Mission> {
            Mission_1,
            Mission_2,
            Mission_3,
            Mission_4,
            Mission_5,
    };

    static MissionManager () {
        // Mission_1
        Mission_1.SetMapEntries (new Mission.MapEntry (1, "MapEntry_1"));
        Mission_1.SetCollectables (
            Collectable.Type.Command_Hit,
            Collectable.Type.Command_Jump
        );

        // Mission_2
        Mission_2.SetMapEntries (
            new Mission.MapEntry (2, "MapEntry_2"),
            new Mission.MapEntry (6, "MapEntry_6")
        );
        Mission_2.SetCollectables (
            Collectable.Type.Command_Dash,
            Collectable.Type.Command_Arrow,
            Collectable.Type.HP_1,
            Collectable.Type.Strength_2
        );

        // Mission_3
        Mission_3.SetMapEntries (new Mission.MapEntry (3, "MapEntry_3"));

        // Mission_4
        Mission_4.SetMapEntries (
            new Mission.MapEntry (4, "MapEntry_4"),
            new Mission.MapEntry (7, "MapEntry_7")
        );

        // Mission_5
        Mission_5.SetMapEntries (new Mission.MapEntry (5, "MapEntry_5"));
    }

    public static Mission GetMission (int missionId) {
        foreach (var mission in MissionListInOrder) {
            if (mission.id == missionId) {
                return mission;
            }
        }

        return null;
    }

    public static Mission GetMissionByMapEntry (int entryId) {
        foreach (var mission in MissionListInOrder) {
            if (mission.CheckHasMapEntry(entryId)) {
                return mission;
            }
        }

        return null;
    }
}