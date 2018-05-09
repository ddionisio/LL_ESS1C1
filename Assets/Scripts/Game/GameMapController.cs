using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {
    public GameMapData mapData;

    [Header("Camera")]
    public GameBounds2D cameraBounds;

    [Header("Signal Listen")]
    public M8.Signal signalGoal;

    public Player player { get; private set; }
    public GameCamera gameCamera { get; private set; }

    public string curSceneName { get; private set; }

    protected override void OnInstanceInit() {
        curSceneName = M8.SceneManager.instance.curScene.name;

        //initialize hud stuff
        HUD.instance.ShowGame();

        //grab relevant stuff from the scene
        var playerGO = GameObject.FindGameObjectWithTag(Tags.player);
        if(playerGO)
            player = playerGO.GetComponent<Player>();

        var gameCameraGO = GameObject.FindGameObjectWithTag(Tags.gameCamera);
        if(gameCameraGO) {
            gameCamera = gameCameraGO.GetComponent<GameCamera>();

            //apply camera bound
            gameCamera.bounds = cameraBounds;
            gameCamera.boundLocked = true;
        }

        //hook up signals
        if(signalGoal) signalGoal.callback += OnSignalGoal;
    }

    protected override void OnInstanceDeinit() {
        if(signalGoal) signalGoal.callback -= OnSignalGoal;

        //clear out game spawns
        if(GameMapPool.isInstantiated)
            GameMapPool.instance.ReleaseAll();
    }

    void OnSignalGoal() {
        //show victory modal
        GameData.instance.Progress();
    }
}
