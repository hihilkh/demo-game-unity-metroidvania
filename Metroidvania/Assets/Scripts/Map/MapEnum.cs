public class MapEnum {

    // Remarks: Never change the TileType enum int value. It is used in map json data.
    public enum TileType : int {
        Dirt = 0,   // The default value 0 is used for any fallback cases

        // Below are for map design
        GroundTag = 1001,
        WallTag = 1002,
        RoofTag = 1003,
        SlippyWallTag = 1004,
        DeathTag = 1005
    }

    // Remarks: Never change the TileTag enum int value. It is used in map json data.
    public enum TileTag : int {
        Ground = 0,   // The default value 0 is used for any fallback cases
        Wall = 1,
        SlippyWall = 2,
        Roof = 3,
        Death = 4
    }
}