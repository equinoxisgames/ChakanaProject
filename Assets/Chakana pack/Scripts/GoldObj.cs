using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldObj : MonoBehaviour
{
    [SerializeField] int goldType;
    [SerializeField] int goldTypeFisic;
    [SerializeField] List<Sprite> goldSprite = new List<Sprite>();

    int amount;
    void Start()
    {
        if (goldType == 1)
        {
            int e = Random.Range(1, 6);
            goldType = e;
        }

        if (goldType == 0)
        {
            if(PlayerPrefs.HasKey("gold" + goldTypeFisic))
            {
                Destroy(gameObject);
            }

            amount = 10;
        }
        else if (goldType == 1) amount = 3;
        else if (goldType == 2) amount = 5;
        else if (goldType == 3) amount = 7;
        else if (goldType == 4) amount = 1;
        else if (goldType == 5) amount = 4;

        if(goldType != 0)
        {
            GetComponent<SpriteRenderer>().sprite = goldSprite[(goldType - 1)];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(goldType == 0)
            {
                PlayerPrefs.SetInt("gold" + goldTypeFisic, 1);
            }

            collision.GetComponent<Hoyustus>().setGold(amount);
            Destroy(gameObject);
        }
    }
}
