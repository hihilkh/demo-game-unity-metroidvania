using System;

public class CharEnum {
    public enum Direction {
        Left,
        Right
    }

    public enum HorizontalSpeed {
        Idle,
        Walk,
        Dash
    }

    public enum Location {
        Ground,
        Air,
        Wall
    }

    // Remarks: Never change the Command enum int value. It is saved in PlayerPrefs to represent the Command
    public enum Command : int {
        Jump = 1,
        Dash = 2,
        Hit = 3,
        Arrow = 4,
        Turn = 5
    }

    public enum CommandSituation {
        GroundTap,
        GroundHold,
        GroundRelease,
        AirTap,
        AirHold,
        AirRelease
    }

    public enum HitType {
        Normal,
        Charged,
        Finishing,
        Drop
    }

    public enum ArrowType {
        Target,
        Straight,
        Triple
    }

    public enum FaceType {
        Normal,
        Normal_Inversed,
        Confused,
        Shocked
    }

    [Flags]
    public enum BodyPart : short {
        None = 0,
        Head = 1,
        Arms = 2,
        Legs = 4,
        Thrusters = 8,
        Arrow = 16
    }
}