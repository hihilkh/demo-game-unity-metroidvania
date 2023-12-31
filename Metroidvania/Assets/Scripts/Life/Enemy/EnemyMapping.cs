﻿using System.Collections.Generic;
using HihiFramework.Core;

public static class EnemyMapping {
    private const string ResourcesFolderName = "Enemies/";

    // Remarks : Only File name
    private static readonly Dictionary<EnemyEnum.EnemyType, string> EnemyResourcesNameDict = new Dictionary<EnemyEnum.EnemyType, string> () {
        { EnemyEnum.EnemyType.Slime, "Slime" },
        { EnemyEnum.EnemyType.Ghost, "Ghost" },
        { EnemyEnum.EnemyType.Tank, "Tank" },
        { EnemyEnum.EnemyType.SlimeSenior, "SlimeSenior" },
        { EnemyEnum.EnemyType.GhostSenior, "GhostSenior" },
        { EnemyEnum.EnemyType.SlimeGuard, "SlimeGuard" },
        { EnemyEnum.EnemyType.SlimeKing, "SlimeKing" },
        { EnemyEnum.EnemyType.GhostKing, "GhostKing" },
        { EnemyEnum.EnemyType.TreasureBox, "TreasureBox" },
    };

    private const string BossResourcesName = "Char/Boss";

    public static string GetEnemyResourcesName (EnemyEnum.EnemyType enemyType) {
        if (enemyType == EnemyEnum.EnemyType.Boss) {
            return BossResourcesName;
        }

        if (EnemyResourcesNameDict.ContainsKey (enemyType)) {
            return ResourcesFolderName + EnemyResourcesNameDict[enemyType];
        } else {
            Log.PrintError ("No matched enemy resources name for enemyType : " + enemyType, LogTypes.MapData);
            return null;
        }
    }
}