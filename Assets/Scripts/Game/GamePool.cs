using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabFromResource("gamePool")]
public class GamePool : M8.SingletonBehaviour<GamePool> {
    public enum ExplodeTypes {
        explode,
        explodeWall
    }

    public string jumpFXPoolRef = "jumpFX";

    private M8.PoolController mPool;

    private M8.GenericParams mExplodeParms = new M8.GenericParams();

    public void ReleaseAll() {
        mPool.ReleaseAll();
    }

    public void ExplodeAt(ExplodeTypes type, Vector2 pt, bool noPhysics) {
        mExplodeParms[ExplodeSpawn.parmSpawnPt] = pt;
        mExplodeParms[ExplodeSpawn.parmNoPhysics] = noPhysics;

        string explodeTypeStr = type.ToString();

        mPool.Spawn(explodeTypeStr, explodeTypeStr, null, mExplodeParms);
    }

    public void JumpFX(Vector3 pos, Vector3 up) {
        var spawn = mPool.Spawn(jumpFXPoolRef, jumpFXPoolRef, null, pos, null);

        spawn.transform.up = up;
    }

    protected override void OnInstanceInit() {
        mPool = GetComponent<M8.PoolController>();
    }
}
