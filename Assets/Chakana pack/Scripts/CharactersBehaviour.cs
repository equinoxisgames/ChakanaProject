using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersBehaviour : MonoBehaviour
{

    [SerializeField] protected int gold;
    [SerializeField] protected float vida;
    [SerializeField] protected float defensa;
    [SerializeField] protected float ataque;
    [SerializeField] protected float ataqueMax;

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
    [SerializeField] protected string explosionInvulnerable;
    protected int layerObject;
    protected Rigidbody2D rb;
    protected bool paralizadoPorAtaque = false;



    //***************************************************************************************************
    //CORRUTINA DE DANIO E INVULNERABILIDAD (POSIBLE SEPARACION DE LA VULNERABILIDAD A METODO INDIVIDUAL)
    //***************************************************************************************************
    protected IEnumerator cooldownRecibirDanio(int direccion)
    {

        Recoil(direccion);
        if (vida <= 0)
        {
            yield break;
        }

        //Aniadir el brillo (Mientras se lo tenga se lo simulara con el cambio de la tonalidad del sprite)
        yield return new WaitForSeconds(0.5f);
        //SE DETIENE EL RECOIL
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);
        //EL OBJECT PUEDE VOLVER A MOVERSE SIN ESTAR EN ESTE ESTADO DE "SER ATACADO"
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
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE

        if(rb.gravityScale == 0) 
            rb.AddForce(new Vector2(direccion * 10, 1), ForceMode2D.Impulse);
        else
            rb.AddForce(new Vector2(direccion * 10, 8), ForceMode2D.Impulse);
        EstablecerInvulnerabilidades(layerObject);
    }


    //***************************************************************************************************
    //FUNCION DE INVULNERABILIDAD A TODO DANIO
    //***************************************************************************************************
    protected void EstablecerInvulnerabilidades(int layerObject)
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        Physics2D.IgnoreLayerCollision(layerObject, 12, true);
        Physics2D.IgnoreLayerCollision(layerObject, 15, true);
    }


    //***************************************************************************************************
    //FUNCION DE INVULNERABILIDAD A TODO DANIO
    //***************************************************************************************************
    protected void QuitarInvulnerabilidades(int layerObject)
    {
        invulnerable = false;
        Physics2D.IgnoreLayerCollision(3, layerObject, false);
        Physics2D.IgnoreLayerCollision(layerObject, 12, false);
        Physics2D.IgnoreLayerCollision(layerObject, 15, false);

    }


    //***************************************************************************************************
    //DETECCION COLISIONES
    //***************************************************************************************************
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        //LAYER EXPLOSION
        if ((collider.gameObject.layer == 12 && collider.gameObject.GetComponent<ExplosionBehaviour>().getTipoExplosion() != explosionInvulnerable))
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

            recibirDanio(collider.gameObject.GetComponent<ExplosionBehaviour>().getDanioExplosion());
            StartCoroutine(cooldownRecibirDanio(direccion));
            //StartCoroutine(cooldownInvulnerabilidadExplosiones());
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE COOLDOWN INVULNERABILIDADES POR EXPLOSIONES
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

        //DE SER TRUE SIGNIFICARIA QUE EL JUGADOR ESTA PARALIZADO VOLVIENDO A SUS VALORES REGULARES (ELIMINACION PARALISIS)
        if (paralizadoPorAtaque)
        {
            playable = true;
            aumentoDanioParalizacion = 1.0f;
            paralizadoPorAtaque = true;
        }
    }


    public void setParalisis()
    {
        rb.velocity = Vector3.zero;
        playable = false;
        aumentoDanioParalizacion = 1.5f;
        paralizadoPorAtaque = true;
    }

    public void quitarParalisis()
    {
        playable = true;
        aumentoDanioParalizacion = 1f;
        paralizadoPorAtaque = false;
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
