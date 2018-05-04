using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var gameDat = GameData.instance;
        Debug.Log("Game Data: " + gameDat);

        var blarg = GameData.instance;
        Debug.Log("yeah: " + gameDat);
	}
	
}
