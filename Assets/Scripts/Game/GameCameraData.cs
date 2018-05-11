using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameCameraData", menuName = "Game/Game Camera Data")]
public class GameCameraData : ScriptableObject {
    [Header("Bounds Settings")]
    public float boundsChangeDelay = 0.3f;

    [Header("Move Settings")]
    public AnimationCurve moveToCurve;
    public float moveToSpeed = 10f;
}
