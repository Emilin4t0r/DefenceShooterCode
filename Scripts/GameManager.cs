using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public int lives = 0;
    public bool gameLost;
    public Animator camAnim, gameAnim, faderAnim;
    public GameObject fader;
    public GameObject explosion;

    void Start() {
        instance = this;
        lives = 3;
    }

    void Update() {
        if (lives < 1 && !gameLost) {
            StartCoroutine(LoseGame());
        }
    }

    public IEnumerator LoseGame() {
        print("You Lose!");
        gameLost = true;
        yield return new WaitForSeconds(2);
        camAnim.SetTrigger("Lose");
        gameAnim.SetTrigger("Lose");
        AudioFW.Play("LoseGame");
        yield return new WaitForSeconds(1.5f);
        GameObject expl = Instantiate(explosion, transform.GetChild(0).transform.position, explosion.transform.rotation);
        transform.GetChild(0).gameObject.SetActive(false);
        Destroy(expl, 0.25f);
        yield return new WaitForSeconds(0.07f);
        fader.SetActive(true);
        faderAnim.SetTrigger("Fade");
        AudioFW.StopLoop("Ambience");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) {
            Destroy(enemy);
        }
    }
}
