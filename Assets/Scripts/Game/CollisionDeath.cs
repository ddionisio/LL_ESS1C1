using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDeath : MonoBehaviour {
    public string filterTag;
    public LayerMask filterLayerMask;

    void OnCollisionEnter2D(Collision2D collision) {
        var go = collision.gameObject;

        if(((1<<go.layer) & filterLayerMask) != 0) {
            if(string.IsNullOrEmpty(filterTag) || go.CompareTag(filterTag)) {
                var entity = go.GetComponent<M8.EntityBase>();
                entity.state = (int)EntityState.Death;
            }
        }
    }
}
