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

    public Transform inactiveRoot;
    public Transform activeRoot;

    public Color baseActiveColor = Color.white;
    public Color baseInactiveColor = Color.gray;

    public Color iconActiveColor = Color.white;
    public Color iconInactiveColor = Color.gray;

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
                if(icon) icon.color = iconActiveColor;
                if(baseImage) baseImage.color = baseActiveColor;
                if(baseInteract) baseInteract.interactable = false;

                if(activeRoot) transform.SetParent(activeRoot, true);
                break;
            case State.Inactive:
                if(icon) icon.color = iconInactiveColor;
                if(baseImage) baseImage.color = baseInactiveColor;
                if(baseInteract) baseInteract.interactable = true;

                if(inactiveRoot) transform.SetParent(inactiveRoot, true);
                break;
        }
    }
}
