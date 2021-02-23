public class MissionEventEnum {

    // Remarks: Never change the EventType enum int value. It is used in PlayerPrefs and map json data.
    public enum EventType {
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,

        FirstHit = 100,
        TouchWallAndTurn = 101,
        AirJump = 102,
        WallAction = 103,
        AirFinishingHit = 104,
        CameraMovement = 105,
        WarningIfNoDash = 106,
        Boss = 107,
        Boss_NotYetBeaten = 108,
        Boss_Beaten = 109,

        Opening = 200,
    }

    public enum SubEventType {
        Dialog,
        Instructrion,
        CommandPanel,
        CameraInput,
        CommandInput,
        MapSwitch,
        Wait,
    }

    public enum Character {
        None,
        Player,
        Boss,
    }

    public enum Expression {
        Normal,
        Confused,
        Shocked,
    }


}