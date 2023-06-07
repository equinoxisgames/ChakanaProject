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
        hoyustus = GameObject.Find("Hoyustus Solicitud Prefab").transform.position + Vector3.up;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, hoyustus, 20 * Time.deltaTime);
        tiempoEliminacion -= Time.deltaTime;

        if (tiempoEliminacion <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void instanciarValores(int layer)
    {
        gameObject.layer = layer;
        gameObject.tag = "Fuego";
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == 6 || collision.gameObject.layer == 16)
        {
            if (collision.gameObject.CompareTag("Player")) {
                try {
                    collision.gameObject.GetComponent<Hoyustus>().recibirDanio(15);
                }
                catch (Exception e) { 
                
                }
            }
            Destroy(gameObject);
        }
    }
}
