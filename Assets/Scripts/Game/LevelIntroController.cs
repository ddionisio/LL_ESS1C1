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

    void Start() {
        if(LoLMusicPlaylist.isInstantiated)
            LoLMusicPlaylist.instance.PlayTrack(musicTrackIndex);

        int ind = Mathf.Clamp(GameData.instance.curLevelIndex, 0, levels.Length);

        //setup texts
        if(titleLabel) titleLabel.text = LoLLocalize.Get(levels[ind].nameTextRef);

        if(formatLabel) formatLabel.text = string.Format("{0} - ", levels[ind].formatText);

        //apply highlight position
        if(highlightRoot && levels[ind].anchor) {
            highlightRoot.position = levels[ind].anchor.position;
        }
    }
}
