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


    void Start()
    {
        Physics2D.IgnoreCollision(this.gameObject.GetComponent<CapsuleCollider2D>(),
                                  GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        Physics2D.IgnoreCollision(this.gameObject.GetComponent<CapsuleCollider2D>(),
                          GameObject.Find("Hoyustus Solicitud Prefab").transform.GetChild(0).GetComponent<BoxCollider2D>());

    }

    void Update()
    {
        if (xObjetivo >= minX && xObjetivo <= maxX) {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(xObjetivo, transform.position.y, transform.position.z), 4 * Time.deltaTime);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" && !atacando) {
            xObjetivo = collider.transform.position.x;

            float distanciaPlayer = Mathf.Abs(transform.position.x - collider.transform.position.x);

            if (distanciaPlayer <= 20)
            {
                tiempoFueraRango = 0;
                tiempoDentroRango += Time.deltaTime;
            }
            else {
                tiempoDentroRango = 0;
                tiempoFueraRango += Time.deltaTime;
            }


            if (distanciaPlayer <= 20 && tiempoDentroRango < 5)
            {
                Debug.Log("Listo para atacar cuerpo a cuerpo");
            }
            else if (distanciaPlayer <= 20 && tiempoDentroRango > 5) {
                Debug.Log("Ataque de aturdimiento");
            }
            else if (distanciaPlayer > 20 && tiempoFueraRango >= 10){
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


    private void ataqueCuerpoCuerpo() { 
        //ATAQUE
    }

    private void ataqueAturdimiento(){
        //ATAQUE
        tiempoDentroRango = 0;
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
        Debug.Log(minX + " " + maxX);
    }

}
