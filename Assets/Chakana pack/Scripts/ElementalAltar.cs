using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalAltar : MonoBehaviour
{
    [SerializeField] string doorName;
    [SerializeField] string keyCode;
    [SerializeField] GameObject altarFX;
    [SerializeField] GameObject txtUse;

    private bool isIn, isOn;
    private string altarName;

    private void Awake()
    {
        altarName = doorName + keyCode;

        if (!PlayerPrefs.HasKey(altarName))
        {
            PlayerPrefs.SetInt(altarName, 0);
        }
        else if (PlayerPrefs.GetInt(altarName) == 1)
        {
            altarFX.SetActive(true);
            isOn = true;
        }
    }

    void Update()
    {
        if (isIn && Input.GetKeyDown(KeyCode.E))
        {
            altarFX.SetActive(true);
            txtUse.SetActive(false);

            isOn = true;
            isIn = false;

            PlayerPrefs.SetInt(altarName, 1);

            int e = PlayerPrefs.GetInt(doorName) + 1;
            PlayerPrefs.SetInt(doorName, e);
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
}
