using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForce : MonoBehaviour {
    public float force;
    public ForceMode2D mode;

    public float dirRotate;

    private Vector2 mDir;

    void Awake() {
        mDir = M8.MathUtil.Rotate(Vector2.up, dirRotate * Mathf.Deg2Rad);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        Rigidbody2D body = collision.GetComponent<Rigidbody2D>();
        if(body) {
            body.AddForce(mDir * force, mode);
        }
    }

    void OnDrawGizmos() {
        //draw dir
        var dir = M8.MathUtil.Rotate(Vector2.up, dirRotate * Mathf.Deg2Rad);

        Gizmos.color = Color.green;

        M8.Gizmo.ArrowLine2D(transform.position, (Vector2)transform.position + dir * 1.5f);
    }
}
