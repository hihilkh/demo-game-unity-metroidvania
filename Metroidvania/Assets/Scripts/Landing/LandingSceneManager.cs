using HihiFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingSceneManager : MonoBehaviour {
    [SerializeField] private LandingSceneUIManager uiManager;

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.Landing_Start, StartBtnClickedHandler);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Landing_Start, StartBtnClickedHandler);
    }

    private void StartBtnClickedHandler (HIHIButton sender) {
        // TODO : Implement TutorialManager.HasDoneTutorial_Opening logic
        //if (TutorialManager.HasDoneTutorial_Opening) {
        //    SceneManager.LoadScene (GameVariable.MainMenuSceneName);
        //} else {
        //    SceneManager.LoadScene (GameVariable.GameSceneName);
        //}
        SceneManager.LoadScene (GameVariable.MainMenuSceneName);
    }
}