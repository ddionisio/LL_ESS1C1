using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalCollectionPageIndex : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    [System.Serializable]
    public struct IllustrationData {
        public GameObject displayGO;
        public M8.Animator.AnimatorData animator;
        public string take;
    }

    [Header("Display")]
    public Text titleLabel;
    public Text descTextLabel;
    public IllustrationData[] illustrations;
    public float illustrateShowDelay = 1f;
    public Selectable replayInteractive;

    [Header("Text Speech")]
    public bool isSpeechAuto = true;
    public string speechAutoGroup = "collectionDesc";
    public int speechAutoGroupStartInd = 0;
    public float speechAutoDelay = 0.5f;

    private M8.UI.Texts.Localizer mTitleLabelLocalizer;
    private M8.UI.Texts.Localizer mDescTextLabelLocalizer;

    private int mModalDisplayIndex;

    public void Replay() {
        StartCoroutine(DoIllustration());
    }

    void Awake() {
        if(titleLabel)
            mTitleLabelLocalizer = titleLabel.GetComponent<M8.UI.Texts.Localizer>();

        if(descTextLabel)
            mDescTextLabelLocalizer = descTextLabel.GetComponent<M8.UI.Texts.Localizer>();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        var collectData = parms.GetValue<CollectionData>(CollectionData.parmCollectionData);

        if(!string.IsNullOrEmpty(collectData.nameTextRef)) {
            if(mTitleLabelLocalizer) {
                mTitleLabelLocalizer.key = collectData.nameTextRef;
                mTitleLabelLocalizer.Apply();
            }
            else if(titleLabel)
                titleLabel.text = collectData.nameText;
        }

        if(!string.IsNullOrEmpty(collectData.descTextRef)) {
            if(mDescTextLabelLocalizer) {
                mDescTextLabelLocalizer.key = collectData.descTextRef;
                mDescTextLabelLocalizer.Apply();
            }
            else if(descTextLabel)
                descTextLabel.text = collectData.descText;
        }

        if(isSpeechAuto) {
            if(speechAutoDelay > 0f)
                StartCoroutine(DoSpeechAutoPlayDelay(collectData));
            else
                PlayTextSpeech(collectData);
        }

        mModalDisplayIndex = collectData.modalDisplayIndex;
                
        illustrations[mModalDisplayIndex].displayGO.SetActive(true);

        if(illustrateShowDelay > 0f) {
            if(replayInteractive) replayInteractive.interactable = false;

            StartCoroutine(DoIllustrationDelay());
        }
        else {
            StartCoroutine(DoIllustration());
        }
    }

    void M8.UIModal.Interface.IPop.Pop() {
        illustrations[mModalDisplayIndex].displayGO.SetActive(false);
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

    IEnumerator DoIllustrationDelay() {
        yield return new WaitForSeconds(illustrateShowDelay);

        StartCoroutine(DoIllustration());
    }

    IEnumerator DoIllustration() {
        if(replayInteractive) replayInteractive.interactable = false;

        var illustration = illustrations[mModalDisplayIndex];
                
        var anim = illustration.animator;
        if(anim) {
            anim.Play(illustration.take);
            while(anim.isPlaying)
                yield return null;
        }

        if(replayInteractive) replayInteractive.interactable = true;
    }
}