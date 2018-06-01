using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelIntroController : MonoBehaviour {
    [System.Serializable]
    public struct LevelData {
        [M8.Localize]
        public string nameTextRef;

        public string formatText;

        public Transform anchor;
    }

    public LevelData[] levels;

    public Text titleLabel;
    public Text formatLabel;
    public Transform highlightRoot;

    public int musicTrackIndex = 0;

    void Awake() {
        //initialize hud
        HUD.instance.isGameActive = false;
    }

    IEnumerator Start() {
        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);

        while(M8.SceneManager.instance.isLoading)
            yield return null;

        int ind = Mathf.Clamp(GameData.instance.curLevelIndex, 0, levels.Length);

        var levelDat = levels[ind];

        //setup texts
        if(titleLabel) titleLabel.text = LoLLocalize.Get(levelDat.nameTextRef);

        if(formatLabel) formatLabel.text = string.Format("{0} - ", levelDat.formatText);

        //apply highlight position
        if(highlightRoot && levelDat.anchor) {
            highlightRoot.position = levelDat.anchor.position;
        }

        if(LoLManager.isInstantiated)
            LoLManager.instance.SpeakText(levelDat.nameTextRef);
    }
}
