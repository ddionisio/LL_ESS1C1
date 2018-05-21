using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyTextOnEnable : MonoBehaviour {
    public GameObject textGO;

    [M8.Localize]
    public string textRef;

    private M8.UI.Texts.Localizer mLocalizer;
    private LoLSpeakTextFromLocalizer mSpeakTextFromLocalizer;

    void OnEnable() {
        mLocalizer.key = textRef;
        mLocalizer.Apply();

        mSpeakTextFromLocalizer.Play();
    }

    void Awake() {
        mLocalizer = textGO.GetComponent<M8.UI.Texts.Localizer>();
        mSpeakTextFromLocalizer = textGO.GetComponent<LoLSpeakTextFromLocalizer>();
    }
}
