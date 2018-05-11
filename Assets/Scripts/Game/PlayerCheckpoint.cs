﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour {
    public float power;

    public float dirRotate; //starting from the top

    private Vector2 mDir;
    
    public void SpawnPlayer(Player player) {
        //set player move dir
        player.moveDir = mDir;
        player.movePower = power;

        //warp player here
        player.transform.position = transform.position;

        player.state = (int)EntityState.Spawn;
    }

    void Awake() {
        mDir = M8.MathUtil.Rotate(Vector2.up, dirRotate * Mathf.Deg2Rad);
    }

    void OnDrawGizmos() {
        //draw dir
        var dir = M8.MathUtil.Rotate(Vector2.up, dirRotate * Mathf.Deg2Rad);

        Gizmos.color = Color.green;

        M8.Gizmo.ArrowLine2D(transform.position, (Vector2)transform.position + dir * 1.5f);

        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}