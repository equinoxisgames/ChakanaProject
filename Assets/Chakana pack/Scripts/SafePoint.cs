using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafePoint : MonoBehaviour
{

    [SerializeField] GameObject fire;
    [SerializeField] GameObject CheckPointOffFX;
    [SerializeField] Transform pivot;
    [SerializeField] GameObject particles;
    [SerializeField] GameObject txtUse;
    [SerializeField] EnemyRespawn respawn;
    [SerializeField] int spNum;
    bool isIn, isOn;

    private GameObject keyObj, joyObj;
    private bool joystick = false;
    Hoyustus player;

    void Start()
    {
        keyObj = txtUse.transform.GetChild(0).gameObject;
        joyObj = txtUse.transform.GetChild(1).gameObject;

        keyObj.SetActive(true);
        joyObj.SetActive(false);

        int joystickCount = Input.GetJoystickNames().Length;

        if (joystickCount > 0)
        {
            if (!joystick)
            {
                joystick = true;

                keyObj.SetActive(false);
                joyObj.SetActive(true);
            }
        }

        if (PlayerPrefs.HasKey("respawn") && PlayerPrefs.GetInt("scenePos") == 0) respawn.ResetEnemies();

        if (PlayerPrefs.GetInt("SP" + spNum) == 1)
        {
            fire.SetActive(true);
            CheckPointOffFX.SetActive(false);
        }

        isOn = true;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (joystick)
            {
                joystick = false;

                keyObj.SetActive(true);
                joyObj.SetActive(false);
            }
        }

        if (Input.GetButtonDown("JoystickButton") || Input.GetAxis("HorizontalJ") != 0f || Input.GetAxis("VerticalJ") != 0f)
        {

            if (!joystick)
            {
                joystick = true;

                keyObj.SetActive(false);
                joyObj.SetActive(true);
            }
        }

        if (isIn && Input.GetButtonDown("Interact")&& isOn)
        {
            fire.SetActive(true);
            CheckPointOffFX.SetActive(false);
            Destroy(Instantiate(particles, pivot), 2.5f);

            PlayerPrefs.SetInt("SP" + spNum, 1);
            PlayerPrefs.SetInt("respawn", SceneManager.GetActiveScene().buildIndex);

            PlayerPrefs.SetInt("GameSaved", 1);

            GetComponent<AudioSource>().Play();
            isOn = false;
            StartCoroutine(Timer());

            player.CurarCompletamente();
            respawn.ResetEnemies();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            isIn = true;
            txtUse.SetActive(true);

            player = collision.gameObject.GetComponent<Hoyustus>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isIn = false;
            txtUse.SetActive(false);
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(3f);
        isOn = true;
    }
}
