using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class CollectableManager {
    public static readonly List<Collectable> AllCollectables = new List<Collectable> () {
        new Collectable (Collectable.Type.Command_Hit, "Command_Hit", "Collectable_Hit", true),
        new Collectable (Collectable.Type.Command_Jump, "Command_Jump", "Collectable_Jump", true),
        new Collectable (Collectable.Type.Command_Dash, "Command_Dash", "Collectable_Dash", true),
        new Collectable (Collectable.Type.Command_Arrow, "Command_Arrow", "Collectable_Arrow", true),
        new Collectable (Collectable.Type.Command_Turn, "Command_Turn", "Collectable_Turn", true),

        new NoteCollectable (Collectable.Type.Note_1, "Note_1", "Note_1_Title", "Note_1_Content"),
        new NoteCollectable (Collectable.Type.Note_2, "Note_2", "Note_2_Title", "Note_2_Content"),
        new NoteCollectable (Collectable.Type.Note_3, "Note_3", "Note_3_Title", "Note_3_Content"),
        new NoteCollectable (Collectable.Type.Note_4, "Note_4", "Note_4_Title", "Note_4_Content"),
        new NoteCollectable (Collectable.Type.Note_5, "Note_5", "Note_5_Title", "Note_5_Content"),
        new NoteCollectable (Collectable.Type.Note_6, "Note_6", "Note_6_Title", "Note_6_Content"),

        new Collectable (Collectable.Type.HP_1, "HP_Up", "Collectable_HPUp", true),
        new Collectable (Collectable.Type.HP_2, "HP_Up", "Collectable_HPUp", true),
        new Collectable (Collectable.Type.Strength_1, "Strength_Up", "Collectable_StrengthUp", true),
        new Collectable (Collectable.Type.Strength_2, "Strength_Up", "Collectable_StrengthUp", true),
        new Collectable (Collectable.Type.FireArrow, "FireArrow", "Collectable_FireArrow", true),

        new EndingCollectable (Collectable.Type.Ending_1, "Ending_1"),
        new EndingCollectable (Collectable.Type.Ending_2, "Ending_2"),
        new EndingCollectable (Collectable.Type.Ending_3, "Ending_3"),
    };

    public static Collectable GetCollectable (Collectable.Type type) {
        foreach (var collectable in AllCollectables) {
            if (collectable.type == type) {
                return collectable;
            }
        }

        Log.PrintWarning ("Cannot find Collectable of type : " + type.ToString (), LogType.General);
        return null;
    }
}