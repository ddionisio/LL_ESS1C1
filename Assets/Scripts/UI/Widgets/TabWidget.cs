using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabWidget : MonoBehaviour {
    public enum State {
        Inactive,
        Active
    }

    public Image icon;
    public Image baseImage;
    public Selectable baseInteract;

    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    public State state {
        get { return mState; }
        set {
            if(mState != value) {
                mState = value;
                ApplyState();
            }
        }
    }

    private State mState = State.Inactive;

    void Awake() {
        ApplyState();
    }

    private void ApplyState() {
        switch(mState) {
            case State.Active:
                if(icon) icon.color = activeColor;
                if(baseImage) baseImage.color = activeColor;
                if(baseInteract) baseInteract.interactable = false;
                break;
            case State.Inactive:
                if(icon) icon.color = inactiveColor;
                if(baseImage) baseImage.color = inactiveColor;
                if(baseInteract) baseInteract.interactable = true;
                break;
        }
    }
}
