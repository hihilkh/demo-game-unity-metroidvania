using System;

public class CharEnum {
    public enum CharType {
        Player,
        Boss,
    }

    /// <summary>
    /// The direction that the player control the camera to look to
    /// </summary>
    [Flags]
    public enum LookDirections {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
    }

    public enum HorizontalSpeed {
        Idle = 0,       // The default value 0 is used for any fallback cases
        Walk = 1,
        Dash = 2,
    }

    // Remarks: Never change the Command enum int value. It is saved in PlayerPrefs to represent the Command
    public enum Command {
        Jump = 0,
        Dash = 1,
        Hit = 2,
        Arrow = 3,
        Turn = 4,
    }

    // Remarks: Never change the InputSituation enum int value. It is saved in PlayerPrefs to represent the InputSituation
    public enum InputSituation {
        GroundTap = 0,
        GroundHold = 1,
        GroundRelease = 2,
        AirTap = 3,
        AirHold = 4,
        AirRelease = 5,
    }

    public enum HitType {
        Normal,
        Charged,
        Finishing,
        Drop,
    }

    public enum ArrowType {
        Target,
        Straight,
        Triple,
    }

    public enum FaceType {
        Normal,
        Normal_Inversed,
        Confused,
        Shocked,
    }

    [Flags]
    public enum BodyParts {
        None = 0,
        Head = 1 << 0,
        Arms = 1 << 1,
        Legs = 1 << 2,
        Thrusters = 1 << 3,
        Arrow = 1 << 4,

        All = Head + Arms + Legs + Thrusters + Arrow,
    }

    [Flags]
    public enum Statuses {
        Normal = 0,         // status that is no special handling, e.g. idling, walking, free falling, jumping
        BeatingBack = 1 << 0,
        Invincible = 1 << 1,
        Dying = 1 << 2,
        Dashing = 1 << 3,
        DashCollingDown = 1 << 4,
        Sliding = 1 << 5,
        JumpCharging = 1 << 6,
        AttackCoolingDown = 1 << 7,
        DropHitCharging = 1 << 8,
        DropHitting = 1 << 9,
    }
}