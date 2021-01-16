using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;


public class GunShoot : MonoBehaviour {
    public GameObject mGShot;
    public GameObject mFlash;
    public GameObject flashParticle;
    public GameObject bodyImpact;
    public GameObject ammoCounter;
    public GameObject pistol;
    public Animator gunAnim;

    public Texture2D cursor, cursorHit;

    public GameObject linkSpawn;
    public GameObject link;
    public GameObject smokeEmit;
    public GameObject smokeParticle;

    public static bool shooting;
    bool outOfAmmo;
    bool reloading;
    bool overheating;

    float barrelHeat;
    public Material barrel;

    public int maxAmmo = 100;
    int currentAmmo;
    public float accuracy;
    public float shootForce;
    public float fireRate;
    float nextFireTime;
    Transform trans;

    public int kills = 0;
    private void Start() {
        trans = transform;
        currentAmmo = maxAmmo;
        flashParticle.SetActive(false);
    }
    void Update() {

        if (!outOfAmmo && !overheating && !GameManager.instance.gameLost && !Menus.paused) {
            if (Input.GetKey(KeyCode.Mouse0)) {
                if (!shooting) {
                    shooting = true;
                    StartCoroutine("StartShootSounds");
                }
                if (Time.time > nextFireTime) {
                    Shoot();
                    --currentAmmo;
                    flashParticle.SetActive(true);
                    nextFireTime = Time.time + fireRate;

                    //spawn belt links
                    GameObject newLink = Instantiate(link, linkSpawn.transform.position, Quaternion.identity);
                    newLink.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.5f, 1.5f), 0, 0), ForceMode.Impulse);
                    Destroy(newLink, 0.5f);
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                if (shooting) {
                    shooting = false;
                    StopShootSounds();
                    flashParticle.SetActive(false);
                }
            }
        } else if (outOfAmmo) {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                AudioFW.Play("ClickEmpty");
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            pistol.GetComponent<Animator>().SetTrigger("Shoot");
        }

        if (Input.GetKeyDown(KeyCode.R) && !overheating && currentAmmo < maxAmmo && !reloading) {
            StartCoroutine(Reload());
        }
        if (Input.GetKeyDown(KeyCode.F) && !overheating && !shooting && !reloading) {
            overheating = true;
            StartCoroutine(BarrelChange());
        }

    }

    private void FixedUpdate() {

        ammoCounter.transform.localEulerAngles = new Vector3(90 + currentAmmo * 3f, 90, 90);

        if (!shooting && !overheating) {
            if (AudioFW.IsLoopPlaying("BurstLoop")) {
                StopShootSounds();
            }
            if (barrelHeat > 0) {
                barrelHeat -= 0.05f;
            } else {
                barrelHeat = 0;
            }
        } else if (shooting && !overheating) {
            barrelHeat += 0.25f;
        } else if (overheating && barrelHeat > 0) {
            barrelHeat -= 1f;
        }

        if (GameManager.instance.gameLost && shooting) {
            shooting = false;
            StopShootSounds();
        }

        if (currentAmmo < 1) {
            if (!outOfAmmo) {
                StartCoroutine(PlayClickWithDelay(0.02f));
                outOfAmmo = true;
                shooting = false;
                flashParticle.SetActive(false);
                StopShootSounds();
                StartCoroutine(Reload());
            }
        }

        Color multip = new Color(0.07f, 0.015f, 0.01f, 1);
        Color intensity = multip * barrelHeat / 30;
        barrel.SetColor("_EmissionColor", intensity);
        barrel.EnableKeyword("_EMISSION");

        if (barrelHeat > 99.9 && !overheating) {
            barrelHeat = 100;
            print("Overheating!");
            overheating = true;
            StopShootSounds();
            StartCoroutine(BarrelChange());
        }        
    }

    void Shoot() {
        CameraShaker.GetInstance("Main Camera").ShakeOnce(1.5f, 2f, 0, 0.5f);

        //Bullet spread calculations
        Vector3 deviation3D = Random.insideUnitCircle * accuracy;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward + deviation3D);
        Vector3 fwd = transform.rotation * rot * Vector3.forward;

        bool enemyHit = false;
        RaycastHit hit;
        if (Physics.Raycast(trans.position, fwd, out hit)) {
            if (hit.transform.CompareTag("Enemy")) {
                Destroy(hit.transform.gameObject, 0.5f);
                hit.transform.GetComponent<Enemy>().dying = true;
                enemyHit = true;
                StartCoroutine(ShowHitMark(false));

                //blood splatter effect
                GameObject impact = Instantiate(bodyImpact, hit.transform.position, bodyImpact.transform.rotation);
                float randScale = Random.Range(2f, 3f);
                impact.transform.localScale = (new Vector3(randScale, randScale, randScale));
                Destroy(impact, 1f);
            } else if (hit.transform.CompareTag("Truck")) {
                hit.transform.GetComponent<Truck>().lives--;                
                StartCoroutine(ShowHitMark(true));
                float randHitSound = Random.Range(0f, 1f);
                if (randHitSound > 0.65f) {
                    AudioFW.PlayRandomPitch("MetalHit");                    
                }
            }
        }
        GameObject newShot = Instantiate(mGShot, trans.position, trans.parent.rotation);
        newShot.GetComponent<Rigidbody>().AddForce(fwd * shootForce, ForceMode.Impulse);
        if (enemyHit) {
            Destroy(newShot, 0.1f);
            EnemyManager.enemiesLeft--;
            kills++;
            Clipboard.instance.ChangeKills(kills.ToString());
        } else {
            Destroy(newShot, 1f);
        }

        GameObject flash = Instantiate(mFlash, trans.GetChild(0).position, Quaternion.identity);
        Destroy(flash, 0.02f);
    }

    IEnumerator Reload() {
        reloading = true;
        AudioFW.Play("Reload");
        currentAmmo = 0;
        outOfAmmo = true;
        shooting = false;
        gunAnim.SetBool("Reloading", true);
        //play reload sound
        yield return new WaitForSeconds(3.5f);
        gunAnim.SetBool("Reloading", false);
        currentAmmo = maxAmmo;
        outOfAmmo = false;
        reloading = false;
    }

    IEnumerator BarrelChange() {

        GameObject smoke = Instantiate(smokeParticle, smokeEmit.transform.position, Quaternion.identity, smokeEmit.transform);
        Destroy(smoke, 2.5f);

        AudioFW.Play("BarrelChange");
        gunAnim.SetBool("ChangingBarrel", true);
        shooting = false;
        overheating = true;        
        yield return new WaitForSeconds(2f);
        barrelHeat = 0;
        Color intensity = new Color(0.07f, 0.015f, 0.01f, 1);
        intensity *= barrelHeat / 25;
        barrel.SetColor("_EmissionColor", intensity);
        barrel.color = intensity;
        yield return new WaitForSeconds(3f);
        barrelHeat = 0;
        overheating = false;
        gunAnim.SetBool("ChangingBarrel", false);
    }

    IEnumerator StartShootSounds() {
        AudioFW.StopSfx("BurstTail");
        AudioFW.Play("BurstStart");
        yield return new WaitForSeconds(0.030f);
        AudioFW.PlayLoop("BurstLoop");
    }
    void StopShootSounds() {
        AudioFW.StopLoop("BurstLoop");
        AudioFW.PlayRandomPitch("BurstTail");
    }
    IEnumerator PlayClickWithDelay(float time) {
        yield return new WaitForSeconds(time);
        AudioFW.Play("ClickEmpty");
    }

    IEnumerator ShowHitMark(bool hitSound) {
        Cursor.SetCursor(cursorHit, new Vector2(20, 20), CursorMode.Auto);
        yield return new WaitForSeconds(0.1f);
        if (!hitSound) {
            AudioFW.PlayRandomPitch("HitMarker");
        }
        yield return new WaitForSeconds(0.1f);
        Cursor.SetCursor(cursor, new Vector2(20, 20), CursorMode.Auto);
    }
}
