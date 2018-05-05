using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "playerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject {
    [Header("Attributes")]

    public float powerMax;

    [M8.RangeMinMax(0f, 1f)]
    public M8.RangeFloat powerScaleRange;

    [Header("Display")]

    public float pullLength = 10f;
    public int pullStepCount = 5;
}
