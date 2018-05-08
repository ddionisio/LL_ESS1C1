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
        int prevPowerStep = _player.explodeCount;

        Vector2 playerPos = _player.physicsBody.position;

        Vector2 delta = playerPos - newPos;

        float dist = delta.magnitude;

        if(dist > 0f) {
            Vector2 dir = delta / dist;

            //update player direction
            _player.moveDir = dir;
                        
            //limit position distance
            float limitDist = _player.data.pullDistanceLimit + _player.physicsCircleCollider.radius;
            if(dist > limitDist) {
                newPos = playerPos - dir * limitDist;
            }

            pullDistance = dist - _player.physicsCircleCollider.radius;
            if(pullDistance > 0f)
                _player.explodeCount = Mathf.RoundToInt(Mathf.Lerp(0f, _player.data.pullStepCount, pullDistance / _player.data.pullDistanceLimit));
            else
                _player.explodeCount = 0;
        }
        else {
            newPos = playerPos;

            _player.explodeCount = 0;
        }

        transform.position = newPos;

        if(pullActiveGO) pullActiveGO.SetActive(_player.explodeCount > 0);
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
            if(_player.explodeCount > 0)
                _player.Move();
        }
    }
}
