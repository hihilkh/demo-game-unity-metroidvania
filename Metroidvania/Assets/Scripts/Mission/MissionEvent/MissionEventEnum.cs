public class MissionEventEnum {

    // Remarks: Never change the EventType enum int value. It is used in PlayerPrefs and map json data.
    public enum EventType {
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,

        FirstHit = 100,

        Opening = 200,
    }

    public enum SubEventType {
        Dialog,
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