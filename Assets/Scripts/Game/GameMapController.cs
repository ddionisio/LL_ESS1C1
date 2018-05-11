using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {
    public GameMapData mapData;

    [Header("Camera")]
    public GameBounds2D cameraBounds;

    [Header("Signal Listen")]
    public M8.Signal signalGoal;
    public M8.Signal signalDeath;

    public Player player { get; private set; }
    public GameCamera gameCamera { get; private set; }

    public string curSceneName { get; private set; }

    private PlayerCheckpoint[] mCheckpoints;
    private int mCurCheckpointInd;

    protected override void OnInstanceInit() {
        curSceneName = M8.SceneManager.instance.curScene.name;

        //initialize hud stuff
        HUD.instance.isGameActive = true;

        //grab relevant stuff from the scene

        //player
        var playerGO = GameObject.FindGameObjectWithTag(Tags.player);
        if(playerGO)
            player = playerGO.GetComponent<Player>();

        //camera
        var gameCameraGO = GameObject.FindGameObjectWithTag(Tags.gameCamera);
        if(gameCameraGO) {
            gameCamera = gameCameraGO.GetComponent<GameCamera>();

            //apply camera bound
            gameCamera.bounds = cameraBounds;
            gameCamera.boundLocked = true;
        }

        //checkpoints
        var checkpointGOs = GameObject.FindGameObjectsWithTag(Tags.gameCheckpoint);

        mCheckpoints = new PlayerCheckpoint[checkpointGOs.Length];

        for(int i = 0; i < checkpointGOs.Length; i++)
            mCheckpoints[i] = checkpointGOs[i].GetComponent<PlayerCheckpoint>();

        System.Array.Sort(mCheckpoints, delegate (PlayerCheckpoint a, PlayerCheckpoint b) {
            if(!a) return 1;
            if(!b) return -1;
            return a.name.CompareTo(b);
        });

        //hook up signals
        if(signalGoal) signalGoal.callback += OnSignalGoal;
        if(signalDeath) signalDeath.callback += OnSignalDeath;
    }

    protected override void OnInstanceDeinit() {
        if(signalGoal) signalGoal.callback -= OnSignalGoal;
        if(signalDeath) signalDeath.callback -= OnSignalDeath;

        //clear out game spawns
        if(GameMapPool.isInstantiated)
            GameMapPool.instance.ReleaseAll();
    }

    IEnumerator Start() {
        //wait for transition
        yield return null;

        //setup initial game state

        mCurCheckpointInd = 0;

        var curCheckpoint = GetCurrentCheckpoint();

        //initialize camera position to first checkpoint
        gameCamera.SetPosition(curCheckpoint.transform.position);

        curCheckpoint.SpawnPlayer(player);
    }
        
    void OnSignalGoal() {
        //show victory modal
        GameData.instance.Progress();
    }

    void OnSignalDeath() {
        //respawn to last checkpoint
        var curCheckpoint = GetCurrentCheckpoint();

        gameCamera.MoveTo(curCheckpoint.transform.position);

        curCheckpoint.SpawnPlayer(player);
    }

    private PlayerCheckpoint GetCurrentCheckpoint() {
        return mCheckpoints[mCurCheckpointInd];
    }
}
