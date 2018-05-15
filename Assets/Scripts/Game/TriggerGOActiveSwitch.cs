using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGOActiveSwitch : MonoBehaviour {
    public GameObject activeGO;
    public GameObject inactiveGO;

    void Awake() {
        //if(activeGO) activeGO.SetActive(true);
        if(inactiveGO) inactiveGO.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(activeGO) activeGO.SetActive(!activeGO.activeSelf);
        if(inactiveGO) inactiveGO.SetActive(!inactiveGO.activeSelf);
    }
}
