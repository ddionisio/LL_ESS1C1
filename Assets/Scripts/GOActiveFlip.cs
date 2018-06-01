using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOActiveFlip : MonoBehaviour {
    public GameObject targetGO;
    public bool startActive;

    public void Flip() {
        targetGO.SetActive(!targetGO.activeSelf);
    }

    void OnEnable() {
        targetGO.SetActive(startActive);
    }
}
