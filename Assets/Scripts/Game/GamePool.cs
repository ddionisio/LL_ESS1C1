using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabFromResource("gamePool")]
public class GamePool : M8.SingletonBehaviour<GamePool> {
    public enum ExplodeTypes {
        explode,
        explodeWall
    }

    private M8.PoolController mPool;

    private M8.GenericParams mExplodeParms = new M8.GenericParams();

    public void ReleaseAll() {
        mPool.ReleaseAll();
    }

    public void ExplodeAt(ExplodeTypes type, Vector2 pt) {
        mExplodeParms[ExplodeSpawn.parmSpawnPt] = pt;

        string explodeTypeStr = type.ToString();

        mPool.Spawn(explodeTypeStr, explodeTypeStr, null, mExplodeParms);
    }

    protected override void OnInstanceInit() {
        mPool = GetComponent<M8.PoolController>();
    }
}
