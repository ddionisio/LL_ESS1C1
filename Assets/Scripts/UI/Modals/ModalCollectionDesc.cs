using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalCollectionDesc : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    [Header("Display")]
    public Text titleLabel;
    public Image iconImage;
    public Image descImage;
    public Text descTextLabel;

    [Header("Text Speech")]
    public bool isSpeechAuto = true;
    public string speechAutoGroup = "collectionDesc";
    public int speechAutoGroupStartInd = 0;
    public float speechAutoDelay = 0.5f;

    private M8.UI.Texts.Localizer mTitleLabelLocalizer;
    private M8.UI.Texts.Localizer mDescTextLabelLocalizer;

    void OnDestroy() {
        UnhookInput();
    }

    void Awake() {
        if(titleLabel)
            mTitleLabelLocalizer = titleLabel.GetComponent<M8.UI.Texts.Localizer>();

        if(descTextLabel)
            mDescTextLabelLocalizer = descTextLabel.GetComponent<M8.UI.Texts.Localizer>();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        var collectData = parms.GetValue<CollectionData>(CollectionData.parmCollectionData);

        if(mTitleLabelLocalizer) {
            mTitleLabelLocalizer.key = collectData.nameTextRef;
            mTitleLabelLocalizer.Apply();
        }
        else if(titleLabel)
            titleLabel.text = collectData.nameText;

        if(mDescTextLabelLocalizer) {
            mDescTextLabelLocalizer.key = collectData.descTextRef;
            mDescTextLabelLocalizer.Apply();
        }
        else if(descTextLabel)
            descTextLabel.text = collectData.descText;

        if(iconImage) {
            iconImage.sprite = collectData.icon;
            iconImage.SetNativeSize();
        }

        if(descImage) {
            descImage.sprite = collectData.descImage;
            descImage.SetNativeSize();
        }

        if(isSpeechAuto) {
            if(speechAutoDelay > 0f)
                StartCoroutine(DoSpeechAutoPlayDelay(collectData));
            else
                PlayTextSpeech(collectData);
        }

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);
    }

    void M8.UIModal.Interface.IPop.Pop() {
        UnhookInput();
    }

    void OnInputEscape(M8.InputManager.Info data) {
        if(data.state == M8.InputManager.State.Released)
            Close();
    }

    void UnhookInput() {
        if(M8.InputManager.instance)
            M8.InputManager.instance.RemoveButtonCall(OnInputEscape);
    }

    void PlayTextSpeech(CollectionData collectData) {
        if(!string.IsNullOrEmpty(speechAutoGroup)) {
            if(!string.IsNullOrEmpty(collectData.nameTextRef))
                LoLManager.instance.SpeakTextQueue(collectData.nameTextRef, speechAutoGroup, speechAutoGroupStartInd);

            if(!string.IsNullOrEmpty(collectData.descTextRef))
                LoLManager.instance.SpeakTextQueue(collectData.descTextRef, speechAutoGroup, speechAutoGroupStartInd + 1);
        }
        else {
            if(!string.IsNullOrEmpty(collectData.descTextRef))
                LoLManager.instance.SpeakText(collectData.descTextRef);
        }
    }

    IEnumerator DoSpeechAutoPlayDelay(CollectionData collectData) {
        yield return new WaitForSeconds(speechAutoDelay);

        PlayTextSpeech(collectData);
    }
}
