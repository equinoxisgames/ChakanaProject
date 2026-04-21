using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ElementalAltar : MonoBehaviour
{
    [SerializeField] string doorName;
    [SerializeField] string keyCode;
    [SerializeField] GameObject btnControls;
    [SerializeField] GameObject particle;
    [SerializeField] Hoyustus player;
    [SerializeField] GameObject tutoObj;
    [SerializeField] GameObject tutoCompleted;
    [SerializeField] EnemyGenerator enemyG;

    private GameObject keyObj, joyObj;
    private bool joystick = false;

    private bool isIn, isOn, isActive;
    private string altarName;

    private void Start()
    {
        keyObj = btnControls.transform.GetChild(0).gameObject;
        joyObj = btnControls.transform.GetChild(1).gameObject;

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

        altarName = doorName + keyCode;

        if (!PlayerPrefs.HasKey(altarName))
        {
            PlayerPrefs.SetInt(altarName, 0);
        }
        else if (PlayerPrefs.GetInt(altarName) == 1)
        {
            isOn = true;
            particle.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isIn && Input.GetButtonDown("Interact") && !isOn)
        {
            btnControls.SetActive(false);
            
            isOn = true;
            isIn = false;

            PlayerPrefs.SetInt(altarName, 1);

            int e = PlayerPrefs.GetInt(doorName) + 1;
            PlayerPrefs.SetInt(doorName, e);

            CameraShakeManager.Instance.ShakeSinHitStop();

            particle.SetActive(false);
            PlayerPrefs.SetInt("unlookSkills", 1);
            
            if(keyCode == "01")
            {
                PlayerPrefs.SetInt("snakeSkill", 1);
                player.setCargaHabilidades(1);
            }
            if(keyCode == "02")
            {
                PlayerPrefs.SetInt("condorSkill", 1);
                player.setCargaHabilidades(0);
            }
            if (keyCode == "03")
            {
                PlayerPrefs.SetInt("spearSkill", 1);
                player.setCargaHabilidades(2);
            }

            HudManager.Instance.RefreshUI();

            StartCoroutine(SkillsTuto());
        }

        if (Input.anyKeyDown)
        {
            if (isOn && isActive)
            {
                tutoObj.SetActive(false);
                Time.timeScale = 1;
                enemyG.StartCombatAuto();
                isActive = false;

                if (PlayerPrefs.GetInt(doorName) == 3)
                {
                    tutoCompleted.SetActive(true);
                    Destroy(tutoCompleted, 3f);
                }
                gameObject.SetActive(false);
            }

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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOn)
        {
            isIn = true;
            btnControls.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOn)
        {
            isIn = false;
            btnControls.SetActive(false);
        }
    }

    IEnumerator SkillsTuto()
    {
        GetComponent<AudioSource>().Play();
        Time.timeScale = 0;
        tutoObj.transform.GetChild(5).gameObject.SetActive(false);
        tutoObj.SetActive(true);

        yield return new WaitForSecondsRealtime(1f);
        tutoObj.transform.GetChild(5).gameObject.SetActive(true);
        isActive = true;
    }
}
