using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndController : MonoBehaviour {
    public int musicTrackIndex = 1;

    [Header("Signals")]
    public M8.Signal signalProceed;

    [Header("Debug")]
    public int debugLevelIndex;

    private bool mIsUnlockNextWaiting;

    void OnDestroy() {
        if(signalProceed) signalProceed.callback -= OnSignalProceed;
    }

    void Awake() {
        if(signalProceed) signalProceed.callback += OnSignalProceed;

        //initialize hud
        HUD.instance.mode = HUD.Mode.None;
    }

    IEnumerator Start() {
        //wait for scene transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);

        //grab level data
        int ind;

        if(GameData.instance.isGameStarted)
            ind = GameData.instance.curLevelIndex;
        else
            ind = debugLevelIndex;

        if(ind < 0 || ind >= GameData.instance.levels.Length) {
            //fail-safe
            GameData.instance.Progress();
            yield break;
        }

        var levelData = GameData.instance.levels[ind];

        HUD.instance.notebookOpenProxy.startPageIndex = levelData.notebookInitialIndex;

        //open modal unlock
        var collectUnlockParms = new M8.GenericParams();
        collectUnlockParms.Add(ModalKnowledgeUnlocks.parmCollectionUnlocks, levelData.collectionUnlocks);

        M8.UIModal.Manager.instance.ModalOpen(Modals.knowledgeUnlock, collectUnlockParms);
        //
        
        HUD.instance.mode = HUD.Mode.Lesson;

        //wait for next to be clicked
        mIsUnlockNextWaiting = true;
        while(mIsUnlockNextWaiting)
            yield return null;

        //open up the end part
        string modalRef = levelData.modalLevelEnd;
        if(!string.IsNullOrEmpty(modalRef)) {
            M8.UIModal.Manager.instance.ModalOpen(modalRef);
        }
        else {
            //fail-safe
            GameData.instance.Progress();
        }
    }

    void OnSignalProceed() {
        if(mIsUnlockNextWaiting) //clicked next from knowledge unlock
            mIsUnlockNextWaiting = false;
        else
            GameData.instance.Progress(); //go to next level
    }
}
