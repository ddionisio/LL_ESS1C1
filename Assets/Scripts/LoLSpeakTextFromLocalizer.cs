using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLSpeakTextFromLocalizer : MonoBehaviour {

    M8.UI.Texts.Localizer localizer;
    
    public bool autoPlay;
    public string autoPlayGroup = "default";
    public int autoPlayIndex = -1;

    public void Play() {
        LoLManager.instance.SpeakText(localizer.key);
    }

    void OnEnable() {
        if(autoPlay)
            LoLManager.instance.SpeakTextQueue(localizer.key, autoPlayGroup, autoPlayIndex);
    }

    void Awake() {
        if(!localizer)
            localizer = GetComponent<M8.UI.Texts.Localizer>();
    }
}
