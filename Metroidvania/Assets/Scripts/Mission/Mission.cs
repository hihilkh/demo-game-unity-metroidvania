using System.Collections.Generic;

public class Mission {
    public int Id { get; }
    public string DisplayNameKey { get; }
    public List<Entry> Entries { get; } = new List<Entry> ();
    public List<Collectable.Type> CollectableTypes { get; } = new List<Collectable.Type> ();

    public class Entry {
        public int Id { get; }
        public string DisplayNameKey { get; }

        public Entry (int id, string displayNameKey) {
            Id = id;
            DisplayNameKey = displayNameKey;
        }
    }

    public Mission (int id, string displayNameKey) {
        Id = id;
        DisplayNameKey = displayNameKey;
    }

    public void SetEntries (params Entry[] entries) {
        Entries.Clear ();
        if (entries != null && entries.Length > 0) {
            Entries.AddRange (entries);
        }
    }

    public void SetCollectables (params Collectable.Type[] collectables) {
        CollectableTypes.Clear ();
        if (collectables != null && collectables.Length > 0) {
            CollectableTypes.AddRange (collectables);
        }
    }

    /// <returns><b>null</b> means the mission do not have entry with corresponding entryId</returns>
    public Entry GetEntry (int entryId) {
        foreach (var entry in Entries) {
            if (entry.Id == entryId) {
                return entry;
            }
        }

        return null;
    }
}
