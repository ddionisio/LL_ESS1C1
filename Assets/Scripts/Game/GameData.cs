﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure to create this in Resources with name: gameData
/// </summary>
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {    
    public const int progressPerLevel = 3; //includes: intro, play, end

    [System.Serializable]
    public struct LevelUnlockData {
        public CollectionData data;
        public bool isDisplayed; //show on result, before quiz
    }

    [System.Serializable]
    public class LevelData {
        public M8.SceneAssetPath scene;
        public string modalLevelEnd; //which modal to use post-level

        public LevelUnlockData[] collectionUnlocks;

        public int notebookInitialIndex;
    }

    [Header("Scenes")]
    public M8.SceneAssetPath introScene;
    public M8.SceneAssetPath endScene;
    public M8.SceneAssetPath levelIntroScene;
    public M8.SceneAssetPath levelEndScene;

    [Header("Data")]
    public int quizBonusPoints = 1000; //points per answer
    public int checkpointPoints = 250;
    public int checkpointNoDeathPoints = 500;

    [Header("Levels")]
    public LevelData[] levels;

    public bool isGameStarted { get; private set; } //true: we got through start normally, false: debug
    public int curLevelIndex { get; private set; }

    private HashSet<string> mCollectionUnlocks = new HashSet<string>();

    private int[] mLevelScores;
    private int[] mLevelMistakeCounts;
    
    /// <summary>
    /// Called in start scene
    /// </summary>
    public void Begin() {
        isGameStarted = true;

        if(LoLManager.instance.curProgress == 0)
            introScene.Load();
        else {
            LoLMusicPlaylist.instance.Play();
            Current();
        }
    }

    /// <summary>
    /// Update level index based on current progress, and load scene
    /// </summary>
    public void Current() {
        int progress = LoLManager.instance.curProgress;

        UpdateLevelIndexFromProgress(progress);

        if(curLevelIndex < levels.Length) {
            int sceneIndex = progress % progressPerLevel;
            switch(sceneIndex) {
                case 0:
                    levelIntroScene.Load();
                    break;
                case 1:
                    levels[curLevelIndex].scene.Load();
                    break;
                case 2:
                    levelEndScene.Load();
                    break;
                default:
                    M8.SceneManager.instance.Reload();
                    break;
            }
        }
        else
            endScene.Load();
    }

    /// <summary>
    /// Update progress, go to next level-scene
    /// </summary>
    public void Progress() {
        var curScene = M8.SceneManager.instance.curScene;

        if(isGameStarted) {
            //we are in intro, proceed
            if(curScene.name == introScene.name) {
                Current();
            }
            //ending if we are already at max
            else if(LoLManager.instance.curProgress == LoLManager.instance.progressMax)
                endScene.Load();
            //proceed to next progress
            else {
                LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
                Current();
            }
        }
        else { //debug
            if(curScene.name == introScene.name) {
                //play first level intro
                curLevelIndex = 0;
                                
                levelIntroScene.Load();
            }
            else if(curScene.name == levelIntroScene.name) {
                //play level
                levels[curLevelIndex].scene.Load();
            }
            else if(curScene.name == levelEndScene.name) {
                //go to next level intro
                if(curLevelIndex < levels.Length - 1) {
                    curLevelIndex++;
                    levelIntroScene.Load();
                }
                else
                    endScene.Load(); //completed
            }
            else {
                //check levels and load level ending
                int levelFoundInd = -1;

                for(int i = 0; i < levels.Length; i++) {
                    if(curScene.name == levels[i].scene.name) {
                        levelFoundInd = i;
                        break;
                    }
                }

                if(levelFoundInd != -1) {
                    curLevelIndex = levelFoundInd;
                    levelEndScene.Load();
                }
                else
                    M8.SceneManager.instance.Reload(); //not found, just reload current
            }
        }
    }

    public void CollectionUnlock(string collectionName) {
        if(!mCollectionUnlocks.Contains(collectionName))
            mCollectionUnlocks.Add(collectionName);
    }

    public bool CollectionIsUnlocked(string collectionName) {
        return mCollectionUnlocks.Contains(collectionName) || DebugControl.instance.collectionsUnlocked;
    }

    public int GetLevelScore(int index) {
        return mLevelScores[index];
    }

    public int GetLevelMistakeCount(int index) {
        return mLevelMistakeCounts[index];
    }

    public void SetLevelScore(int index, int score, int mistakeCount) {
        mLevelScores[index] = score;
        mLevelMistakeCounts[index] = mistakeCount;
    }

    public void SetCurrentLevelScore(int score, int mistakeCount) {
        SetLevelScore(curLevelIndex, score, mistakeCount);
    }

    protected override void OnInstanceInit() {
        //compute max progress
        if(LoLManager.isInstantiated) {            
            LoLManager.instance.progressMax = levels.Length * progressPerLevel;
        }
        else
            curLevelIndex = DebugControl.instance.levelIndex;

        mCollectionUnlocks.Clear();

        mLevelScores = new int[levels.Length];
        mLevelMistakeCounts = new int[levels.Length];
    }

    private void UpdateLevelIndexFromProgress(int progress) {
        curLevelIndex = Mathf.Clamp(progress / progressPerLevel, 0, levels.Length);        
    }
}
