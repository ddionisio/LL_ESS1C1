using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGoal : MonoBehaviour {    
    GameObject _displayGO;

    [Header("Animations")]
    public M8.Animator.AnimatorData animator;
    public string takeActive;
    public string takeTriggered;

    private bool mIsDisplayActive;

    public void SetDisplayActive(bool active) {
        if(mIsDisplayActive != active) {
            mIsDisplayActive = active;

            

            if(_displayGO) _displayGO.SetActive(active);

            if(animator) {
                if(active) {
                    if(!string.IsNullOrEmpty(takeActive))
                        animator.Play(takeActive);
                }
                else
                    animator.Stop();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        switch(collision.tag) {
            case Tags.player:
                Player player = collision.GetComponent<Player>();

                player.Victory(transform.position);

                //play fancy animation
                if(animator && !string.IsNullOrEmpty(takeTriggered))
                    animator.Play(takeTriggered);
                break;
        }
    }

    void Awake() {
        mIsDisplayActive = false;

        if(_displayGO) _displayGO.SetActive(false);
    }
}
