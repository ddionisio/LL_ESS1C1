using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionUnlockWidget : MonoBehaviour {
    public GameObject highlightGO;
    public Image iconImage;
    public Text nameLabel;
    public Selectable interaction;

    [Header("Unlock Display")]
    public Transform unlockMoveDisplayRoot; //move towards notebook icon upon unlock
    public float unlockMoveStartDelay = 0.3f;
    public float unlockMoveDelay = 0.3f;

    public event System.Action unlockedCallback;

    public bool isInteractive {
        get { return interaction ? interaction.interactable : false; }
        set {
            if(interaction)
                interaction.interactable = value;
        }
    }

    public bool isHighlight {
        get { return highlightGO ? highlightGO.activeSelf : false; }
        set {
            if(highlightGO)
                highlightGO.SetActive(value);
        }
    }

    private CollectionData mData;
    private bool mIsLocked;
    
    public void Init(CollectionData data) {
        mData = data;

        iconImage.sprite = data.icon;
        nameLabel.text = data.nameText;

        if(unlockMoveDisplayRoot)
            unlockMoveDisplayRoot.gameObject.SetActive(false);

        mIsLocked = !GameData.instance.CollectionIsUnlocked(data.name);
    }

    public void Click() {
        mData.OpenModal();

        if(mIsLocked) //do unlock stuff after modal for collection is closed
            StartCoroutine(DoUnlock());
    }
    
    IEnumerator DoUnlock() {
        //wait for collection modal to close
        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.ModalIsInStack(mData.modalRef))
            yield return null;

        GameData.instance.CollectionUnlock(mData.name);

        mIsLocked = false;

        StartCoroutine(DoMoveUnlockDisplay());

        if(unlockedCallback != null)
            unlockedCallback();
    }

    IEnumerator DoMoveUnlockDisplay() {
        if(!unlockMoveDisplayRoot)
            yield break;

        Vector2 startPos = transform.position;
        Vector2 endPos = HUD.instance.notebookIconRoot.position;

        unlockMoveDisplayRoot.position = startPos;
        unlockMoveDisplayRoot.gameObject.SetActive(true);

        if(unlockMoveStartDelay > 0f)
            yield return new WaitForSeconds(unlockMoveStartDelay);

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);
                
        float curT = 0f;
        while(curT < unlockMoveDelay) {
            yield return null;
            curT += Time.deltaTime;

            float t = easeFunc(curT, unlockMoveDelay, 0f, 0f);

            unlockMoveDisplayRoot.position = Vector2.Lerp(startPos, endPos, t);
        }

        unlockMoveDisplayRoot.gameObject.SetActive(false);
    }
}
