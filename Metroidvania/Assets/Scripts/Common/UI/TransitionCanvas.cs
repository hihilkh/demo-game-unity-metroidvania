using System;
using System.Collections;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.UI;

public class TransitionCanvas : MonoBehaviour {
    [SerializeField] private Image mask;

    private bool isFadedIn = true;
    private bool isFading = false;

    /// <returns><b>false</b> means already doing fading animation and failed to do again.</returns>
    public bool FadeIn (float totalFadingTime, Action onFinished = null) {
        return Fade (true, totalFadingTime, onFinished);
    }

    /// <returns><b>false</b> means already doing fading animation and failed to do again.</returns>
    public bool FadeOut (float totalFadingTime, Action onFinished = null) {
        return Fade (false, totalFadingTime, onFinished);
    }

    /// <returns><b>false</b> means already doing fading animation and failed to do again.</returns>
    private bool Fade (bool isFadeIn, float totalFadingTime, Action onFinished = null) {
        if (isFading) {
            return false;
        }

        if (isFadedIn == isFadeIn) {
            onFinished?.Invoke ();
            return true;
        }

        StartCoroutine (StartFading (isFadeIn, totalFadingTime, onFinished));

        return true;
    }

    public void FadeImmediately (bool isFadeIn) {
        StartFading (isFadeIn, -1);
    }

    /// <param name="isFadeIn"><b>false</b> means fade out</param>
    /// <param name="totalFadingTime">If smaller or equal to 0, it means immediately finish the fading</param>
    private IEnumerator StartFading (bool isFadeIn, float totalFadingTime, Action onFinished = null) {
        var fromAlpha = isFadeIn ? 0 : 1;
        var toAlpha = isFadeIn ? 1 : 0;

        if (totalFadingTime > 0) {
            mask.gameObject.SetActive (true);

            var startTime = Time.unscaledTime;
            isFading = true;

            while (Time.unscaledTime - startTime < totalFadingTime) {
                var alpha = Mathf.SmoothStep (fromAlpha, toAlpha, (Time.unscaledTime - startTime) / totalFadingTime);
                mask.color = new Color (mask.color.r, mask.color.g, mask.color.b, alpha);
                yield return null;
            }
        }

        mask.color = new Color (mask.color.r, mask.color.g, mask.color.b, toAlpha);
        mask.gameObject.SetActive (isFadeIn);

        isFading = false;
        isFadedIn = isFadeIn;

        onFinished?.Invoke ();
    }
}