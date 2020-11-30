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

    public enum Action {
        Idle,
        Walking,
        Jumping,
        Landing
    }
}
