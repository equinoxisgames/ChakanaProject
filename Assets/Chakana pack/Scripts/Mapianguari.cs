using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

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


    void Start()
    {
        Physics2D.IgnoreLayerCollision(13, 15, true);
        vida = 200;
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

        //SE DESACTIVAN LAS COLISIONES DEL CUERPO DEL BOSS CON EL DASHBODY DE HOYUSTUS Y SU CUERPO ESTANDAR
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").transform.GetChild(0).GetComponent<BoxCollider2D>());

    }


    private void FixedUpdate()
    {
        if (vida <= 0) {
            StartCoroutine(Muerte());
        }
    }

    //CORRUTINA DE MUERTE HOYUSTUS
    private IEnumerator Muerte() {
        campoVision.enabled = false;
        xObjetivo = transform.position.x;
        //TIEMPO ANIMACION DEL Boss
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }


    //CAMBIO PARA LA PERSECUSION DEL PLAYER
    void Update()
    {
        if (xObjetivo >= minX && xObjetivo <= maxX && !atacando) {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(xObjetivo, transform.position.y, transform.position.z), 4 * Time.deltaTime);
        }
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {

        //LAYER EXPLOSION Y ARMA DEL PLAYER
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
                //CAMBIAR A UN SCRIPT PARA ARMA GENERALIZADO
                recibirDanio(collider.gameObject.GetComponent<LanzaHoyustus>().getDanioArma());
            }
        }

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

            if (distanciaPlayer <= 12 && tiempoDentroRango < 5)
            {
                if(ataqueDisponible)
                    StartCoroutine(ataqueCuerpoCuerpo());               
            }
            else if (distanciaPlayer <= 12 && tiempoDentroRango > 5) {
                Debug.Log("Ataque de aturdimiento");
            }
            else if (distanciaPlayer > 12 && tiempoFueraRango >= 10){
                Debug.Log("Listo para atacar a distancia");
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


    private void ataqueAturdimiento() { 
        //ATAQUE
    }

    private IEnumerator ataqueCuerpoCuerpo(){

        ataqueDisponible = false;
        ataqueCuerpo.enabled = true;
        atacando = true;
        //EXTENDER UN POCO LA DIMENSION DEL BOXCOLLIDER
        //EL TIEMPO DEPENDERA DE LA ANIMACION
        yield return new WaitForSeconds(0.3f);
        ataqueCuerpo.enabled = false;
        atacando = false;
        yield return new WaitForSeconds(1.5f);
        ataqueDisponible = true;
    }


    private void ataqueDistancia() {
        atacando = true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6) {
            minX = collision.transform.GetChild(0).gameObject.transform.position.x;
            maxX = collision.transform.GetChild(1).gameObject.transform.position.x;
        }
    }

}
