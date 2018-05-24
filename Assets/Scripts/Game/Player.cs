using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : M8.EntityBase {
    public enum PhysicsMode {
        Disabled,
        Static,
        Dynamic
    }

    public PlayerData data;

    public GameObject displayGO;
    public GameObject inputIndicatorGO;

    [Header("Animation")]
    public M8.Animator.AnimatorData animator;
    public string takeSpawn;
    public string takeLaunch;
    public string takeLaunchEnd;

    [Header("Spawn")]
    public GameObject spawnGO;
    public Transform spawnCannonRoot;
        
    [Header("Signals")]
    public SignalBool signalCanJumpUpdate; //update on when we can explode
    public M8.Signal signalJump;
    public M8.Signal signalGoal; //triggered goal
    public M8.Signal signalDeath;

    public Vector2 moveDir { get; set; }
    public float movePower { get; set; }

    public CircleCollider2D physicsCircleCollider { get; private set; }
    public Rigidbody2D physicsBody { get; private set; }

    public bool isGrounded { get { return mGroundCollContacts.Count > 0; } }
    public Vector2 groundMoveDir { get { return mGroundMoveDir; } }

    public float cannonToMoveDirT {
        get { return mCannonToMoveDirT; }
        set {
            mCannonToMoveDirT = value;

            if(spawnCannonRoot)
                spawnCannonRoot.up = Vector2.Lerp(Vector2.up, moveDir, mCannonToMoveDirT);
        }
    }

    //if true, we can explode at explodablePosition
    public bool canJump {
        get { return mCanJump; }
        private set {
            if(mCanJump != value) {
                mCanJump = value;

                if(inputIndicatorGO) inputIndicatorGO.SetActive(mCanJump);

                if(signalCanJumpUpdate) signalCanJumpUpdate.Invoke(mCanJump);
            }
        }
    }
    
    public Vector2 jumpPosition { get { return mJumpCheckHit.point; } }

    public bool isMoveSpeedLimit { get { return mIsMoveSpeedLimit; } set { mIsMoveSpeedLimit = value; } }
    public bool isMoveActive { get { return mIsMoveActive; } set { mIsMoveActive = value; } }

    protected PhysicsMode physicsMode {
        get { return mPhysicsMode; }
        set {
            if(mPhysicsMode != value) {
                mPhysicsMode = value;
                ApplyPhysicsMode();
            }
        }
    }

    private PhysicsMode mPhysicsMode;

    private const int contactCacheCount = 16;

    private ContactPoint2D[] mContactPoints = new ContactPoint2D[contactCacheCount];
    private int mContactPointsCount;

    private M8.CacheList<Collider2D> mGroundCollContacts = new M8.CacheList<Collider2D>(contactCacheCount);

    private float mGroundSlopeLimitCos;
    private float mAboveLimitCos;

    private Vector2 mGameCamCurVel;

    private Coroutine mRout;
    
    private Vector2 mVictoryPos;

    private bool mCanJump;
    private RaycastHit2D mJumpCheckHit;

    private float mLastJumpTime;
        
    private Vector2 mGroundMoveDir = Vector2.zero;

    private bool mIsMoveSpeedLimit = true;
    private bool mIsMoveActive = true;
    private bool mIsMoveTrigger = false;

    private float mCannonToMoveDirT;

    /// <summary>
    /// Apply current move power towards move dir, this will set player state to move
    /// </summary>
    public void Launch() {
        state = (int)EntityState.PlayerLaunch;
    }

    public void Jump() {
        if(canJump) {
            //spawn explosion at explode hit
            var delta = (physicsBody.position - jumpPosition);

            float jumpDist = delta.magnitude;

            float deltaDist;
            if(data.jumpUplift != 0f) {
                delta.y += data.jumpUplift;

                deltaDist = delta.magnitude;
            }
            else
                deltaDist = jumpDist;

            if(deltaDist > 0f) {
                var dir = delta / deltaDist;

                Vector3 baseForce = dir * data.jumpPower;
                physicsBody.AddForceAtPosition(baseForce, jumpPosition, ForceMode2D.Impulse);
            }

            mLastJumpTime = Time.time;
            canJump = false;
        }
    }

    public void FlipGroundMoveDir() {
        mGroundMoveDir.x *= -1f;
    }

    public void Victory(Vector2 goalPos) {
        mVictoryPos = goalPos;

        state = (int)EntityState.Victory;
    }

    protected override void OnDespawned() {
        //reset stuff here
        ClearRoutine();

        physicsMode = PhysicsMode.Disabled;

        //hide display
        if(displayGO) displayGO.SetActive(false);

        //hide spawn
        if(spawnGO) spawnGO.SetActive(false);
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //stats

        //start ai, player control, etc
    }

    protected override void StateChanged() {
        var prevEntityState = (EntityState)prevState;
        var curEntityState = (EntityState)state;

        ClearRoutine();

        switch(curEntityState) {
            case EntityState.Spawn:
                physicsMode = PhysicsMode.Disabled;

                //hide display
                if(displayGO) displayGO.SetActive(false);

                //set spawn position to player
                if(spawnGO) spawnGO.transform.position = transform.position;

                if(spawnGO) spawnGO.SetActive(true);

                //preemptive ground move dir for those that need it during launch
                mGroundMoveDir.x = Mathf.Sign(moveDir.x);

                mRout = StartCoroutine(DoSpawn());
                break;
                
            case EntityState.PlayerLaunchReady:
                physicsMode = PhysicsMode.Disabled;
                break;

            case EntityState.PlayerLaunch:
                physicsMode = PhysicsMode.Disabled;

                mRout = StartCoroutine(DoLaunch());
                break;

            case EntityState.PlayerLock:
                physicsMode = PhysicsMode.Disabled;
                break;

            case EntityState.PlayerMove:
                physicsMode = PhysicsMode.Dynamic;

                //show display
                if(displayGO) displayGO.SetActive(true);

                mGameCamCurVel = Vector2.zero;

                mLastJumpTime = Time.time;
                break;

            case EntityState.Death:
                physicsMode = PhysicsMode.Disabled;

                mRout = StartCoroutine(DoDeath());
                break;

            case EntityState.Victory:
                physicsMode = PhysicsMode.Disabled;

                mGameCamCurVel = Vector2.zero;

                mRout = StartCoroutine(DoVictory());
                break;
        }

        data.InvokeStateSignal(curEntityState);
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //hide display
        if(displayGO) displayGO.SetActive(false);
        if(inputIndicatorGO) inputIndicatorGO.SetActive(false);
        if(spawnGO) spawnGO.SetActive(false);

        //initialize data/variables

        physicsCircleCollider = GetComponent<CircleCollider2D>();
        physicsBody = GetComponent<Rigidbody2D>();

        ApplyPhysicsSettings();

        mPhysicsMode = PhysicsMode.Disabled;
        ApplyPhysicsMode();
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void Update() {
#if UNITY_EDITOR
        //editor mode update for settings
        if(physicsMode == PhysicsMode.Dynamic)
            ApplyPhysicsSettings();
#endif

        EntityState entState = (EntityState)state;
        switch(entState) {
            case EntityState.PlayerMove:
                //camera follow
                CameraFollowUpdate();
                UpdateJump();
                break;
        }
    }

    void FixedUpdate() {
        EntityState entState = (EntityState)state;
        switch(entState) {
            case EntityState.PlayerMove:
                if(isGrounded || mIsMoveTrigger) {
                    //check speed limit (NOTE: don't try this at home)
                    var curVel = physicsBody.velocity;
                    var speedX = Mathf.Abs(curVel.x);
                    if(speedX > data.moveSpeedLimit) {
                        if(isMoveSpeedLimit) {
                            curVel.x = Mathf.Sign(curVel.x) * data.moveSpeedLimit;
                            physicsBody.velocity = curVel;
                        }
                    }
                    else if(isMoveActive) {
                        //move
                        physicsBody.AddForce(mGroundMoveDir * data.moveForce, ForceMode2D.Force);
                    }
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        mContactPointsCount = coll.GetContacts(mContactPoints);

        //don't do anything else if we are not dynamic
        if(physicsMode != PhysicsMode.Dynamic && mContactPointsCount > 0)
            return;

        //fancy fx

        float contactSumCount = 0f;
        Vector2 contactSum = Vector2.zero;

        int nearestSideContactPointInd = -1;
        float nearestSideSeparation = float.MaxValue;
                        
        for(int i = 0; i < mContactPointsCount; i++) {
            var contactPt = mContactPoints[i];

            if(!contactPt.collider)
                continue;

            //check if we contact ground

            //check if it exists
            int groundCollContactInd = -1;
            for(int j = 0; j < mGroundCollContacts.Count; j++) {
                if(contactPt.collider == mGroundCollContacts[j]) {
                    groundCollContactInd = j;
                    break;
                }
            }

            if(groundCollContactInd != -1)
                continue;

            //check normal
            float dot = Vector2.Dot(Vector2.up, contactPt.normal);
            float separation = contactPt.separation;

            if(dot >= mGroundSlopeLimitCos) {
                //ground
                mGroundCollContacts.Add(contactPt.collider);
            }
            else if(dot <= mAboveLimitCos) {
                //ceilling
            }
            else {
                //side
                if(nearestSideContactPointInd == -1 || separation < nearestSideSeparation) {
                    nearestSideContactPointInd = i;
                    nearestSideSeparation = separation;
                }
            }

            contactSum += contactPt.point;
            contactSumCount += 1f;
        }

        //update ground move
        if(nearestSideContactPointInd != -1) {
            mGroundMoveDir.x = Mathf.Sign(mContactPoints[nearestSideContactPointInd].normal.x);

            //wall explode
            if(!isGrounded) {
                GamePool.instance.ExplodeAt(GamePool.ExplodeTypes.explodeWall, mContactPoints[nearestSideContactPointInd].point);
                //physicsBody.AddForce(mGroundMoveDir * data.wallImpulse, ForceMode2D.Impulse);
            }
        }
    }

    void OnCollisionExit2D(Collision2D coll) {
        mContactPointsCount = coll.GetContacts(mContactPoints);

        //don't do anything else if we are not dynamic
        if(physicsMode != PhysicsMode.Dynamic)
            return;

        if(mContactPointsCount > 0) {
            //check if we left any ground
            for(int i = 0; i < mContactPointsCount; i++) {
                var contactPt = mContactPoints[i];

                //ignore contacts with no collider
                if(!contactPt.collider)
                    continue;

                for(int j = 0; j < mGroundCollContacts.Count; j++) {
                    if(contactPt.collider == mGroundCollContacts[j]) {
                        mGroundCollContacts.RemoveAt(j);
                        break;
                    }
                }
            }
        }
        else { //check single collider
            //check if we left any ground            
            for(int i = 0; i < mGroundCollContacts.Count; i++) {
                if(coll.collider == mGroundCollContacts[i]) {
                    mGroundCollContacts.RemoveAt(i);
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(data.moveTriggerTag) && collision.gameObject.CompareTag(data.moveTriggerTag)) {
            mIsMoveTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(mIsMoveTrigger && collision.gameObject.CompareTag(data.moveTriggerTag)) {
            mIsMoveTrigger = false;
            Debug.Log("done");
        }
    }

    private void ApplyPhysicsSettings() {
        mGroundSlopeLimitCos = Mathf.Cos(data.groundSlopeAngleLimit * Mathf.Deg2Rad);
        mAboveLimitCos = Mathf.Cos(data.aboveAngleLimit * Mathf.Deg2Rad);

        physicsBody.useAutoMass = true;
        physicsBody.drag = data.drag;
        physicsBody.angularDrag = data.dragAngular;

        physicsCircleCollider.density = data.density;
    }

    private void ApplyPhysicsMode() {
        //clear out physics data
        ClearPhysicsData();

        switch(mPhysicsMode) {
            case PhysicsMode.Disabled:
                physicsCircleCollider.enabled = false;
                physicsBody.simulated = false;
                break;

            case PhysicsMode.Static:
                physicsCircleCollider.enabled = true;
                physicsBody.simulated = true;
                physicsBody.bodyType = RigidbodyType2D.Static;
                break;

            case PhysicsMode.Dynamic:
                physicsCircleCollider.enabled = true;
                physicsBody.simulated = true;
                physicsBody.bodyType = RigidbodyType2D.Dynamic;

                //TODO: check for jumpable trigger?
                break;
        }
    }

    private void CameraFollowUpdate() {
        Vector2 playerPos = physicsBody.position;

        var gameCam = GameMapController.instance.gameCamera;
        var gameCamPos = gameCam.position;

        var gameCamNewPos = Vector2.SmoothDamp(gameCamPos, playerPos, ref mGameCamCurVel, data.cameraTime);

        gameCam.SetPosition(gameCamNewPos);
    }

    private void ClearPhysicsData() {
        mContactPointsCount = 0;
        mGroundCollContacts.Clear();

        physicsBody.velocity = Vector2.zero;
        physicsBody.angularVelocity = 0f;

        canJump = false;

        mIsMoveTrigger = false;
    }

    private void UpdateJump() {
        if(mIsMoveTrigger) {
            canJump = Time.time - mLastJumpTime > data.moveTriggerCooldown;

            mJumpCheckHit.point = physicsBody.position - Vector2.down * physicsCircleCollider.radius;
        }
        else {
            if(Time.time - mLastJumpTime > data.jumpCooldown) {
                Vector2 dir = Vector2.down;

                mJumpCheckHit = Physics2D.CircleCast(physicsBody.position, physicsCircleCollider.radius, dir, data.jumpCastDistance, data.jumpCastLayerMask);

                canJump = mJumpCheckHit.collider != null;
            }
            else
                canJump = false;
        }
    }

    private void ClearRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoSpawn() {
        //focus camera to player
        Vector2 playerPos = physicsBody.position;

        var gameCam = GameMapController.instance.gameCamera;
        gameCam.MoveTo(playerPos);

        //wait for camera to move to player
        while(gameCam.isMoving)
            yield return null;

        //do fancy stuff
        if(animator && !string.IsNullOrEmpty(takeSpawn)) {
            animator.Play(takeSpawn);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        //ready for play
        state = (int)EntityState.PlayerLaunchReady;
    }

    IEnumerator DoLaunch() {
        //play launch animation and hit it
        if(animator && !string.IsNullOrEmpty(takeLaunch)) {
            animator.Play(takeLaunch);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;
                
        //set rotation to move dir
        transform.up = moveDir;

        state = (int)EntityState.PlayerMove;
                
        //initial impulse
        Vector2 initialImpulsePos = physicsBody.position - moveDir * physicsCircleCollider.radius;

        physicsBody.AddForceAtPosition(moveDir * movePower, initialImpulsePos, ForceMode2D.Impulse);

        mGroundMoveDir.x = Mathf.Sign(moveDir.x);

        //play end part
        if(animator && !string.IsNullOrEmpty(takeLaunchEnd))
            animator.Play(takeLaunchEnd);
    }

    IEnumerator DoDeath() {
        //do fancy stuff
        yield return new WaitForSeconds(1f);

        mRout = null;

        //hide display
        if(displayGO) displayGO.SetActive(false);

        if(signalDeath) signalDeath.Invoke();
    }

    IEnumerator DoVictory() {
        yield return null;

        //move towards victory
        transform.position = mVictoryPos;

        //CameraFollowUpdate()

        //play victory animation

        //move upwards

        //signal victory
        if(signalGoal)
            signalGoal.Invoke();

        mRout = null;
    }
}
