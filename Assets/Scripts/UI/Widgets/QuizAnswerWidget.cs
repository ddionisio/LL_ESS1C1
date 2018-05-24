using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuizAnswerWidget : MonoBehaviour, IPointerClickHandler {
    public enum State {
        Normal,
        Wrong,
        Correct,
        Locked
    }

    public Image iconImage;
    public Color iconDisableColor = Color.gray;

    public Text textLabel;
    public Color textDisableColor = Color.gray;

    public Graphic graphicRaycastTarget;

    public GameObject correctGO;
    public GameObject wrongGO;

    public int index { get; private set; }

    public State state {
        get { return mState; }
        set {
            if(mState != value) {
                mState = value;
                ApplyState();
            }
        }
    }

    public event System.Action<int> clickCallback;

    private State mState;

    private Color mIconDefaultColor;
    private Color mTextDefaultColor;
    private bool mIsColorInit;

    public void Init(int index) {
        this.index = index;

        if(!mIsColorInit) {
            if(iconImage) mIconDefaultColor = iconImage.color;
            if(textLabel) mTextDefaultColor = textLabel.color;

            mIsColorInit = true;
        }

        mState = State.Normal;
        ApplyState();
    }

    public void Init(int index, Sprite icon, string textRef) {
        if(iconImage) {
            iconImage.sprite = icon;
            iconImage.SetNativeSize();
        }

        if(textLabel) {
            textLabel.text = LoLLocalize.Get(textRef);
        }

        Init(index);
    }

    void ApplyState() {
        switch(mState) {
            case State.Normal:
                if(iconImage) iconImage.color = mIconDefaultColor;
                if(textLabel) textLabel.color = mTextDefaultColor;

                if(graphicRaycastTarget) graphicRaycastTarget.raycastTarget = true;

                if(correctGO) correctGO.SetActive(false);
                if(wrongGO) wrongGO.SetActive(false);
                break;
            case State.Wrong:
                if(iconImage) iconImage.color = iconDisableColor;
                if(textLabel) textLabel.color = textDisableColor;

                if(graphicRaycastTarget) graphicRaycastTarget.raycastTarget = false;

                if(correctGO) correctGO.SetActive(false);
                if(wrongGO) wrongGO.SetActive(true);
                break;
            case State.Correct:
                if(iconImage) iconImage.color = iconDisableColor;
                if(textLabel) textLabel.color = textDisableColor;

                if(graphicRaycastTarget) graphicRaycastTarget.raycastTarget = false;

                if(correctGO) correctGO.SetActive(true);
                if(wrongGO) wrongGO.SetActive(false);
                break;
            case State.Locked:
                if(iconImage) iconImage.color = iconDisableColor;
                if(textLabel) textLabel.color = textDisableColor;

                if(graphicRaycastTarget) graphicRaycastTarget.raycastTarget = false;

                if(correctGO) correctGO.SetActive(false);
                if(wrongGO) wrongGO.SetActive(false);
                break;
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(clickCallback != null)
            clickCallback(index);
    }
}
