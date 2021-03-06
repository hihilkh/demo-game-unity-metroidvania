using System;

public class EnemyEnum {

    // Remarks: Never change the EnemyType enum int value. It is used in map json data.
    public enum EnemyType {
        Slime = 0,   // The default value 0 is used for any fallback cases
        Ghost = 1,
        Tank = 2,
        SlimeSenior = 3,
        GhostSenior = 4,
        SlimeGuard = 5,
        SlimeKing = 6,
        GhostKing = 7,
        TreasureBox = 8,
    }

    public enum MovementType {
        Walking,
        Flying,
    }

    [Flags]
    public enum Statuses {
        Normal = 0,
        BeatingBack = 1 << 0,
        Invincible = 1 << 1,
        Dying = 1 << 2,
        DetectedChar = 1 << 3,
        CheckChasing = 1 << 4,
    }
}