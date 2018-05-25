using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IPush {

    public ScoreWidget scoreWidget;

    private int mCurScore;
    private int mBonusScore;

    public void Proceed() {
        Close();

        if(LoLManager.isInstantiated)
            LoLManager.instance.curScore = mCurScore + mBonusScore;

        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(scoreWidget) {
            mCurScore = LoLManager.isInstantiated ? LoLManager.instance.curScore : 0;
            mBonusScore = GameMapController.isInstantiated ? GameMapController.instance.score : 0;

            scoreWidget.Init(mCurScore, mBonusScore);
        }
    }
}
