using System.Collections.Generic;

// TODO : Think of getting mission and collectable details by map export to JSON
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
        Mission_1.SetEntries (new Mission.Entry (1, "Entry_1"));
        Mission_1.SetCollectables (
            Collectable.Type.Command_Hit,
            Collectable.Type.Command_Jump,
            Collectable.Type.Note_1
        );

        // Mission_2
        Mission_2.SetEntries (
            new Mission.Entry (2, "Entry_2"),
            new Mission.Entry (6, "Entry_6")
        );
        Mission_2.SetCollectables (
            Collectable.Type.Command_Dash,
            Collectable.Type.Command_Arrow,
            Collectable.Type.Note_2,
            Collectable.Type.HP_1,
            Collectable.Type.Strength_2
        );

        // Mission_3
        Mission_3.SetEntries (new Mission.Entry (3, "Entry_3"));
        Mission_3.SetCollectables (
            Collectable.Type.Command_Turn,
            Collectable.Type.Note_3,
            Collectable.Type.Ending_2
        );

        // Mission_4
        Mission_4.SetEntries (
            new Mission.Entry (4, "Entry_4"),
            new Mission.Entry (7, "Entry_7")
        );
        Mission_4.SetCollectables (
            Collectable.Type.Note_4,
            Collectable.Type.Note_5,
            Collectable.Type.Note_6,
            Collectable.Type.Strength_1,
            Collectable.Type.FireArrow
        );

        // Mission_5
        Mission_5.SetEntries (new Mission.Entry (5, "Entry_5"));
        Mission_5.SetCollectables (
            Collectable.Type.HP_2,
            Collectable.Type.Ending_1,
            Collectable.Type.Ending_3
        );
    }

    public static Mission GetMission (int missionId) {
        foreach (var mission in MissionListInOrder) {
            if (mission.Id == missionId) {
                return mission;
            }
        }

        return null;
    }

    public static Mission GetMissionByEntry (int entryId) {
        foreach (var mission in MissionListInOrder) {
            if (mission.GetEntry (entryId) != null) {
                return mission;
            }
        }

        return null;
    }
}