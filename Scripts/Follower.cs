using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour {

    public float smoothSpeed;

    void Update() {
        float dist = Vector3.Distance(transform.position, GunAim.aimSpot);
        Vector3 followPos = Vector3.MoveTowards(transform.position, GunAim.aimSpot, dist * smoothSpeed);
        transform.position = followPos + new Vector3(0, 0, 0);
    }
}
