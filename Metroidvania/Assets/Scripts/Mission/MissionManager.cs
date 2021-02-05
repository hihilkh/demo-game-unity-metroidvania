using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

// TODO : Thin of getting mission and collectable details by map export to JSON
public static class MissionManager {
    public const int FirstMissionId = 1;

    private static readonly MissionDetails Mission_1 = new MissionDetails (1, "Mission_1");
    private static readonly MissionDetails Mission_2 = new MissionDetails (2, "Mission_2");
    private static readonly MissionDetails Mission_3 = new MissionDetails (3, "Mission_3");
    private static readonly MissionDetails Mission_4 = new MissionDetails (4, "Mission_4");
    private static readonly MissionDetails Mission_5 = new MissionDetails (5, "Mission_5");

    public static List<MissionDetails> MissionList { get; private set; } = new List<MissionDetails> {
            Mission_1,
            Mission_2,
            Mission_3,
            Mission_4,
            Mission_5,
    };

    static MissionManager () {
        // Mission_1
        Mission_1.SetMapEntries (new MissionDetails.MapEntry (1, "MapEntry_1"));
        Mission_1.SetCollectables (
            Collectable.Type.Command_Hit,
            Collectable.Type.Command_Jump
        );

        // Mission_2
        Mission_2.SetMapEntries (
            new MissionDetails.MapEntry (2, "MapEntry_2"),
            new MissionDetails.MapEntry (6, "MapEntry_6")
        );
        Mission_2.SetCollectables (
            Collectable.Type.Command_Dash,
            Collectable.Type.Command_Arrow,
            Collectable.Type.HP_1,
            Collectable.Type.Strength_2
        );

        // Mission_3
        Mission_3.SetMapEntries (new MissionDetails.MapEntry (3, "MapEntry_3"));

        // Mission_4
        Mission_4.SetMapEntries (
            new MissionDetails.MapEntry (4, "MapEntry_4"),
            new MissionDetails.MapEntry (7, "MapEntry_7")
        );

        // Mission_5
        Mission_5.SetMapEntries (new MissionDetails.MapEntry (5, "MapEntry_5"));
    }

    public static MissionDetails GetMission (int missionId) {
        foreach (var mission in MissionList) {
            if (mission.id == missionId) {
                return mission;
            }
        }

        return null;
    }

    public static MissionDetails GetMissionByMapEntry (int entryId) {
        foreach (var mission in MissionList) {
            if (mission.CheckHasMapEntry(entryId)) {
                return mission;
            }
        }

        return null;
    }
}