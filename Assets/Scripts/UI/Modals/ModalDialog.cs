using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalDialog : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    public const string modalName = "dialog";

    public const string parmNameTextRef = "n";
    public const string parmDialogTextRef = "t";

    public Text nameLabel;
    public Text textLabel;

    public bool isTextSpeechAuto = true;

    public M8.Signal signalNext; //when the next button is pressed.

    private static M8.GenericParams mParms = new M8.GenericParams();

    private string mDialogTextRef;

    public static void Open(string nameTextRef, string dialogTextRef) {
        //check to see if there's one already opened
        var uiMgr = M8.UIModal.Manager.instance;

        var dlg = uiMgr.ModalGetController<ModalDialog>(modalName);
        if(dlg) {
            dlg.SetupContent(nameTextRef, dialogTextRef);

            if(dlg.isTextSpeechAuto)
                dlg.PlayDialogSpeech();
        }
        else {
            mParms[parmNameTextRef] = nameTextRef;
            mParms[parmDialogTextRef] = dialogTextRef;

            uiMgr.ModalOpen(modalName, mParms);
        }
    }

    public void Next() {
        if(signalNext != null)
            signalNext.Invoke();
    }

    public void PlayDialogSpeech() {
        if(LoLManager.isInstantiated && !string.IsNullOrEmpty(mDialogTextRef))
            LoLManager.instance.SpeakText(mDialogTextRef);
    }
        
    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        //play text speech if auto
        if(isTextSpeechAuto)
            PlayDialogSpeech();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            SetupContent(parms.GetValue<string>(parmNameTextRef), parms.GetValue<string>(parmDialogTextRef));
        }
    }

    private void SetupContent(string nameTextRef, string dialogTextRef) {
        //setup other stuff?

        mDialogTextRef = dialogTextRef;

        nameLabel.text = M8.Localize.Get(nameTextRef);
        textLabel.text = M8.Localize.Get(dialogTextRef);
    }
}