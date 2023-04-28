using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafePoint : MonoBehaviour
{

    [SerializeField] GameObject fire;
    [SerializeField] GameObject txt;
    bool isIn, isOn;
    void Start()
    {
        if (PlayerPrefs.GetInt("SP01") == 1)
        {
            fire.SetActive(true);
        }
    }

    void Update()
    {
        if(isIn && Input.GetKeyDown(KeyCode.E))
        {
            fire.SetActive(true);
            txt.SetActive(false);

            isOn = true;

            PlayerPrefs.SetInt("SP01", 0);
            print(PlayerPrefs.GetInt("SP01"));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player" && !isOn)
        {
            isIn = true;
            txt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && !isOn)
        {
            isIn = false;
            txt.SetActive(false);
        }
    }
}
