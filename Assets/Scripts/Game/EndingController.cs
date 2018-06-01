using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string take = "play";

    public Text scoreLabel;

    public int musicTrackIndex = 2;

    void Awake() {
        //initialize hud
        HUD.instance.isGameActive = false;
    }

    IEnumerator Start() {
        if(animator && !string.IsNullOrEmpty(take))
            animator.ResetTake(take);

        //wait for scene transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);

        //setup infos
        if(LoLManager.isInstantiated) {
            if(scoreLabel) scoreLabel.text = LoLManager.instance.curScore.ToString();
        }

        //play animation
        if(animator && !string.IsNullOrEmpty(take))
            animator.Play(take);
    }
}