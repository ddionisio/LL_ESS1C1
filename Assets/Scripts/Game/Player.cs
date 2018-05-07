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

    public Vector2 moveDir { get; set; }
    public float movePower { get; set; }

    public CircleCollider2D physicsCircleCollider { get; private set; }
    public Rigidbody2D physicsBody { get; private set; }

    public bool isGrounded { get { return mGroundCollContacts.Count > 0; } }

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

    private Vector2 mSpawnPos;

    /// <summary>
    /// Apply current move power towards move dir, this will set player state to move
    /// </summary>
    public void Move() {
        state = (int)EntityState.PlayerMove;

        Vector2 toPos = physicsBody.position - moveDir * physicsCircleCollider.radius;

        physicsBody.AddForceAtPosition(moveDir * movePower, toPos, ForceMode2D.Impulse);
    }

    protected override void OnDespawned() {
        //reset stuff here
        ClearRoutine();

        physicsMode = PhysicsMode.Disabled;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.
        mSpawnPos = transform.position;

        //start ai, player control, etc
        state = (int)EntityState.Spawn;
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

                //save current position for respawn
                mSpawnPos = transform.position;

                movePower = 0f;

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
                break;

            case EntityState.PlayerDeath:
                physicsMode = PhysicsMode.Disabled;

                mRout = StartCoroutine(DoDeath());
                break;

            case EntityState.Victory:
                physicsMode = PhysicsMode.Disabled;
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
                Vector2 playerPos = physicsBody.position;

                //camera follow
                var gameCam = GameMapController.instance.gameCamera;
                var gameCamPos = gameCam.position;

                var gameCamNewPos = Vector2.SmoothDamp(gameCamPos, playerPos, ref mGameCamCurVel, data.cameraTime);

                gameCam.SetPosition(gameCamNewPos);

                if(isGrounded) {
                    //check if we need to rest
                    float speedSqr = physicsBody.velocity.sqrMagnitude;
                    float restSpeedThresholdSqr = data.restSpeedThreshold * data.restSpeedThreshold;
                    if(speedSqr <= restSpeedThresholdSqr) {
                        //set to idle
                        state = (int)EntityState.PlayerIdle;
                    }
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        mContactPointsCount = coll.GetContacts(mContactPoints);

        //don't do anything else if we are not dynamic
        if(physicsMode != PhysicsMode.Dynamic)
            return;

        //fancy fx

        //check if we contact ground
        for(int i = 0; i < mContactPointsCount; i++) {
            var contactPt = mContactPoints[i];

            if(!contactPt.collider)
                continue;

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

    private void ClearPhysicsData() {
        mContactPointsCount = 0;
        mGroundCollContacts.Clear();
    }

    private void ClearRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoSpawn() {
        //set current position
        transform.position = mSpawnPos;

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

        //respawn
        state = (int)EntityState.Spawn;
    }
}
