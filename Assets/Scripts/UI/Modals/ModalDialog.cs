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

    private static M8.GenericParams mParms = new M8.GenericParams();

    public static void Open(string nameTextRef, string dialogTextRef) {
        mParms[parmNameTextRef] = nameTextRef;
        mParms[parmDialogTextRef] = dialogTextRef;
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            nameLabel.text = M8.Localize.Get(parms.GetValue<string>(parmNameTextRef));
            textLabel.text = M8.Localize.Get(parms.GetValue<string>(parmDialogTextRef));
        }
    }
}