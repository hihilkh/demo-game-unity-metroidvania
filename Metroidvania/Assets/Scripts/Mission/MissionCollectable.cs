using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MissionCollectable {
    // Remarks: Never change the Type enum int value. It is saved in PlayerPrefs
    public enum Type : int {
        // Key Item
        Key_Walk = 1,

        // Command
        Command_Jump = 101,
        Command_Dash = 102,
        Command_Hit = 103,
        Command_Arrow = 104,
        Command_Turn = 105,

        // Plot
        Plot_1 = 201,
        Plot_2 = 202,
        Plot_3 = 203,
        Plot_4 = 204,
        Plot_5 = 205,


    }

    // TODO 
    public static string GetCollectableIconResourcesName (Type type, bool isCollected) {
        switch (type) {
            default: return isCollected ? "dummy1" : "dummy2";
        }
    }
}