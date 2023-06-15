using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjDestroy : MonoBehaviour
{
    [SerializeField] private GameObject vfxDestroy;
    [SerializeField] private bool door, gold;
    [SerializeField] private int doorNum;
    [SerializeField] GameObject goldObj;
    void Start()
    {
        if (PlayerPrefs.HasKey("puerta" + doorNum) && door)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 14)
        {
            if (door)
            {
                PlayerPrefs.SetInt("puerta" + doorNum, 1);
            }

            if (gold)
            {
                int e = Random.Range(1, 4);

                for (int i = 0; i <= e; i++)
                {
                    Instantiate(goldObj, transform.position, Quaternion.identity);
                }
            }

            Instantiate(vfxDestroy, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
