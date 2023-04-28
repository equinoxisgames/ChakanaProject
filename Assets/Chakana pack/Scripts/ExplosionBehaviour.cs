using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    private int limitRadio = 3;
    private float indiceExplosion = 6;
    public float danioExplosion = 45;
    private bool habilitada;


    private void Start()
    {
        habilitada = false;
    }

    void FixedUpdate()
    {
        Debug.Log(habilitada);
        this.gameObject.transform.localScale += Vector3.one * Time.deltaTime * indiceExplosion;
        if (transform.localScale.x >= limitRadio)
        {
            Debug.Log(limitRadio);
            Destroy(this.gameObject);
        }
    }

    public void modificarValores(int limitRadio, float danioExplosion, float indiceExplosion) {
        explosionHabilitada();
        this.indiceExplosion = indiceExplosion;
        this.danioExplosion = danioExplosion;
        this.limitRadio = limitRadio;
    }


    public void modificarValores(int limitRadio, float danioExplosion, float indiceExplosion, int layer){
        gameObject.layer = layer;
        habilitada = true;
        this.indiceExplosion = indiceExplosion;
        this.danioExplosion = danioExplosion;
        this.limitRadio = limitRadio;
    }

    private void explosionHabilitada() {
        habilitada = true;
    }

}
