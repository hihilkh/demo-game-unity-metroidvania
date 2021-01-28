using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class EnemyMapping {
    private const string ResourcesFolderName = "Enemies/";

    // Remarks : Only File name
    private static readonly Dictionary<EnemyEnum.EnemyType, string> EnemyResourcesNameDict = new Dictionary<EnemyEnum.EnemyType, string> () {
        { EnemyEnum.EnemyType.Slime, "Slime" },
        { EnemyEnum.EnemyType.Ghost, "Ghost" },
    };

    public static string GetEnemyResourcesName (EnemyEnum.EnemyType enemyType) {
        if (EnemyResourcesNameDict.ContainsKey (enemyType)) {
            return ResourcesFolderName + EnemyResourcesNameDict[enemyType];
        } else {
            Log.PrintError ("No matched enemy resources name for enemyType : " + enemyType, LogType.MapData);
            return null;
        }
    }
}