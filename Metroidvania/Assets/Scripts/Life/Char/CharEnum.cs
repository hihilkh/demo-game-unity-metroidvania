﻿using System;

public class CharEnum {
    /// <summary>
    /// The direction that the player control the camera to look to
    /// </summary>
    [Flags]
    public enum LookDirection : short {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
    }

    public enum HorizontalSpeed {
        Idle,
        Walk,
        Dash,
    }

    // Remarks: Never change the Command enum int value. It is saved in PlayerPrefs to represent the Command
    public enum Command : int {
        Jump = 1,
        Dash = 2,
        Hit = 3,
        Arrow = 4,
        Turn = 5,
    }

    public enum CommandSituation {
        GroundTap,
        GroundHold,
        GroundRelease,
        AirTap,
        AirHold,
        AirRelease,
    }

    public enum HitType {
        Normal,
        Charged,
        Finishing,
        Drop,
    }

    public enum ArrowType {
        Target,
        Straight,
        Triple,
    }

    public enum FaceType {
        Normal,
        Normal_Inversed,
        Confused,
        Shocked,
    }

    [Flags]
    public enum BodyPart : short {
        None = 0,
        Head = 1 << 0,
        Arms = 1 << 1,
        Legs = 1 << 2,
        Thrusters = 1 << 3,
        Arrow = 1 << 4,
    }
}