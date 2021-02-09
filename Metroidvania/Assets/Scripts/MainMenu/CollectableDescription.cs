using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectableDescription : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private static Color NotCollectedMaskColor = new Color (200f / 255f, 200f / 255f, 200f / 255f, 128f / 255f);

    public void Show (Collectable collectable, bool isCollected) {
        var icon = Resources.Load<Sprite> (collectable.GetIconResourcesName ());
        iconImage.sprite = icon;

        LangManager.SetText (new LocalizedTextDetails (descriptionText, isCollected ? collectable.displayNameKey : "UnknownCollectableDescription"));

        iconImage.color = isCollected ? Color.white : NotCollectedMaskColor;
        descriptionText.color = isCollected ? Color.white : NotCollectedMaskColor;

        gameObject.SetActive (true);
    }

    public void Hide () {
        gameObject.SetActive (false);
    }
}