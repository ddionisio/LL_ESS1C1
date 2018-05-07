﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M8;

public class ExplodeSpawn : MonoBehaviour, IPoolSpawn, IPoolDespawn {
    public const string parmSpawnPt = "spt";
        
    public float radius = 1.0f;
    public float power = 50f;
    public ForceMode2D mode = ForceMode2D.Impulse;

    public LayerMask layerMask;

    public float delay = 0.1f;

    public M8.Animator.AnimatorData animator;
    public string takeExplode = "explode";

    private PoolDataController mPoolCtrl;

    private const int colliderCapacity = 8;
    private Collider2D[] mColliders = new Collider2D[colliderCapacity];
    private int mCollidersCount;

    private int mTakeExplodeInd;

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

        Vector2 explodePos;

        if(parms != null && parms.TryGetValue<Vector2>(parmSpawnPt, out explodePos)) {
            transform.position = explodePos;
        }
        else
            explodePos = transform.position;

        StartCoroutine(DoExplode(explodePos));
    }
        
    IEnumerator DoExplode(Vector2 pos) {
        yield return new WaitForSeconds(delay);

        //explode
        mCollidersCount = Physics2D.OverlapCircleNonAlloc(pos, radius, mColliders, layerMask);

        for(int i = 0; i < mCollidersCount; i++) {
            var coll = mColliders[i];

            var body = coll.GetComponent<Rigidbody2D>();
            if(body)
                body.AddExplosionForceAtPosition(pos, power, pos, radius, mode);
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