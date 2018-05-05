using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameCameraData", menuName = "Game/Game Camera Data")]
public class GameCameraData : ScriptableObject {
    [M8.TagSelector]
    public string boundsTag;

    [Header("Move Settings")]
    public AnimationCurve moveToCurve;
    public float moveToSpeed = 10f;

    public GameBounds2D GetBoundsFromScene() {
        var go = GameObject.FindGameObjectWithTag(boundsTag);
        if(!go)
            return null;

        var gameBounds2D = go.GetComponent<GameBounds2D>();
        return gameBounds2D;
    }
}
