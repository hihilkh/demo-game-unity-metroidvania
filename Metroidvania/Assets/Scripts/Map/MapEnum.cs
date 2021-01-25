public class MapEnum {

    // Remarks: Never change the TileType enum int value. It is used in map json data.
    public enum TileType : int {
        Dirt = 0,   // The default value 0 is used for any fallback cases

        // Below are for map design
        GroundTag = 1001,
        Ground2Tag = 1002,
        SlippyWallTag = 1003,
        DeathTag = 1004,
    }

    // Remarks: Never change the TileMapType enum int value. It is used in map json data.
    public enum TileMapType : int {
        Ground = 0,   // The default value 0 is used for any fallback cases
        Ground2 = 1,
        SlippyWall = 2,
        Death = 3,
        Background = 4,
    }

    // Remarks: Never change the SwitchType enum int value. It is used in map json data.
    public enum SwitchType : int {
        Normal = 0,   // The default value 0 is used for any fallback cases
        Arrow = 1,
    }

    // Remarks: Never change the HiddenPathOpenType enum int value. It is used in map json data.
    public enum HiddenPathOpenType : int {
        DownToUp = 0,   // The default value 0 is used for any fallback cases
        UpToDown = 1,
        LeftToRight = 2,
        RightToLeft = 3,
    }
}