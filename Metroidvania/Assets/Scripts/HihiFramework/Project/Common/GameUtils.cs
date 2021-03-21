using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameUtils : Singleton<GameUtils> {
    private static CharModel Character;
    private static bool IsCharModelInitialized = false;
    private const string CharResourcesName = "Char/Character";

    private static TransitionCanvas TransitionCanvasInstance;
    private const float ScreenFadingTime_Fast = 1f;
    private const float ScreenFadingTime_Slow = 2f;

    /// <summary>
    /// Only trigger for LoadSceneMode.Single<br />
    /// Input:<br />
    /// first string : fromSceneName<br />
    /// second string : toSceneName
    /// </summary>
    public static event Action<string, string> SingleSceneChanging;
    /// <summary>
    /// Only trigger for LoadSceneMode.Single<br />
    /// Input:<br />
    /// first string : currentSceneName
    /// </summary>
    public static event Action<string> SingleSceneChanged;

    #region CharModel

    public static CharModel FindOrSpawnChar () {
        // isCharModelInitialized is to prevent calling charModel after it is destroyed while quiting application 
        if (!IsCharModelInitialized) {
            IsCharModelInitialized = true;

            var charModelList = FindObjectsOfType<CharModel> ();

            foreach (var charModel in charModelList) {
                if (charModel.CharType == CharEnum.CharType.Player) {
                    Character = charModel;
                    break;
                }
            }

            if (Character == null) {
                Character = Instantiate (Resources.Load<CharModel> (CharResourcesName));
                // Remarks :
                // Do Reset() explicitly instead of doing inside Awake() to ensure after getting the model from this method,
                // the model is already initialized
                Character.Reset (Vector3.zero, LifeEnum.HorizontalDirection.Right, true);
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
        GetTransitionCanvas ().FadeImmediately (isBlockedSight);
    }

    private static float GetFadingTime (bool isFast) {
        return isFast ? ScreenFadingTime_Fast : ScreenFadingTime_Slow;
    }

    public static void ScreenFadeIn (bool isFast, Action onFinished = null) {
        GetTransitionCanvas ().FadeIn (GetFadingTime (isFast), onFinished);
    }

    public static void ScreenFadeOut (bool isFast, Action onFinished = null) {
        GetTransitionCanvas ().FadeOut (GetFadingTime (isFast), onFinished);
    }

    #endregion

    #region Scene Management

    public static void LoadSingleScene (string sceneName, bool isWithFadeIn = true, bool isFastFadeIn = true) {
        Action onFadeInFinished = () => {
            SingleSceneChanging?.Invoke (SceneManager.GetActiveScene ().name, sceneName);
            SceneManager.LoadScene (sceneName);
            SingleSceneChanged?.Invoke (sceneName);
        };

        if (isWithFadeIn) {
            ScreenFadeIn (isFastFadeIn, onFadeInFinished);
        } else {
            onFadeInFinished ();
        }
    }

    public static void LoadGameScene (int missionId, int entryId) {
        UserManager.SelectedMissionId = missionId;
        UserManager.SelectedEntryId = entryId;

        LoadSingleScene (GameVariable.GameSceneName);
    }

    #endregion
}