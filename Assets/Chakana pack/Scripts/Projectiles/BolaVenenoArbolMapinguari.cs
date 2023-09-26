using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BolaVenenoArbolMapinguari : BolaVeneno
{
    private Vector3 hoyustus;
    private GameObject explosion2;

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

        if (transform.position == hoyustus) {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Instantiate(explosion2, transform.position, Quaternion.identity), 1);
            Destroy(gameObject);
        }
        if (tiempoEliminacion <= 0)
        {
            Destroy(this.gameObject);
        }
    }


    public void InstanciarValores(GameObject explosion, GameObject explosion2) {
        this.explosion = explosion;
        this.explosion2 = explosion2;
        this.explosion.GetComponent<ExplosionBehaviour>().modificarValores(6, 25, 6, 12, "Veneno", "ExplosionEnemy");
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.layer == 6 || collision.gameObject.layer == 16)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(Instantiate(explosion2, transform.position, Quaternion.identity), 1);
            Destroy(gameObject);
        }
    }

}
