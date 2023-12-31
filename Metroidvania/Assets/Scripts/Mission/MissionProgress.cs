﻿using System;
using System.Collections.Generic;

[Serializable]
public class AllMissionProgress {
    public List<MissionProgressWithId> progressWithIdList;

    public AllMissionProgress () {
        progressWithIdList = new List<MissionProgressWithId> ();
    }

    public AllMissionProgress (Dictionary<int, MissionProgress> progressDict) {
        progressWithIdList = new List<MissionProgressWithId> ();

        if (progressDict != null) {
            foreach (var pair in progressDict) {
                progressWithIdList.Add (new MissionProgressWithId (pair.Key, pair.Value));
            }
        }
    }

    public Dictionary<int, MissionProgress> ConvertToDict () {
        var dict = new Dictionary<int, MissionProgress> ();

        if (progressWithIdList != null) {
            foreach (var progressWithId in progressWithIdList) {
                dict.Add (progressWithId.id, progressWithId.progress);
            }
        }

        return dict;
    }
}

[Serializable]
public class MissionProgressWithId {
    public int id;
    public MissionProgress progress;

    public MissionProgressWithId () {

    }

    public MissionProgressWithId (int id, MissionProgress progress) {
        this.id = id;
        this.progress = progress;
    }
}

[Serializable]
public class MissionProgress {
    public List<int> unlockedEntryIds;
    public bool isCleared;

    public bool IsUnlocked => unlockedEntryIds != null && unlockedEntryIds.Count > 0;

    public MissionProgress () {
        unlockedEntryIds = new List<int> ();
        isCleared = false;
    }

    /// <returns>Is the map entry just unlocked</returns>
    public bool AddUnlockedEntry (int entryId) {
        if (unlockedEntryIds == null) {
            unlockedEntryIds = new List<int> ();
        }

        if (unlockedEntryIds.Contains (entryId)) {
            return false;
        } else {
            unlockedEntryIds.Add (entryId);
            return true;
        }
    }

    #region Development use only

    /// <summary>
    /// For development use only
    /// </summary>
    public void ClearAllProgress () {
        unlockedEntryIds = new List<int> ();
        isCleared = false;
    }

    #endregion
}