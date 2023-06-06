using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRoute : MonoBehaviour
{
    [SerializeField] private GameObject tutoObj;
    [SerializeField] private GameObject aditionalObj;
    [SerializeField] private int tutoNum;

    private bool isActive;
    private GameObject keyObj, joyObj;
    private bool joystick = false;
    private Transform player;

    void Start()
    {
        if(PlayerPrefs.HasKey("tutorial" + tutoNum))
        {
            Destroy(gameObject);
        }

        keyObj = tutoObj.transform.GetChild(0).gameObject;
        joyObj = tutoObj.transform.GetChild(1).gameObject;

        string[] joystickNames = Input.GetJoystickNames();
        print(joystickNames[0]);

        if (joystickNames.Length > 0 && joystickNames[0] != "")
        {
            keyObj.SetActive(false);
            joyObj.SetActive(true);
        }
        else
        {
            keyObj.SetActive(true);
            joyObj.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        TutoDetector();

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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            tutoObj.SetActive(true);
            if (aditionalObj != null) aditionalObj.SetActive(true);
            isActive = true;
            player = collision.transform;
        }
    }

    private void TutoDetector()
    {
        if (isActive)
        {
            tutoObj.transform.position = player.position;

            if(tutoNum == 1 && Input.GetAxis("Jump") == 1)
            {
                tutoObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1); 
                Destroy(gameObject);
            }
            if (tutoNum == 2 && Input.GetAxis("Interact") == 1)
            {
                tutoObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 3 && Input.GetAxis("Atacar") == 1)
            {
                tutoObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 4 && Input.GetAxis("Dash") == 1)
            {
                tutoObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 5 && Input.GetAxis("Horizontal") != 0)
            {
                tutoObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
        }
    }
}
