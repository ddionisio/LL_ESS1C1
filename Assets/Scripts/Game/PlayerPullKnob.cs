using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerPullKnob : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField]
    Player _player;

    [Header("Signals")]
    public M8.Signal lockSignal;
    public M8.Signal unlockSignal;

    [Header("Display")]
    public GameObject indicatorGO;

    private bool mIsLocked;
    private bool mIsDragging;

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            ResetDrag();            
        }
    }

    void OnDestroy() {
        if(lockSignal) lockSignal.callback -= OnSignalLock;
        if(unlockSignal) unlockSignal.callback -= OnSignalUnlock;
    }

    void Awake() {
        if(!_player)
            _player = GetComponentInParent<Player>();

        if(lockSignal) lockSignal.callback += OnSignalLock;
        if(unlockSignal) unlockSignal.callback += OnSignalUnlock;
    }

    void OnSignalLock() {
        mIsLocked = true;
        mIsDragging = false;

        if(indicatorGO) indicatorGO.SetActive(false);
    }

    void OnSignalUnlock() {
        mIsLocked = false;

        if(!mIsDragging) {
            if(indicatorGO) indicatorGO.SetActive(true);
        }
    }

    void UpdatePosition(Vector2 newPos) {

    }

    void ResetDrag() {
        mIsDragging = false;

        transform.localPosition = Vector3.zero;

        if(indicatorGO) indicatorGO.SetActive(!mIsLocked);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(mIsLocked)
            return;

        mIsDragging = true;

        if(indicatorGO) indicatorGO.SetActive(false);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(mIsLocked || !mIsDragging)
            return;

        var cam = eventData.pressEventCamera;

        Vector2 curPos = cam.ScreenToWorldPoint(eventData.position);

        //position ourself to current position with restriction
        transform.position = curPos;

        //update player direction

        //Vector2 delta = (curPos - mDragLastPos) * dragScale;

        //mDragLastPos = curPos;

        //_gameCamera.StopMoveTo(); //cancel any move to's
        //_gameCamera.SetPosition(_gameCamera.position - delta);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(mIsDragging) {
            mIsDragging = false;

            var cam = eventData.pressEventCamera;

            Vector2 curPos = cam.ScreenToWorldPoint(eventData.position);

            //move player
        }
    }
}
