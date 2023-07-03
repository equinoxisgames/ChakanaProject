using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Tzantza : CharactersBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private bool siguiendo = false;
    [SerializeField] private GameObject bolaFuego;
    [SerializeField] private GameObject explosion;
    [SerializeField] private Vector3 objetivo;
    [SerializeField] private float rangoAtaque;
    [SerializeField] private bool ataqueDisponible;
    [SerializeField] private bool atacando;
    [SerializeField] private float cooldownAtaque;
    [SerializeField] GameObject deathFX;

    [SerializeField] private GameObject combFX01;
    [SerializeField] private GameObject combFX02;
    [SerializeField] private GameObject combFX03;

    private GameObject combObj01, combObj02, combObj03;
    private void Muerte()
    {
        if (vida <= 0) {
            Instantiate(deathFX, transform.position, Quaternion.identity);

            Destroy(this.gameObject);
        }      
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        explosionInvulnerable = "ExplosionEnemy";
        explosion = Resources.Load<GameObject>("Explosion");
        bolaFuego = Resources.Load<GameObject>("BolaVeneno");
        layerObject = transform.gameObject.layer;
        fuerzaRecoil = 2f;
        ataqueDisponible = true;
        vidaMax = vida;
    }


    void Update()
    {
        Muerte();

        if (siguiendo && playable) {
            Move();
        }
        
    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 8, 0.5f), ForceMode2D.Impulse);
        //EstablecerInvulnerabilidades(layerObject);
    }


    private IEnumerator Ataque(Vector3 objetivoAtaque) {
        ataqueDisponible = false;
        GameObject bolaFuegoGenerada = Instantiate(bolaFuego, transform.position, Quaternion.identity);
        bolaFuegoGenerada.SetActive(false);
        bolaFuegoGenerada.name += "Enemy";
        atacando = true;
        playable = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForEndOfFrame();
        try
        {
            bolaFuegoGenerada.AddComponent<BolaFuego>().instanciarValores(layerObject, objetivoAtaque);
            bolaFuegoGenerada.SetActive(true);
        }
        catch (Exception e) { }
        //REVISAR SI ES IGUAL DE BUENO CON DOS DE ESTOS RETORNOS
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        try
        {
            bolaFuegoGenerada.GetComponent<BolaFuego>().aniadirFuerza();
        }
        catch (Exception e) { }
        yield return new WaitForSeconds(0.5f);
        atacando = false;
        playable = true;
        yield return new WaitForSeconds(cooldownAtaque);
        ataqueDisponible = true;
    }


    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14 && playable)
        {
            int direccion = 1;
            if (collider.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }

            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            if (collider.transform.parent != null)
            {
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                recibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());
            }
            return;
        }


        if (!collider.name.Contains("Enemy")) {
            triggerElementos_1_1_1(collider);
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            siguiendo = true;
            objetivo = collision.transform.position;

            try
            {
                if (Vector3.Distance(transform.position, collision.transform.position) <= rangoAtaque && ataqueDisponible)
                {
                    StartCoroutine(Ataque(collision.transform.position));
                }
            } catch { }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.name.Contains("Enemy"))
        {
            collisionElementos_1_1_1(collision);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            siguiendo = false;
            rb.velocity = Vector2.zero;
        }
    }


    private void Move()
    {
        Vector2 direction = objetivo - transform.position;
        float distance = Vector2.Distance(transform.position, objetivo);

        if (objetivo.x < transform.position.x) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (objetivo.x > transform.position.x) {
            transform.localScale = Vector3.one;
        }

        if (Vector3.Distance(transform.position, objetivo) > 3.2f)
        {
            rb.velocity = direction.normalized * movementSpeed * (1 - afectacionViento);
        }
        else {
            rb.velocity = Vector2.zero;
        }
    }

    private IEnumerator combinacionesElementales()
    {
        if (counterEstados == 11)
        {
            //VIENTO - FUEGO

            if (combObj01 == null) combObj01 = Instantiate(combFX01, transform.position, Quaternion.identity);

            estadoViento = false;
            afectacionViento = 0;
            counterEstados = 10;
            aumentoFuegoPotenciado = 3;
            ataque = ataqueMax * 0.75f;
            StopCoroutine("afectacionEstadoFuego");
            StartCoroutine("afectacionEstadoFuego");
        }
        else if (counterEstados == 101)
        {
            //VENENO - VIENTO

            if (combObj02 == null) combObj02 = Instantiate(combFX02, transform.position, Quaternion.identity, transform);

            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoViento");
            rb.velocity = Vector3.zero;
            counterEstados = 0;
            estadoVeneno = false;
            estadoViento = false;
            playable = false;
            aumentoDanioParalizacion = 1.5f;
            yield return new WaitForSeconds(2f);
            playable = true;
            aumentoDanioParalizacion = 1f;

            if (combObj02 != null) Destroy(combObj02);

            //StartCoroutine(setParalisis());

        }
        else if (counterEstados == 110)
        {
            //FUEGO - VENENO

            if (combObj03 == null) combObj03 = Instantiate(combFX03, transform.position, Quaternion.identity);

            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoFuego");
            counterEstados = 0;
            explosion.GetComponent<ExplosionBehaviour>().modificarValores(3, 45, 6, 12, "Untagged", "ExplosionPlayer");
            Instantiate(explosion, transform.position, Quaternion.identity);
            estadoVeneno = false;
            estadoFuego = false;
        }
        yield return new WaitForEndOfFrame();
    }

}