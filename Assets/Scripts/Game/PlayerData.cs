using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "playerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject {
    [Header("Attributes")]

    public float powerMax;

    [M8.RangeMinMax(0f, 1f)]
    public M8.RangeFloat powerScaleRange;

    [Header("State Signals")]
    public EntityStateSignalInvoke[] stateSignalInvokes;

    [Header("Display")]

    public float pullLength = 10f;
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
