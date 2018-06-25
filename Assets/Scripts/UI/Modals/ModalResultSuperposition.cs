using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModalResultSuperposition : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public enum State {
        Invalid,
        Result,
        ResultExit,
        ResultEnd,
        ResultEndExit
    }

    [System.Serializable]
    public class AnswerSlot {
        public int answerIndex;
        public RectTransform area;
        public GameObject highlightGO;
        public GameObject errorGO;

        public bool isHighlighted { get { return highlightGO && highlightGO.activeSelf; } set { if(highlightGO) highlightGO.SetActive(value); } }

        public bool isLocked { get; set; }
        
        private Rect mWorldRect;

        private static Vector3[] _cornerBuffer = new Vector3[4];

        private static Rect GenerateWorldRect(RectTransform rt) {
            rt.GetWorldCorners(_cornerBuffer);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for(int i = 0; i < _cornerBuffer.Length; i++) {
                Vector2 pt = _cornerBuffer[i];

                if(pt.x < min.x) min.x = pt.x;
                if(pt.x > max.x) max.x = pt.x;
                if(pt.y < min.y) min.y = pt.y;
                if(pt.y > max.y) max.y = pt.y;
            }

            return new Rect() { min = min, max = max };
        }

        public void Init() {
            mWorldRect = GenerateWorldRect(area);
        }

        public void Start() {
            isHighlighted = false;

            isLocked = false;

            if(errorGO) errorGO.SetActive(false);
        }

        public void Error() {
            if(errorGO) errorGO.SetActive(true);
        }

        public bool CheckAreaAndUpdateHighlight(RectTransform rt) {
            var rtRect = GenerateWorldRect(rt);

            bool isIntersect = mWorldRect.Overlaps(rtRect);

            isHighlighted = isIntersect;

            return isIntersect;
        }
    }

    [Header("Data")]
    public AnswerSlot[] answerSlots;
    public QuizAnswerDragWidget[] answers;

    [Header("GameObject Roots")]
    public GameObject resultGO;
    public GameObject resultEndGO;
    public GameObject nextGO;

    [Header("Result Animations")]
    public M8.Animator.AnimatorData animator;
    public string takeResultEnter = "resultEnter";
    public string takeResultExit = "resultExit";
    public string takeResultEndEnter = "resultEndEnter";
    public string takeResultEndExit = "resultEndExit";

    public ScoreWidget scoreWidget;

    [Header("SFX")]
    public string sfxPathComplete = "Audio/goal.wav";
    public string sfxPathCorrect = "Audio/correct.wav";
    public string sfxPathWrong = "Audio/wrong.wav";

    [Header("Error")]
    public ErrorCounterWidget errorCounter;
    public Transform errorDecrementPoint;
    public float errorDecrementEndDelay = 0.3f;

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
    
    private int mCurScore;

    private State mState = State.Invalid;
    private Coroutine mRout;
    
    private Vector2[] mAnswerPositions;

    public void Next() {
        switch(state) {
            case State.Result:
                //add up score
                /*int errorDeduction = Mathf.RoundToInt(GameData.instance.quizBonusPoints * 0.25f);
                int total = GameData.instance.quizBonusPoints * answerSlots.Length;
                int minScore = GameData.instance.quizBonusPoints / 2;

                mCurScore = Mathf.Clamp(total - errorDeduction * mErrorCount, minScore, total);*/

                //add up score
                mCurScore = GameData.instance.quizBonusPoints * answerSlots.Length;

                int curTotalScore = LoLManager.instance.curScore;

                if(scoreWidget)
                    scoreWidget.Init(curTotalScore, mCurScore);

                state = State.ResultExit;
                break;

            case State.ResultEnd:
                //exit out
                state = State.ResultEndExit;
                break;
        }
    }

    void Awake() {
        //init answer slots
        for(int i = 0; i < answerSlots.Length; i++) {
            answerSlots[i].Init();
        }

        //grab original positions of answers, setup callback
        mAnswerPositions = new Vector2[answers.Length];
        for(int i = 0; i < answers.Length; i++) {
            mAnswerPositions[i] = answers[i].transform.position;

            answers[i].dragCallback += OnAnswerDrag;
            answers[i].dragEndCallback += OnAnswerEndDrag;
        }

        mState = State.Invalid;
        ApplyState();
    }

    void ApplyState() {
        switch(mState) {
            case State.Invalid:
                if(resultGO) resultGO.SetActive(false);
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
        M8.ArrayUtil.Shuffle(mAnswerPositions);

        //start slots
        for(int i = 0; i < answerSlots.Length; i++) {
            answerSlots[i].Start();
        }

        //start answers
        for(int i = 0; i < answers.Length; i++) {
            answers[i].Init(i, mAnswerPositions[i]);
        }

        mCurScore = 0;

        errorCounter.Init();

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

        int errorCount = errorCounter.count;

        if(errorCounter.errorGOs.Length > 0) {
            int errorScoreDeduction = (mCurScore - Mathf.RoundToInt(mCurScore * 0.1f)) / errorCounter.errorGOs.Length;

            //reduce bonus score based on error counter
            while(errorCounter.count > 0) {
                errorCounter.DecrementTransfer(errorDecrementPoint.position);
                while(errorCounter.isPlaying)
                    yield return null;

                mCurScore -= errorScoreDeduction;
                scoreWidget.UpdateBonusScore(mCurScore);

                if(errorDecrementEndDelay > 0f)
                    yield return new WaitForSeconds(errorDecrementEndDelay);
            }
        }

        scoreWidget.PlayResult();

        GameData.instance.SetCurrentLevelScore(mCurScore, errorCount);

        LoLManager.instance.curScore += mCurScore;

        if(nextGO) nextGO.SetActive(true);

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

    void OnAnswerDrag(QuizAnswerDragWidget answer, PointerEventData eventData) {
        //update which slot is highlighted
        int curHighlightInd = -1;

        for(int i = 0; i < answerSlots.Length; i++) {
            if(answerSlots[i].isLocked) //already filled
                continue;

            if(answerSlots[i].CheckAreaAndUpdateHighlight(answer.rTransform)) {
                curHighlightInd = i;
                break;
            }
        }

        //remove previous highlight
        if(curHighlightInd != -1) {
            for(int i = 0; i < answerSlots.Length; i++) {
                if(curHighlightInd == i || answerSlots[i].isLocked)
                    continue;

                answerSlots[i].isHighlighted = false;
            }
        }
    }

    void OnAnswerEndDrag(QuizAnswerDragWidget answer, PointerEventData eventData) {
        //check which slot is highlighted from drag
        AnswerSlot slot = null;

        for(int i = 0; i < answerSlots.Length; i++) {
            if(answerSlots[i].isLocked) //already filled
                continue;

            if(answerSlots[i].isHighlighted) {
                slot = answerSlots[i];
                break;
            }
        }

        if(slot != null) {
            slot.isHighlighted = false;

            //check if slot is correct
            if(answer.index == slot.answerIndex) {
                slot.isLocked = true;                

                answer.transform.position = slot.area.position;
                answer.Place();
                answer.isCorrect = true;

                //check if all slots are set
                int correctCount = 0;
                for(int i = 0; i < answerSlots.Length; i++) {
                    if(answerSlots[i].isLocked)
                        correctCount++;
                }

                if(correctCount == answerSlots.Length) {
                    //all slots answered
                    if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathComplete))
                        LoLManager.instance.PlaySound(sfxPathComplete, false, false);

                    //set all other answers as locked
                    for(int i = 0; i < answers.Length; i++) {
                        answers[i].isDragLocked = true;
                    }

                    if(nextGO) nextGO.SetActive(true);
                }
                else {
                    if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathCorrect))
                        LoLManager.instance.PlaySound(sfxPathCorrect, false, false);
                }
            }
            else { //error
                if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathWrong))
                    LoLManager.instance.PlaySound(sfxPathWrong, false, false);

                slot.Error();

                errorCounter.Increment();

                answer.Revert();
            }
        }
        else {
            //revert
            answer.Revert();
        }
    }
}
