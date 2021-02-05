﻿using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class Collectable {
    // Remarks: Never change the Type enum int value. It is used in PlayerPrefs and map json data.
    public enum Type : int {
        // Command
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,
        Command_Arrow = 3,
        Command_Turn = 4,

        // Plot
        Note_1 = 100,
        Note_2 = 101,
        Note_3 = 102,
        Note_4 = 103,
        Note_5 = 104,

        // Others
        HP_1 = 200,
        HP_2 = 201,
        Strength_1 = 202,
        Strength_2 = 203,

    }

    private const string ResourcesFolderName = "Collectables/";

    public Type type { get; private set; }
    public string displayNameKey { get; private set; }
    private string iconResourcesName;
    public bool isWithCircleFrame { get; private set; }

    public Collectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame) {
        this.type = type;
        this.displayNameKey = displayNameKey;
        this.iconResourcesName = iconResourcesName;
        this.isWithCircleFrame = isWithCircleFrame;
    }

    public string GetIconResourcesName () {
        return ResourcesFolderName + iconResourcesName;
    }
}

public class NoteCollectable : Collectable {
    public string noteTitleKey { get; private set; }
    public string noteContentKey { get; private set; }

    private NoteCollectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame) : base (type, displayNameKey, iconResourcesName, isWithCircleFrame) {
        // Do not allow constructor without note info
    }

    public NoteCollectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame, string noteTitleKey, string noteContentKey) : base (type, displayNameKey, iconResourcesName, isWithCircleFrame) {
        this.noteTitleKey = noteTitleKey;
        this.noteContentKey = noteContentKey;
    }
}