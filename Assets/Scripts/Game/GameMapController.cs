using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapController : M8.SingletonBehaviour<GameMapController> {
    public GameMapData data;

    public OffscreenIndicatorPosition checkpointIndicator;
    public TriggerGoal goal;

    [Header("Debug")]
    public int startCheckpointIndex = 0; //for debug purpose
        
    public Player player { get; private set; }
    public GameCamera gameCamera { get; private set; }

    public string curSceneName { get; private set; }

    public int score { get; private set; }

    private PlayerCheckpoint[] mCheckpoints;
    private int mCurCheckpointInd;

    private bool mDiedCheckpoint;
        
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
            return a.name.CompareTo(b.name);
        });

        //update checkpoint indices
        for(int i = 0; i < mCheckpoints.Length; i++)
            mCheckpoints[i].index = i;

        mCurCheckpointInd = startCheckpointIndex;

        //apply first camera bounds and position
        if(mCheckpoints.Length > 0) {
            var firstCheckpoint = GetCurrentCheckpoint();

            var gameBounds = firstCheckpoint.cameraBounds;
            if(gameBounds)
                gameCamera.SetBounds(gameBounds.rect, false);

            //initialize camera position
            gameCamera.SetPosition(firstCheckpoint.transform.position);
        }

        //indicator
        if(checkpointIndicator) {
            checkpointIndicator.gameObject.SetActive(false);
            checkpointIndicator.targetCamera = gameCamera.camera2D.unityCamera;
        }

        //goal
        if(goal)
            goal.SetDisplayActive(false);

        //hook up signals
        if(data.signalGoal) data.signalGoal.callback += OnSignalGoal;
        if(data.signalDeath) data.signalDeath.callback += OnSignalDeath;
        if(data.signalPlayerCheckpoint) data.signalPlayerCheckpoint.callback += OnSignalPlayerCheckpoint;
    }

    protected override void OnInstanceDeinit() {
        if(data.signalGoal) data.signalGoal.callback -= OnSignalGoal;
        if(data.signalDeath) data.signalDeath.callback -= OnSignalDeath;
        if(data.signalPlayerCheckpoint) data.signalPlayerCheckpoint.callback -= OnSignalPlayerCheckpoint;

        //clear out game spawns
        if(GamePool.isInstantiated)
            GamePool.instance.ReleaseAll();
    }

    IEnumerator Start() {
        //wait for transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //states
        score = 0;
        mDiedCheckpoint = false;

        //setup initial game state
        ShowNextCheckpoint();

        var curCheckpoint = GetCurrentCheckpoint();
                
        curCheckpoint.SpawnPlayer(player);
    }
        
    void OnSignalGoal() {
        //add score
        if(mDiedCheckpoint)
            score += GameData.instance.checkpointPoints;
        else
            score += GameData.instance.checkpointNoDeathPoints;

        mDiedCheckpoint = false;
        //

        //show victory modal
        M8.UIModal.Manager.instance.ModalOpen(Modals.victory);
    }

    void OnSignalDeath() {
        mDiedCheckpoint = true;

        //respawn to last checkpoint
        var curCheckpoint = GetCurrentCheckpoint();

        gameCamera.MoveTo(curCheckpoint.transform.position);

        curCheckpoint.SpawnPlayer(player);
    }

    void OnSignalPlayerCheckpoint(PlayerCheckpoint checkpoint) {
        //change current checkpoint index
        if(checkpoint.index != -1) {
            //add score
            if(mDiedCheckpoint)
                score += GameData.instance.checkpointPoints;
            else
                score += GameData.instance.checkpointNoDeathPoints;

            mDiedCheckpoint = false;
            //

            mCurCheckpointInd = checkpoint.index;

            ShowNextCheckpoint();

            //apply camera bounds
            if(checkpoint.cameraBounds)
                gameCamera.SetBounds(checkpoint.cameraBounds.rect, true);
        }
        else
            Debug.LogWarning("Unknown Player Checkpoint: " + checkpoint.name);
    }

    private PlayerCheckpoint GetCurrentCheckpoint() {
        return mCheckpoints[mCurCheckpointInd];
    }

    private void ShowNextCheckpoint() {
        //hide current
        if(mCheckpoints[mCurCheckpointInd].displayGO)
            mCheckpoints[mCurCheckpointInd].displayGO.SetActive(false);

        int nextCheckpointInd = mCurCheckpointInd + 1;
        if(nextCheckpointInd < mCheckpoints.Length) {
            var nextCheckpoint = mCheckpoints[nextCheckpointInd];

            if(nextCheckpoint.displayGO)
                nextCheckpoint.displayGO.SetActive(true);

            //setup indicator
            if(checkpointIndicator) {
                checkpointIndicator.gameObject.SetActive(true);
                checkpointIndicator.targetPosition = nextCheckpoint.displayGO ? nextCheckpoint.displayGO.transform.position : nextCheckpoint.transform.position;
            }
        }
        else if(goal) {
            //show goal
            goal.SetDisplayActive(true);

            //setup indicator
            if(checkpointIndicator) {
                checkpointIndicator.gameObject.SetActive(true);
                checkpointIndicator.targetPosition = goal.transform.position;
            }
        }
    }
}
