using TMPro;
using UnityEngine;

public class LandingSceneUIManager : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI startText;

    private void Start () {
        SetTexts ();
    }
    
    private void SetTexts () {
        LangManager.SetText (new LocalizedTextDetails (startText, "PressToStart"));
    }

}