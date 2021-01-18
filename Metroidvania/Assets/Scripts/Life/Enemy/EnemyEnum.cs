using System;

public class EnemyEnum {
    public enum MovementType {
        Walking,
        Flying,
    }

    [Flags]
    public enum Status : short {
        Normal = 0,
        BeatingBack = 1 << 0,
        Invincible = 1 << 1,
    }
}