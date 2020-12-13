using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using HIHIFramework.UI;

public class LandingSceneManager : MonoBehaviour {
    [SerializeField] private LandingSceneUIManager uiManager;

    private void Awake () {
        UIEventManager.AddEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.Landing_Start, OnStartBtnClick);
    }

    private void OnStartBtnClick () {
        Log.Print ("OnStartBtnClick");
    }
}