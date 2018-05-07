using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapPool : M8.SingletonBehaviour<GameMapPool> {
    public string explodeType = "explode";

    private M8.PoolController mPool;

    private M8.GenericParams mExplodeParms = new M8.GenericParams();

    public void ReleaseAll() {
        mPool.ReleaseAll();
    }

    public void ExplodeAt(Vector2 pt) {
        mExplodeParms[ExplodeSpawn.parmSpawnPt] = pt;

        mPool.Spawn(explodeType, explodeType, null, mExplodeParms);
    }

    protected override void OnInstanceInit() {
        mPool = GetComponent<M8.PoolController>();
    }
}
