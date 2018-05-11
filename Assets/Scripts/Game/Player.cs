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

    public GameObject displayRoot;

    [Header("Signals")]
    public SignalBool signalCanExplodeUpdate; //update on when we can explode
    public M8.Signal signalExplode;
    public M8.Signal signalGoal; //triggered goal
    public M8.Signal signalDeath;

    public Vector2 moveDir { get; set; }
    public float movePower { get; set; }

    public CircleCollider2D physicsCircleCollider { get; private set; }
    public Rigidbody2D physicsBody { get; private set; }

    public bool isGrounded { get { return mGroundCollContacts.Count > 0; } }

    //if true, we can explode at explodablePosition
    public bool canExplode {
        get { return mCanExplode; }
        private set {
            if(mCanExplode != value) {
                mCanExplode = value;

                if(signalCanExplodeUpdate) signalCanExplodeUpdate.Invoke(mCanExplode);
            }
        }
    }


    public Vector2 explodablePosition { get { return mExplodableHit.point; } }

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

    private Vector2 mGameCamCurVel;

    private Coroutine mRout;
    
    private Vector2 mVictoryPos;

    private bool mCanExplode;
    private RaycastHit2D mExplodableHit; //raycast hit info of explodable point

    private Vector2[] mExplodeCheckDirs = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private RaycastHit2D[] mExplodeCheckHits;

    private float mLastExplodeTime;

    /// <summary>
    /// Apply current move power towards move dir, this will set player state to move
    /// </summary>
    public void Move() {
        state = (int)EntityState.PlayerMove;
        
        //initial impulse
        Vector2 initialImpulsePos = physicsBody.position - moveDir * physicsCircleCollider.radius;

        physicsBody.AddForceAtPosition(moveDir * movePower, initialImpulsePos, ForceMode2D.Impulse);
    }

    public void Explode() {
        if(canExplode) {
            //spawn explosion at explode hit
            GameMapPool.instance.ExplodeAt(mExplodableHit.point);

            mLastExplodeTime = Time.time;
            canExplode = false;
        }
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
        if(displayRoot) displayRoot.SetActive(false);
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

                mRout = StartCoroutine(DoSpawn());
                break;

            case EntityState.PlayerIdle:
                physicsMode = PhysicsMode.Disabled;
                
                //focus camera to player
                Vector2 playerPos = physicsBody.position;

                var gameCam = GameMapController.instance.gameCamera;
                gameCam.MoveTo(playerPos);
                break;

            case EntityState.PlayerLock:
                physicsMode = PhysicsMode.Disabled;
                break;

            case EntityState.PlayerMove:
                physicsMode = PhysicsMode.Dynamic;

                mGameCamCurVel = Vector2.zero;

                mLastExplodeTime = Time.time;
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
        if(displayRoot) displayRoot.SetActive(false);

        //initialize data/variables
        mExplodeCheckHits = new RaycastHit2D[mExplodeCheckDirs.Length];

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

                if(isGrounded) {
                    //check if we need to rest
                    float speedSqr = physicsBody.velocity.sqrMagnitude;
                    float restSpeedThresholdSqr = data.restSpeedThreshold * data.restSpeedThreshold;
                    if(speedSqr <= restSpeedThresholdSqr) {
                        //set to death
                        state = (int)EntityState.Death;
                    }
                }

                UpdateExplodablePoint();
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

            if(dot >= mGroundSlopeLimitCos) {
                mGroundCollContacts.Add(contactPt.collider);
            }

            contactSum += contactPt.point;
            contactSumCount += 1f;
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

    private void ApplyPhysicsSettings() {
        mGroundSlopeLimitCos = Mathf.Cos(data.groundSlopeAngleLimit * Mathf.Deg2Rad);

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
                break;
        }
    }

    private void UpdateExplodablePoint() {
        //check delay from move
        if(Time.time - mLastExplodeTime < data.explodeCooldown) {
            canExplode = false;
            return;
        }

        //cast in 4-direction and check nearest distance
        Vector2 checkPos = physicsBody.position;
        float dist = data.explodeCastDistance + physicsCircleCollider.radius;

        for(int i = 0; i < mExplodeCheckDirs.Length; i++) {
            mExplodeCheckHits[i] = Physics2D.Raycast(checkPos, mExplodeCheckDirs[i], dist, data.explodeCastLayerMask);
        }

        //grab nearest
        float nearestDist = float.MaxValue;
        int nearestInd = -1;

        for(int i = 0; i < mExplodeCheckHits.Length; i++) {
            if(mExplodeCheckHits[i].collider != null) {
                if(mExplodeCheckHits[i].distance < nearestDist) {
                    nearestDist = mExplodeCheckHits[i].distance;
                    nearestInd = i;
                }
            }
        }

        if(nearestInd != -1) {
            mExplodableHit = mExplodeCheckHits[nearestInd];
            canExplode = true;
        }
        else
            canExplode = false;
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

        canExplode = false;
    }

    private void ClearRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoSpawn() {
        //show display
        if(displayRoot) displayRoot.SetActive(true);

        //do fancy stuff
        yield return new WaitForSeconds(1f);

        mRout = null;

        //ready for play
        state = (int)EntityState.PlayerIdle;
    }

    IEnumerator DoDeath() {
        //do fancy stuff
        yield return new WaitForSeconds(1f);

        mRout = null;

        //hide display
        if(displayRoot) displayRoot.SetActive(false);

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
