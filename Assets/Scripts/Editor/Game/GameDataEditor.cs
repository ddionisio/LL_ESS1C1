using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameData))]
public class GameDataEditor : Editor {
    private string[] mGameLevelSceneLabels;

    private M8.SceneAssetPath[] mScenes;
    private string[] mSceneNames;

    void OnEnable() {
        mGameLevelSceneLabels = System.Enum.GetNames(typeof(GameData.LevelScene));

        var scenes = EditorBuildSettings.scenes;

        mScenes = new M8.SceneAssetPath[scenes.Length];
        mSceneNames = new string[scenes.Length];

        for(int i = 0; i < scenes.Length; i++) {
            var name = M8.SceneAssetPath.LoadableName(scenes[i].path);
            var path = scenes[i].path;

            mScenes[i] = new M8.SceneAssetPath() { name = name, path = path };
            mSceneNames[i] = name;
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        //
        var gameData = target as GameData;

        if(gameData.levels == null) {
            gameData.levels = new GameData.LevelData[0];
            EditorUtility.SetDirty(gameData);
            return;
        }

        //select scenes
        M8.EditorExt.Utility.DrawSeparator();

        for(int i = 0; i < gameData.levels.Length; i++) {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Level " + (i+1));

            var level = gameData.levels[i];

            if(level.scenes == null || level.scenes.Length == 0) {
                //fill default scenes
                level.scenes = CreateDefaultScenes();
                EditorUtility.SetDirty(gameData);
            }
            else if(level.scenes.Length != mGameLevelSceneLabels.Length) {
                System.Array.Resize(ref level.scenes, mGameLevelSceneLabels.Length);
                EditorUtility.SetDirty(gameData);
            }
            else { //scene list
                for(int sceneInd = 0; sceneInd < level.scenes.Length; sceneInd++) {

                    M8.SceneAssetPath newScene;
                    if(SelectScene(mGameLevelSceneLabels[sceneInd], level.scenes[sceneInd], out newScene)) {
                        Undo.RecordObject(gameData, string.Format("Select New Scene For Level {0} {1}", i+1, mGameLevelSceneLabels[sceneInd]));

                        level.scenes[sceneInd] = newScene;
                    }
                }
            }

            //controls
            GUILayout.BeginHorizontal(GUI.skin.box);

            if(GUILayout.Button("Up")) {
                if(i > 0) {
                    Undo.RecordObject(gameData, "Move Level Up");

                    var prev = gameData.levels[i - 1];
                    gameData.levels[i - 1] = gameData.levels[i];
                    gameData.levels[i] = prev;
                }
            }

            if(GUILayout.Button("Dn")) {
                if(i < gameData.levels.Length - 1) {
                    Undo.RecordObject(gameData, "Move Level Down");

                    var prev = gameData.levels[i + 1];
                    gameData.levels[i + 1] = gameData.levels[i];
                    gameData.levels[i] = prev;
                }
            }

            bool isAddOrRemoved = false;

            if(GUILayout.Button("+")) {
                Undo.RecordObject(gameData, "Insert New Level");
                M8.ArrayUtil.InsertAfter(ref gameData.levels, i, CreateNewLevel());
                isAddOrRemoved = true;
            }

            if(GUILayout.Button("-")) {
                Undo.RecordObject(gameData, "Remove Level");
                gameData.levels = M8.ArrayUtil.RemoveAt(gameData.levels, i);
                isAddOrRemoved = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            
            if(isAddOrRemoved)
                break;
        }

        //add new
        if(GUILayout.Button("Add New Level")) {
            Undo.RecordObject(gameData, "Add New Level");

            System.Array.Resize(ref gameData.levels, gameData.levels.Length + 1);

            gameData.levels[gameData.levels.Length - 1] = CreateNewLevel();
        }
    }

    private GameData.LevelData CreateNewLevel() {
        var newLevel = new GameData.LevelData();

        //fill default scenes
        newLevel.scenes = CreateDefaultScenes();

        return newLevel;
    }

    private M8.SceneAssetPath[] CreateDefaultScenes() {
        var scenes = new M8.SceneAssetPath[mGameLevelSceneLabels.Length];

        if(mScenes.Length > 0) {
            for(int sceneInd = 0; sceneInd < mGameLevelSceneLabels.Length; sceneInd++)
                scenes[sceneInd] = mScenes[0];
        }

        return scenes;
    }

    private bool SelectScene(string label, M8.SceneAssetPath curScene, out M8.SceneAssetPath newScene) {
        int curInd = -1;
        for(int i = 0; i < mSceneNames.Length; i++) {
            if(curScene.name == mSceneNames[i]) {
                curInd = i;
                break;
            }
        }

        if(curInd == -1) {
            newScene = mScenes[0];
            return true;
        }

        int newInd = EditorGUILayout.Popup(label, curInd, mSceneNames);
        if(newInd != curInd) {
            newScene = mScenes[newInd];
            return true;
        }

        newScene = curScene;
        return false;
    }
}