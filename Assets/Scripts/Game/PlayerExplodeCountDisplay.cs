using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExplodeCountDisplay : MonoBehaviour {
    [Header("Display")]
    public GameObject[] explodeGOs;

    [Header("Signals")]
    public SignalInt signalExplodeCount;

    void OnDestroy() {
        if(signalExplodeCount) signalExplodeCount.callback -= OnSignalExplodeCount;
    }

    void Awake() {
        for(int i = 0; i < explodeGOs.Length; i++) {
            if(explodeGOs[i])
                explodeGOs[i].SetActive(false);
        }

        if(signalExplodeCount) signalExplodeCount.callback += OnSignalExplodeCount;
    }

    void OnSignalExplodeCount(int count) {
        int activeCount = Mathf.Clamp(count, 0, explodeGOs.Length);

        for(int i = 0; i < activeCount; i++) {
            if(explodeGOs[i])
                explodeGOs[i].SetActive(true);
        }

        for(int i = activeCount; i < explodeGOs.Length; i++) {
            if(explodeGOs[i])
                explodeGOs[i].SetActive(false);
        }
    }
}
