using TMPro;
using UnityEngine;
using HihiFramework.UI;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private GameObject baseUI;
    [SerializeField] private TextMeshProUGUI startText;

    private void Start () {
        SetTexts ();
    }
    
    private void SetTexts () {
        LangManager.SetText (new LocalizedTextDetails (startText, "PressToStart"));
    }

    public void HideUI () {
        baseUI.SetActive (false);
    }
}