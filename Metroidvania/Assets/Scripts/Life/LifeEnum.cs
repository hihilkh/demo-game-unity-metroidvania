using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeEnum {
    // Remarks: Never change the HorizontalDirection enum int value. It is used in map json data.
    public enum HorizontalDirection : short {
        Left = 0,       // The default value 0 is used for any fallback cases
        Right = 1,
    }

    public enum Location {
        Unknown,
        Ground,
        Air,
    }
}