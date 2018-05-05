using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController {

    public static void LoadCurrentProgressScene() {
        var nextScene = GameData.instance.GetSceneFromCurrentProgress(LoLManager.instance.curProgress);

        nextScene.Load();
    }

    public static void ProgressAndLoadNextScene() {
        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        LoadCurrentProgressScene();
    }

    public static void Complete() {
        LoLManager.instance.Complete();
    }
}
