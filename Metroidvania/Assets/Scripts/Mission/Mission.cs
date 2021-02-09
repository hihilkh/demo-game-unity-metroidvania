using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }
    public List<MapEntry> mapEntries { get; private set; }
    public List<Collectable.Type> collectableTypes { get; private set; }

    public class MapEntry {
        public int id { get; private set; }
        public string displayNameKey { get; private set; }

        public MapEntry (int id, string displayNameKey) {
            this.id = id;
            this.displayNameKey = displayNameKey;
        }
    }

    public Mission (int id, string displayNameKey) {
        this.id = id;
        this.displayNameKey = displayNameKey;
        this.mapEntries = new List<MapEntry> ();
        this.collectableTypes = new List<Collectable.Type> ();
    }

    public void SetMapEntries (params MapEntry[] mapEntries) {
        this.mapEntries.Clear ();
        if (mapEntries != null && mapEntries.Length > 0) {
            this.mapEntries.AddRange (mapEntries);
        }
    }

    public void SetCollectables (params Collectable.Type[] collectables) {
        this.collectableTypes.Clear ();
        if (collectables != null && collectables.Length > 0) {
            this.collectableTypes.AddRange (collectables);
        }
    }

    /// <returns><b>null</b> means the mission do not have entry with corresponding entryId</returns>
    public MapEntry GetMapEntry (int entryId) {
        foreach (var mapEntry in mapEntries) {
            if (mapEntry.id == entryId) {
                return mapEntry;
            }
        }

        return null;
    }
}
