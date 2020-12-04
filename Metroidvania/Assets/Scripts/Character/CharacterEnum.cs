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
        Walk,
        Dash
    }

    public enum Animation {
        Idle,
        Walking,
        Jumping,
        Landing,
        Dashing
    }

    public enum Command {
        Jump,
        Dash,
        Hit,
        Arrow,
        Turn
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
}