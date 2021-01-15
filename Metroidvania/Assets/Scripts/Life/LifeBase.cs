using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LifeBase : MonoBehaviour {
    protected virtual int zLayer => 0;
    protected abstract int totalHP { get; }
    protected int currentHP;

    [SerializeField] protected Transform baseTransform;
    public CharEnum.HorizontalDirection facingDirection { get; protected set; }

    public virtual void Init (Vector2 pos, CharEnum.HorizontalDirection direction) {
        currentHP = totalHP;
        SetPosAndDirection (pos, direction);
    }

    public virtual void SetPosAndDirection (Vector2 pos, CharEnum.HorizontalDirection direction) {
        SetPos (pos);
        facingDirection = direction;
    }

    public virtual void SetPos (Vector2 pos) {
        baseTransform.position = new Vector3 (pos.x, pos.y, zLayer);
    }

    public virtual void SetPosByOffset (Vector2 offset) {
        baseTransform.position += (Vector3)offset;
    }

    #region HP related

    /// <summary>
    /// Hurt the life by <paramref name="dp"/> (damage point) and if hp come to zero, it calls Die ()
    /// </summary>
    /// <returns>isAlive</returns>
    public virtual bool Hurt (int dp) {
        currentHP = Mathf.Max (0, currentHP - dp);

        if (currentHP == 0) {
            Die ();
            return false;
        } else {
            return true;
        }
    }

    protected virtual void Die () {
        currentHP = 0;
    }

    #endregion
}