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
        None = -1,

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

    public Sprite[] actionSprites;

    [Header("Rock Output")]
    public RockOutputData[] rockOutputs;
    public float outputMoveDelay = 1.0f;

    [Header("Rock Result")]
    public RockResultData[] rockResults;
    public RockActionToResult[] actionToResults;    
    public ResultType[] rockResultStart; //possible starting result

    [Header("Rock Input")]
    public Image rockInputIcon;
    public M8.UI.Texts.Localizer rockInputLocalizer;

    public Image rockActionIcon;

    public Image rockOutputIcon;
    
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

    public string takeRockActionProcess = "rockActionProcess"; //apply action, display output, output to input
    public string takeRockActionError = "rockActionError";

    public ScoreWidget scoreWidget;

    [Header("SFX")]
    public string sfxPathComplete = "Audio/goal.wav";
    public string sfxPathCorrect = "Audio/correct.wav";
    public string sfxPathWrong = "Audio/wrong.wav";
    public string sfxPathResultChange = "Audio/correct.wav";

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

    private ResultType mCurResultType;

    private int mCurScore;

    private State mState = State.Invalid;
    private Coroutine mRout;

    public void Next() {
        switch(state) {
            case State.Result:
                //add up score
                mCurScore = GameData.instance.quizBonusPoints * rockOutputs.Length;

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
            RockOutputData outputCorrect = null;

            //check if this result matches output slots
            bool isOutputUnlocked = false;

            for(int i = 0; i < rockOutputs.Length; i++) {
                if(rockOutputs[i].resultKey == convertType) {
                    if(rockOutputs[i].isLocked) {
                        isOutputUnlocked = true;
                        outputCorrect = rockOutputs[i];
                        break;
                    }
                }
            }

            ApplyResult(convertType, true, rockAction, outputCorrect);

            if(isOutputUnlocked) {
                //check if we got all outputs unlocked
                int unlockCount = 1; //count the unlock found
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
        else { //error
            if(animator && !string.IsNullOrEmpty(takeRockActionError))
                animator.Play(takeRockActionError);

            errorCounter.Increment();
                        
            if(LoLManager.isInstantiated && !string.IsNullOrEmpty(sfxPathWrong))
                LoLManager.instance.PlaySound(sfxPathWrong, false, false);
        }
    }

    public void ApplyIconOutputToInput() {
        rockInputIcon.sprite = rockOutputIcon.sprite;
        rockInputIcon.SetNativeSize();
    }

    RockResultData GetResultData(ResultType type) {
        for(int i = 0; i < rockResults.Length; i++) {
            if(rockResults[i].type == type) {
                return rockResults[i];
            }
        }

        return null;
    }

    void ApplyResult(ResultType type, bool isAnimate, RockAction rockAction, RockOutputData outputCorrect) {
        mCurResultType = type;

        RockResultData resultData = GetResultData(type);
        if(resultData == null)
            return;

        if(rockAction != RockAction.None) {
            rockActionIcon.sprite = actionSprites[(int)rockAction];
            rockActionIcon.SetNativeSize();
        }

        //setup output, will be applied to input after animation
        rockOutputIcon.sprite = resultData.sprite;
        rockOutputIcon.SetNativeSize();
        
        //prepare input text
        rockInputLocalizer.key = resultData.textRef;
        rockInputLocalizer.Apply();

        if(isAnimate) { //do the action animation, and process output to result, then update input
            StartCoroutine(DoResult(outputCorrect));
        }
        else {
            //apply icon
            rockInputIcon.sprite = resultData.sprite;
            rockInputIcon.SetNativeSize();

            //reset display state
            rockInputIcon.gameObject.SetActive(true);
            rockInputLocalizer.gameObject.SetActive(true);
            rockActionIcon.gameObject.SetActive(false);
            rockOutputIcon.gameObject.SetActive(false);
        }
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
        ApplyResult(rockResultStart[Random.Range(0, rockResultStart.Length)], false, RockAction.None, null);

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

    IEnumerator DoResult(RockOutputData outputData) {        
        rockInputLocalizer.gameObject.SetActive(false);

        if(animator && !string.IsNullOrEmpty(takeRockActionProcess)) {
            animator.Play(takeRockActionProcess);
            while(animator.isPlaying)
                yield return null;
        }

        rockInputLocalizer.gameObject.SetActive(true);

        if(outputData != null)
            StartCoroutine(DoOutputCorrect(outputData));
    }

    IEnumerator DoOutputCorrect(RockOutputData outputData) {
        Vector2 fromPos = rockInputIcon.rectTransform.position;
        Vector2 toPos = outputData.unlockedRoot.position;

        outputData.isLocked = false;

        outputData.unlockedRoot.position = fromPos;
        
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
