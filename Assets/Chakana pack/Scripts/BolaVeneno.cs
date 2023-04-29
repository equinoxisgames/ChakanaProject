using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BolaVeneno : MonoBehaviour
{
    private Rigidbody2D rb;
    private float tiempoEliminacion = 8f;

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
            if (this.gameObject.layer == 11) {
                //PLAYER
            }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Player" || collision.gameObject.layer == 6) {
            if (this.gameObject.layer == 11){
                //PLAYER
            }
            else if (this.gameObject.layer == 3)
            {
                //EXPLOSION
                //GENERAR CHARCO
                Destroy(this.gameObject);
            }
        }
    }
}
