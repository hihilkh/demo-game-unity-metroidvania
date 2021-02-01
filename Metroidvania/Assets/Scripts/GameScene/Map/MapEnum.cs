public class MapEnum {

    // Remarks: Never change the TileType enum int value. It is used in map json data.
    public enum TileType : int {
        Soil = 0,   // The default value 0 is used for any fallback cases

        Ground_Top = 1,
        Ground_Bottom = 2,
        Ground_Left = 3,
        Ground_Right = 4,
        Ground_OuterTopLeft = 5,
        Ground_OuterTopRight = 6,
        Ground_OuterBottomLeft = 7,
        Ground_OuterBottomRight = 8,
        Ground_InnerTopLeft = 9,
        Ground_InnerTopRight = 10,
        Ground_InnerBottomLeft = 11,
        Ground_InnerBottomRight = 12,
        Ground_Monotone = 13,
        Ground_Single = 14,

        Slippy_Top = 15,
        Slippy_Bottom = 16,
        Slippy_Left = 17,
        Slippy_Right = 18,
        Slippy_OuterTopLeft = 19,
        Slippy_OuterTopRight = 20,
        Slippy_OuterBottomLeft = 21,
        Slippy_OuterBottomRight = 22,
        Slippy_InnerTopLeft = 23,
        Slippy_InnerTopRight = 24,
        Slippy_InnerBottomLeft = 25,
        Slippy_InnerBottomRight = 26,
        Slippy_FromTopGround = 27,
        Slippy_FromBottomGround = 28,
        Slippy_FromLeftGround = 29,
        Slippy_FromRightGround = 30,

        Stick_Vert_1 = 31,
        Stick_Vert_2 = 32,
        Stick_Hori_1 = 33,
        Stick_Hori_2 = 34,
        Billboard_Left_1 = 35,
        Billboard_Left_2 = 36,
        Billboard_Right_1 = 37,
        Billboard_Right_2 = 38,
        Billboard_TopLeft_1 = 39,
        Billboard_TopLeft_2 = 40,
        Billboard_Up_1 = 41,
        Billboard_Up_2 = 42,

        Sting_Top = 43,
        Sting_Bottom = 44,
        Sting_Left = 45,
        Sting_Right = 46,

        Switch_Off_1 = 47,
        Switch_Off_2 = 48,
        Switch_Off_3 = 49,
        Switch_Off_4 = 50,

        Switch_On_1 = 51,
        Switch_On_2 = 52,
        Switch_On_3 = 53,
        Switch_On_4 = 54,

        Target = 55,

        Grass = 56,

        Torch_1 = 57,
        Torch_2 = 58,
        Torch_3 = 59,
        Torch_4 = 60,

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
        DropHit = 2,
    }

    // Remarks: Never change the HiddenPathOpenType enum int value. It is used in map json data.
    public enum HiddenPathOpenType : int {
        DownToUp = 0,   // The default value 0 is used for any fallback cases
        UpToDown = 1,
        LeftToRight = 2,
        RightToLeft = 3,
    }
}