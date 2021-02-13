using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public partial class GameUtils : Singleton<GameUtils> {
    private static CharModel charModel;
    private static bool isCharModelInitialized = false;

    private static TransitionCanvas transitionCanvas;

    #region CharModel

    public static CharModel FindOrSpawnChar () {
        // isCharModelInitialized is to prevent calling charModel after it is destroyed while quiting application 
        if (!isCharModelInitialized) {
            isCharModelInitialized = true;

            charModel = FindObjectOfType<CharModel> ();

            if (charModel == null) {
                charModel = Instantiate (Resources.Load<CharModel> (GameVariable.CharPrefabResourcesName));
                // Remarks :
                // Do Reset() explicitly instead of doing inside Awake() to ensure after getting the model from this method,
                // the model is already initialized
                charModel.Reset (Vector3.zero, LifeEnum.HorizontalDirection.Right);
                DontDestroyOnLoad (charModel);
            }
        }

        return charModel;
    }

    #endregion

    #region TransitionCanvas

    private static TransitionCanvas GetTransitionCanvas () {
        if (!transitionCanvas) {
            transitionCanvas = FindObjectOfType<TransitionCanvas> ();

            if (transitionCanvas == null) {
                transitionCanvas = Instantiate (Resources.Load<TransitionCanvas> (GameVariable.TransitionCanvasPrefabResourcesName));
                DontDestroyOnLoad (transitionCanvas);
            }
        }

        return transitionCanvas;
    }

    public static void SetScreen (bool isBlockedSight) {
        GetTransitionCanvas ().SetFadedIn (isBlockedSight);
    }

    public static void ScreenFadeIn (Action onFInished = null) {
        GetTransitionCanvas ().FadeIn (onFInished);
    }

    public static void ScreenFadeOut (Action onFInished = null) {
        GetTransitionCanvas ().FadeOut (onFInished);
    }

    #endregion
}