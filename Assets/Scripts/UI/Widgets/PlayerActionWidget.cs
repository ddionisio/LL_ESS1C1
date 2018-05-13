using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionWidget : MonoBehaviour {

    [Header("Signals")]
    public SignalBool signalGameActiveUpdate;
    public M8.Signal signalPlayerLaunchReady;
    public SignalBool signalPlayerCanJumpUpdate;

    private Selectable mSelectable;

    private bool mIsLaunch;

    public void Click() {
        Player player = GameMapController.instance.player;

        if(mIsLaunch) {
            mSelectable.interactable = false;
            mIsLaunch = false;

            //launch player
            player.Move();
        }
        else {
            //explode
            player.Jump();
        }
    }

    void OnDestroy() {
        if(signalGameActiveUpdate) signalGameActiveUpdate.callback -= OnSignalGameActiveUpdate;
        if(signalPlayerLaunchReady) signalPlayerLaunchReady.callback -= OnSignalPlayerIdle;
        if(signalPlayerCanJumpUpdate) signalPlayerCanJumpUpdate.callback -= OnSignalPlayerCanJumpUpdate;
    }

    void Awake() {
        mSelectable = GetComponent<Selectable>();
        mSelectable.interactable = false;

        if(signalGameActiveUpdate) signalGameActiveUpdate.callback += OnSignalGameActiveUpdate;
        if(signalPlayerLaunchReady) signalPlayerLaunchReady.callback += OnSignalPlayerIdle;
        if(signalPlayerCanJumpUpdate) signalPlayerCanJumpUpdate.callback += OnSignalPlayerCanJumpUpdate;
    }

    void OnSignalGameActiveUpdate(bool active) {
        if(active) {
            //setup initial state
            mSelectable.interactable = false;
            mIsLaunch = false;
        }
    }

    void OnSignalPlayerIdle() {
        //launch mode
        mIsLaunch = true;
        mSelectable.interactable = true;
    }

    void OnSignalPlayerCanJumpUpdate(bool canExplode) {
        bool isInteractible = mIsLaunch || canExplode;

        mSelectable.interactable = isInteractible;
    }
}
