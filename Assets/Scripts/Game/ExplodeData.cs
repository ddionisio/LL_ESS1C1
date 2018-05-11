using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "explodeData", menuName = "Game/Explode Data")]
public class ExplodeData : ScriptableObject {
    public float radius = 1.0f;
    public float power = 50f;
    public float uplift;
    public ForceMode2D mode = ForceMode2D.Impulse;

    public LayerMask layerMask;

    public float delay = 0.1f;
}
