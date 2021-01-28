using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public partial class GameUtils : Singleton<GameUtils> {
    public static CharModel FindOrSpawnChar () {
        var charModel = FindObjectOfType<CharModel> ();

        if (charModel == null) {
            charModel = Instantiate (Resources.Load<CharModel> (GameVariable.CharPrefabResourcesName));
            // Remarks :
            // Do Init() explicitly instead of doing inside Awake() to ensure after getting the model from this method,
            // the model is already initialized
            charModel.Init ();
            DontDestroyOnLoad (charModel);
        }

        return charModel;
    }
}