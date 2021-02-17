using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPlayer : DialogCharacterBase {
    [SerializeField] private GameObject head_Normal;
    [SerializeField] private GameObject head_Confused;
    [SerializeField] private GameObject head_Shocked;
    [SerializeField] private List<GameObject> armList;
    [SerializeField] private GameObject weapon;

    [SerializeField] private MissionEventEnum.Character _character;
    public override MissionEventEnum.Character Character => _character;

    private Dictionary<MissionEventEnum.Expression, GameObject> _expressionToHeadDict = null;
    private Dictionary<MissionEventEnum.Expression, GameObject> ExpressionToHeadDict {
        get {
            if (_expressionToHeadDict == null) {
                _expressionToHeadDict = new Dictionary<MissionEventEnum.Expression, GameObject> () {
                    { MissionEventEnum.Expression.Normal, head_Normal },
                    { MissionEventEnum.Expression.Confused, head_Confused },
                    { MissionEventEnum.Expression.Shocked, head_Shocked },
                };
            }

            return _expressionToHeadDict;
        }
    }

    private List<Image> _allImages = null;
    private List<Image> AllImages {
        get {
            if (_allImages == null) {
                var images = GetComponentsInChildren<Image> ();
                _allImages = new List<Image> (images);
            }

            return _allImages;
        }
    }

    public override void Show (MissionEventEnum.Expression expression, bool isDim) {
        SetBodyParts ();
        SetExpression (expression);
        SetDim (isDim);

        gameObject.SetActive (true);
    }

    public override void Hide () {
        gameObject.SetActive (false);
    }

    public override void SetExpression (MissionEventEnum.Expression expression) {
        foreach (var pair in ExpressionToHeadDict) {
            pair.Value.SetActive (pair.Key == expression);
        }
    }

    public override void SetDim (bool isDim) {
        var color = isDim ? GameVariable.DisabledUIMaskColor_NoAlpha : Color.white;
        foreach (var image in AllImages) {
            image.color = color;
        }
    }

    private void SetBodyParts () {
        if (Character == MissionEventEnum.Character.Player) {
            var obtainedBodyParts = GameUtils.FindOrSpawnChar ().ObtainedBodyParts;

            var hasArms = (obtainedBodyParts & CharEnum.BodyParts.Arms) == CharEnum.BodyParts.Arms;
            var hasWeapon = (obtainedBodyParts & CharEnum.BodyParts.Arrow) == CharEnum.BodyParts.Arrow;

            foreach (var arm in armList) {
                arm.SetActive (hasArms);
            }
            weapon.SetActive (hasWeapon);
        }
    }
}