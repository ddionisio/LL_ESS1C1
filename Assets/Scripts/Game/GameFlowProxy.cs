using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper Behaviour to hookup calls to GameData's flow
/// </summary>
public class GameFlowProxy : MonoBehaviour {
    /// <summary>
    /// Called in start scene
    /// </summary>
    public void Begin() {
        GameData.instance.Begin();
    }

    public void Current() {
        GameData.instance.Current();
    }

    /// <summary>
    /// Go to next level
    /// </summary>
    public void Progress() {
        GameData.instance.Progress();
    }

    public void Complete() {
        LoLManager.instance.Complete();
    }
}
