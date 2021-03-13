using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class LifeHPView : MonoBehaviour {

    [SerializeField] private GameObject hpMask;
    [SerializeField] private SpriteRenderer hpBaseSpriteRenderer;
    [SerializeField] private SpriteRenderer hpSpriteRenderer;

    private Coroutine fadeOutCoroutine = null;
    private const float FadeOutPeriod = 0.3f;

    private static readonly Dictionary<float, Color> HPColorThresholdDict = new Dictionary<float, Color> {
        { 0.6f, Color.green },
        { 0.3f, Color.yellow },
        { 0f, Color.red },
    };

    private float hpMaskFullScaleX;

    public void Awake () {
        hpMaskFullScaleX = hpMask.transform.localScale.x;
    }

    public void UpdateView (int totalHP, int currentHP) {
        if (totalHP == currentHP) {
            SetVisible (false);
            return;
        } else if (totalHP < currentHP) {
            Log.PrintWarning ("Somehow totalHP < currentHP. Please check.", LogTypes.UI | LogTypes.Life);
            SetVisible (false);
            return;
        }

        SetVisible (currentHP > 0);
        var percent = Mathf.Max ((float)currentHP / (float)totalHP, 0);

        foreach (var pair in HPColorThresholdDict) {
            if (percent >= pair.Key) {
                hpSpriteRenderer.color = pair.Value;
                var maskScale = new Vector3 (Mathf.Lerp (0, hpMaskFullScaleX, percent), 1, 1);
                hpMask.transform.localScale = maskScale;
                break;
            }
        }
    }

    private void SetVisible (bool isVisible) {
        if (isVisible) {
            if (fadeOutCoroutine != null) {
                StopCoroutine (fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            hpBaseSpriteRenderer.color = new Color (hpBaseSpriteRenderer.color.r, hpBaseSpriteRenderer.color.g, hpBaseSpriteRenderer.color.b, 1);
        } else {
            if (hpSpriteRenderer.color.a == 0) {
                return;
            }

            if (fadeOutCoroutine == null) {
                fadeOutCoroutine = StartCoroutine (FadeOutCoroutine ());
            }
        }
    }

    private IEnumerator FadeOutCoroutine () {
        var startFadeOutTime = Time.time;

        while (Time.time - startFadeOutTime < FadeOutPeriod) {
            var alpha = 1 - ((Time.time - startFadeOutTime) / FadeOutPeriod);
            hpBaseSpriteRenderer.color = new Color (hpBaseSpriteRenderer.color.r, hpBaseSpriteRenderer.color.g, hpBaseSpriteRenderer.color.b, alpha);
            hpSpriteRenderer.color = new Color (hpSpriteRenderer.color.r, hpSpriteRenderer.color.g, hpSpriteRenderer.color.b, alpha);

            yield return null;
        }

        hpBaseSpriteRenderer.color = new Color (hpBaseSpriteRenderer.color.r, hpBaseSpriteRenderer.color.g, hpBaseSpriteRenderer.color.b, 0);
        hpSpriteRenderer.color = new Color (hpSpriteRenderer.color.r, hpSpriteRenderer.color.g, hpSpriteRenderer.color.b, 0);

        fadeOutCoroutine = null;
    }
}