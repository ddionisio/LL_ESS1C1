using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.ResourcePath("gameData")]
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {

    protected override void OnInstanceInit() {
        
    }
}
