using System.Collections.Generic;
using HihiFramework.Core;

public static class CollectableManager {
    private static readonly List<Collectable> AllCollectables = new List<Collectable> () {
        new Collectable (Collectable.Type.Command_Hit, "Command_Hit", "Collectable_Hit", true, MissionEventEnum.EventType.Command_Hit),
        new Collectable (Collectable.Type.Command_Jump, "Command_Jump", "Collectable_Jump", true, MissionEventEnum.EventType.Command_Jump),
        new Collectable (Collectable.Type.Command_Dash, "Command_Dash", "Collectable_Dash", true, MissionEventEnum.EventType.Command_Dash),
        new Collectable (Collectable.Type.Command_Arrow, "Command_Arrow", "Collectable_Arrow", true),
        new Collectable (Collectable.Type.Command_Turn, "Command_Turn", "Collectable_Turn", true),

        new NoteCollectable (Collectable.Type.Note_1, "Note_1", "Note_1_Content"),
        new NoteCollectable (Collectable.Type.Note_2, "Note_2", "Note_2_Content"),
        new NoteCollectable (Collectable.Type.Note_3, "Note_3", "Note_3_Content"),
        new NoteCollectable (Collectable.Type.Note_4, "Note_4", "Note_4_Content"),
        new NoteCollectable (Collectable.Type.Note_5, "Note_5", "Note_5_Content"),
        new NoteCollectable (Collectable.Type.Note_6, "Note_6", "Note_6_Content"),

        new Collectable (Collectable.Type.HPUp_1, "HP_Up", "Collectable_HPUp", true),
        new Collectable (Collectable.Type.HPUp_2, "HP_Up", "Collectable_HPUp", true),
        new Collectable (Collectable.Type.PowerUp_1, "Power_Up", "Collectable_PowerUp", true),
        new Collectable (Collectable.Type.PowerUp_2, "Power_Up", "Collectable_PowerUp", true),
        new Collectable (Collectable.Type.FireArrow, "FireArrow", "Collectable_FireArrow", true),

        new EndingCollectable (Collectable.Type.Ending_1, "Ending_1"),
        new EndingCollectable (Collectable.Type.Ending_2, "Ending_2"),
        new EndingCollectable (Collectable.Type.Ending_3, "Ending_3"),
    };

    public static Collectable GetCollectable (Collectable.Type type) {
        foreach (var collectable in AllCollectables) {
            if (collectable.CollectableType == type) {
                return collectable;
            }
        }

        Log.PrintWarning ("Cannot find Collectable of type : " + type, LogTypes.General);
        return null;
    }

    public static List<NoteCollectable> GetAllNotes () {
        var list = new List<NoteCollectable> ();

        foreach (var collectable in AllCollectables) {
            if (collectable is NoteCollectable) {
                list.Add ((NoteCollectable)collectable);
            }
        }

        return list;
    }
}