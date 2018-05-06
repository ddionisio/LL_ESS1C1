using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {
    public GameMapData mapData;

    public Player player { get; private set; }
    public GameCamera gameCamera { get; private set; }

    protected override void OnInstanceInit() {
        //grab relevant stuff from the scene
        var playerGO = GameObject.FindGameObjectWithTag(Tags.player);
        if(playerGO)
            player = playerGO.GetComponent<Player>();

        var gameCameraGO = GameObject.FindGameObjectWithTag(Tags.gameCamera);
        if(gameCameraGO)
            gameCamera = gameCameraGO.GetComponent<GameCamera>();
    }
}
