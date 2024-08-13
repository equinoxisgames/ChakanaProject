using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ElementalAltar : MonoBehaviour
{
    [SerializeField] string doorName;
    [SerializeField] string keyCode;
    [SerializeField] GameObject altarFX;
    [SerializeField] GameObject altarOffFX;
    [SerializeField] GameObject txtUse;
    [SerializeField] GameObject details;
    [SerializeField] TextMeshProUGUI detailTxt;
    [SerializeField] GameObject explodeFx;
    [SerializeField] AudioClip audioComplete;

    private GameObject keyObj, joyObj;
    private bool joystick = false;

    private bool isIn, isOn;
    private string altarName;

    private void Start()
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

        altarName = doorName + keyCode;

        if (!PlayerPrefs.HasKey(altarName))
        {
            PlayerPrefs.SetInt(altarName, 0);
        }
        else if (PlayerPrefs.GetInt(altarName) == 1)
        {
            altarFX.SetActive(true);
            altarOffFX.SetActive(false);
            isOn = true;
        }
    }

    void Update()
    {
        if (isIn && Input.GetButtonDown("Interact"))
        {
            altarFX.SetActive(true);
            altarOffFX.SetActive(false);
            txtUse.SetActive(false);
            
            isOn = true;
            isIn = false;

            PlayerPrefs.SetInt(altarName, 1);

            int e = PlayerPrefs.GetInt(doorName) + 1;
            PlayerPrefs.SetInt(doorName, e);
            StartCoroutine(ShowDetails());
        }

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
        if (collision.transform.tag == "Player" && !isOn)
        {
            isIn = true;
            txtUse.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOn)
        {
            isIn = false;
            txtUse.SetActive(false);
        }
    }

    IEnumerator ShowDetails()
    {
        int e = 3 - PlayerPrefs.GetInt(doorName);

        if(e == 2)
        {
            detailTxt.text = e + " MORE" + " ALTARS";
            GetComponent<AudioSource>().Play();
        }
        else if(e == 1)
        {
            detailTxt.text = e + " MORE" + " ALTAR";
            GetComponent<AudioSource>().Play();
        }
        else
        {
            detailTxt.text = "THE DOOR HAS OPENED";
            GetComponent<AudioSource>().clip = audioComplete;
            GetComponent<AudioSource>().Play();
            Instantiate(explodeFx);
        }

        details.SetActive(true);

        yield return new WaitForSeconds(4.4f);

        details.SetActive(false);
    }
}
