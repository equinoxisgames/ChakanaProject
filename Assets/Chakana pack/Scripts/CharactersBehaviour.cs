using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersBehaviour : MonoBehaviour
{

    protected int gold;
    protected float vida;
    protected float defensa;
    protected float ataque;
    protected float ataqueMax;

    [Header("Estados Elementales")]
    [SerializeField] protected bool estadoViento;
    [SerializeField] protected bool estadoFuego;
    [SerializeField] protected bool estadoVeneno;
    [SerializeField] protected int counterEstados;
    [SerializeField] protected float afectacionViento;
    [SerializeField] protected float afectacionFuego = 5;
    [SerializeField] protected float afectacionVeneno = 0.05f;
    [SerializeField] protected int aumentoFuegoPotenciado = 1;
    [SerializeField] protected float aumentoDanioParalizacion = 0f;


    //***************************************************************************************************
    //DETECCION COLISIONES
    //***************************************************************************************************
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            recibirDanioExplosion(collision.gameObject.GetComponent<ExplosionBehaviour>().danioExplosion);
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE ESTADO VIENTO
    //***************************************************************************************************
    protected IEnumerator afectacionEstadoViento()
    {
        afectacionViento = 0.10f;
        yield return new WaitForSeconds(10f);
        estadoViento = false;
        afectacionViento = 0;
        counterEstados = 0;
    }


    //***************************************************************************************************
    //CORRUTINA DE ESTADO FUEGO
    //***************************************************************************************************
    protected IEnumerator afectacionEstadoFuego()
    {
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(2f);
            vida -= (afectacionFuego * aumentoFuegoPotenciado);
        }
        aumentoFuegoPotenciado = 1;
        estadoFuego = false;
        counterEstados = 0;
        ataque = this.ataqueMax;
    }


    //***************************************************************************************************
    //CORRUTINA DE ESTADO VENENO
    //***************************************************************************************************
    protected IEnumerator afectacionEstadoVeneno()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(2f);
            vida *= (1 - afectacionVeneno);
        }
        estadoVeneno = false;
        counterEstados = 0;
    }


    //***************************************************************************************************
    //RECIBIR DANIO ATAQUE ENEMIGO
    //***************************************************************************************************
    public void recibirDanioAtaque(float ataque) {
        vida -= (ataque * aumentoDanioParalizacion);
    }


    //***************************************************************************************************
    //RECIBIR DANIO EXPLOSION
    //***************************************************************************************************
    public void recibirDanioExplosion(float danio) {
        vida -= danio;
    }


    //***************************************************************************************************
    //GETTERS
    //***************************************************************************************************

    public float getVida()
    {
        return vida;
    }


    public float getAtaque()
    {
        return ataque;
    }


    public int getGold()
    {
        return gold;
    }
}
