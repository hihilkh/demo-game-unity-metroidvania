using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionDetails {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }
    public List<MapEntry> mapEntries { get; private set; }
    public List<Collectable.Type> collectables { get; private set; }

    public class MapEntry {
        public int id { get; private set; }
        public string displayNameKey { get; private set; }

        public MapEntry (int id, string displayNameKey) {
            this.id = id;
            this.displayNameKey = displayNameKey;
        }
    }

    public MissionDetails (int id, string displayNameKey) {
        this.id = id;
        this.displayNameKey = displayNameKey;
        this.mapEntries = new List<MapEntry> ();
        this.collectables = new List<Collectable.Type> ();
    }

    public void SetMapEntries (params MapEntry[] mapEntries) {
        this.mapEntries.Clear ();
        if (mapEntries != null && mapEntries.Length > 0) {
            this.mapEntries.AddRange (mapEntries);
        }
    }

    public void SetCollectables (params Collectable.Type[] collectables) {
        this.collectables.Clear ();
        if (collectables != null && collectables.Length > 0) {
            this.collectables.AddRange (collectables);
        }
    }

    public bool CheckHasMapEntry (int entryId) {
        foreach (var mapEntry in mapEntries) {
            if (mapEntry.id == entryId) {
                return true;
            }
        }

        return false;
    }

    public int GetCollectableCount () {
        if (collectables == null) {
            return 0;
        }

        return collectables.Count;
    }
}
