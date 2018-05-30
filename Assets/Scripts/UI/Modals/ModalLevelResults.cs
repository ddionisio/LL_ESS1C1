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

    [System.Serializable]
    public class QuestionData {
        public GameObject questionGO;
        public QuizAnswerWidget[] answers;        
        public int answerCorrectIndex;
        public bool answerDisableShuffle;

        public int score { get; private set; }
        public int wrongCount { get; private set; }

        public event System.Action<int, bool> answerCallback;

        private Vector2[] mAnswerLocalPoints;

        public void Init() {
            //grab anchor points, assumes all answers are on the same parent and of uniform dimension
            mAnswerLocalPoints = new Vector2[answers.Length];
            for(int i = 0; i < answers.Length; i++) {
                mAnswerLocalPoints[i] = answers[i].transform.localPosition;

                //setup answer callback
                answers[i].clickCallback += OnAnswerSubmit;
            }
        }

        public void Start() {
            //shuffle points
            //shuffle answer points
            if(!answerDisableShuffle)
                M8.ArrayUtil.Shuffle(mAnswerLocalPoints);

            //initialize answers
            for(int i = 0; i < answers.Length; i++) {
                answers[i].transform.localPosition = mAnswerLocalPoints[i];

                answers[i].Init(i);
            }

            score = 0;
            wrongCount = 0;
        }

        void OnAnswerSubmit(int answerInd) {
            bool isCorrect = answerCorrectIndex == answerInd;
            if(isCorrect) { //correct
                //set answer index to correct
                //lock other answers
                for(int i = 0; i < answers.Length; i++) {
                    if(i == answerInd)
                        answers[i].state = QuizAnswerWidget.State.Correct;
                    else if(answers[i].state == QuizAnswerWidget.State.Normal)
                        answers[i].state = QuizAnswerWidget.State.Locked;
                }

                //compute score
                int totalPoints = GameData.instance.quizBonusPoints * (answers.Length - 1);

                score = Mathf.Clamp(totalPoints - (GameData.instance.quizBonusPoints * wrongCount), 0, int.MaxValue);
            }
            else { //wrong
                answers[answerInd].state = QuizAnswerWidget.State.Wrong;
                wrongCount++;

                //show points deduction?
            }

            if(answerCallback != null)
                answerCallback(answerInd, isCorrect);
        }
    }

    [Header("GameObject Roots")]
    public GameObject resultGO;
    public GameObject questionGO;
    public GameObject resultEndGO;
    public GameObject nextGO;

    [Header("Data")]
    public QuestionData[] questions;
    public float questionStartDelay = 0.4f;

    public Text resultScoreLabel;
    
    [Header("Result Animations")]
    public M8.Animator.AnimatorData animator;
    public string takeResultEnter;
    public string takeResultExit;
    public string takeResultEndEnter;
    public string takeResultEndExit;

    public ScoreWidget scoreWidget;

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

    private int mCurQuestionInd;
    private int mCurScore;

    private State mState = State.Invalid;
    private Coroutine mRout;

    public void Next() {
        switch(state) {
            case State.Result:
                questions[mCurQuestionInd].questionGO.SetActive(false);

                //go to the next question?
                if(mCurQuestionInd < questions.Length-1) {
                    if(nextGO) nextGO.SetActive(false);

                    mCurQuestionInd++;
                    mRout = StartCoroutine(DoQuestionShow());
                }
                else {
                    //add up score
                    if(scoreWidget)
                        scoreWidget.Init(LoLManager.instance.curScore, mCurScore);
                    else
                        LoLManager.instance.curScore += mCurScore;

                    state = State.ResultExit;
                }
                break;

            case State.ResultEnd:
                //exit out
                state = State.ResultEndExit;
                break;
        }
    }

    void Awake() {
        for(int i = 0; i < questions.Length; i++) {
            questions[i].Init();
            questions[i].answerCallback += OnAnswerSubmit;
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
                //if(resultScoreLabel)
                    //resultScoreLabel.text = mResultScore.ToString(); //leading zeros?

                mRout = StartCoroutine(DoResultEndEnter());
                break;

            case State.ResultEndExit:
                if(nextGO) nextGO.SetActive(false);

                mRout = StartCoroutine(DoResultEndExit());
                break;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        for(int i = 0; i < questions.Length; i++) {
            questions[i].Start();
            questions[i].questionGO.SetActive(false);
        }

        mCurQuestionInd = 0;
        mCurScore = 0;

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
        
        mRout = StartCoroutine(DoQuestionShow());
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

    IEnumerator DoQuestionShow() {
        yield return new WaitForSeconds(questionStartDelay);

        questions[mCurQuestionInd].questionGO.SetActive(true);

        mRout = null;
    }

    void OnAnswerSubmit(int answerInd, bool isCorrect) {
        var question = questions[mCurQuestionInd];

        if(isCorrect) {
            mCurScore += question.score;

            //show next
            if(nextGO) nextGO.SetActive(true);
        }
    }
}
