using System;
using HihiFramework.Core;
using UnityEngine;

public partial class GameUtils : Singleton<GameUtils> {
    private static CharModel Character;
    private static bool IsCharModelInitialized = false;

    private static TransitionCanvas TransitionCanvasInstance;

    #region CharModel

    public static CharModel FindOrSpawnChar () {
        // isCharModelInitialized is to prevent calling charModel after it is destroyed while quiting application 
        if (!IsCharModelInitialized) {
            IsCharModelInitialized = true;

            Character = FindObjectOfType<CharModel> ();

            if (Character == null) {
                Character = Instantiate (Resources.Load<CharModel> (GameVariable.CharPrefabResourcesName));
                // Remarks :
                // Do Reset() explicitly instead of doing inside Awake() to ensure after getting the model from this method,
                // the model is already initialized
                Character.Reset (Vector3.zero, LifeEnum.HorizontalDirection.Right);
                DontDestroyOnLoad (Character);
            }
        }

        return Character;
    }

    #endregion

    #region TransitionCanvas

    private static TransitionCanvas GetTransitionCanvas () {
        if (!TransitionCanvasInstance) {
            TransitionCanvasInstance = FindObjectOfType<TransitionCanvas> ();

            if (TransitionCanvasInstance == null) {
                TransitionCanvasInstance = Instantiate (Resources.Load<TransitionCanvas> (GameVariable.TransitionCanvasPrefabResourcesName));
                DontDestroyOnLoad (TransitionCanvasInstance);
            }
        }

        return TransitionCanvasInstance;
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