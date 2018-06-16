using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "collectionData", menuName = "Game/Collection Data")]
public class CollectionData : ScriptableObject {
    public const string parmCollectionData = "pcd";

    public Sprite icon;
    [SerializeField]
    [M8.Localize]
    string _nameTextRef;

    [Header("Description")]
    public Sprite descImage;
    [SerializeField]
    [M8.Localize]
    string _descTextRef;

    [Header("UI")]
    public string modalRef; //use this for certain collection that opens a specialized UI
    public int modalDisplayIndex; //which display to show based on index

    public string nameTextRef { get { return _nameTextRef; } }
    public string descTextRef { get { return _descTextRef; } }

    public string nameText { get { return !string.IsNullOrEmpty(_nameTextRef) ? LoLLocalize.Get(_nameTextRef) : ""; } }
    public string descText { get { return !string.IsNullOrEmpty(_descTextRef) ? LoLLocalize.Get(_descTextRef) : ""; } }

    public void OpenModal() {
        var parms = new M8.GenericParamArg(parmCollectionData, this);

        if(!string.IsNullOrEmpty(modalRef)) {
            M8.UIModal.Manager.instance.ModalOpen(modalRef, parms);
        }
        else {
            M8.UIModal.Manager.instance.ModalOpen(Modals.collectionInfo, parms);
        }
    }
}
