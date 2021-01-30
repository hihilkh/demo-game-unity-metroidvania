using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntry {
    public int id { get; private set; }
    public string displayNameKey { get; private set; }

    public MapEntry (int id, string displayNameKey) {
        this.id = id;
        this.displayNameKey = displayNameKey;
    }
}