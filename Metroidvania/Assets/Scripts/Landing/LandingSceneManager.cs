using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine.SceneManagement;

public class LandingSceneManager : MonoBehaviour {
    [SerializeField] private LandingSceneUIManager uiManager;

    private void Awake () {
        UIEventManager.AddEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnStartBtnClick () {
        if (TutorialManager.HasDoneTutorial_Opening) {
            SceneManager.LoadScene (GameVariable.MainMenuSceneName);
        } else {
            SceneManager.LoadScene (GameVariable.GameSceneName);
        }
    }
}