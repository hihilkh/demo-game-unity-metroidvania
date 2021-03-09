public class MapEnum {

    // Remarks: Never change the TileType enum int value. It is used in map json data.
    public enum TileType {
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

        Slippy_Top_FromLeftGround = 27,
        Slippy_Top_FromRightGround = 28,
        Slippy_Bottom_FromLeftGround = 29,
        Slippy_Bottom_FromRightGround = 30,
        Slippy_Left_FromTopGround = 31,
        Slippy_Left_FromBottomGround = 32,
        Slippy_Right_FromTopGround = 33,
        Slippy_Right_FromBottomGround = 34,

        Stick_Vert_1 = 35,
        Stick_Vert_2 = 36,
        Stick_Hori_1 = 37,
        Stick_Hori_2 = 38,
        Billboard_Left_1 = 39,
        Billboard_Left_2 = 40,
        Billboard_Right_1 = 41,
        Billboard_Right_2 = 42,
        Billboard_TopLeft_1 = 43,
        Billboard_TopLeft_2 = 44,
        Billboard_Up_1 = 45,
        Billboard_Up_2 = 46,

        Sting_Top = 47,
        Sting_Bottom = 48,
        Sting_Left = 49,
        Sting_Right = 50,

        Switch_Off_1 = 51,
        Switch_Off_2 = 52,
        Switch_Off_3 = 53,

        Switch_On_1 = 54,
        Switch_On_2 = 55,
        Switch_On_3 = 56,

        Target = 57,

        Grass = 58,

        Torch_1 = 59,
        Torch_2 = 60,
        Torch_3 = 61,
        Torch_4 = 62,

        Cloud1_1 = 63,
        Cloud1_2 = 64,
        Cloud1_3 = 65,
        Cloud1_4 = 66,
        Cloud1_5 = 67,
        Cloud1_6 = 68,
        Cloud1_7 = 69,
        Cloud1_8 = 70,

        Cloud2_1 = 71,
        Cloud2_2 = 72,
        Cloud2_3 = 73,
        Cloud2_4 = 74,
        Cloud2_5 = 75,
        Cloud2_6 = 76,
        Cloud2_7 = 77,
        Cloud2_8 = 78,

        Collapse_1 = 79,
        Collapse_2 = 80,
        Collapse_3 = 81,
        Collapse_4 = 82,
        Collapse_5 = 83,
        Collapse_6 = 84,
        Collapse_7 = 85,
        Collapse_8 = 86,
        Collapse_9 = 87,
        Collapse_10 = 88,
        Collapse_11 = 89,
        Collapse_12 = 90,
        Collapse_13 = 91,
        Collapse_14 = 92,
        Collapse_15 = 93,
        Collapse_16 = 94,

        Tree_Inner_1 = 95,
        Tree_Inner_2 = 96,
        Tree_Inner_3 = 97,
        Tree_Inner_4 = 98,
        Tree_Inner_5 = 99,
        Tree_Inner_6 = 100,
        Tree_Inner_7 = 101,
        Tree_Inner_8 = 102,

        Tree_Outer_1 = 103,
        Tree_Outer_2 = 104,
        Tree_Outer_3 = 105,
        Tree_Outer_4 = 106,
        Tree_Outer_5 = 107,
        Tree_Outer_6 = 108,
        Tree_Outer_7 = 109,
        Tree_Outer_8 = 110,

        // Below are for map design
        GroundTag = 1001,
        Ground2Tag = 1002,
        SlippyWallTag = 1003,
        DeathTag = 1004,
    }

    // Remarks: Never change the TileMapType enum int value. It is used in map json data.
    public enum TileMapType {
        Ground = 0,   // The default value 0 is used for any fallback cases
        SlippyWall = 1,
        Death = 2,
        Background = 3,
    }

    // Remarks: Never change the SwitchType enum int value. It is used in map json data.
    public enum SwitchType {
        OnOff = 0,   // The default value 0 is used for any fallback cases
        Arrow = 1,
        DropHit = 2,
        Enemy = 3,
        MissionEvent = 4,
    }

    // Remarks: Never change the HiddenPathType enum int value. It is used in map json data.
    public enum HiddenPathType {
        HideWhenSwitchOn = 0,   // The default value 0 is used for any fallback cases
        ShowWhenSwitchOn = 1,
    }

    // Remarks: Never change the HiddenPathOrdering enum int value. It is used in map json data.
    /// <summary>
    /// The ordering while switched on.<br />
    /// If HiddenPathType = HideWhenSwitchOn, it will hide by this order.<br />
    /// If HiddenPathType = ShowWhenSwitchOn, it will show by this order.
    /// </summary>
    public enum HiddenPathOrdering {
        DownToUp = 0,   // The default value 0 is used for any fallback cases
        UpToDown = 1,
        LeftToRight = 2,
        RightToLeft = 3,
    }

    // Remarks: Never change the ImageBackgroundType enum int value. It is used in map json data.
    public enum ImageBGType {
        Plain = 0,   // The default value 0 is used for any fallback cases
        RightToLeft = 1,
        DownToUp = 2,
    }
}