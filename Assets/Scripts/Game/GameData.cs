using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure to create this in Resources with name: gameData
/// </summary>
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {

    protected override void OnInstanceInit() {
        
    }

    public M8.SceneAssetPath GetSceneFromCurrentProgress(int progress) {
        /*if(progress < scenes.Length)
            return scenes[progress];

        if(string.IsNullOrEmpty(endScene.name))
            return M8.SceneManager.instance.rootScene;

        return endScene;*/

        return M8.SceneManager.instance.rootScene;
    }
}
