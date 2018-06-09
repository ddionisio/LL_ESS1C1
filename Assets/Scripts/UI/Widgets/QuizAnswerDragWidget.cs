using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuizAnswerDragWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public GameObject detailGO; //only active when in placed
    public GameObject correctGO;

    public float revertDelay = 0.3f;

    public int index { get; private set; }
    public bool isDragLocked { get; set; }

    public RectTransform rTransform { get; private set; }

    public bool isCorrect { get { return correctGO && correctGO.activeSelf; } set { if(correctGO) correctGO.SetActive(value); } }

    public event System.Action<QuizAnswerDragWidget, PointerEventData> dragCallback;
    public event System.Action<QuizAnswerDragWidget, PointerEventData> dragEndCallback;

    private Vector2 mOriginalPos;

    private Coroutine mRevertRout;
    private bool mIsDragging;

    public void Revert() {
        mIsDragging = false;

        if(mRevertRout != null)
            StopCoroutine(mRevertRout);

        mRevertRout = StartCoroutine(DoRevert());
    }

    public void Place() {
        isDragLocked = true;

        if(detailGO) detailGO.SetActive(true);
    }

    public void Init(int index, Vector2 originalPos) {
        this.index = index;

        isDragLocked = false;

        if(mRevertRout != null) {
            StopCoroutine(mRevertRout);
            mRevertRout = null;
        }

        transform.position = mOriginalPos = originalPos;

        rTransform = transform as RectTransform;

        if(detailGO) detailGO.SetActive(true);

        isCorrect = false;
    }

    void OnApplicationFocus(bool hasFocus) {
        if(!hasFocus) {
            if(mIsDragging)
                Revert();
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(isDragLocked || mRevertRout != null)
            return;

        if(detailGO) detailGO.SetActive(false);

        transform.SetAsLastSibling();

        mIsDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(isDragLocked || mRevertRout != null) {
            mIsDragging = false;
            return;
        }

        transform.position = eventData.position;

        if(dragCallback != null)
            dragCallback(this, eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(isDragLocked || mRevertRout != null) {
            mIsDragging = false;
            return;
        }

        if(!mIsDragging)
            return;

        mIsDragging = false;

        if(dragEndCallback != null)
            dragEndCallback(this, eventData);
    }

    IEnumerator DoRevert() {
        Vector2 startPos = transform.position;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        float curTime = 0f;

        while(curTime < revertDelay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = easeFunc(curTime, revertDelay, 0f, 0f);

            transform.position = Vector2.Lerp(startPos, mOriginalPos, t);
        }

        if(detailGO) detailGO.SetActive(true);

        mRevertRout = null;
    }
}
