using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionDetails {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }
    public string mapSceneName { get; private set; }
    public List<MissionCollectable.Type> collectables { get; private set; }

    public MissionDetails (int id, string displayNameKey, string mapSceneName, params MissionCollectable.Type[] collectables) {
        this.id = id;
        this.displayNameKey = displayNameKey;
        this.mapSceneName = mapSceneName;
        this.collectables = new List<MissionCollectable.Type> ();
        if (collectables != null && collectables.Length > 0) {
            this.collectables.AddRange (collectables);
        }
    }

    public int GetCollectableCount () {
        if (collectables == null) {
            return 0;
        }

        return collectables.Count;
    }

    #region All Missions

    private static readonly MissionDetails Mission_1 = new MissionDetails (1, "Mission_Tutorial", "Map_Tutorial", MissionCollectable.Type.Command_Hit);

    // TODO : Naming without order
    private static readonly MissionDetails Mission_2 = new MissionDetails (2, "Mission_2", "Map_2");
    private static readonly MissionDetails Mission_3 = new MissionDetails (3, "Mission_3", "Map_3");
    private static readonly MissionDetails Mission_4 = new MissionDetails (4, "Mission_4", "Map_4");
    private static readonly MissionDetails Mission_5 = new MissionDetails (5, "Mission_5", "Map_5");

    public static List<MissionDetails> OrderedMissionList { get; private set; } = new List<MissionDetails> {
            Mission_1,
            Mission_2,
            Mission_3,
            Mission_4,
            Mission_5,
    };

    #endregion
}
