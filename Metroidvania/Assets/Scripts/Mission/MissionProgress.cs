using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<Collectable.Type> collectedCollectables;

    public bool isUnlocked => unlockedEntryIds != null && unlockedEntryIds.Count > 0;

    public MissionProgress () {
        unlockedEntryIds = new List<int> ();
        isCleared = false;
        collectedCollectables = new List<Collectable.Type> ();
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

    public void AddCollectedCollectable (Collectable.Type collectable) {
        if (collectedCollectables == null) {
            collectedCollectables = new List<Collectable.Type> ();
        }

        if (!collectedCollectables.Contains (collectable)) {
            collectedCollectables.Add (collectable);
        }
    }

    /// <summary>
    /// For development use only
    /// </summary>
    public void ClearAllCollectedCollectables () {
        if (collectedCollectables == null) {
            collectedCollectables = new List<Collectable.Type> ();
        }

        collectedCollectables.Clear ();
    }
}