using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When active, orient itself towards player's move dir
/// </summary>
public class PlayerArrowDisplay : MonoBehaviour {
    [SerializeField]
    Player _player;

    private Transform mTrans;

    void Awake() {
        if(!_player)
            _player = GetComponentInParent<Player>();

        mTrans = transform;
    }

    void OnEnable() {
        Update();
    }

    void Update() {
        //update up dir
        mTrans.up = _player.moveDir;
    }
}
