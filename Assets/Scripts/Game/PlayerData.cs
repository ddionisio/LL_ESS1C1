using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "playerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject {
    [Header("Attributes")]
    public float jumpUplift = 10f;
    public float jumpPower = 50f;
    public float jumpCooldown = 0.1f;
    public LayerMask jumpCastLayerMask;
    public float jumpCastDistance = 0.1f;

    public float moveForce = 20f;
    public float moveSpeedLimit = 10f;

    [Header("Physics")]
    public float density = 0.5f;

    public float drag = 0.5f;
    public float dragAngular = 0.1f;
    
    public float groundSlopeAngleLimit = 50f;
    public float aboveAngleLimit = 145.0f; //determines collideflag above, should be > 90, around 140'ish

    [Header("Camera")]
    public float cameraTime = 0.15f;

    [Header("State Signals")]
    public EntityStateSignalInvoke[] stateSignalInvokes;

    [Header("Display")]

    public float pullDistanceLimit = 2f;
    public int pullStepCount = 5;

    public void InvokeStateSignal(EntityState state) {
        for(int i = 0; i < stateSignalInvokes.Length; i++) {
            if(stateSignalInvokes[i].state == state) {
                stateSignalInvokes[i].Invoke();
                break;
            }
        }
    }
}
