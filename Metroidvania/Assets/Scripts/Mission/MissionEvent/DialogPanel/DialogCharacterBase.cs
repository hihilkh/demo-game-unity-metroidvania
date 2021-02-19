using UnityEngine;

public abstract class DialogCharacterBase : MonoBehaviour {
    public abstract MissionEventEnum.Character Character { get; }

    public abstract void Show (MissionEventEnum.Expression expression, bool isDim);
    public abstract void Hide ();

    public abstract void SetExpression (MissionEventEnum.Expression expression);
    public abstract void SetDim (bool isDim);
}