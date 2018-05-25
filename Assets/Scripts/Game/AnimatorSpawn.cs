using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSpawn : MonoBehaviour, M8.IPoolSpawn {
    public M8.Animator.AnimatorData animator;
    public string takePlay;

    public float playDelay = 0f;
        
    private M8.PoolDataController mPoolDataCtrl;

    private int mTakePlayInd;

    void Awake() {
        if(!animator)
            animator = GetComponent<M8.Animator.AnimatorData>();

        mPoolDataCtrl = GetComponent<M8.PoolDataController>();

        mTakePlayInd = animator.GetTakeIndex(takePlay);
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        StartCoroutine(DoPlay());
    }

    IEnumerator DoPlay() {
        if(playDelay > 0f) {
            yield return new WaitForSeconds(playDelay);
        }

        animator.Play(mTakePlayInd);
        while(animator.isPlaying)
            yield return null;

        if(mPoolDataCtrl)
            M8.PoolController.ReleaseAuto(mPoolDataCtrl);
        else
            M8.PoolController.ReleaseAuto(gameObject);
    }
}
