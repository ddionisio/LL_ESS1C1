using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M8;

public class ExplodeSpawn : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    public const string parmSpawnPt = "spt";

    public ExplodeData data;

    public M8.Animator.AnimatorData animator;
    public string takeExplode = "explode";

    private PoolDataController mPoolCtrl;

    private const int colliderCapacity = 8;
    private Collider2D[] mColliders = new Collider2D[colliderCapacity];
    private int mCollidersCount;

    private int mTakeExplodeInd;

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        if(data)
            Gizmos.DrawWireSphere(transform.position, data.radius);
    }

    void Awake() {
        if(animator) {
            mTakeExplodeInd = animator.GetTakeIndex(takeExplode);
        }
    }

    void IPoolDespawn.OnDespawned() {
        StopAllCoroutines();
    }

    void IPoolSpawn.OnSpawned(GenericParams parms) {
        if(!mPoolCtrl)
            mPoolCtrl = GetComponent<PoolDataController>();

        if(animator)
            animator.ResetTake(mTakeExplodeInd);

        Vector2 explodePos;

        if(parms != null && parms.TryGetValue<Vector2>(parmSpawnPt, out explodePos)) {
            transform.position = explodePos;
        }
        else
            explodePos = transform.position;

        StartCoroutine(DoExplode(explodePos));
    }
        
    IEnumerator DoExplode(Vector2 pos) {
        if(data.delay > 0f)
            yield return new WaitForSeconds(data.delay);

        //explode
        mCollidersCount = Physics2D.OverlapCircleNonAlloc(pos, data.radius, mColliders, data.layerMask);

        for(int i = 0; i < mCollidersCount; i++) {
            var coll = mColliders[i];

            var body = coll.GetComponent<Rigidbody2D>();
            if(body)
                body.AddExplosionForceAtPosition(pos, data.power, pos, data.radius, data.uplift, false, data.mode);
        }

        //animation stuff
        if(animator) {
            animator.Play(mTakeExplodeInd);
            while(animator.isPlaying)
                yield return null;
        }

        if(mPoolCtrl)
            mPoolCtrl.Release();
        else
            gameObject.SetActive(false);
    }
}
