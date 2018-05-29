using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndController : MonoBehaviour {
    [Header("Signals")]
    public M8.Signal signalProceed;

    [Header("Debug")]
    public int debugLevelIndex;

    void OnDestroy() {
        if(signalProceed) signalProceed.callback -= OnSignalProceed;
    }

    void Awake() {
        if(signalProceed) signalProceed.callback += OnSignalProceed;

        //initialize hud
        HUD.instance.isGameActive = false;
    }

    IEnumerator Start() {
        //wait for scene transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //open up the proper modal for the current level index
        int ind;

        if(GameData.instance.isGameStarted)
            ind = GameData.instance.curLevelIndex;
        else
            ind = debugLevelIndex;

        if(ind >= 0 && ind < GameData.instance.levels.Length) {
            string modalRef = GameData.instance.levels[ind].modalLevelEnd;
            if(!string.IsNullOrEmpty(modalRef)) {
                M8.UIModal.Manager.instance.ModalOpen(modalRef);
            }
            else {
                //fail-safe
                GameData.instance.Progress();
            }
        }
        else {
            //fail-safe
            GameData.instance.Progress();
        }
    }

    void OnSignalProceed() {
        GameData.instance.Progress();
    }
}
