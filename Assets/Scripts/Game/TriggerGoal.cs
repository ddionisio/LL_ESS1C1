using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGoal : MonoBehaviour {


    void OnTriggerEnter2D(Collider2D collision) {
        switch(collision.tag) {
            case Tags.player:
                Player player = collision.GetComponent<Player>();

                player.Victory(transform.position);

                //play fancy animation
                break;
        }
    }
}
