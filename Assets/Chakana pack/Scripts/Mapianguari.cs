using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;

public class Mapianguari : CharactersBehaviour
{
    public bool persiguiendo = true, atacando = false, cambiandoPlataforma;
    public float rangoAtaqueCuerpo;
    public float tiempoDentroRango, tiempoFueraRango;
    public float minX, maxX;
    public float xObjetivo;
    public bool ataqueDisponible;
    private BoxCollider2D ataqueCuerpo, campoVision;
    private CapsuleCollider2D cuerpo;
    private float movementVelocity = 4;
    //private float movementVelocitySecondStage = 8;
    private float maxVida;
    private bool segundaEtapa = false;

    [SerializeField] private GameObject bolaVeneno;
    [SerializeField] private GameObject explosion;

    [SerializeField] private GameObject plataformaUno;
    [SerializeField] private GameObject plataformaDos;
    [SerializeField] private GameObject plataformaTres;
    [SerializeField] public int nuevaPlataforma;
    [SerializeField] public int plataformaActual;

    void Start()
    {
        //Physics2D.IgnoreLayerCollision(13, 15, true);

        plataformaActual = 0;
        nuevaPlataforma = 0;

        //INICIALIZACION VARIABLES
        explosionInvulnerable = "ExplosionEnemy";
        vida = 200;
        maxVida = vida;
        ataqueMax = 20;
        ataque = ataqueMax;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        counterEstados = 0;
        layerObject = transform.gameObject.layer;
        ataqueDisponible = true;
        cuerpo = transform.GetComponent<CapsuleCollider2D>();
        campoVision = transform.GetChild(0).GetComponent<BoxCollider2D>();
        ataqueCuerpo = transform.GetChild(1).GetComponent<BoxCollider2D>();

        ataqueCuerpo.enabled = false;
        layerObject = transform.gameObject.layer;

        //SE DESACTIVAN LAS COLISIONES DEL CUERPO DEL BOSS CON EL DASHBODY DE HOYUSTUS Y SU CUERPO ESTANDAR
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").transform.GetChild(0).GetComponent<BoxCollider2D>());


        //CARGA DE PREFABS
        bolaVeneno = Resources.Load<GameObject>("BolaVeneno");
        explosion = Resources.Load<GameObject>("Explosion");
    }


    private void FixedUpdate()
    {
        if (vida <= maxVida / 2) {
            movementVelocity = 8;
            segundaEtapa = true;
        }
        if (vida <= 0) {
            StartCoroutine(Muerte());
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE MUERTE
    //***************************************************************************************************
    private IEnumerator Muerte() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        campoVision.enabled = false;
        xObjetivo = transform.position.x;
        //TIEMPO ANIMACION DEL Boss
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }


    void Update()
    {

        if (nuevaPlataforma != plataformaActual) {
            plataformaActual = nuevaPlataforma;
            transform.position = new Vector3(transform.position.x, 5 + plataformaActual * 20, 0);
        }
        //MODIFICACION DE POSICION A SEGUIR AL PLAYER AL ESTAR EN LA MISMA PLATAFORMA
        if (xObjetivo >= minX && xObjetivo <= maxX && !atacando) {
            transform.position = Vector3.MoveTowards(this.transform.position, Vector3.right * xObjetivo, movementVelocity * (1 - afectacionViento) * Time.deltaTime);
        }
    }

    //***************************************************************************************************
    //DETECCION DE TRIGGERS
    //***************************************************************************************************
    private new void OnTriggerEnter2D(Collider2D collider)
    {

        //DETECCIONS DE TRIGGERS DE OBJETOS CON LAYER EXPLOSION O ARMA_PLAYER
        if (collider.gameObject.layer == 12 || collider.gameObject.layer == 14)
        {
            //direccion nos dara la orientacion de recoil al sufrir danio
            int direccion = 1;
            if (collider.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }

            StartCoroutine(cooldownRecibirDanio(direccion));
            if (collider.gameObject.layer == 12) {
                Debug.Log("golpeado por explosion");
                recibirDanio(collider.gameObject.GetComponent<ExplosionBehaviour>().getDanioExplosion());
            }
            else {
                Debug.Log("golpeado por arma");
                //EN EL CASO DE TRATARSE DE LA LANZA DE HOYUSTUS
                if (collider.transform.parent != null)
                {
                    recibirDanio(collider.transform.parent.parent.GetComponent<CharactersBehaviour>().getAtaque());
                }
                else {
                    //Reg
                    // DE LO CONTRARIO SE RECIBIRIA EL DANIO DE EL ARCO (NO DISPONIBLE EN LA BETA) O DE LA EXPLOSION DE LA HABILIDAD CONDOR
                    if (collider.transform.name == "Explosion(Clone)") {
                        recibirDanio(collider.transform.GetComponent<ExplosionBehaviour>().getDanioExplosion());
                    }
                }
            }
        }

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
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
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
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
        yield return new WaitForEndOfFrame();
    }


   private void OnTriggerStay2D(Collider2D collider){

        if (collider.gameObject.tag == "Player") {
            xObjetivo = collider.transform.position.x;


            //Cambio Orientacion
            if (xObjetivo < transform.position.x) {
                transform.localScale = new Vector3(-5, 5, 1);
            }
            else if (xObjetivo > transform.position.x) {
                transform.localScale = new Vector3(5, 5, 1);
            }

            float distanciaPlayer = Mathf.Abs(transform.position.x - collider.transform.position.x);

            if (distanciaPlayer <= 12)
            {
                tiempoFueraRango = 0;
                tiempoDentroRango += Time.deltaTime;
            }
            else {
                tiempoDentroRango = 0;
                tiempoFueraRango += Time.deltaTime;
            }

            if (ataqueDisponible && distanciaPlayer <= 12 && tiempoDentroRango < 5)
            {
                StartCoroutine(ataqueCuerpoCuerpo());               
            }
            else if (ataqueDisponible && distanciaPlayer <= 12 && tiempoDentroRango > 5) {
                StartCoroutine(ataqueAturdimiento());
            }
            else if (ataqueDisponible && distanciaPlayer > 12 && tiempoFueraRango >= 10){
                StartCoroutine(ataqueDistancia());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player") {
            //SIGNIFICARIA QUE EL PLAYER ESTA EN OTRA PLATAFORMA
            tiempoDentroRango = 0;
            tiempoFueraRango = 0;
            //cambio de plataforma
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE DE ATURDIMIENTO
    //***************************************************************************************************
    private IEnumerator ataqueAturdimiento() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        atacando = true;
        ataqueDisponible = false;
        //GameObject Hoyustus = GameObject.
        //TIEMPO PARA LA ANIMACION
        Debug.Log("Preparando ataque inmovilizador");
        yield return new WaitForSeconds(1.5f);

        //GENERACION DEL CHARCO DE VENENO
        if (segundaEtapa) { 
        
        }

        //SE EVALUA SI HOYUSTUS ESTA EN EL RANGO DEL ATAQUE
        if (Mathf.Abs(transform.position.x - GameObject.FindObjectOfType<Hoyustus>().GetComponent<Transform>().position.x) <= 15) {

            StartCoroutine(aturdirPlayer());
            Debug.Log("Te inmovilizo");
            yield return new WaitForSeconds(0.5f);
        }

        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = true;
        atacando = false;
    }


    private IEnumerator aturdirPlayer() {
        GameObject.FindObjectOfType<Hoyustus>().setParalisis();
        yield return new WaitForSeconds(3f);
        GameObject.FindObjectOfType<Hoyustus>().quitarParalisis();
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE CUERPO A CUERPO
    //***************************************************************************************************
    private IEnumerator ataqueCuerpoCuerpo(){

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        ataqueDisponible = false;
        ataqueCuerpo.enabled = true;
        atacando = true;
        //EXTENDER UN POCO LA DIMENSION DEL BOXCOLLIDER
        //EL TIEMPO DEPENDERA DE LA ANIMACION
        yield return new WaitForSeconds(0.3f);
        ataqueCuerpo.enabled = false;

        //DASH TRAS ATAQUE EN LA SEGUNDA ETAPA
        if (segundaEtapa) {
            Debug.Log("EMBESTIDA");
            //transform.position = transform.position + Vector3.up * 0.1f;
            rb.gravityScale = 0;
            rb.velocity = new Vector2(6f * -transform.localScale.x, 0f);
            yield return new WaitForSeconds(0.5f);
            rb.gravityScale = 5;
            rb.velocity = Vector2.zero;
        }
        atacando = false;
        yield return new WaitForSeconds(1.5f);
        ataqueDisponible = true;
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE A DISTANCIA
    //***************************************************************************************************
    private IEnumerator ataqueDistancia() {
        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        atacando = true;
        ataqueDisponible = false;
        Debug.Log("Listo para atacar a distancia");
        //CORREGIR POR EL TIEMPO DE LA ANIMACION
        yield return new WaitForSeconds(1f);
        if (!segundaEtapa)
        {
            float auxDisparo = -10f;
            for (int i = 0; i < 10; i++)
            {
                GameObject bolaVenenoGenerada = Instantiate(bolaVeneno, transform.position, Quaternion.identity);
                yield return new WaitForEndOfFrame();
                bolaVenenoGenerada.GetComponent<BolaVeneno>().aniadirFuerza(-transform.localScale.x, layerObject, 5, 20 + auxDisparo * 1.5f, explosion);
                auxDisparo++; 
                yield return new WaitForSeconds(0.05f);
            }
        }
        else { 
        
        }


        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        atacando = false;
        ataqueDisponible = true;
        tiempoDentroRango = 0f;
        tiempoFueraRango = 0f;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //AL TOCAR UNA PLATAFORMA SE ESTABLECEN SUS LIMITES DE MOVIMIENTO EN X
        if (collision.gameObject.layer == 6) {
            minX = collision.transform.GetChild(0).gameObject.transform.position.x;
            maxX = collision.transform.GetChild(1).gameObject.transform.position.x;
        }
    }

}
