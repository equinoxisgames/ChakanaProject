using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class BolaVeneno : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    protected float tiempoEliminacion = 5f;
    protected GameObject explosion;
    [SerializeField] private GameObject charco;

    bool generado = false;

    private void Update()
    {
        tiempoEliminacion -= Time.deltaTime;
        if (tiempoEliminacion <= 0) {
            //HACER LA DIFERENCIACION CON EL LAYER SI TIENE UNA CAPA PLAYER O ENEMY
            //PLAYER
            if (gameObject.layer == 0) {
                Destroy(charco);
                Destroy(gameObject);
            }
            //ENEMY
            else if (gameObject.layer == 3) {
                //EXPLOSION
                Destroy(charco);
                Destroy(gameObject);
            }
        }
    }

    public void AniadirFuerza(float direccion, int layer) {
        transform.gameObject.layer = layer;
        gameObject.tag = "Veneno";

        rb.WakeUp();
        rb.AddForce(new Vector3(12f * -direccion, 12f, 0f), ForceMode2D.Impulse);

        /*float distanceToTarget = Vector2.Distance(transform.position, target.position);

        Vector3 newPos = target.position;
        newPos.y += newPos.y + distanceToTarget / 3;
        print(newPos.y);

        Vector2 direction = (newPos - transform.position).normalized;

        float forceMultiplier = Mathf.Clamp(distanceToTarget, 1f, maxForceMultiplier);

        rb.WakeUp();

        Vector2 launchForce = direction * baseProjectileSpeed * forceMultiplier;
        rb.velocity = launchForce;*/
    }


    public void AniadirFuerza(float direccion, int layer, float velocityX, float velocityY, GameObject explosion)
    {
        transform.gameObject.layer = layer;
        gameObject.tag = "Veneno";
        rb.WakeUp();
        rb.AddForce(new Vector3(velocityX * -direccion, velocityY, 0f), ForceMode2D.Impulse);
        this.explosion = explosion;
        this.explosion.GetComponent<ExplosionBehaviour>().modificarValores(3, 50, 6, 12, "Veneno", "ExplosionEnemy");
    }


    private IEnumerator GenerarCharco(Vector3 position) 
    {
        //GetComponent<SpriteRenderer>().enabled = false;
        generado = true;
        transform.GetChild(0).gameObject.SetActive(false);
        rb.linearVelocity= Vector3.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<CircleCollider2D>().enabled = false;
        GameObject charcoGenerado = Instantiate(charco, transform.position, Quaternion.identity);
        charcoGenerado.name = "CharcoVenenoPlayer";
        yield return new WaitForSeconds(1.5f);
        Destroy(charcoGenerado);
        Destroy(this.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (generado) return;

        //ENEMY
        if ((collider.gameObject.tag == "Player" || collider.gameObject.layer == 6 || collider.gameObject.layer == 16 || collider.gameObject.layer == 17) && gameObject.layer == 3)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(charco);
            Destroy(gameObject);
        }
        else if (gameObject.layer == 14 && (collider.gameObject.layer == 6)) {
            tiempoEliminacion = 2f;
            //GENERAR CHARCO
            StartCoroutine(GenerarCharco(transform.localPosition));
        }
        else if ((collider.gameObject.layer == 3 || collider.gameObject.layer == 19 || collider.gameObject.layer == 16) && transform.gameObject.layer == 14)
        {
            tiempoEliminacion = 2f;

            StartCoroutine(GenerarCharco(transform.localPosition));
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //PLAYER
        if (collision.gameObject.layer == 6 || (collision.gameObject.layer == 17
            && collision.transform.position.y < transform.position.y))
        {
            tiempoEliminacion = 5;
            //GENERAR CHARCO
            StartCoroutine(GenerarCharco(transform.localPosition));
        }
    }


    public void setExplosion(GameObject explosion) {
        this.explosion = explosion;
    }
}
