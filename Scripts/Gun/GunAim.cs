using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAim : MonoBehaviour {

    public static Vector3 aimSpot;
    public GameObject target;

    private void Awake() {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 100;
        aimSpot = Camera.main.ScreenToWorldPoint(mousePos);
        if (!GameManager.instance.gameLost && !Menus.paused) {
            transform.LookAt(target.transform.position);
        }
    }
}
