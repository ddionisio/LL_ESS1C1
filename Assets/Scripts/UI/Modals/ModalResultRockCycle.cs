using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalResultRockCycle : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public enum State {
        Invalid,
        Result,
        ResultExit,
        ResultEnd,
        ResultEndExit
    }

    public enum ResultType {
        Sediments,
        Magma,
        Igneous,
        Metamorphic,
        Sedimentary
    }

    public enum RockAction {
        CompactCement,
        HeatPressure,
        WeatheringErosion,
        Melt,
        Cool
    }

    [System.Serializable]
    public class RockOutputData {
        public ResultType resultKey;

        public GameObject lockedGO;

        public GameObject unlockedGO;
        public Transform unlockedRoot;

        public bool isLocked {
            get { return lockedGO.activeSelf; }
            set {
                lockedGO.SetActive(value);
                unlockedGO.SetActive(!value);
            }
        }
    }

    [System.Serializable]
    public class RockResultData {
        public ResultType type;
        public Sprite sprite;
        [M8.Localize]
        public string textRef;
    }

    [System.Serializable]
    public class RockActionToResult {
        public RockAction action;
        public ResultType from;
        public ResultType to;
    }

    [Header("Rock Output")]
    public RockOutputData[] rockOutputs;
    public float outputMoveDelay = 1.0f;

    [Header("Rock Result")]
    public RockResultData[] rockResults;
    public RockActionToResult[] actionToResults;
    public Image rockResultIcon;
    public M8.UI.Texts.Localizer rockResultLocalizer;
    public ResultType[] rockResultStart; //possible starting result
    
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
    public string takeRockResultChange = "rockResultChange";

    public ScoreWidget scoreWidget;

    [Header("SFX")]
    public string sfxPathComplete = "Audio/goal.wav";
    public string sfxPathCorrect = "Audio/correct.wav";
    public string sfxPathWrong = "Audio/wrong.wav";
    public string sfxPathResultChange = "Audio/correct.wav";

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

    private ResultType mCurResultType;

    private int mCurScore;

    private State mState = State.Invalid;
    private Coroutine mRout;

    public void Next() {
        switch(state) {
            case State.Result:
                //add up score
                mCurScore = (GameData.instance.quizBonusPoints / 2) * rockOutputs.Length;

                if(scoreWidget)
                    scoreWidget.Init(LoLManager.instance.curScore, mCurScore);
                else
                    LoLManager.instance.curScore += mCurScore;

                state = State.ResultExit;
                break;

            case State.ResultEnd:
                //exit out
                state = State.ResultEndExit;
                break;
        }
    }

    public void RockAct(int actionIndex) {
        var rockAction = (RockAction)actionIndex;

        //grab action to result index
        bool canConvert = false;
        ResultType convertType = ResultType.Igneous;

        for(int i = 0; i < actionToResults.Length; i++) {
            var actionToResult = actionToResults[i];

            if(mCurResultType == actionToResult.from && rockAction == actionToResult.action) {
                canConvert = true;
                convertType = actionToResult.to;
                break;
            }
        }

        if(canConvert) {
            ApplyResult(convertType, true);

            //check if this result matches output slots
            bool isOutputUnlocked = false;

            for(int i = 0; i < rockOutputs.Length; i++) {
                if(rockOutputs[i].resultKey == convertType) {
                    if(rockOutputs[i].isLocked) {
                        rockOutputs[i].isLocked = false;
                        isOutputUnlocked = true;

                        StartCoroutine(DoOutputCorrect(rockOutputs[i]));
                        break;
                    }
                }
            }

            if(isOutputUnlocked) {
                //check if we got all outputs unlocked
                int unlockCount = 0;
                for(int i = 0; i < rockOutputs.Length; i++) {
                    if(!rockOutputs[i].isLocked)
                        unlockCount++;
                }

                if(unlockCount == rockOutputs.Length) {
                    if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathComplete))
                        LoLManager.instance.PlaySound(sfxPathComplete, false, false);

                    //show next
                    if(nextGO) nextGO.SetActive(true);
                }
                else {
                    if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathCorrect))
                        LoLManager.instance.PlaySound(sfxPathCorrect, false, false);
                }
            }
            else {
                if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathResultChange))
                    LoLManager.instance.PlaySound(sfxPathResultChange, false, false);
            }
        }
        else {
            //error
            if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathWrong))
                LoLManager.instance.PlaySound(sfxPathWrong, false, false);
        }
    }

    RockResultData GetResultData(ResultType type) {
        for(int i = 0; i < rockResults.Length; i++) {
            if(rockResults[i].type == type) {
                return rockResults[i];
            }
        }

        return null;
    }

    void ApplyResult(ResultType type, bool isAnimate) {
        mCurResultType = type;

        RockResultData resultData = GetResultData(type);
        if(resultData == null)
            return;

        if(isAnimate) {
            if(animator && !string.IsNullOrEmpty(takeRockResultChange))
                animator.Play(takeRockResultChange);
        }

        rockResultIcon.sprite = resultData.sprite;
        rockResultIcon.SetNativeSize();

        rockResultLocalizer.key = resultData.textRef;
        rockResultLocalizer.Apply();
    }

    void Awake() {
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
        mCurScore = 0;

        //initialize slots
        for(int i = 0; i < rockOutputs.Length; i++) {
            rockOutputs[i].isLocked = true;
        }

        //initialize result
        ApplyResult(rockResultStart[Random.Range(0, rockResultStart.Length)], false);

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

    IEnumerator DoOutputCorrect(RockOutputData outputData) {
        Vector2 fromPos = rockResultIcon.rectTransform.position;
        Vector2 toPos = outputData.unlockedRoot.position;

        outputData.unlockedRoot.position = toPos;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        float curTime = 0f;
        while(curTime < outputMoveDelay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = easeFunc(curTime, outputMoveDelay, 0f, 0f);

            outputData.unlockedRoot.position = Vector2.Lerp(fromPos, toPos, t);
        }
    }
}
