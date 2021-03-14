using TMPro;
using UnityEngine;
using HihiFramework.UI;
using HihiFramework.Core;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private GameObject baseUI;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start () {
        SetTexts ();
    }
    
    private void SetTexts () {
        LangManager.SetText (new LocalizedTextDetails (startText, "PressToStart"));
        LangManager.SetText (new LocalizedTextDetails (versionText, FrameworkUtils.GetVersionNoStr (), false));
    }

    public void HideUI () {
        baseUI.SetActive (false);
    }
}