using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "playerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject {
    [Header("Attributes")]
    public LayerMask explodeCastLayerMask;
    public float explodeCastDistance = 0.5f;
    public float explodeCooldown = 0.5f;

    [Header("Physics")]
    public float density = 0.5f;

    public float drag = 0.5f;
    public float dragAngular = 0.1f;

    public float restSpeedThreshold = 0.1f;
    public float groundSlopeAngleLimit = 50f;

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
