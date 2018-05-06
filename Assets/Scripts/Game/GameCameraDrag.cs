using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameCameraDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField]
    GameCamera _gameCamera;

    [Header("Settings")]
    public float dragScale = 0.3f;

    [Header("Signals")]
    public M8.Signal lockSignal;
    public M8.Signal unlockSignal;

    private bool mIsLocked;
    private bool mIsDragging;

    private Vector2 mDragLastPos;

    void OnApplicationFocus(bool focus) {
        if(!focus)
            mIsDragging = false;
    }

    void OnDestroy() {
        if(lockSignal) lockSignal.callback -= OnSignalLock;
        if(unlockSignal) unlockSignal.callback -= OnSignalUnlock;
    }

    void Awake() {
        if(!_gameCamera)
            _gameCamera = GetComponentInParent<GameCamera>();

        if(lockSignal) lockSignal.callback += OnSignalLock;
        if(unlockSignal) unlockSignal.callback += OnSignalUnlock;
    }

    void OnSignalLock() {
        mIsLocked = true;
        mIsDragging = false;
    }

    void OnSignalUnlock() {
        mIsLocked = false;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(mIsLocked)
            return;

        mDragLastPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        mIsDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(mIsLocked || !mIsDragging)
            return;

        //TODO: fancy acceleration/decceleration

        Vector2 curPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);

        Vector2 delta = (curPos - mDragLastPos) * dragScale;

        mDragLastPos = curPos;

        _gameCamera.StopMoveTo(); //cancel any move to's
        _gameCamera.SetPosition(_gameCamera.position - delta);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        mIsDragging = false;
    }
}