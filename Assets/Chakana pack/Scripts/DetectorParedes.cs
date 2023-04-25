using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorParedes : MonoBehaviour
{

    //***************************************************************************************************
    //DETECCION DE COLISIONES
    //***************************************************************************************************
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 6)
        {
            transform.GetComponentInParent<Hoyustus>().isTocandoPared(0);
        }
    }



    //***************************************************************************************************
    //EXIT DE TRIGGERS
    //***************************************************************************************************
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 6)
        {
            transform.GetComponentInParent<Hoyustus>().isTocandoPared(1);
        }
    }
}

