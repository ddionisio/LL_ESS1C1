using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroController : MonoBehaviour {
    void Awake() {
        //initialize hud
        HUD.instance.isGameActive = false;
    }
}
