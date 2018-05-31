using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLPlaySoundButtonClick : LoLPlaySound {
    public Button button;

    void OnDestroy() {
        if(button)
            button.onClick.RemoveListener(OnClick);
    }

    void Awake() {
        if(!button)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
    }

    void OnClick() {
        Play();
    }
}
