using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetInteractibleOnAnimatorPlay : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string takeTarget;
    public Selectable selectable;

    public bool interactibleWhilePlaying = false;

    private bool mIsPlaying;

    void OnDestroy() {
        if(animator) animator.takeCompleteCallback -= OnTakeComplete;
    }

    void OnEnable() {
        mIsPlaying = animator.isPlaying && (string.IsNullOrEmpty(takeTarget) || animator.currentPlayingTakeName == takeTarget);
        RefreshState();
    }

    void Awake() {
        animator.takeCompleteCallback += OnTakeComplete;
    }

    void Update() {
        if(mIsPlaying != animator.isPlaying) {
            mIsPlaying = animator.isPlaying;
            RefreshState();
        }
    }

    void RefreshState() {
        selectable.interactable = mIsPlaying ? interactibleWhilePlaying : !interactibleWhilePlaying;
    }

    void OnTakeComplete(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(string.IsNullOrEmpty(takeTarget) || take.name == takeTarget)
            selectable.interactable = !interactibleWhilePlaying;
    }
}
