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
    [SerializeField] protected float aumentoDanioParalizacion = 1f;
    [SerializeField] protected bool invulnerable = false;
    [SerializeField] protected bool playable = true;
    protected int layerObject;
    protected Rigidbody2D rb;



    //***************************************************************************************************
    //CORRUTINA DE DANIO E INVULNERABILIDAD (POSIBLE SEPARACION DE LA VULNERABILIDAD A METODO INDIVIDUAL)
    //***************************************************************************************************
    protected IEnumerator cooldownRecibirDanio(int direccion)
    {
        //La disminucion de la vida se deberia hacer en funcion del ataque del enemigo u objeto con el que el character se encuentre
        Recoil(direccion);
        if (vida <= 0)
        {
            //StartCoroutine(Muerte());
            yield break;
        }
        //Aniadir el brillo (Mientras se lo tenga se lo simulara con el cambio de la tonalidad del sprite)
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);
        playable = true;
        yield return new WaitForSeconds(2f);
        QuitarInvulnerabilidades(layerObject);
        //this.GetComponent<SpriteRenderer>().color = Color.white;
    }


    //***************************************************************************************************
    //LOGICA DE RECOIL
    //***************************************************************************************************
    protected void Recoil(int direccion)
    {
        playable = false;
        rb.AddForce(new Vector2(direccion * 10, 8), ForceMode2D.Impulse);
        invulnerable = true;
        EstablecerInvulnerabilidades(layerObject);
    }


    //***************************************************************************************************
    //FUNCION DE INVULNERABILIDAD A TODO DANIO
    //***************************************************************************************************
    protected void EstablecerInvulnerabilidades(int layerObject)
    {
        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        Physics2D.IgnoreLayerCollision(layerObject, 12, true);
    }


    //***************************************************************************************************
    //FUNCION DE INVULNERABILIDAD A TODO DANIO
    //***************************************************************************************************
    protected void QuitarInvulnerabilidades(int layerObject)
    {
        invulnerable = false;
        Physics2D.IgnoreLayerCollision(3, layerObject, false);
        Physics2D.IgnoreLayerCollision(layerObject, 12, false);

    }


    //***************************************************************************************************
    //DETECCION COLISIONES
    //***************************************************************************************************
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Explosion")
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
            recibirDanio(collider.gameObject.GetComponent<ExplosionBehaviour>().danioExplosion);
            StartCoroutine(cooldownInvulnerabilidadExplosiones());           
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE DETECCION DE EXPLOSIONES
    //***************************************************************************************************
    protected IEnumerator cooldownInvulnerabilidadExplosiones() {
        yield return new WaitForSeconds(2f);
        QuitarInvulnerabilidades(layerObject);
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
    public void recibirDanio(float danio) {
        vida -= (danio * aumentoDanioParalizacion);
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
