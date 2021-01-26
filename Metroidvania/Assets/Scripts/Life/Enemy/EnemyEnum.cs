using System;

public class EnemyEnum {

    // Remarks: Never change the EnemyType enum int value. It is used in map json data.
    public enum EnemyType : int {
        Slime = 0,   // The default value 0 is used for any fallback cases
        Ghost = 1,
    }

    public enum MovementType {
        Walking,
        Flying,
    }

    [Flags]
    public enum Status : short {
        Normal = 0,
        BeatingBack = 1 << 0,
        Invincible = 1 << 1,
        Dying = 1 << 2,
    }
}