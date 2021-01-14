using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public partial class GameUtils : Singleton<GameUtils> {
    public CharModel hi;
    public static CharModel FindOrSpawnChar () {
        var charModel = FindObjectOfType<CharModel> ();

        if (charModel == null) {
            charModel = Instantiate (Resources.Load<CharModel> (GameVariable.CharPrefabResourcesName));
            charModel.InitChar (Vector3.zero, CharEnum.HorizontalDirection.Right, false);
            DontDestroyOnLoad (charModel);
        }

        return charModel;
    }
}