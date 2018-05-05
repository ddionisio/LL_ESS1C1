using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : M8.EntityBase {
    public PlayerData playerData;

    public Vector2 moveDir { get; set; }
    public float movePower { get; set; }

    /// <summary>
    /// Apply current move power towards move dir, this will set player state to move
    /// </summary>
    public void Move() {
        
    }

    protected override void OnDespawned() {
        //reset stuff here
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
    }

    protected override void StateChanged() {
        var prevEntityState = (EntityState)prevState;
        var curEntityState = (EntityState)state;

        switch(curEntityState) {
            case EntityState.Invalid:
                break;

            case EntityState.Spawn:
                break;

            case EntityState.PlayerIdle:
                break;

            case EntityState.PlayerLock:
                break;

            case EntityState.PlayerMove:
                break;

            case EntityState.Victory:
                break;
        }

        playerData.InvokeStateSignal(curEntityState);
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
