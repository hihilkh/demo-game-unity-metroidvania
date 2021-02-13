using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using TMPro;
using UnityEngine;

public class ReadyGo : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private TextMeshProUGUI goText;

    private const string ReadyGoAnimStateName = "ReadyGo";

    private void Start () {
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (readyText, "Ready"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (goText, "Go"));

        LangManager.SetTexts (localizedTextDetailsList);

        // Remarks : Somehow set outline color to black will lead to next step (set outline width) getting error...
        var color = new Color32 (1, 1, 1, 255);
        var width = 0.1f;

        readyText.outlineColor = color;
        readyText.outlineWidth = width;

        goText.outlineColor = color;
        goText.outlineWidth = width;
    }

    public void Play (Action onFinished = null) {
        FrameworkUtils.Instance.StartSingleAnim (animator, ReadyGoAnimStateName, onFinished);
    }
}