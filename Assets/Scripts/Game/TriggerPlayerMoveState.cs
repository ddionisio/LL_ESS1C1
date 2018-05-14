using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Change player move state when triggered
/// </summary>
public class TriggerPlayerMoveState : MonoBehaviour {
    public bool isOnce = true;

    [Header("Signals")]
    public M8.Signal signalReset;

    [Header("States")]
    public bool isMoveSpeedLimit = true;
    public bool isMoveActive = true;

    private Collider2D mColl;

    public void ResetState() {
        mColl.enabled = true;
    }

    void OnDestroy() {
        if(signalReset) signalReset.callback -= OnSignalReset;
    }

    void Awake() {
        mColl = GetComponent<Collider2D>();

        if(signalReset) signalReset.callback += OnSignalReset;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.GetComponent<Player>();

        player.isMoveActive = isMoveActive;
        player.isMoveSpeedLimit = isMoveSpeedLimit;

        if(isOnce)
            mColl.enabled = false;
    }

    void OnSignalReset() {
        ResetState();
    }
}
