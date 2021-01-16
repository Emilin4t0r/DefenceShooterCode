using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EZCameraShake;

public class Truck : MonoBehaviour {
    public int lives;
    public GameObject truckDeath;
    NavMeshAgent agent;
    bool damaged;
    public GameObject flames;

    public float startSpeed, damagedSpeed;
    public GameObject[] wheels = new GameObject[4];

    MeshCollider col;
    void Start() {
        col = transform.GetComponent<MeshCollider>();
        agent = transform.GetComponent<NavMeshAgent>();
        agent.speed = startSpeed; 
        lives = 25;
    }

    private void FixedUpdate() {

        RotateWheels(350);

        if (lives < 15 && !damaged) {
            damaged = true;
            agent.speed = damagedSpeed;
            AudioFW.Play("TruckDamaged");
        }
        if (lives < 1) {
            EnemyManager.enemiesLeft--;
            GunShoot gun = GameObject.Find("BulletSpawn").GetComponent<GunShoot>();
            gun.kills++;
            Clipboard.instance.ChangeKills(gun.kills.ToString());
            Destroy(gameObject);
            GameObject deadTruck = Instantiate(truckDeath, transform.position, transform.rotation);
            Destroy(deadTruck, 5f);
            CameraShaker.GetInstance("Main Camera").ShakeOnce(6f, 5f, 0.0f, 2.5f);
            AudioFW.Play("TruckDie");
        }

        if (damaged) {
            flames.SetActive(true);
        }
    }

    void RotateWheels(float speed) {
        foreach (GameObject wheel in wheels) {
            wheel.transform.Rotate(-Time.deltaTime * speed, 0, 0);
        }
    }
}
