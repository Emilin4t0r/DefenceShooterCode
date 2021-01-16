using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLook : MonoBehaviour {
    public GameObject target;

    float x, y, z = 0;
    private void Awake() {
        x = transform.position.x;
        z = transform.position.z;
    }

    void Update() {
        
            transform.position = target.transform.position;
            y = transform.position.y;
            x = transform.position.x;
        if (!GameManager.instance.gameLost) {
            transform.position = new Vector3(x, y, z);
        }
    }
}
