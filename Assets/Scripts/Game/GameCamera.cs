using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {
    public GameCameraData data;

    public GameBounds2D bounds { get; private set; }

    public M8.Camera2D camera2D { get; private set; }

    public Vector2 position { get { return transform.position; } }

    /// <summary>
    /// Local space
    /// </summary>
    public Rect cameraViewRect { get; private set; }
    public Vector2 cameraViewExtents { get; private set; }

    public bool isMoving { get { return mMoveToRout != null; } }

    private Coroutine mMoveToRout;

    public void MoveTo(Vector2 dest) {
        StopMoveTo();

        //clamp
        dest = bounds.Clamp(dest, cameraViewExtents);

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
        pos = bounds.Clamp(pos, cameraViewExtents);

        transform.position = pos;
    }

    public bool isVisible(Rect rect) {
        rect.center = transform.worldToLocalMatrix.MultiplyPoint3x4(rect.center);

        return cameraViewRect.Overlaps(rect);
    }
        
    void OnDisable() {
        StopMoveTo();
    }

    void Awake() {
        bounds = data.GetBoundsFromScene();

        camera2D = GetComponentInChildren<M8.Camera2D>();

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
}
