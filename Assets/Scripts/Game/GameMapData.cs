using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "mapData", menuName = "Game/Map Data")]
public class GameMapData : ScriptableObject {
    [Header("Signal Listen")]
    public M8.Signal signalGoal;
    public M8.Signal signalDeath;
    public SignalPlayerCheckpoint signalPlayerCheckpoint;
}
