using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M8;

public class GameCamera : MonoBehaviour {
    public GameCameraData data;

    [SerializeField]
    bool _boundLocked = true;
        
    public Camera2D camera2D { get; private set; }

    public Vector2 position { get { return transform.position; } }

    public bool boundLocked {
        get { return _boundLocked; }
        set { _boundLocked = value; }
    }

    /// <summary>
    /// Local space
    /// </summary>
    public Rect cameraViewRect { get; private set; }
    public Vector2 cameraViewExtents { get; private set; }

    public bool isMoving { get { return mMoveToRout != null; } }
        
    private Coroutine mMoveToRout;

    //interpolate
    private Rect mBoundsRectNext;
    private Coroutine mBoundsChangeRout;

    private Rect mBoundsRect;

    public void SetBounds(Rect newBounds, bool interpolate) {
        //don't interpolate if current bounds is invalid
        if(mBoundsRect.size.x == 0f || mBoundsRect.size.y == 0f)
            interpolate = false;
        
        if(interpolate) {
            mBoundsRectNext = newBounds;

            if(mBoundsChangeRout != null)
                StopCoroutine(mBoundsChangeRout);

            mBoundsChangeRout = StartCoroutine(DoBoundsChange());
        }
        else {
            mBoundsRect = newBounds;

            SetPosition(transform.position); //refresh clamp
        }
    }

    public void MoveTo(Vector2 dest) {
        StopMoveTo();
                
        //clamp
        if(boundLocked)
            dest = mBoundsRect.Clamp(dest, cameraViewExtents);

        //ignore if we are exactly on dest
        if(position == dest)
            return;

        mMoveToRout = StartCoroutine(DoMoveTo(dest));
    }

    public void StopMoveTo() {
        if(mMoveToRout != null) {
            StopCoroutine(mMoveToRout);
            mMoveToRout = null;
        }
    }

    public void SetPosition(Vector2 pos) {
        //clamp
        if(boundLocked)
            pos = mBoundsRect.Clamp(pos, cameraViewExtents);

        transform.position = pos;
    }

    public bool isVisible(Rect rect) {
        rect.center = transform.worldToLocalMatrix.MultiplyPoint3x4(rect.center);

        return cameraViewRect.Overlaps(rect);
    }
        
    void OnDisable() {
        StopMoveTo();

        if(mBoundsChangeRout != null) {
            StopCoroutine(mBoundsChangeRout);
            mBoundsChangeRout = null;

            mBoundsRect = mBoundsRectNext;
        }
    }

    void Awake() {
        camera2D = GetComponentInChildren<Camera2D>();

        var unityCam = camera2D.unityCamera;

        //setup view bounds
        var minExt = unityCam.ViewportToWorldPoint(Vector3.zero);
        var maxExt = unityCam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        var mtxToLocal = transform.worldToLocalMatrix;

        var minExtL = mtxToLocal.MultiplyPoint3x4(minExt);
        var maxExtL = mtxToLocal.MultiplyPoint3x4(maxExt);

        cameraViewRect = new Rect(minExt, new Vector2(Mathf.Abs(maxExtL.x - minExtL.x), Mathf.Abs(maxExtL.y - minExtL.y)));
        cameraViewExtents = cameraViewRect.size * 0.5f;
    }

    IEnumerator DoMoveTo(Vector2 dest) {
        float curTime = 0f;

        Vector2 start = transform.position;

        float dist = (dest - start).magnitude;
        float delay = dist / data.moveToSpeed;

        while(curTime < delay) {
            float t = Mathf.Clamp01(curTime / delay);

            var newPos = Vector2.Lerp(start, dest, data.moveToCurve.Evaluate(t));
            SetPosition(newPos); 

            yield return null;

            curTime += Time.deltaTime;
        }

        SetPosition(dest);

        mMoveToRout = null;
    }

    IEnumerator DoBoundsChange() {
        //ease out
        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);

        float curTime = 0f;
        float delay = data.boundsChangeDelay;

        Rect prevBoundsRect = mBoundsRect;

        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = easeFunc(curTime, delay, 0f, 0f);

            mBoundsRect.center = Vector2.Lerp(prevBoundsRect.center, mBoundsRectNext.center, t);
            mBoundsRect.size = Vector2.Lerp(prevBoundsRect.size, mBoundsRectNext.size, t);

            if(boundLocked)
                SetPosition(transform.position); //update clamp
        }

        mBoundsChangeRout = null;
    }
}
