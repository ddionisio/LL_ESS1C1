using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Disable this for the first checkpoint
/// </summary>
public class PlayerCheckpointTrigger : MonoBehaviour {
    public PlayerCheckpoint checkpoint;

    public GameObject untriggeredGO; //when trigger hasn't been activated
    public GameObject triggeredGO; //on trigger enter

    [Header("Signals")]
    public SignalPlayerCheckpoint signalTriggered;
        
    private Collider2D mTriggerColl;

    void Awake() {
        if(!checkpoint)
            checkpoint = GetComponentInParent<PlayerCheckpoint>();

        mTriggerColl = GetComponent<Collider2D>();

        if(untriggeredGO) untriggeredGO.SetActive(true);
        if(triggeredGO) triggeredGO.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(untriggeredGO) untriggeredGO.SetActive(false);
        if(triggeredGO) triggeredGO.SetActive(true);

        mTriggerColl.enabled = false; //no longer need to trigger

        if(signalTriggered)
            signalTriggered.Invoke(checkpoint);
    }
}
