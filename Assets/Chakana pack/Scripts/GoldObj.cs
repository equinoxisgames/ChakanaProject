using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldObj : MonoBehaviour
{
    [SerializeField] int goldType;
    [SerializeField] int goldTypeFisic;
    [SerializeField] bool heal;
    [SerializeField] GameObject healFX;

    Hoyustus player;
    Transform tr;
    int amount;
    bool isMove = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
        tr = player.transform;

        if (goldType == 0)
        {
            if(PlayerPrefs.HasKey("goldfisic" + goldTypeFisic))
            {
                Destroy(gameObject);
            }

            amount = 10;
        }
        else
        {
            int e = Random.Range(1, 11);

            if (e > 6)
            {
                Destroy(gameObject);
            }

            StartCoroutine(StartMove());
            transform.parent = null;
        }

        if (goldType == 1) amount = 3;
        else if (goldType == 2) amount = 5;
        else if (goldType == 3) amount = 7;
        else if (goldType == 4) amount = 1;
        else if (goldType == 5) amount = 4;
    }

    private void LateUpdate()
    {
        if (isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, tr.position, 18 * Time.deltaTime);
        }
    }

    private IEnumerator StartMove()
    {
        yield return new WaitForSeconds(0.5f);

        isMove = true;
        print("hola");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && !heal)
        {
            if(goldType == 0)
            {
                PlayerPrefs.SetInt("gold" + goldTypeFisic, 1);
            }

            player.setGold(amount);
            StartCoroutine(Effect());
        }
        else if(collision.tag == "Player" && heal)
        {
            player.setCargaCuracion(25);
            StartCoroutine(Effect());
        }
    }

    IEnumerator Effect()
    {
        yield return new WaitForSeconds(0.1f);

        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(Instantiate(healFX, transform.position, Quaternion.identity), 1.2f);

        yield return new WaitForSeconds(0.61f);

        Destroy(gameObject);
    }
}
