using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionWidget : MonoBehaviour {
    public CollectionData data;

    [Header("Display")]
    public Image iconImage;
    public Text nameLabel;

    public void OpenDesc() {
        M8.UIModal.Manager.instance.ModalOpen(Modals.collectionInfo, new M8.GenericParamArg(ModalCollectionDesc.parmCollectionData, data));
    }

    public void Init() {
        if(iconImage) {
            iconImage.sprite = data.icon;
            iconImage.SetNativeSize();
        }

        if(nameLabel) {
            nameLabel.text = data.nameText;
        }
    }
}
