using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

//[RequireComponent(typeof(ParticleSystem))]
public class BolaVeneno : MonoBehaviour
{
    private Rigidbody2D rb;
    private float tiempoEliminacion = 5f;
    private GameObject explosion;
    private GameObject particulas;
    private bool activado = true;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        particulas = transform.GetChild(0).gameObject;
        particulas.SetActive(false);
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


    private IEnumerator GenerarCharco(Vector3 position) {
        GetComponent<SpriteRenderer>().enabled = false;
        rb.velocity= Vector3.zero;
        rb.isKinematic = true;
        this.GetComponent<CircleCollider2D>().enabled = false;
        //particulas.SetActive(true);
        //GetComponent<CircleCollider2D>().enabled = false;
        GameObject charco = new GameObject();
        charco.transform.position = position;
        charco.SetActive(false);
        charco.tag = "Veneno";
        charco.layer = 11;
        charco.AddComponent<BoxCollider2D>();
        charco.GetComponent<BoxCollider2D>().isTrigger = true;
        charco.GetComponent<BoxCollider2D>().size = new Vector2(10f, 1f);
        charco.SetActive(true);
        charco.transform.parent = this.gameObject.transform;
        yield return new WaitForSeconds(5f);
        //Destroy(charco.gameObject);
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        //ENEMY
        if (collider.gameObject.tag == "Player" || collider.gameObject.layer == 6)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //PLAYER
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.layer == 6)
        {
            tiempoEliminacion = 5;
            //GENERAR CHARCO
            Debug.Log("Generar Charco");
            //Destroy(gameObject);
            StartCoroutine(GenerarCharco(transform.localPosition));
        }

    }


    /*private void charco() {
        GameObject charco = new GameObject();
        charco.SetActive(false);
        charco.tag = "Veneno";
        charco.AddComponent<BoxCollider2D>();
        charco.GetComponent<BoxCollider2D>().isTrigger = true;
        charco.GetComponent<BoxCollider2D>().size = new Vector2(10f, 1f);
        GameObject charcoGenerado = Instantiate(charco, transform.position + Vector3.down * 5f, Quaternion.identity);
        //StartCoroutine(destruirCharco(charcoGenerado));
    }*/


    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Pan");
        if (other.layer == 6) {
            Debug.Log("F");
        }
    }
}
