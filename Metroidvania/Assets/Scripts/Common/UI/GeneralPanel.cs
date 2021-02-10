using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;

public abstract class GeneralPanel : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private HIHIButton closeBtn;

    private bool isAddedEventListeners = false;

    protected virtual void OnDestroy () {
        RemoveEventListeners ();
    }

    protected virtual void Show () {
        AddEventListeners ();

        gameObject.SetActive (true);
    }

    protected virtual void Hide () {
        Action onFinished = () => {
            gameObject.SetActive (false);
        };

        RemoveEventListeners ();
        FrameworkUtils.Instance.StartSingleAnim (animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    #region Events

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            UIEventManager.AddEventHandler (BtnOnClickType.Panel_CloseBtn, OnCloseBtnClick);
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Panel_CloseBtn, OnCloseBtnClick);

            isAddedEventListeners = false;
        }
    }

    private void OnCloseBtnClick (HIHIButton btn) {
        if (btn == closeBtn) {
            Hide ();
        }
    }

    #endregion
}
