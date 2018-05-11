using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Disable this for the first checkpoint
/// </summary>
public class PlayerCheckpointTrigger : MonoBehaviour {
    public PlayerCheckpoint checkpoint;

    void Awake() {
        if(!checkpoint)
            checkpoint = GetComponentInParent<PlayerCheckpoint>();
    }
}
