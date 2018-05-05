using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Match BoxCollider to GameCamera's view bounds
/// </summary>
public class GameCameraBoxCollider : MonoBehaviour {
    [SerializeField]
    GameCamera _gameCamera;
    [SerializeField]
    BoxCollider2D _boxCollider;
    [SerializeField]
    Vector2 _sizeExt = Vector2.one;

    void Awake() {
        if(!_gameCamera)
            _gameCamera = GetComponentInParent<GameCamera>();

        if(!_boxCollider)
            _boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start () {
        _boxCollider.offset = Vector2.zero;
        _boxCollider.size = _gameCamera.cameraViewRect.size + _sizeExt;
    }	
}
