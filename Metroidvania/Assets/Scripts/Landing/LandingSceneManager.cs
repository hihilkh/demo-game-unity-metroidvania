using System.Collections;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingSceneManager : MonoBehaviour {
    [SerializeField] private LandingSceneUIManager uiManager;
    [SerializeField] private Transform startingRefPoint;
    [SerializeField] private Transform endingRefPoint;

    private bool isWithGameScene = false;
    private bool isGoingToGameScene = false;
    private bool isControlledByGameScene = false;
    private GameSceneManager gameSceneManager = null;

    private CharModel charModel;
    private Vector2 shiftBackOffset;
    private Vector2 landingToGameOffset;


    private void Awake () {
        GameUtils.SetScreen (true);

        charModel = GameUtils.FindOrSpawnChar ();
        shiftBackOffset = startingRefPoint.position - endingRefPoint.position;
    }

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.Landing_Start, StartBtnClickedHandler);

        charModel.SetActive (true);
        charModel.Reset (startingRefPoint.position, LifeEnum.HorizontalDirection.Right);
        charModel.SetAllowMove (true);
        charModel.SetAllowUserControl (false);

        isWithGameScene = !UserManager.CheckIsFirstMissionCleared ();
        if (isWithGameScene) {
            var ao = SceneManager.LoadSceneAsync (GameVariable.GameSceneName, LoadSceneMode.Additive);

            ao.completed += (AsyncOperation obj) => {
                gameSceneManager = FindObjectOfType<GameSceneManager> ();
                landingToGameOffset = gameSceneManager.GetSelectedEntryPos () - (Vector2)endingRefPoint.position;
                GameUtils.ScreenFadeOut ();
            };
        } else {
            GameUtils.ScreenFadeOut ();
        }
    }


    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Landing_Start, StartBtnClickedHandler);
    }

    private void Update () {
        if (isControlledByGameScene) {
            return;
        }

        var charPos = charModel.GetPos ();
        if (charPos.x > endingRefPoint.position.x) {
            if (isGoingToGameScene) {
                if (gameSceneManager == null) {
                    Log.PrintError ("No GameSceneManager. Cannot start game.", LogTypes.GameFlow);
                } else {
                    isControlledByGameScene = true;
                    charModel.SetPosByOffset (landingToGameOffset);
                    gameSceneManager.StartGameFromLanding ();
                    // Wait a frame to prevent any unfinished logic of starting game (e.g. OnCollisionExit2D)
                    StartCoroutine (WaitAndUnloadScene ());
                }
            } else {
                charModel.SetPosByOffset (shiftBackOffset);
            }
        }
    }

    private IEnumerator WaitAndUnloadScene () {
        yield return null;

        FrameworkUtils.UnloadSceneAndResourcesAsync (GameVariable.LandingSceneName);
    }

    #region Events

    private void StartBtnClickedHandler (HIHIButton sender) {
        if (isWithGameScene) {
            uiManager.HideUI ();
            UserManager.SetFromLandingMissionAndEntry ();
            isGoingToGameScene = true;
        } else {
            SceneManager.LoadScene (GameVariable.MainMenuSceneName);
        }
    }

    #endregion
}