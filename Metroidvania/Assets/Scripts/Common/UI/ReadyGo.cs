using System;
using System.Collections.Generic;
using HihiFramework.Core;
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

        GameUtils.AddBlackOutline (readyText);
        GameUtils.AddBlackOutline (goText);
    }

    public void Play (Action onFinished = null) {
        FrameworkUtils.Instance.StartSingleAnim (animator, ReadyGoAnimStateName, onFinished);
    }
}