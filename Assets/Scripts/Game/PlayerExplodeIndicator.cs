using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExplodeIndicator : MonoBehaviour {
    public GameObject displayRoot;

    [Header("Signals")]
    public SignalBool signalCanExplodeUpdate;

    void OnDestroy() {
        if(signalCanExplodeUpdate) signalCanExplodeUpdate.callback -= OnSignalCanExplodeUpdate;
    }

    void Awake() {
        if(displayRoot) displayRoot.SetActive(false);

        if(signalCanExplodeUpdate) signalCanExplodeUpdate.callback += OnSignalCanExplodeUpdate;
    }

    void OnSignalCanExplodeUpdate(bool canExplode) {
        if(displayRoot) displayRoot.SetActive(canExplode);
    }
}
