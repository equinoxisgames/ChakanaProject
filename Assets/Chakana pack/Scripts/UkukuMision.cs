using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UkukuMision : MonoBehaviour
{
    [SerializeField] private string num;
    [SerializeField] private GameObject txt;

    private bool isActive;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();

        if (!PlayerPrefs.HasKey("ukukuM"))
        {
            PlayerPrefs.SetInt("ukukuM", 0);
        }

        if (!PlayerPrefs.HasKey("ukukuM" + num))
        {
            PlayerPrefs.SetInt("ukukuM"  + num, 0);
        }
        else if (PlayerPrefs.GetInt("ukukuM" + num) == 1)
        {
            GetComponent<UkukuMision>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.E))
        {
            int e = PlayerPrefs.GetInt("ukukuM");
            e += 1;
            PlayerPrefs.SetInt("ukukuM", e);

            PlayerPrefs.SetInt("ukukuM" + num, 1);
            txt.SetActive(false);

            GetComponent<UkukuMision>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            isActive = true;
            txt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            isActive = false;
            txt.SetActive(false);
        }
    }
}
