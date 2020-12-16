using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionDetails {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }
    public string mapSceneName { get; private set; }

    public MissionDetails (int id, string displayNameKey, string mapSceneName) {
        this.id = id;
        this.displayNameKey = displayNameKey;
        this.mapSceneName = mapSceneName;
    }

    #region All Missions

    private static MissionDetails Mission_Tutorial = new MissionDetails (1, "Tutorial", "Map_Tutorial");

    // TODO : Naming without order
    private static MissionDetails Mission_2 = new MissionDetails (2, "Mission2", "Map_2");
    private static MissionDetails Mission_3 = new MissionDetails (3, "Mission3", "Map_3");
    private static MissionDetails Mission_4 = new MissionDetails (4, "Mission4", "Map_4");
    private static MissionDetails Mission_5 = new MissionDetails (5, "Mission5", "Map_5");

    public static IList<MissionDetails> OrderedMissionList { get; private set; } = new List<MissionDetails> {
            Mission_Tutorial,
            Mission_2,
            Mission_3,
            Mission_4,
            Mission_5
    }.AsReadOnly ();

    #endregion
}
