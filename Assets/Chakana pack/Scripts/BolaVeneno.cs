using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BolaVeneno : MonoBehaviour
{
    private Rigidbody2D rb;
    private float tiempoEliminacion = 5f;
    private GameObject explosion;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.Sleep();

    }

    private void Update()
    {
        tiempoEliminacion -= Time.deltaTime;
        if (tiempoEliminacion <= 0) {
            //HACER LA DIFERENCIACION CON EL LAYER SI TIENE UNA CAPA PLAYER O ENEMY
            //PLAYER
            if (this.gameObject.layer == 11) {
                Destroy(this.gameObject);
            }
            //ENEMY
            else if (this.gameObject.layer == 3) {
                //EXPLOSION
                //GENERAR CHARCO
                Destroy(this.gameObject);
            }
        }
    }

    public void aniadirFuerza(float direccion, int layer) {
        transform.gameObject.layer = layer;
        rb.WakeUp();
        rb.AddForce(new Vector3(12f * -direccion, 18f, 0f), ForceMode2D.Impulse);
    }


    public void aniadirFuerza(float direccion, int layer, float velocityX, float velocityY)
    {
        transform.gameObject.layer = layer;
        rb.WakeUp();
        rb.AddForce(new Vector3(velocityX * -direccion, velocityY, 0f), ForceMode2D.Impulse);
    }


    public void aniadirFuerza(float direccion, int layer, float velocityX, float velocityY, GameObject explosion)
    {
        aniadirFuerza(direccion, layer, velocityX, velocityY);
        this.explosion = explosion;
        this.explosion.GetComponent<ExplosionBehaviour>().modificarValores(3, 5, 6, 12, "Veneno", "ExplosionEnemy");
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (this.gameObject.layer == 11)
        {
            //PLAYER
            if (collider.gameObject.tag == "Enemy" || collider.gameObject.layer == 6)
            {
                //GENERAR CHARCO
            }
        }
        else if (this.gameObject.layer == 3)
        {
            //ENEMY
            if (collider.gameObject.tag == "Player" || collider.gameObject.layer == 6) 
            {
                Instantiate(explosion, transform.position, Quaternion.identity);
                //GENERAR CHARCO
            }
        }
    }
}
