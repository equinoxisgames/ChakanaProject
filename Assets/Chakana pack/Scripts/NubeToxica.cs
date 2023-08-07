using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NubeToxica : MonoBehaviour
{
    [SerializeField] private float visibilidadNube = 0;
    [SerializeField] private float signoVisibilidad = 0;
    [SerializeField] private bool danioPlayerDisponible = true;
    [SerializeField] private Hoyustus hoyustus;
    void Start()
    {
        hoyustus = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
    }

    private void Update()
    {
        if (visibilidadNube >= 0 || visibilidadNube <= 10)
        {
            visibilidadNube += Time.deltaTime * signoVisibilidad;
            if (visibilidadNube >= 10)
            {
                Debug.Log("a");
                if (danioPlayerDisponible)
                {
                    Debug.Log("b");
                    danioPlayerDisponible = false;
                    StartCoroutine(daniarPlayer());
                }
                visibilidadNube = 10;
                //booleano para herir al player con el gas
            }
            else if (visibilidadNube > 8)
            {
                transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (visibilidadNube > 6)
            {
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(4).gameObject.SetActive(false);
            }
            else if (visibilidadNube > 4)
            {
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(3).gameObject.SetActive(false);
            }
            else if (visibilidadNube > 2)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
            }
            else if (visibilidadNube > 0)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
                signoVisibilidad = 0;
            }
        }
    }

    private IEnumerator daniarPlayer() {
        Debug.Log("c");
        hoyustus.recibirDanio(25);
        yield return new WaitForSeconds(1f);
        danioPlayerDisponible = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            signoVisibilidad = 1;        
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            signoVisibilidad = -1;
        }
    }

}
