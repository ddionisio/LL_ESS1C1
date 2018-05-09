using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure to create this in Resources with name: gameData
/// </summary>
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {    
    public const int progressPerLevel = 3; //for now: per scene on each level

    public enum LevelScene {
        Intro,
        Play,
        Post
    }

    [System.Serializable]
    public class LevelData {
        public M8.SceneAssetPath[] scenes;
    }

    public M8.SceneAssetPath introScene;

    [HideInInspector]
    public LevelData[] levels;

    public int curLevelIndex { get; private set; }

    private bool mIsBeginCalled; //true: we got through start normally, false: debug

    /// <summary>
    /// Called in start scene
    /// </summary>
    public void Begin() {
        mIsBeginCalled = true;

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
        if(mIsBeginCalled) {
            //complete if we are already at max
            if(LoLManager.instance.curProgress == LoLManager.instance.progressMax)
                LoLManager.instance.Complete();
            else {
                LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
                Current();
            }
        }
        else {
            var curScene = M8.SceneManager.instance.curScene;

            int nextLevelInd = -1;
            int nextSceneInd = -1;

            //figure out which progress we are in based on current scene
            for(int levelInd = 0; levelInd < levels.Length; levelInd++) {
                var level = levels[levelInd];

                for(int sceneInd = 0; sceneInd < level.scenes.Length; sceneInd++) {
                    if(level.scenes[sceneInd].name == curScene.name) {
                        //proceed to next level
                        if(sceneInd == level.scenes.Length - 1) {
                            nextLevelInd = levelInd + 1;
                            nextSceneInd = 0;
                        }
                        //proceed to next scene
                        else {
                            nextLevelInd = levelInd;
                            nextSceneInd++;
                        }
                        break;
                    }
                }

                if(nextLevelInd != -1)
                    break;
            }

            if(nextLevelInd == -1) {
                //no match, just reload level
                M8.SceneManager.instance.Reload();
            }
            else if(nextLevelInd >= levels.Length) {
                //complete
                Debug.Log("Finish");
            }
            else {
                curLevelIndex = nextLevelInd;

                levels[nextLevelInd].scenes[nextSceneInd].Load();
            }
        }
    }

    protected override void OnInstanceInit() {
        //compute max progress
        if(LoLManager.isInstantiated) {
            int progressCount = 0;

            for(int levelInd = 0; levelInd < levels.Length; levelInd++) {
                progressCount += levels[levelInd].scenes.Length;                                
            }

            LoLManager.instance.progressMax = progressCount;
        }
    }

    private M8.SceneAssetPath GetSceneFromCurrentProgress(int progress) {

        curLevelIndex = Mathf.Clamp(progress / progressPerLevel, 0, levels.Length - 1);

        int sceneIndex = progress % levels[curLevelIndex].scenes.Length;

        return levels[curLevelIndex].scenes[sceneIndex];
    }
}
