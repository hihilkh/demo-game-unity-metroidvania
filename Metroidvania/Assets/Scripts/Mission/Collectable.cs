using System.Collections.Generic;

public class Collectable {
    // Remarks: Never change the Type enum int value. It is used in PlayerPrefs and map json data.
    public enum Type {
        // Command
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,
        Command_Arrow = 3,
        Command_Turn = 4,

        // Note
        Note_1 = 100,
        Note_2 = 101,
        Note_3 = 102,
        Note_4 = 103,
        Note_5 = 104,
        Note_6 = 105,

        // Others
        HPUp_1 = 200,
        HPUp_2 = 201,
        PowerUp_1 = 202,
        PowerUp_2 = 203,
        FireArrow = 204,

        // Ending
        Ending_1 = 300,
        Ending_2 = 301,
        Ending_3 = 302,
    }

    public static readonly List<Type> HPUpTypeList = new List<Type> () {
        Type.HPUp_1,
        Type.HPUp_2,
    };

    public static readonly List<Type> PowerUpTypeList = new List<Type> () {
        Type.PowerUp_1,
        Type.PowerUp_2,
    };

    private const string ResourcesFolderName = "Collectables/";

    public Type CollectableType { get; }
    public string DisplayNameKey { get; }
    private string iconResourcesName;
    public bool IsWithCircleFrame { get; }
    public MissionEventEnum.EventType? EventType { get; }

    public Collectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame, MissionEventEnum.EventType? eventType = null) {
        this.CollectableType = type;
        this.DisplayNameKey = displayNameKey;
        this.iconResourcesName = iconResourcesName;
        this.IsWithCircleFrame = isWithCircleFrame;
        this.EventType = eventType;
    }

    public string GetIconResourcesName () {
        return ResourcesFolderName + iconResourcesName;
    }
}

public class NoteCollectable : Collectable {
    public string NoteContentKey { get; private set; }

    private NoteCollectable (Type type, string displayNameKey, string iconResourcesName, bool isWithCircleFrame, MissionEventEnum.EventType? eventType = null) : base (type, displayNameKey, iconResourcesName, isWithCircleFrame, eventType) {
        // Do not allow constructor without note info
    }

    public NoteCollectable (Type type, string displayNameKey, string noteContentKey, MissionEventEnum.EventType? eventType = null) : base (type, displayNameKey, "Collectable_Note", false, eventType) {
        NoteContentKey = noteContentKey;
    }
}

public class EndingCollectable : Collectable {
    public EndingCollectable (Type type, string displayNameKey, MissionEventEnum.EventType? eventType = null) : base (type, displayNameKey, "Collectable_Ending", false, eventType) {
    }
}