using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public partial class GameUtils : Singleton<GameUtils> {
    public static CharModel FindOrSpawnChar () {
        var charModel = FindObjectOfType<CharModel> ();

        if (charModel == null) {
            charModel = Instantiate (Resources.Load<CharModel> (GameVariable.CharPrefabResourcesName));
            charModel.Init (Vector3.zero, CharEnum.HorizontalDirection.Right);
            DontDestroyOnLoad (charModel);
        }

        return charModel;
    }
}