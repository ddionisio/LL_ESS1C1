using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalKnowledgeUnlocks : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public const string parmCollectionUnlocks = "cu";

    public CollectionUnlockWidget[] unlockWidgets;
    public GameObject nextGO;

    public string soundPlayPath;

    [Header("Signals")]
    public M8.Signal signalNext;

    private int mCurUnlockIndex;
    private int mUnlockDisplayCount;

    public void Next() {
        if(nextGO) nextGO.SetActive(false);

        Close();

        if(signalNext)
            signalNext.Invoke();
    }

    void Awake() {
        //hook up callbacks
        for(int i = 0; i < unlockWidgets.Length; i++) {
            unlockWidgets[i].unlockedCallback += OnCollectionUnlocked;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(nextGO) nextGO.SetActive(false);

        mCurUnlockIndex = 0;

        var levelUnlocks = parms.GetValue<GameData.LevelUnlockData[]>(parmCollectionUnlocks);

        List<CollectionData> collects = new List<CollectionData>();

        for(int i = 0; i < levelUnlocks.Length; i++) {
            if(levelUnlocks[i].isDisplayed) {
                if(collects.Count < unlockWidgets.Length) {
                    collects.Add(levelUnlocks[i].data);
                }
                else {
                    //ran out of room to display, just unlock it
                    GameData.instance.CollectionUnlock(levelUnlocks[i].data.name);
                }
            }
            else {
                //not part of display, just unlock it
                GameData.instance.CollectionUnlock(levelUnlocks[i].data.name);
            }
        }

        for(int i = 0; i < collects.Count; i++) {
            unlockWidgets[i].gameObject.SetActive(true);

            unlockWidgets[i].Init(collects[i]);

            bool isHighlight = i == mCurUnlockIndex;

            unlockWidgets[i].isHighlight = isHighlight;
            unlockWidgets[i].isInteractive = isHighlight;
        }

        //hide leftover unlocks
        for(int i = collects.Count; i < unlockWidgets.Length; i++) {
            unlockWidgets[i].gameObject.SetActive(false);
        }

        mUnlockDisplayCount = collects.Count;

        LoLManager.instance.PlaySound(soundPlayPath, false, false);
    }

    void M8.UIModal.Interface.IPop.Pop() {
        
    }

    void OnCollectionUnlocked() {
        unlockWidgets[mCurUnlockIndex].isHighlight = false;

        mCurUnlockIndex++;
        if(mCurUnlockIndex == mUnlockDisplayCount) {
            //ready to move on
            if(nextGO) nextGO.SetActive(true);
        }
        else {
            unlockWidgets[mCurUnlockIndex].isHighlight = true;
            unlockWidgets[mCurUnlockIndex].isInteractive = true;
        }
    }
}
