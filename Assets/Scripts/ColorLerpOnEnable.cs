using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorLerpOnEnable : MonoBehaviour {
    public SpriteRenderer spriteRenderer;

    public Color start;
    public Color end = Color.white;

    public DG.Tweening.Ease easeType;

    public float startDelay;
    public float delay = 1f;

    [Header("Signals")]
    public M8.Signal signalWait; //wait for this call before starting
    
    DG.Tweening.EaseFunction mEaseFunc;
    private bool mIsSignalWaiting;

    void OnEnable() {
        if(mEaseFunc == null)
            mEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(easeType);

        if(signalWait != null)
            mIsSignalWaiting = true;

        StartCoroutine(DoEase());
    }

    void OnDestroy() {
        if(signalWait != null)
            signalWait.callback -= OnSignalWait;
    }

    void Awake() {
        if(signalWait != null)
            signalWait.callback += OnSignalWait;
    }

    void OnSignalWait() {
        mIsSignalWaiting = false;
    }

    IEnumerator DoEase() {
        spriteRenderer.color = start;

        while(mIsSignalWaiting)
            yield return null;

        if(startDelay > 0f) {
            yield return new WaitForSeconds(startDelay);
        }

        float curTime = 0f;

        while(curTime < delay) {
            float t = mEaseFunc(curTime, delay, 0f, 0f);

            spriteRenderer.color = Color.Lerp(start, end, t);

            yield return null;

            curTime += Time.deltaTime;
        }
    }
}
