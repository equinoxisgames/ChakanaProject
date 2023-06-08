using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Tzantza : CharactersBehaviour
{
    //variables

    //private CinemachineVirtualCamera cm;
    //private SpriteRenderer sp;
    //private Hoyustus hoyustusPlayerCotroller;
    //private bool applyForce;
    //public float detectionRadius = 3;
    //public LayerMask playerLayer;

    //public Vector2 tzantzaHeadPossition;
    //public bool inTzantzaHead;
    //public int tzantzaLives;

    //public string tzantzaName;

    [SerializeField] private float movementSpeed;
    [SerializeField] private bool siguiendo = false;
    [SerializeField] private GameObject bolaFuego;
    [SerializeField] private GameObject explosion;
    [SerializeField] private Vector3 objetivo;
    [SerializeField] private float rangoAtaque;
    [SerializeField] private bool ataqueDisponible;
    [SerializeField] private bool atacando;


    private void Awake()
    {
        //cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        //sp = GetComponent<SpriteRenderer>();
    }

    private void Muerte()
    {
        if (vida <= 0) {
            Destroy(this.gameObject);
        }      
    }

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.name = tzantzaName; 
        rb = GetComponent<Rigidbody2D>();
        explosionInvulnerable = "ExplosionEnemy";
        explosion = Resources.Load<GameObject>("Explosion");
        bolaFuego = Resources.Load<GameObject>("BolaVeneno");
        layerObject = transform.gameObject.layer;
        //hoyustusPlayerCotroller = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
        fuerzaRecoil = 2f;
        ataqueDisponible = true;
    }

    // Update is called once per frame
    void Update()
    {
        Muerte();
        /*detectionRadius = 15;
        movementSpeed = 7;

        Vector2 direction = hoyustusPlayerCotroller.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hoyustusPlayerCotroller.transform.position);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        if (distance <= detectionRadius)
        {
            rb.velocity = direction.normalized * movementSpeed;
            TzantzaFlip(direction.normalized.x);
        }
        else {
            rb.velocity = direction.normalized * -0.5f;

            if (distance <= detectionRadius +1)
                transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            //TzantzaFlipBack(direction.normalized.x);
        }*/


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
        bolaFuegoGenerada.AddComponent<BolaFuego>().instanciarValores(layerObject, objetivoAtaque);
        bolaFuegoGenerada.SetActive(true);

        //REVISAR SI ES IGUAL DE BUENO CON DOS DE ESTOS RETORNOS
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        bolaFuegoGenerada.GetComponent<BolaFuego>().aniadirFuerza();
        yield return new WaitForSeconds(0.7f);
        atacando = false;
        playable = true;
        yield return new WaitForSeconds(2.3f);
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
        }


        if (!collider.name.Contains("Enemy")) {
            if (collider.gameObject.tag == "Viento")
            {
                if (estadoViento)
                {
                    StopCoroutine("afectacionEstadoViento");
                }
                else if (counterEstados > 0)
                {
                    counterEstados += 1;
                    StartCoroutine("combinacionesElementales");
                    return;

                }

                estadoViento = true;
                counterEstados = 1;
                StartCoroutine("afectacionEstadoViento");
            }
            else if (collider.gameObject.tag == "Fuego")
            {
                if (estadoFuego)
                {
                    StopCoroutine("afectacionEstadoFuego");
                }
                else if (counterEstados > 0)
                {
                    counterEstados += 10;
                    StartCoroutine("combinacionesElementales");
                    return;
                }
                estadoFuego = true;
                counterEstados = 10;
                StartCoroutine("afectacionEstadoFuego");
            }
            else if (collider.gameObject.tag == "Veneno")
            {
                if (estadoVeneno)
                {
                    StopCoroutine("afectacionEstadoVeneno");
                }
                else if (counterEstados > 0)
                {
                    counterEstados += 100;
                    StartCoroutine("combinacionesElementales");
                    return;
                }
                estadoVeneno = true;
                counterEstados = 100;
                StartCoroutine("afectacionEstadoVeneno");
            }
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            siguiendo = true;
            objetivo = collision.transform.position;

            if (Vector3.Distance(transform.position, collision.transform.position) <= rangoAtaque && ataqueDisponible) {
                StartCoroutine(Ataque(collision.transform.position));
            }
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

        rb.velocity = direction.normalized * movementSpeed * (1 - afectacionViento);
    }

    private IEnumerator combinacionesElementales()
    {
        if (counterEstados == 11)
        {
            //VIENTO - FUEGO
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
            //StartCoroutine(setParalisis());

        }
        else if (counterEstados == 110)
        {
            //FUEGO - VENENO
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

    /*
    private void TzantzaFlip(float xDirection)
    {
        //Debug.Log("TzantzaFlip xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x);

        if (xDirection<0 && transform.localScale.x >0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        }else if (xDirection > 0 && transform.localScale.x < 0)
        {

            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }
    */
    /*
    private void TzantzaFlipBack(float xDirection)
    {

        

        // if (xDirection < 0 && transform.localScale.x > 0)
        if (xDirection > 0 &&  transform.localScale.x < 0)
        {

            Debug.Log("Entra if 1 .. TzantzaFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: " + hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3((-1*hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        //else if (xDirection > 0 && transform.localScale.x < 0)
        else if (xDirection < 0 && transform.localScale.x > 0)
        {

            Debug.Log("Entra if 2 .. TzantzaFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: "+ hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            

        }

    }
    */
}