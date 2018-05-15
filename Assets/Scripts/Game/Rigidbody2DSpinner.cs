using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//make sure body is kinematic
public class Rigidbody2DSpinner : MonoBehaviour {
    public float rotatePerSecond;

    private Rigidbody2D mBody;

    void Awake() {
        mBody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate () {
        float deltaAngle = rotatePerSecond * Time.fixedDeltaTime;

        mBody.MoveRotation(mBody.rotation + deltaAngle);
	}
}
