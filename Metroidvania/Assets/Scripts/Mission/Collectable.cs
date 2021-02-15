public class Collectable {
    // Remarks: Never change the Type enum int value. It is used in PlayerPrefs and map json data.
    public enum Type {
        // Command
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,
        Command_Arrow = 3,
        Command_Turn = 4,

        // TODO : Note
        Note_1 = 100,
        Note_2 = 101,
        Note_3 = 102,
        Note_4 = 103,
        Note_5 = 104,
        Note_6 = 105,

        // Others
        HP_1 = 200,
        HP_2 = 201,
        Strength_1 = 202,
        Strength_2 = 203,
        FireArrow = 204,

        // Ending
        Ending_1 = 300,
        Ending_2 = 301,
        Ending_3 = 302,
    }

    private const string ResourcesFolderName = "Collectables/";

    public Type CollectableType { get; private set; }
    public string DisplayNameKey { get; private set; }
    private string iconResourcesName;
    public bool IsWithCircleFrame { get; private set; }

    public Collectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame) {
        this.CollectableType = type;
        this.DisplayNameKey = displayNameKey;
        this.iconResourcesName = iconResourcesName;
        this.IsWithCircleFrame = isWithCircleFrame;
    }

    public string GetIconResourcesName () {
        return ResourcesFolderName + iconResourcesName;
    }
}

public class NoteCollectable : Collectable {
    public string NoteContentKey { get; private set; }

    private NoteCollectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame) : base (type, displayNameKey, iconResourcesName, isWithCircleFrame) {
        // Do not allow constructor without note info
    }

    public NoteCollectable (Type type, string displayNameKey, string noteContentKey) : base (type, displayNameKey, "Collectable_Note", false) {
        NoteContentKey = noteContentKey;
    }
}

public class EndingCollectable : Collectable {
    public EndingCollectable (Type type, string displayNameKey) : base (type, displayNameKey, "Collectable_Ending", false) {
    }
}