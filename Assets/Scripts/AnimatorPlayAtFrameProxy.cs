using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Convenience for callback
/// </summary>
public class AnimatorPlayAtFrameProxy : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string take;
    public int frame;

    public void Play() {
        animator.PlayAtFrame(take, frame);
    }
}
