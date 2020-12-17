using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine.SceneManagement;

public class LandingSceneManager : MonoBehaviour {
    [SerializeField] private LandingSceneUIManager uiManager;

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnStartBtnClick () {
        // TODO : Implement TutorialManager.HasDoneTutorial_Opening logic
        //if (TutorialManager.HasDoneTutorial_Opening) {
        //    SceneManager.LoadScene (GameVariable.MainMenuSceneName);
        //} else {
        //    SceneManager.LoadScene (GameVariable.GameSceneName);
        //}
        SceneManager.LoadScene (GameVariable.MainMenuSceneName);
    }
}