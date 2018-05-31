using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDSetGameVisible : MonoBehaviour {
    public bool isGameActive;

    void Start() {
        HUD.instance.isGameActive = isGameActive;
    }
}
