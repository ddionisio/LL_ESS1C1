using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenIndicatorPosition : MonoBehaviour {

    public Vector3 targetPosition { get; set; }
    public Camera targetCamera { get; set; }

    public Transform displayRoot;

    [SerializeField]
    public Camera _displayCamera;
    public Camera displayCamera { get { return _displayCamera; } set { _displayCamera = value; } }

    void Update() {
        if(!targetCamera) return;
        
        Vector3 vp = targetCamera.WorldToViewportPoint(targetPosition);

        bool isEdge = false;

        if(vp.x > 1) {
            vp.x = 1; isEdge = true;
        }
        else if(vp.x < 0) {
            vp.x = 0; isEdge = true;
        }

        if(vp.y > 1) {
            vp.y = 1; isEdge = true;
        }
        else if(vp.y < 0) {
            vp.y = 0; isEdge = true;
        }

        if(isEdge) {
            displayRoot.gameObject.SetActive(true);

            if(!displayCamera)
                displayCamera = targetCamera;

            Vector3 pos = displayCamera.ViewportToWorldPoint(vp);
            displayRoot.position = new Vector3(pos.x, pos.y, 0f);
            displayRoot.up = new Vector3(vp.x - 0.5f, vp.y - 0.5f, 0.0f);
        }
        else {
            displayRoot.gameObject.SetActive(false);
        }
    }
}
