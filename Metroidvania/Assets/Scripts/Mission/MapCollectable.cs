using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapCollectable {
    // Remarks: Never change the Type enum int value. It is used in PlayerPrefs and map json data.
    public enum Type : int {
        // Command
        Command_Hit = 0,   // The default value 0 is used for any fallback cases
        Command_Jump = 1,
        Command_Dash = 2,
        Command_Arrow = 3,
        Command_Turn = 4,

        // Plot
        Plot_1 = 100,
        Plot_2 = 101,
        Plot_3 = 102,
        Plot_4 = 103,
        Plot_5 = 104,

        // Others
        HP_1 = 200,
        HP_2 = 201,
        Strength_1 = 202,
        Strength_2 = 203,

    }

    // TODO 
    public static string GetCollectableIconResourcesName (Type type, bool isCollected) {
        switch (type) {
            default: return isCollected ? "dummy1" : "dummy2";
        }
    }
}