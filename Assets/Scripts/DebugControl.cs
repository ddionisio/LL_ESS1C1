using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure to create this in Resources with name: debugControl
/// </summary>
[CreateAssetMenu(fileName = "debugControl", menuName = "Game/Debug Control")]
public class DebugControl : M8.SingletonScriptableObject<DebugControl> {
    public int levelIndex;
    public bool collectionsUnlocked;
}
