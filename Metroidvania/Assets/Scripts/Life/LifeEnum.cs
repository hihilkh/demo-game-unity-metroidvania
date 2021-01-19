using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeEnum {
    public enum HorizontalDirection {
        Left,
        Right,
    }

    public enum Location {
        Unknown,
        Ground,
        Air,
        Wall,
    }

    [Flags]
    public enum Status : short {
        Normal = 0,
        BeatingBack = 1 << 0,
        Invincible = 1 << 1,
        Dying = 1 << 2,
    }
}
