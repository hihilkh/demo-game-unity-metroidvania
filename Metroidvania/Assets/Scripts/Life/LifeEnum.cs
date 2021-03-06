public class LifeEnum {
    // Remarks: Never change the HorizontalDirection enum int value. It is used in map json data.
    public enum HorizontalDirection {
        Left = 0,       // The default value 0 is used for any fallback cases
        Right = 1,
    }

    public enum Location {
        Unknown,
        Ground,
        Air,
    }

    public enum CollisionType {
        Ground,
        Wall,
        Roof,
        Death,
        Char,
        Enemy,
    }

    public enum CollisionChangedType {
        None,
        Enter,
        Exit,
    }
}