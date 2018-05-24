using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalLevelResults : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public enum State {
        Invalid,
        Result,
        ResultExit,
        ResultEnd,
        ResultEndExit
    }

    [Header("GameObject Roots")]
    public GameObject resultGO;
    public GameObject questionGO;
    public GameObject resultEndGO;
    public GameObject nextGO;

    [Header("Data")]
    public QuizAnswerWidget[] answers;
    public int answerCorrectIndex;

    public Text resultScoreLabel;
    
    [Header("Result Animations")]
    public M8.Animator.AnimatorData animator;
    public string takeResultEnter;
    public string takeResultExit;
    public string takeResultEndEnter;
    public string takeResultEndExit;

    [Header("Signals")]
    public M8.Signal signalExit;

    protected State state {
        get { return mState; }
        set {
            if(mState != value) {
                mState = value;

                if(mRout != null) {
                    StopCoroutine(mRout);
                    mRout = null;
                }

                ApplyState();
            }
        }
    }

    private Vector2[] mAnswerLocalPoints;

    private State mState = State.Invalid;
    private Coroutine mRout;

    private int mWrongCount;
    private int mResultScore;
        
    public void Next() {
        switch(state) {
            case State.Result:
                state = State.ResultExit;
                break;

            case State.ResultEnd:
                //exit out
                state = State.ResultEndExit;
                break;
        }
    }

    void Awake() {
        //grab anchor points, assumes all answers are on the same parent and of uniform dimension
        mAnswerLocalPoints = new Vector2[answers.Length];
        for(int i = 0; i < answers.Length; i++) {
            mAnswerLocalPoints[i] = answers[i].transform.localPosition;

            //setup answer callback
            answers[i].clickCallback += OnAnswerSubmit;
        }

        mState = State.Invalid;
        ApplyState();
    }

    void ApplyState() {
        switch(mState) {
            case State.Invalid:
                if(resultGO) resultGO.SetActive(false);
                if(questionGO) questionGO.SetActive(false);
                if(resultEndGO) resultEndGO.SetActive(false);
                if(nextGO) nextGO.SetActive(false);
                break;

            case State.Result:
                mRout = StartCoroutine(DoResultEnter());
                break;

            case State.ResultExit:
                if(nextGO) nextGO.SetActive(false); //assume next was pressed to proceed to end

                mRout = StartCoroutine(DoResultExit());
                break;

            case State.ResultEnd:
                if(nextGO) nextGO.SetActive(true);

                //apply score to text
                if(resultScoreLabel)
                    resultScoreLabel.text = mResultScore.ToString(); //leading zeros?

                mRout = StartCoroutine(DoResultEndEnter());
                break;

            case State.ResultEndExit:
                if(nextGO) nextGO.SetActive(false);

                mRout = StartCoroutine(DoResultEndExit());
                break;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        
        //shuffle answer points
        M8.ArrayUtil.Shuffle(mAnswerLocalPoints);

        //initialize answers
        for(int i = 0; i < answers.Length; i++) {
            answers[i].transform.localPosition = mAnswerLocalPoints[i];

            answers[i].Init(i);
        }

        mWrongCount = 0;

        state = State.Result;
    }

    void M8.UIModal.Interface.IPop.Pop() {
        state = State.Invalid;
    }

    IEnumerator DoResultEnter() {
        if(animator && !string.IsNullOrEmpty(takeResultEnter)) {
            animator.Play(takeResultEnter);
            while(animator.isPlaying)
                yield return null;
        }
        
        mRout = null;
    }

    IEnumerator DoResultExit() {
        if(animator && !string.IsNullOrEmpty(takeResultExit)) {
            animator.Play(takeResultExit);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        state = State.ResultEnd;
    }

    IEnumerator DoResultEndEnter() {
        if(animator && !string.IsNullOrEmpty(takeResultEndEnter)) {
            animator.Play(takeResultEndEnter);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;
    }

    IEnumerator DoResultEndExit() {
        if(animator && !string.IsNullOrEmpty(takeResultEndExit)) {
            animator.Play(takeResultEndExit);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;
                
        Close();

        //let listeners know
        if(signalExit)
            signalExit.Invoke();
    }

    void OnAnswerSubmit(int answerInd) {
        if(answerCorrectIndex == answerInd) { //correct
            //set answer index to correct
            //lock other answers
            for(int i = 0; i < answers.Length; i++) {
                if(i == answerInd)
                    answers[i].state = QuizAnswerWidget.State.Correct;
                else if(answers[i].state == QuizAnswerWidget.State.Normal)
                    answers[i].state = QuizAnswerWidget.State.Locked;
            }

            //compute score
            mResultScore = Mathf.Clamp(GameData.instance.quizTotalPoints - (GameData.instance.quizWrongPointDeduct * mWrongCount), 0, int.MaxValue);

            LoLManager.instance.curScore += mResultScore;
                        
            //show next
            if(nextGO) nextGO.SetActive(true);
        }
        else { //wrong
            answers[answerInd].state = QuizAnswerWidget.State.Wrong;
            mWrongCount++;

            //show points deduction?
        }
    }
}
