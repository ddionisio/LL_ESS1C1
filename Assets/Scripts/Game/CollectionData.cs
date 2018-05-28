using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "collectionData", menuName = "Game/Collection Data")]
public class CollectionData : ScriptableObject {
    public Sprite icon;
    [M8.Localize]
    string _nameTextRef;

    [Header("Description")]
    public Sprite descImage;
    [M8.Localize]
    string _descTextRef;

    public string nameTextRef { get { return _nameTextRef; } }
    public string descTextRef { get { return _descTextRef; } }

    public string nameText { get { return !string.IsNullOrEmpty(_nameTextRef) ? LoLLocalize.Get(_nameTextRef) : ""; } }
    public string descText { get { return !string.IsNullOrEmpty(_descTextRef) ? LoLLocalize.Get(_descTextRef) : ""; } }
}
