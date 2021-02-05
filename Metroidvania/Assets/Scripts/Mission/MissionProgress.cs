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
    public bool isUnlocked;
    public bool isCleared;
    public List<Collectable.Type> collectedCollectables;

    public MissionProgress () {
        isUnlocked = false;
        isCleared = false;
        collectedCollectables = new List<Collectable.Type> ();
    }

    public MissionProgress (bool isUnlocked, bool isCleared, params Collectable.Type[] collectedCollectables) {
        this.isUnlocked = isUnlocked;
        this.isCleared = isCleared;
        this.collectedCollectables = new List<Collectable.Type> ();
        if (collectedCollectables != null && collectedCollectables.Length > 0) {
            this.collectedCollectables.AddRange (collectedCollectables);
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