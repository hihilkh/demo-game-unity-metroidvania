using System;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;

public abstract class GeneralPanelBase : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private HIHIButton closeBtn;

    private bool isAddedEventListeners = false;
    public event Action PanelHid;

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
            PanelHid?.Invoke ();
        };

        RemoveEventListeners ();
        FrameworkUtils.Instance.StartSingleAnim (animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    #region Events

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            UIEventManager.AddEventHandler (BtnOnClickType.Panel_CloseBtn, CloseBtnClickedHandler);
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Panel_CloseBtn, CloseBtnClickedHandler);

            isAddedEventListeners = false;
        }
    }

    private void CloseBtnClickedHandler (HIHIButton sender) {
        if (sender == closeBtn) {
            Hide ();
        }
    }

    #endregion
}
