using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionWidget : MonoBehaviour {
    public CollectionData data;

    [Header("Display")]
    public Image iconImage;
    public Text nameLabel;

    public bool autoInit;

    public void OpenDesc() {
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
    }

    void Awake() {
        if(autoInit)
            Init();
    }
}
