using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalNotebook : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    public const string modalName = "notebook";
    public const string parmPageIndex = "pgInd";

    [System.Serializable]
    public struct PageData {
        public GameObject rootGO;
        public TabWidget tab;

        [M8.Localize]
        public string titleTextRef;
    }

    public Text titleLabel;
    public PageData[] pages;

    private int mCurPageActiveInd;

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

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(parms != null)
            mCurPageActiveInd = parms.GetValue<int>(parmPageIndex);
        else
            mCurPageActiveInd = 0;

        //setup pages
        ApplyCurrentPageActive();
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
