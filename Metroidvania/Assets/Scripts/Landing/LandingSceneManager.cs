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
        // TODO : Implement MissionEventEnum.EventType.Opening logic

        SceneManager.LoadScene (GameVariable.MainMenuSceneName);
    }
}