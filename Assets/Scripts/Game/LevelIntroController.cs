using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelIntroController : MonoBehaviour {
    [System.Serializable]
    public struct LevelData {
        [M8.Localize]
        public string nameTextRef;
        
        public Transform anchor;

        public GameObject activeGO;
    }

    public LevelData[] levels;

    public Text titleLabel;
    public Transform highlightRoot;

    public int musicTrackIndex = 0;

    void Awake() {
        //initialize hud
        HUD.instance.mode = HUD.Mode.Lesson;

        for(int i = 0; i < levels.Length; i++) {
            if(levels[i].activeGO)
                levels[i].activeGO.SetActive(false);
        }
    }

    IEnumerator Start() {
        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);

        while(M8.SceneManager.instance.isLoading)
            yield return null;

        int ind = Mathf.Clamp(GameData.instance.curLevelIndex, 0, levels.Length);

        var levelDat = levels[ind];

        string prependText = "";
        switch(ind) {
            case 0:
                prependText = "I - ";
                break;
            case 1:
                prependText = "II - ";
                break;
            case 2:
                prependText = "III - ";
                break;
            case 3:
                prependText = "IV - ";
                break;
        }

        //setup texts
        if(titleLabel) titleLabel.text = prependText + LoLLocalize.Get(levelDat.nameTextRef);

        //apply highlight position
        if(highlightRoot && levelDat.anchor) {
            highlightRoot.position = levelDat.anchor.position;
        }

        if(levelDat.activeGO)
            levelDat.activeGO.SetActive(true);

        if(LoLManager.isInstantiated)
            LoLManager.instance.SpeakText(levelDat.nameTextRef);
    }
}
