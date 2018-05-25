using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreWidget : MonoBehaviour {
    public Text currentScoreLabel;
    public Text bonusScoreLabel;
    public Text resultScoreLabel;

    public float resultStartDelay;
    public float resultIncDelay = 0.5f;

    private int mCurScore;
    private int mBonusScore;

    public void Init(int curScore, int bonusScore) {
        mCurScore = curScore;
        mBonusScore = bonusScore;

        if(currentScoreLabel) currentScoreLabel.text = mCurScore.ToString();
        if(bonusScoreLabel) bonusScoreLabel.text = "+" + mBonusScore.ToString();
        if(resultScoreLabel) resultScoreLabel.text = "0";
    }

    public void PlayResult() {
        if(!resultScoreLabel)
            return;

        StartCoroutine(DoPlayResult());
    }

    IEnumerator DoPlayResult() {
        if(resultStartDelay > 0f)
            yield return new WaitForSeconds(resultStartDelay);

        float startScore = mCurScore;
        float endScore = mCurScore + mBonusScore;

        float curTime = 0f;
        while(curTime < resultIncDelay) {
            yield return null;
            curTime += Time.deltaTime;

            float t = Mathf.Clamp01(curTime / resultIncDelay);

            int score = Mathf.RoundToInt(Mathf.Lerp(startScore, endScore, t));

            resultScoreLabel.text = score.ToString();
        }        
    }
}
