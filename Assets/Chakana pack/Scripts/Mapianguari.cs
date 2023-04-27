using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Mapianguari : MonoBehaviour
{
    public bool persiguiendo, atacando, cambiandoPlataforma;
    public float rangoAtaqueCuerpo;
    public float tiempoDentroRango, tiempoFueraRango;
    public float minX, maxX;
    public float xObjetivo;
    public bool ataqueDisponible;
    private BoxCollider2D ataqueCuerpo, campoVision;
    private CapsuleCollider2D cuerpo;


    void Start()
    {
        ataqueDisponible = true;
        cuerpo = transform.GetComponent<CapsuleCollider2D>();
        campoVision = transform.GetChild(0).GetComponent<BoxCollider2D>();
        ataqueCuerpo = transform.GetChild(1).GetComponent<BoxCollider2D>();

        ataqueCuerpo.enabled = false;

        //Deshabilitar 
        Physics2D.IgnoreLayerCollision(13, 14, true);
        //Physics2D.IgnoreLayerCollision(3, 11, true);
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").transform.GetChild(0).GetComponent<BoxCollider2D>());

    }

    void Update()
    {
        if (xObjetivo >= minX && xObjetivo <= maxX) {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(xObjetivo, transform.position.y, transform.position.z), 4 * Time.deltaTime);
        }
    }

    //EVALUAR SI ESTO ES O NO NECESARIO PUES LA LOGICA DE DANIO SE DARIA DESDE LANZA HOYUSTUS
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 14)
        {
            Debug.Log("Golpeado");
        }
    }


    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" && !atacando) {
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
        //EXTENDER UN POCO LA DIMENSION DEL BOXCOLLIDER
        //EL TIEMPO DEPENDERA DE LA ANIMACION
        yield return new WaitForSeconds(0.3f);
        ataqueCuerpo.enabled = false;
        yield return new WaitForSeconds(1.5f);
        ataqueDisponible = true;
    }

    private IEnumerator coolDownAtaque(Collider2D collider) {
        ataqueDisponible = false;
        collider.enabled = true;
        //EL TIEMPO DEPENDERA DE LA ANIMACION
        yield return new WaitForSeconds(0.3f);
        collider.enabled = false;
        tiempoDentroRango = 0;
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
