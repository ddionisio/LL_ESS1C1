using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFlipMoveDirWidget : MonoBehaviour {
    public Transform directionRoot;

    [Header("Signals")]
    public SignalBool signalGameActiveUpdate;
    public M8.Signal signalPlayerLaunchReady;
    public M8.Signal signalPlayerMove;

    private Selectable mSelectable;
    private bool mIsInteractive;

    public void Click() {
        var player = GameMapController.instance.player;

        player.FlipGroundMoveDir();
    }

    void OnDestroy() {
        if(signalGameActiveUpdate) signalGameActiveUpdate.callback -= OnSignalGameActiveUpdate;
        if(signalPlayerLaunchReady) signalPlayerLaunchReady.callback -= OnSignalPlayerLaunchReady;
        if(signalPlayerMove) signalPlayerMove.callback -= OnSignalPlayerMove;
    }

    void Awake() {
        mSelectable = GetComponent<Selectable>();

        SetInteractive(false);

        if(signalGameActiveUpdate) signalGameActiveUpdate.callback += OnSignalGameActiveUpdate;
        if(signalPlayerLaunchReady) signalPlayerLaunchReady.callback += OnSignalPlayerLaunchReady;
        if(signalPlayerMove) signalPlayerMove.callback += OnSignalPlayerMove;
    }

    void Update() {
        if(!mIsInteractive)
            return;

        var player = GameMapController.instance.player;

        //disable if we are no longer moving
        if(player.state != (int)EntityState.PlayerMove) {
            SetInteractive(false);
            return;
        }

        //update direction display
        UpdateDirectionDisplay(GameMapController.instance.player);
    }

    void OnSignalGameActiveUpdate(bool active) {
        if(active) {
            //reset state
            SetInteractive(false);
        }
    }

    void OnSignalPlayerLaunchReady() {
        //update direction display
        UpdateDirectionDisplay(GameMapController.instance.player);
    }

    void OnSignalPlayerMove() {
        SetInteractive(true);
    }

    private void UpdateDirectionDisplay(Player player) {
        if(directionRoot) {
            Vector3 s = directionRoot.localScale;

            s.x = player.groundMoveDir.x;

            directionRoot.localScale = s;
        }
    }

    private void SetInteractive(bool interactive) {
        mIsInteractive = interactive;
        mSelectable.interactable = interactive;
    }
}
