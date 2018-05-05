using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {

    public Player player { get; private set; }
    public GameCamera gameCamera { get; private set; }

    protected override void OnInstanceInit() {
        //grab relevant stuff from the scene

    }
}
