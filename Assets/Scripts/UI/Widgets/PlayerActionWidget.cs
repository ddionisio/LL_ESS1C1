using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionWidget : MonoBehaviour {
    public Image icon;
    public Color iconDisableColor = Color.gray;

    [Header("Signals")]
    public SignalBool signalGameActiveUpdate;
    public M8.Signal signalPlayerLaunchReady;
    public SignalBool signalPlayerCanJumpUpdate;

    private Selectable mSelectable;

    private bool mIsLaunch;

    private Color mIconDefaultColor;

    public void Click() {
        Player player = GameMapController.instance.player;

        if(mIsLaunch) {
            SetInteractable(false);

            mIsLaunch = false;

            //launch player
            player.Launch();
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
        if(icon) mIconDefaultColor = icon.color;

        mSelectable = GetComponent<Selectable>();

        SetInteractable(false);

        if(signalGameActiveUpdate) signalGameActiveUpdate.callback += OnSignalGameActiveUpdate;
        if(signalPlayerLaunchReady) signalPlayerLaunchReady.callback += OnSignalPlayerIdle;
        if(signalPlayerCanJumpUpdate) signalPlayerCanJumpUpdate.callback += OnSignalPlayerCanJumpUpdate;
    }

    void OnSignalGameActiveUpdate(bool active) {
        if(active) {
            //setup initial state
            SetInteractable(false);

            mIsLaunch = false;
        }
    }

    void OnSignalPlayerIdle() {
        //launch mode
        mIsLaunch = true;

        SetInteractable(true);
    }

    void OnSignalPlayerCanJumpUpdate(bool canExplode) {
        bool isInteractible = mIsLaunch || canExplode;

        SetInteractable(isInteractible);
    }

    void SetInteractable(bool yes) {
        mSelectable.interactable = yes;

        if(icon) icon.color = yes ? mIconDefaultColor : iconDisableColor;
    }
}
