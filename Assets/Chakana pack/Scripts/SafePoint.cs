using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafePoint : MonoBehaviour
{

    [SerializeField] GameObject fire;
    [SerializeField] Transform pivot;
    [SerializeField] GameObject particles;
    [SerializeField] GameObject txt;
    bool isIn, isOn;
    void Start()
    {
        if (PlayerPrefs.GetInt("SP01") == 1)
        {
            fire.SetActive(true);
        }

        isOn = true;
    }

    void Update()
    {
        if(isIn && Input.GetKeyDown(KeyCode.E) && isOn)
        {
            fire.SetActive(true);
            Destroy(Instantiate(particles, pivot), 2.5f);

            PlayerPrefs.SetInt("SP01", 1);

            isOn = false;
            StartCoroutine(Timer());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            isIn = true;
            txt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isIn = false;
            txt.SetActive(false);
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(3f);
        isOn = true;
    }
}
