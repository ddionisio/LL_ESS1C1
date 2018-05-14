using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class ObstacleObject : EntityBase {
    public GameObject displayGO;

    public float delay;

    [Header("Signals")]
    public Signal signalRelease; //free up this obstacle

    private Rigidbody2D mBody;

    private bool mIsSignalsActive = false;

    protected override void StateChanged() {
        StopAllCoroutines();

        var entState = (EntityState)state;

        switch(entState) {
            case EntityState.Death:
                SetSignals(false);

                if(mBody) mBody.simulated = false;

                StartCoroutine(DoDeath());
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        StopAllCoroutines();

        SetSignals(false);
                
        ResetState();
    }

    protected override void OnSpawned(GenericParams parms) {
        //populate data/state for ai, player control, etc.
        SetSignals(true);

        //start ai, player control, etc
        StartCoroutine(DoSpawn());
    }

    protected override void OnDestroy() {
        //dealloc here
        SetSignals(false);

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mBody = GetComponent<Rigidbody2D>();

        ResetState();
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void OnSignalRelease() {
        Release();
    }

    IEnumerator DoSpawn() {
        if(delay > 0f)
            yield return new WaitForSeconds(delay);

        if(displayGO) displayGO.SetActive(true);

        //animation

        if(mBody) mBody.simulated = true;
    }

    IEnumerator DoDeath() {
        //animation
        yield return null;

        Release();
    }

    private void ResetState() {
        if(displayGO) displayGO.SetActive(false);
        if(mBody) mBody.simulated = false;
    }

    private void SetSignals(bool active) {
        if(mIsSignalsActive != active) {
            mIsSignalsActive = active;

            if(mIsSignalsActive) {
                if(signalRelease) signalRelease.callback += OnSignalRelease;
            }
            else {
                if(signalRelease) signalRelease.callback -= OnSignalRelease;
            }
        }
    }
}