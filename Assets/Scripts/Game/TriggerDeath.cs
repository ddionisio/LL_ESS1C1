using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDeath : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collision) {
        var entity = collision.GetComponent<M8.EntityBase>();
        entity.state = (int)EntityState.Death;
    }
}