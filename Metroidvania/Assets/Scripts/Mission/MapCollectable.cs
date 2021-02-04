using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MapCollectable {
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
    public string noteTitleKey { get; private set; }
    public string noteContentKey { get; private set; }

    public MapCollectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame, string noteTitleKey = null, string noteContentKey = null) {
        this.type = type;
        this.displayNameKey = displayNameKey;
        this.iconResourcesName = iconResourcesName;
        this.isWithCircleFrame = isWithCircleFrame;
        this.noteTitleKey = noteTitleKey;
        this.noteContentKey = noteContentKey;
    }

    public string GetIconResourcesName () {
        return ResourcesFolderName + iconResourcesName;
    }

    public bool GetIsWithNote () {
        return noteTitleKey != null;
    }

    #region All Collectables

    public static readonly List<MapCollectable> AllCollectables = new List<MapCollectable> () {
        new MapCollectable (Type.Command_Hit, "Command_Hit", "Collectable_Hit", true),
        new MapCollectable (Type.Command_Jump, "Command_Jump", "Collectable_Jump", true),
        new MapCollectable (Type.Command_Dash, "Command_Dash", "Collectable_Dash", true),
        new MapCollectable (Type.Command_Arrow, "Command_Arrow", "Collectable_Arrow", true),
        new MapCollectable (Type.Command_Turn, "Command_Turn", "Collectable_Turn", true),

        new MapCollectable (Type.Note_1, "Note_1", "Collectable_Note", false, "Note_1_Title", "Note_1_Content"),
        new MapCollectable (Type.Note_2, "Note_2", "Collectable_Note", false, "Note_2_Title", "Note_2_Content"),
        new MapCollectable (Type.Note_3, "Note_3", "Collectable_Note", false, "Note_3_Title", "Note_3_Content"),
        new MapCollectable (Type.Note_4, "Note_4", "Collectable_Note", false, "Note_4_Title", "Note_4_Content"),
        new MapCollectable (Type.Note_5, "Note_5", "Collectable_Note", false, "Note_5_Title", "Note_5_Content"),

        new MapCollectable (Type.HP_1, "HP_Up", "Collectable_HPUp", true),
        new MapCollectable (Type.HP_2, "HP_Up", "Collectable_HPUp", true),
        new MapCollectable (Type.Strength_1, "Strength_Up", "Collectable_StrengthUp", true),
        new MapCollectable (Type.Strength_2, "Strength_Up", "Collectable_StrengthUp", true),
    };

    public static MapCollectable GetCollectable (Type type) {
        foreach (var collectable in AllCollectables) {
            if (collectable.type == type) {
                return collectable;
            }
        }

        Log.PrintWarning ("Cannot find MapCollectable of type : " + type.ToString (), LogType.General);
        return null;
    }

    #endregion

}