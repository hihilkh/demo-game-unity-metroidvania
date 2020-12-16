using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnum {
    public enum Direction {
        Left,
        Right
    }

    public enum HorizontalSpeed {
        Zero,
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

    public enum JumpChargeLevel {
        Zero = 0,
        One = 1,
        Two = 2
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
}