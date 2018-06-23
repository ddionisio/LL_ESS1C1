using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string take = "play";

    public Text[] levelScoreLabels;
    public Text scoreLabel;

    public int musicTrackIndex = 2;

    void Awake() {
        //initialize hud
        HUD.instance.mode = HUD.Mode.Lesson;
        HUD.instance.notebookOpenProxy.startPageIndex = 0;
    }

    IEnumerator Start() {
        if(animator && !string.IsNullOrEmpty(take))
            animator.ResetTake(take);

        //wait for scene transition
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //setup infos
        if(LoLManager.isInstantiated) {
            for(int i = 0; i < levelScoreLabels.Length; i++) {
                if(levelScoreLabels[i])
                    levelScoreLabels[i].text = GameData.instance.GetLevelScore(i).ToString();
            }

            if(scoreLabel) scoreLabel.text = LoLManager.instance.curScore.ToString();
        }

        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);
                
        //play animation
        if(animator && !string.IsNullOrEmpty(take))
            animator.Play(take);
    }
}