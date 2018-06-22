using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorCounterWidget : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string takePlay;

    public GameObject[] errorGOs;
    public float errorShowDelay = 0.3f;
    public float errorTransferDelay = 0.3f;

    public int count { get { return mCurCount; } }
    public bool isPlaying { get { return mPlayCounter > 0; } }

    private int mCurCount;

    private int mPlayCounter;

    public void Init() {
        mCurCount = 0;
        mPlayCounter = 0;

        for(int i = 0; i < errorGOs.Length; i++) {
            errorGOs[i].SetActive(false);
        }
    }

    public void Increment() {
        if(mCurCount == errorGOs.Length)
            return;

        int ind = mCurCount;

        mCurCount++;

        StartCoroutine(DoIncrement(ind));
    }

    public void DecrementTransfer(Vector3 destPos) {
        if(mCurCount > 0) {
            mCurCount--;
            int errorGOIndex = mCurCount;

            StartCoroutine(DoDecrementTransfer(errorGOIndex, destPos));
        }
    }

    void OnDisable() {
        mPlayCounter = 0;
    }

    IEnumerator DoIncrement(int errorGOIndex) {
        mPlayCounter++;

        if(animator && !string.IsNullOrEmpty(takePlay)) {
            animator.Play(takePlay);
            while(animator.isPlaying)
                yield return null;
        }

        var errorGO = errorGOs[errorGOIndex];
        var errorT = errorGO.transform;

        errorGO.SetActive(true);

        var startScale = new Vector3(0f, 0f, 1f);
        var endScale = Vector3.one;

        errorT.localScale = startScale;

        float curT = 0f;
        while(curT < errorShowDelay) {
            yield return null;

            curT += Time.deltaTime;

            float t = Mathf.Clamp01(curT / errorShowDelay);

            errorT.localScale = Vector3.Lerp(startScale, endScale, t);
        }

        mPlayCounter--;
    }

    IEnumerator DoDecrementTransfer(int errorGOIndex, Vector3 destPos) {
        mPlayCounter++;

        var errorGO = errorGOs[errorGOIndex];
        var errorT = errorGO.transform;

        var startPos = errorT.position;

        float curT = 0f;
        while(curT < errorShowDelay) {
            yield return null;

            curT += Time.deltaTime;

            float t = Mathf.Clamp01(curT / errorShowDelay);

            errorT.position = Vector3.Lerp(startPos, destPos, t);
        }

        //turn off and revert
        errorGO.SetActive(false);
        errorT.position = startPos;

        mPlayCounter--;
    }
}
