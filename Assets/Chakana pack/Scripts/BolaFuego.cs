using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolaFuego : MonoBehaviour
{
    private Vector3 hoyustus;
    private Rigidbody2D rb;
    private float tiempoEliminacion = 5f;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        hoyustus = GameObject.Find("Hoyustus Solicitud Prefab").transform.position;
    }

    void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, hoyustus, 20 * Time.deltaTime);
        tiempoEliminacion -= Time.deltaTime;

        if (tiempoEliminacion <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void instanciarValores(int layer, Vector3 objetivo)
    {
        gameObject.layer = layer;
        gameObject.tag = "Fuego";
        this.gameObject.SetActive(true);
    }


    public void aniadirFuerza() {
        Vector2 direction = hoyustus - transform.position;
        float distance = Vector2.Distance(transform.position, hoyustus);
        rb.velocity = direction.normalized * 20;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == 6 || collision.gameObject.layer == 16)
        {
            if (collision.gameObject.CompareTag("Player")) {
                collision.gameObject.GetComponent<Hoyustus>().recibirDanio(15);
                collision.gameObject.GetComponent<Hoyustus>().ejecucionCorrutinaPrueba((transform.position.x <= collision.transform.position.x) ? 1 : -1, 2f);

            }
            Destroy(gameObject);
        }
    }
}
