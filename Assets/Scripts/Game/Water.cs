using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour {
    
    public Transform fillRoot;
    public Transform topRoot;

    private BuoyancyEffector2D mBuoyancy;
    private BoxCollider2D mColl;

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            if(!mBuoyancy) mBuoyancy = GetComponent<BuoyancyEffector2D>();
            if(!mColl) mColl = GetComponent<BoxCollider2D>();

            if(!mColl || !mBuoyancy)
                return;

            var collBounds = mColl.bounds;

            if(topRoot) {
                Vector2 topPos = new Vector2(
                    collBounds.center.x,
                    transform.position.y + mBuoyancy.surfaceLevel);

                topRoot.position = topPos;

                Vector3 topScale = topRoot.localScale;
                topScale.x = collBounds.size.x;

                topRoot.localScale = topScale;
            }

            if(fillRoot) {
                Vector2 fillPos = new Vector2(
                    collBounds.center.x,
                    transform.position.y);

                fillRoot.position = fillPos;

                Vector3 fillScale = fillRoot.localScale;
                fillScale.x = collBounds.size.x;
                fillScale.y = mBuoyancy.surfaceLevel;

                fillRoot.localScale = fillScale;
            }

            return;
        }
    }
#endif

    void Awake() {
#if UNITY_EDITOR
        if(!Application.isPlaying)
            return;
#endif

        mBuoyancy = GetComponent<BuoyancyEffector2D>();
        mColl = GetComponent<BoxCollider2D>();
    }
}
