using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShake : MonoBehaviour {

    public float shakeAmount;
    Vector3 startPos;
    private void Awake() {
        startPos = transform.position;   
    }

    void Update() {
        if (GunShoot.shooting) {
            transform.position = startPos + Random.insideUnitSphere * shakeAmount;
        } else if (transform.position != startPos) {
            transform.position = startPos;
        }
    }
}
