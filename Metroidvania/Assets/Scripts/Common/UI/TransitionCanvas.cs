using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.UI;

public class TransitionCanvas : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private Image mask;

    private const string FadeInAnimStateName = "FadeIn";
    private const string FadeOutAnimStateName = "FadeOut";

    private bool isFadedIn = true;
    private bool isFading = false;

    /// <returns><b>false</b> means already doing fading animation and failed to do again.</returns>
    public bool FadeIn (Action onFinished = null) {
        return Fade (true, onFinished);
    }

    /// <returns><b>false</b> means already doing fading animation and failed to do again.</returns>
    public bool FadeOut (Action onFinished = null) {
        return Fade (false, onFinished);
    }

    private bool Fade (bool isFadeIn, Action onFinished = null) {
        if (isFading) {
            return false;
        }

        if (isFadedIn == isFadeIn) {
            onFinished?.Invoke ();
            return true;
        }

        isFading = true;

        Action onFadeFinished = () => {
            isFading = false;
            isFadedIn = isFadeIn;
            onFinished?.Invoke ();
        };

        var state = isFadeIn ? FadeInAnimStateName : FadeOutAnimStateName;
        FrameworkUtils.Instance.StartSingleAnim (animator, state, onFadeFinished);

        return true;
    }

    public void SetFadedIn (bool isFadedIn) {
        this.isFadedIn = isFadedIn;

        mask.gameObject.SetActive (isFadedIn);
        var alpha = isFadedIn ? 1 : 0;
        mask.color = new Color (mask.color.r, mask.color.g, mask.color.b, alpha);
    }
}