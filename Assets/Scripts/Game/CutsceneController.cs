﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour {
    [System.Serializable]
    public struct Data {
        public string name;
        public GameObject root;
        public M8.Animator.AnimatorData animator;
        public string takeEnter;        
        public string takeExit;
    }

    public string playMusicPath; //if not empty, play this music
    public bool startMusicPlaylist = true; //make sure playMusicPath is empty
    
    public M8.Animator.AnimatorData animator;
    public string takeStart; //prep background
    public string takeInteractEnter; //'next' button enter
    public string takeInteractExit; //'next' button exit

    public Data[] pages;

    public bool playOnStart;
    public bool progressOnEnd;

    private int mCurPageInd;

    public void Play() {
        StopAllCoroutines();

        HideAllPages();

        StartCoroutine(DoPlay());
    }

    public void NextPage() {
        StartCoroutine(DoGoNextPage());
    }

    void Awake() {
        HideAllPages();
    }

    IEnumerator Start() {
        if(playOnStart) {
            while(M8.SceneManager.instance.isLoading)
                yield return null;

            StartCoroutine(DoPlay());
        }
    }

    void HideAllPages() {
        for(int i = 0; i < pages.Length; i++) {
            if(pages[i].root)
                pages[i].root.SetActive(false);
        }
    }

    // Use this for initialization
    IEnumerator DoPlay() {
        //setup initial display
        animator.ResetTake(animator.GetTakeIndex(takeStart));
        animator.ResetTake(animator.GetTakeIndex(takeInteractEnter));
                
        animator.Play(takeStart);
        while(animator.isPlaying)
            yield return null;

        //music
        if(!string.IsNullOrEmpty(playMusicPath)) {
            LoLMusicPlaylist.instance.Stop(); //ensure playlist is stopped

            LoLManager.instance.PlaySound(playMusicPath, true, true);
        }
        else if(startMusicPlaylist)
            LoLMusicPlaylist.instance.Play();

        //start up the first page
        mCurPageInd = 0;
        ShowCurrentPage();
    }

    void ShowCurrentPage() {
        StartCoroutine(DoShowCurrentPage());
    }

    IEnumerator DoShowCurrentPage() {
        if(mCurPageInd < pages.Length) {
            var page = pages[mCurPageInd];

            if(page.root)
                page.root.SetActive(true);

            if(page.animator && !string.IsNullOrEmpty(page.takeEnter)) {
                page.animator.Play(page.takeEnter);
                while(page.animator.isPlaying)
                    yield return null;
            }
        }

        animator.Play(takeInteractEnter);
    }

    IEnumerator DoGoNextPage() {
        animator.Play(takeInteractExit);

        bool isLastPage = pages.Length == 0 || mCurPageInd == pages.Length - 1;

        if(mCurPageInd < pages.Length) {
            var page = pages[mCurPageInd];

            if(page.animator && !string.IsNullOrEmpty(page.takeExit)) {
                page.animator.Play(page.takeExit);
                while(page.animator.isPlaying)
                    yield return null;
            }

            //only deactivate if it's the last page or the next page has a different root
            if(isLastPage || page.root != pages[mCurPageInd + 1].root)
                page.root.SetActive(false);
        }

        if(!isLastPage) {
            mCurPageInd++;
            ShowCurrentPage();
        }
        else if(progressOnEnd) //proceed
            GameData.instance.Progress();
    }
}
