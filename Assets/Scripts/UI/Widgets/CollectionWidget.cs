using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionWidget : MonoBehaviour {
    public CollectionData data;

    [Header("Display")]
    public Image iconImage;
    public Text nameLabel;
    public GameObject lockedGO;

    public bool autoInit;

    public bool isLocked {
        get { return lockedGO ? lockedGO.activeSelf : false; }
        set {
            if(lockedGO) {
                lockedGO.SetActive(value);

                if(iconImage)
                    iconImage.gameObject.SetActive(!value);

                if(nameLabel)
                    nameLabel.gameObject.SetActive(!value);
            }
        }
    }

    public void OpenDesc() {
        if(isLocked)
            ModalDialog.Open("", "dialogCollectionLocked");
        else
            data.OpenModal();
    }

    public void Init() {
        if(iconImage) {
            iconImage.sprite = data.icon;
            iconImage.SetNativeSize();
        }

        if(nameLabel) {
            nameLabel.text = data.nameText;
        }

        //check if locked
        isLocked = !GameData.instance.CollectionIsUnlocked(data.name);
    }

    void OnEnable() {
        if(autoInit)
            Init();
    }
}
