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
    public GameObject indicatorGO; //indicator display that pulling is ready
    public GameObject pullActiveGO; //pull strength display while pulling
    public GameObject knobGO; //knob display while pulling

    public bool isLocked {
        get { return mIsLocked; }
        set {
            if(mIsLocked != value) {
                mIsLocked = value;

                if(mColl) mColl.enabled = !mIsLocked;
            }
        }
    }

    public float pullDistance { get; private set; }
    public int powerStep { get; private set; }

    private bool mIsLocked;
    private Collider2D mColl;

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

        mColl = GetComponent<Collider2D>();
        if(mColl)
            mIsLocked = mColl.enabled;

        if(lockSignal) lockSignal.callback += OnSignalLock;
        if(unlockSignal) unlockSignal.callback += OnSignalUnlock;
    }

    void OnSignalLock() {
        isLocked = true;

        ResetDrag();
    }

    void OnSignalUnlock() {
        isLocked = false;

        //refresh position
        transform.position = _player.physicsBody.position;

        if(indicatorGO) indicatorGO.SetActive(!mIsDragging);
    }

    void UpdatePosition(Vector2 newPos) {
        int prevPowerStep = powerStep;

        Vector2 playerPos = _player.physicsBody.position;

        Vector2 delta = playerPos - newPos;

        pullDistance = delta.magnitude;
                
        if(pullDistance > 0f) {
            Vector2 dir = delta / pullDistance;

            //update player direction
            _player.moveDir = dir;

            //limit distance
            if(pullDistance > _player.data.pullDistanceLimit) {
                pullDistance = _player.data.pullDistanceLimit;

                newPos = playerPos - dir * _player.data.pullDistanceLimit;

                powerStep = _player.data.pullStepCount;
            }
            else
                powerStep = Mathf.RoundToInt((pullDistance / _player.data.pullDistanceLimit) * _player.data.pullStepCount);
        }
        else
            powerStep = 0;

        transform.position = newPos;

        //compute power
        if(powerStep != prevPowerStep) {
            float powerT = (float)powerStep / _player.data.pullStepCount;

            _player.movePower = _player.data.GetPower(powerT);
        }
    }

    void ResetDrag() {
        mIsDragging = false;

        //refresh position
        transform.position = _player.physicsBody.position;

        if(indicatorGO) indicatorGO.SetActive(!isLocked);
        if(pullActiveGO) pullActiveGO.SetActive(false);
        if(knobGO) knobGO.SetActive(false);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(isLocked)
            return;

        mIsDragging = true;

        if(indicatorGO) indicatorGO.SetActive(false);
        if(pullActiveGO) pullActiveGO.SetActive(true);
        if(knobGO) knobGO.SetActive(true);

        //focus camera to player
        Vector2 playerPos = _player.physicsBody.position;

        var gameCam = GameMapController.instance.gameCamera;
        gameCam.MoveTo(playerPos);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(isLocked || !mIsDragging)
            return;

        var cam = eventData.pressEventCamera;

        Vector2 curPos = cam.ScreenToWorldPoint(eventData.position);

        UpdatePosition(curPos);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(mIsDragging) {
            var cam = eventData.pressEventCamera;

            Vector2 curPos = cam.ScreenToWorldPoint(eventData.position);

            UpdatePosition(curPos);

            ResetDrag();

            //move player if we have power, otherwise just cancel
            if(powerStep > 0)
                _player.Move();
        }
    }
}
