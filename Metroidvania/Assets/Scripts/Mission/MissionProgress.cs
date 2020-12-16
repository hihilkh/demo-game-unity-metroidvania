using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AllMissionProgress {
    public List<MissionProgress> progress;

    public AllMissionProgress () {
        progress = new List<MissionProgress> ();
    }

    public AllMissionProgress (List<MissionProgress> progress) {
        this.progress = progress;
    }
}

[Serializable]
public class MissionProgress {
    public int id;
    public bool isCleared;
    public float currentProgress;

    public MissionProgress () {

    }

    public MissionProgress (int id, bool isCleared, float currentProgress) {
        this.id = id;
        this.isCleared = isCleared;
        this.currentProgress = currentProgress;
    }
}