using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    [SerializeField] private int limitRadio = 3;
    [SerializeField] private float indiceExplosion = 6;
    [SerializeField] private float danioExplosion = 45;
    [SerializeField] private bool habilitada;


    private void Start()
    {
        habilitada = false;
    }

    void FixedUpdate()
    {
        //Debug.Log(habilitada);
        this.gameObject.transform.localScale += Vector3.one * Time.deltaTime * indiceExplosion;
        if (transform.localScale.x >= limitRadio)
        {
            Destroy(this.gameObject);
        }
    }

    public void modificarValores(int limitRadio, float danioExplosion, float indiceExplosion) {
        explosionHabilitada();
        this.indiceExplosion = indiceExplosion;
        this.danioExplosion = danioExplosion;
        this.limitRadio = limitRadio;
    }


    public void modificarValores(int limitRadio, float danioExplosion, float indiceExplosion, int layer, string tag){
        this.transform.gameObject.layer = layer;
        this.transform.gameObject.tag = tag;
        habilitada = true;
        this.indiceExplosion = indiceExplosion;
        this.danioExplosion = danioExplosion;
        this.limitRadio = limitRadio;
    }

    private void explosionHabilitada() {
        habilitada = true;
    }

    public float getDanioExplosion() {
        return danioExplosion;
    }

}
