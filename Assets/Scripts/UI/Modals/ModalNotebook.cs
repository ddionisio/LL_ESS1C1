using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalNotebook : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public const string modalName = "notebook";
    public const string parmPageIndex = "pgInd";

    [System.Serializable]
    public struct PageData {
        public string name;

        public GameObject rootGO;
        public TabWidget tab;

        [M8.Localize]
        public string titleTextRef;
    }

    public Text titleLabel;
    public PageData[] pages;

    private int mCurPageActiveInd;

    private bool mIsPaused;

    private static M8.GenericParams mParms = new M8.GenericParams();

    public static void Open(int activePageInd) {
        mParms[parmPageIndex] = activePageInd;

        M8.UIModal.Manager.instance.ModalOpen(modalName, mParms);
    }

    public void SetPageActive(int ind) {
        if(mCurPageActiveInd != ind) {
            mCurPageActiveInd = ind;
            ApplyCurrentPageActive();
        }
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);
    }

    void OnDestroy() {
        //fail-safe
        Pause(false);

        UnhookInput();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        Pause(true);

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);

        if(parms != null)
            mCurPageActiveInd = parms.GetValue<int>(parmPageIndex);
        else
            mCurPageActiveInd = 0;

        //setup pages
        ApplyCurrentPageActive();
    }

    void M8.UIModal.Interface.IPop.Pop() {
        Pause(false);

        UnhookInput();
    }

    void OnInputEscape(M8.InputManager.Info data) {
        if(data.state == M8.InputManager.State.Released)
            Close();
    }

    void UnhookInput() {
        if(M8.InputManager.instance)
            M8.InputManager.instance.RemoveButtonCall(OnInputEscape);
    }

    void Pause(bool pause) {
        if(mIsPaused != pause) {
            mIsPaused = pause;

            if(M8.SceneManager.instance) {
                if(mIsPaused)
                    M8.SceneManager.instance.Pause();
                else
                    M8.SceneManager.instance.Resume();
            }
        }
    }

    private void ApplyCurrentPageActive() {
        for(int i = 0; i < pages.Length; i++) {
            var page = pages[i];

            if(page.rootGO) page.rootGO.SetActive(i == mCurPageActiveInd);

            if(page.tab) page.tab.state = i == mCurPageActiveInd ? TabWidget.State.Active : TabWidget.State.Inactive;
        }

        if(titleLabel) titleLabel.text = M8.Localize.Get(pages[mCurPageActiveInd].titleTextRef);
    }
}
