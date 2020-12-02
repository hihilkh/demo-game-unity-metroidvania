using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnum {
    public enum Direction {
        Left,
        Right
    }

    public enum Animation {
        Idle,
        Walking,
        Jumping,
        Landing
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