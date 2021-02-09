using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }
    public List<Entry> entries { get; private set; }
    public List<Collectable.Type> collectableTypes { get; private set; }

    public class Entry {
        public int id { get; private set; }
        public string displayNameKey { get; private set; }

        public Entry (int id, string displayNameKey) {
            this.id = id;
            this.displayNameKey = displayNameKey;
        }
    }

    public Mission (int id, string displayNameKey) {
        this.id = id;
        this.displayNameKey = displayNameKey;
        this.entries = new List<Entry> ();
        this.collectableTypes = new List<Collectable.Type> ();
    }

    public void SetEntries (params Entry[] entries) {
        this.entries.Clear ();
        if (entries != null && entries.Length > 0) {
            this.entries.AddRange (entries);
        }
    }

    public void SetCollectables (params Collectable.Type[] collectables) {
        this.collectableTypes.Clear ();
        if (collectables != null && collectables.Length > 0) {
            this.collectableTypes.AddRange (collectables);
        }
    }

    /// <returns><b>null</b> means the mission do not have entry with corresponding entryId</returns>
    public Entry GetEntry (int entryId) {
        foreach (var entry in entries) {
            if (entry.id == entryId) {
                return entry;
            }
        }

        return null;
    }
}
