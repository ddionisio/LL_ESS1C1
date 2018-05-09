using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure to create this in Resources with name: gameData
/// </summary>
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    public const int progressPerLevel = 3; //for now: per scene on each level

    [System.Serializable]
    public struct LevelData {
        public string name; //beautify inspector

        [Header("Scenes")]
        public M8.SceneAssetPath preScene;
        public M8.SceneAssetPath scene;
        public M8.SceneAssetPath postScene;
    }

    public M8.SceneAssetPath introScene;

    public LevelData[] levels;

    public int curLevelIndex { get; private set; }

    /// <summary>
    /// Called in start scene
    /// </summary>
    public void Begin() {
        if(LoLManager.instance.curProgress == 0)
            introScene.Load();
        else {
            LoLMusicPlaylist.instance.Play();
            Current();
        }
    }

    /// <summary>
    /// Load current level-scene from current progress
    /// </summary>
    public void Current() {
        var nextScene = GetSceneFromCurrentProgress(LoLManager.instance.curProgress);
        nextScene.Load();
    }

    /// <summary>
    /// Update progress, go to next level-scene
    /// </summary>
    public void Progress() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
        Current();
    }

    protected override void OnInstanceInit() {
        
    }

    private M8.SceneAssetPath GetSceneFromCurrentProgress(int progress) {

        curLevelIndex = Mathf.Clamp(progress / progressPerLevel, 0, levels.Length - 1);

        int sceneIndex = progress % progressPerLevel;

        switch(sceneIndex) {
            case 0:
                return levels[curLevelIndex].preScene;
            case 1:
                return levels[curLevelIndex].scene;
            case 2:
                return levels[curLevelIndex].postScene;
        }

        return M8.SceneManager.instance.rootScene;
    }
}
