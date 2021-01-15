using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class CharArrowBase : MonoBehaviour
{
    [SerializeField] protected CharParams charParams;
    [SerializeField] protected SpriteRenderer arrowSprite;
    [SerializeField] protected Rigidbody2D rb;

    protected bool hasHitAnything = false;
    private const float VanishPeriod = 1f;

    protected void SetInitPos (Vector3 pos) {
        // keep posZ so that it render on top
        transform.position = new Vector3 (pos.x, pos.y, transform.position.z);
    }

    protected void UpdateArrowPointingDirection () {
        transform.right = rb.velocity.normalized;
    }

    private void Hit (Transform target) {
        hasHitAnything = true;
        FrameworkUtils.InsertChildrenToParent (target, gameObject);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

        StartCoroutine (Vanish ());
    }

    private IEnumerator Vanish () {
        var startTime = Time.time;

        while (Time.time - startTime < VanishPeriod) {
            var progress = (Time.time - startTime) / VanishPeriod;
            arrowSprite.color = new Color (arrowSprite.color.r, arrowSprite.color.g, arrowSprite.color.b, 1 - progress);
            yield return null;
        }

        DestroySelf ();
    }

    private void DestroySelf () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == GameVariable.PlayerTag || collision.tag == GameVariable.AttackTag) {
            return;
        }

        Hit (collision.transform);
        // TODO
    }
}
