using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetGOTG : MonoBehaviour
{
    private LevelLoader Levelloader;
    private GameObject hud;
    private Animation anim;
    private Animator playerAnim;
    public AnimationClip clip2;

    void Start()
    {
        Levelloader = GameObject.Find("TransitionTrigger").GetComponent<LevelLoader>();
        hud = GameObject.Find("HUDMenu");
        anim = GetComponent<Animation>();

        StartCoroutine(EnableTrigger());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player") return;

        collision.GetComponent<Hoyustus>().enabled = false;
        collision.GetComponent<AudioSource>().enabled = false;
        collision.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        playerAnim = collision.GetComponent<Animator>();
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        hud.SetActive(false);

        playerAnim.SetFloat("XVelocity", 0);
        playerAnim.SetBool("Grounded", true);
        playerAnim.SetBool("Dashing", false);
        playerAnim.SetBool("Atacando", false);
        playerAnim.SetBool("SecondJump", false);
        playerAnim.SetBool("Jumping", false);
        playerAnim.SetBool("Walking", false);

        yield return new WaitForSeconds(0.5f);

        anim.clip = clip2;
        anim.Play();

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadSceneAsync(18);
    }

    IEnumerator EnableTrigger()
    {
        yield return new WaitForSeconds(1.5f);

        GetComponent<CircleCollider2D>().enabled = true;
    }
}
