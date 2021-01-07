﻿public class MapEnum {

    // Remarks: Never change the TileType enum int value. It is used in map json data.
    public enum TileType : int {
        Grass = 0   // The default value 0 is used for any fallback cases
    }

    // Remarks: Never change the TileTag enum int value. It is used in map json data.
    public enum TileTag : int {
        Ground = 0,   // The default value 0 is used for any fallback cases
        Wall = 1
    }
}