using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaunchDialogWidget : MonoBehaviour {
    public GameObject displayGO;
    public float displayShowDelay = 5f;

    [Header("Signals")]
    public SignalBool signalGameHUDActive;

    void OnDisable() {
        if(displayGO) displayGO.SetActive(false);
    }

    void OnDestroy() {
        if(signalGameHUDActive) signalGameHUDActive.callback -= OnSignalGameHUDActive;
    }

    void Awake() {
        if(displayGO) displayGO.SetActive(false);

        if(signalGameHUDActive) signalGameHUDActive.callback += OnSignalGameHUDActive;
    }

    void OnSignalGameHUDActive(bool active) {
        if(active) {
            StopAllCoroutines();
            StartCoroutine(DoShowDisplay());
        }
    }

    IEnumerator DoShowDisplay() {
        //wait for launch ready
        while(GameMapController.instance.player.state != (int)EntityState.PlayerLaunchReady)
            yield return null;

        if(GameData.instance.curLevelIndex > 0) {
            float curTime = 0f;
            while(curTime < displayShowDelay) {
                yield return null;
                curTime += Time.deltaTime;

                //check if player has launched
                if(GameMapController.instance.player.state == (int)EntityState.PlayerLaunch)
                    yield break;
            }
        }

        if(displayGO) displayGO.SetActive(true);

        //hide once player launches
        while(GameMapController.instance.player.state == (int)EntityState.PlayerLaunchReady)
            yield return null;

        if(displayGO) displayGO.SetActive(false);
    }
}
