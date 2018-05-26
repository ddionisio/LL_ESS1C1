using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlayAnimator : MonoBehaviour {

    public M8.Animator.AnimatorData animator;
    public string take;

    void OnTriggerEnter2D(Collider2D collision) {
        animator.Play(take);
    }
}
