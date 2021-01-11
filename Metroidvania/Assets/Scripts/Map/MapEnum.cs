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
    }
}