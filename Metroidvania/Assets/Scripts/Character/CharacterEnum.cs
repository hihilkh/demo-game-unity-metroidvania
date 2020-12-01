using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnum
{
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
        Hit,
        Jump,
        Dash,
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
}
