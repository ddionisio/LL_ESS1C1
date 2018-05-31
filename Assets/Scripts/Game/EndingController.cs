using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string take = "play";

    public Text scoreLabel;

    void Awake() {
        //initialize hud
        HUD.instance.isGameActive = false;
    }

    IEnumerator Start() {
        //wait for scene transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //setup infos
        if(LoLManager.isInstantiated) {
            if(scoreLabel) scoreLabel.text = LoLManager.instance.curScore.ToString();
        }

        //play animation
        if(animator && !string.IsNullOrEmpty(take))
            animator.Play(take);
    }
}