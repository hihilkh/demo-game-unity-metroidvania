﻿using System;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;

public abstract class GeneralPanelBase : MonoBehaviour {
    [SerializeField] private Animator _animator;
    protected Animator Animator => _animator;
    [SerializeField] private HihiButton _closeBtn;
    protected HihiButton CloseBtn => _closeBtn;


    private bool isAddedEventHandlers = false;
    public event Action PanelHid;

    protected virtual void OnDestroy () {
        RemoveEventHandlers ();
    }

    protected virtual void Show () {
        AddEventHandlers ();

        gameObject.SetActive (true);
    }

    protected virtual void Hide () {
        Action onFinished = () => {
            gameObject.SetActive (false);
            PanelHid?.Invoke ();
        };

        RemoveEventHandlers ();
        FrameworkUtils.Instance.StartSingleAnim (Animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    #region Events

    private void AddEventHandlers () {
        if (!isAddedEventHandlers) {
            isAddedEventHandlers = true;

            UIEventManager.AddEventHandler (BtnOnClickType.Panel_CloseBtn, CloseBtnClickedHandler);
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Panel_CloseBtn, CloseBtnClickedHandler);

            isAddedEventHandlers = false;
        }
    }

    private void CloseBtnClickedHandler (HihiButton sender) {
        if (sender == CloseBtn) {
            Hide ();
        }
    }

    #endregion
}
