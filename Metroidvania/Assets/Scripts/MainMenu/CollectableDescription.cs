using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectableDescription : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Show (Collectable collectable, bool isCollected) {
        var icon = Resources.Load<Sprite> (collectable.GetIconResourcesName ());
        iconImage.sprite = icon;

        LangManager.SetText (new LocalizedTextDetails (descriptionText, isCollected ? collectable.DisplayNameKey : GameVariable.UnknownTextKey));

        iconImage.color = isCollected ? Color.white : GameVariable.DisabledUIMaskColor;
        descriptionText.color = isCollected ? Color.white : GameVariable.DisabledUIMaskColor;

        gameObject.SetActive (true);
    }

    public void Hide () {
        gameObject.SetActive (false);
    }
}