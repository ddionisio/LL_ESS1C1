using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelIntroController : MonoBehaviour {

    void Awake() {
        //initialize hud
        HUD.instance.HideGame();
    }
}
