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
}
