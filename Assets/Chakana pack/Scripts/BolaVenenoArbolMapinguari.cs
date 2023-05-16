using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BolaVenenoArbolMapinguari : BolaVeneno
{
    private Vector3 hoyustus;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        //rb.isKinematic = true;
        //rb.Sleep();
        hoyustus = GameObject.Find("Hoyustus Solicitud Prefab").transform.position + Vector3.up;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, hoyustus, 20 * Time.deltaTime);
        tiempoEliminacion -= Time.deltaTime;

        if (transform.position == hoyustus) {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        if (tiempoEliminacion <= 0)
        {
            Destroy(this.gameObject);
        }
    }


    public void instanciarValores(GameObject explosion, int layer) {
        this.explosion = explosion;
        gameObject.layer = layer;
        this.explosion.GetComponent<ExplosionBehaviour>().modificarValores(6, 25, 6, 12, "Veneno", "ExplosionEnemy");
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.layer == 6 || collision.gameObject.layer == 16)
        {
            Debug.Log(collision.gameObject.name);
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void activar() {
        rb.WakeUp();
    }
}
