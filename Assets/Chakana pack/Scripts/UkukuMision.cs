using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.FantasyInventory.Scripts.Interface;

public class UkukuMision : MonoBehaviour
{
    [SerializeField] private string num;
    [SerializeField] private GameObject txt;
    [SerializeField] private bool isDestroyed;
    [SerializeField] private GameObject ukukuInv;
    [SerializeField] private Inventory inventory;

    private bool isActive;
    private bool isUkukuActive;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();

        if (!PlayerPrefs.HasKey("ukukuM"))
        {
            PlayerPrefs.SetInt("ukukuM", 0);
        }

        if (PlayerPrefs.GetInt("ukukuM" + num) == 1)
        {
            GetComponent<UkukuMision>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    void Update()
    {
        if (isActive && Input.GetButtonDown("Interact"))
        {
            int e = PlayerPrefs.GetInt("ukukuM");
            e += 1;
            PlayerPrefs.SetInt("ukukuM", e);

            PlayerPrefs.SetInt("ukukuM" + num, 1);
            txt.SetActive(false);

            StartCoroutine(ShowInventory());

            GetComponent<UkukuMision>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            inventory.NewInventory();
            GetComponent<AudioSource>().Play();

            if (isDestroyed)
            {
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        if (isUkukuActive)
        {
            if (Input.anyKeyDown)
            {
                isUkukuActive = false;
                ukukuInv.transform.GetChild(5).gameObject.SetActive(false);
                Time.timeScale = 1;
                GetComponent<UkukuMision>().enabled = false;
            }
        }
    }

    IEnumerator ShowInventory()
    {
        yield return new WaitForSeconds(0.1f);

        int misionCount = 0;

        if (PlayerPrefs.HasKey("ukukuM" + "01"))
        {
            ukukuInv.transform.GetChild(0).gameObject.SetActive(true);
            misionCount++;
        }

        if (PlayerPrefs.HasKey("ukukuM" + "02"))
        {
            ukukuInv.transform.GetChild(1).gameObject.SetActive(true);
            misionCount++;
        }

        if (PlayerPrefs.HasKey("ukukuM" + "03"))
        {
            ukukuInv.transform.GetChild(2).gameObject.SetActive(true);
            misionCount++;
        }

        if (PlayerPrefs.HasKey("ukukuM" + "04"))
        {
            ukukuInv.transform.GetChild(3).gameObject.SetActive(true);
            misionCount++;
        }

        if(misionCount == 4)
        {
            ukukuInv.transform.GetChild(4).gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2);

        if (PlayerPrefs.HasKey("ukukuM" + "01"))
        {
            ukukuInv.transform.GetChild(0).gameObject.SetActive(false);
        }
        
        if (PlayerPrefs.HasKey("ukukuM" + "02"))
        {
            ukukuInv.transform.GetChild(1).gameObject.SetActive(false);
        }
        
        if (PlayerPrefs.HasKey("ukukuM" + "03"))
        {
            ukukuInv.transform.GetChild(2).gameObject.SetActive(false);
        }
        
        if (PlayerPrefs.HasKey("ukukuM" + "04"))
        {
            ukukuInv.transform.GetChild(3).gameObject.SetActive(false);
        }

        if (misionCount == 4)
        {
            ukukuInv.transform.GetChild(4).gameObject.SetActive(false);

            StartCoroutine(UkukuEvent());
        }
    }

    private IEnumerator UkukuEvent()
    {
        ukukuInv.transform.GetChild(5).gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        GetComponent<UkukuMision>().enabled = true;
        isUkukuActive = true;
        Time.timeScale = 0;
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
