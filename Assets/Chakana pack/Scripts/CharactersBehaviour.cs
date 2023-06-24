using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersBehaviour : MonoBehaviour
{
    [Header("Atributos Basicos")]
    [SerializeField] protected int gold;
    [SerializeField] protected float vida;
    [SerializeField] protected float defensa;
    [SerializeField] protected float ataque;
    [SerializeField] protected float ataqueMax;
    [Space(5)]

    [Header("Estados Elementales")]
    [SerializeField] protected bool estadoViento;
    [SerializeField] protected bool estadoFuego;
    [SerializeField] protected bool estadoVeneno;
    [SerializeField] protected int counterEstados;
    [SerializeField] protected float afectacionViento;
    [SerializeField] protected float afectacionFuego = 15;
    [SerializeField] protected float afectacionVeneno = 0.05f;
    [SerializeField] protected int aumentoFuegoPotenciado = 1;
    [SerializeField] protected float aumentoDanioParalizacion = 1f;
    [Space(5)]

    [Header("Invulnerabilidad")]
    [SerializeField] protected bool invulnerable = false;
    [SerializeField] protected bool playable = true;
    [SerializeField] protected string explosionInvulnerable;
    [SerializeField] protected bool inmuneDash = false;

    [SerializeField] GameObject vientoFX;
    [SerializeField] GameObject fuegoFX;
    [SerializeField] GameObject venenoFX;
    [SerializeField] GameObject recieveDmgFX;
    protected int layerObject;
    protected Rigidbody2D rb;
    protected bool paralizadoPorAtaque = false;
    public float fuerzaRecoil;
    private GameObject vientoObj, fuegoObj, venenoObj;
    private float vidaMax = 0;

    

    //***************************************************************************************************
    //CORRUTINA DE DANIO E INVULNERABILIDAD (POSIBLE SEPARACION DE LA VULNERABILIDAD A METODO INDIVIDUAL)
    //***************************************************************************************************
    protected IEnumerator cooldownRecibirDanio(int direccion, float fuerzaRecoil)
    {
        Recoil(direccion, fuerzaRecoil);
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
    protected virtual void Recoil(int direccion, float fuerzaRecoil)
    {
        /*playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE

        if(rb.gravityScale == 0) 
            rb.AddForce(new Vector2(direccion * 10, 1), ForceMode2D.Impulse);
        else
            rb.AddForce(new Vector2(direccion * 10, rb.gravityScale * 4), ForceMode2D.Impulse);
        EstablecerInvulnerabilidades(layerObject);*/
    }


    //***************************************************************************************************
    //FUNCION DE INVULNERABILIDAD A TODO DANIO
    //***************************************************************************************************
    protected void EstablecerInvulnerabilidades(int layerObject)
    {
        invulnerable = true;
        //Physics2D.IgnoreLayerCollision(3, layerObject, true);
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
            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            triggerElementos_1_1_1(collider);
            return;
            //StartCoroutine(cooldownInvulnerabilidadExplosiones());
        }

        if ((collider.gameObject.layer != gameObject.layer))
        {
            triggerElementos_1_1_1(collider);
            return;
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
        if (vientoObj == null) vientoObj = Instantiate(vientoFX, transform.position, Quaternion.identity, transform);
        afectacionViento = 0.10f;
        print(gameObject.name);
        yield return new WaitForSeconds(10f);
        estadoViento = false;
        afectacionViento = 0;
        counterEstados = 0;
        if (vientoObj != null) Destroy(vientoObj);
    }


    //***************************************************************************************************
    //CORRUTINA DE ESTADO FUEGO
    //***************************************************************************************************
    protected IEnumerator afectacionEstadoFuego()
    {
        if (fuegoObj == null) fuegoObj = Instantiate(fuegoFX, transform.position, Quaternion.identity, transform);
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(2f);
            vida -= (afectacionFuego * aumentoFuegoPotenciado);
        }
        aumentoFuegoPotenciado = 1;
        estadoFuego = false;
        counterEstados = 0;
        ataque = this.ataqueMax;
        if (fuegoObj != null) Destroy(fuegoObj);
    }


    //***************************************************************************************************
    //CORRUTINA DE ESTADO VENENO
    //***************************************************************************************************
    protected IEnumerator afectacionEstadoVeneno()
    {
        if (venenoObj == null) venenoObj = Instantiate(venenoFX, transform.position, venenoFX.transform.rotation, transform);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(2f);
            float dmg = vidaMax * afectacionVeneno;
            vida -= dmg;
            print(vidaMax);
        }
        estadoVeneno = false;
        counterEstados = 0;
        if (venenoObj != null) Destroy(venenoObj);
    }


    //***************************************************************************************************
    //RECIBIR DANIO ATAQUE ENEMIGO
    //***************************************************************************************************
    public void recibirDanio(float danio) {

        if(vidaMax == 0) vidaMax = vida;

        vida -= (danio * aumentoDanioParalizacion);

        Instantiate(recieveDmgFX, transform.position, Quaternion.identity);
        //GetComponent<AudioSource>().Stop();
        //GetComponent<AudioSource>().Play();

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


    protected void collisionElementos_1_1_1(Collision2D collider) {
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
        if (collider.gameObject.tag == "Viento")
        {
            //REINICIO ESTADO VIENTO
            if (estadoViento)
            {
                StopCoroutine("afectacionEstadoViento");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;

            }

            //SE ESTABLECE EL ESTADO DE VIENTO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");            
        }
       
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
        else if (collider.gameObject.tag == "Fuego")
        {
            //REINICIO ESTADO FUEGO
            if (estadoFuego)
            {
                StopCoroutine("afectacionEstadoFuego");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;
            }

            //SE ESTABLECE EL ESTADO DE FUEGO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
        }

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VENENO
        else if (collider.gameObject.tag == "Veneno")
        {
            //REINICIO ESTADO VENENO
            if (estadoVeneno)
            {
                StopCoroutine("afectacionEstadoVeneno");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 100;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;
            }

            //SE ESTABLECE EL ESTADO DE VENENO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoVeneno = true;
            counterEstados = 100;
            StartCoroutine("afectacionEstadoVeneno");
        }

    }

    protected void triggerElementos_1_1_1(Collider2D collider)
    {
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
        if (collider.gameObject.tag == "Viento")
        {
            //REINICIO ESTADO VIENTO
            if (estadoViento)
            {
                StopCoroutine("afectacionEstadoViento");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;

            }

            //SE ESTABLECE EL ESTADO DE VIENTO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");
        }

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
        else if (collider.gameObject.tag == "Fuego")
        {
            //REINICIO ESTADO FUEGO
            if (estadoFuego)
            {
                StopCoroutine("afectacionEstadoFuego");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;
            }

            //SE ESTABLECE EL ESTADO DE FUEGO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
        }

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VENENO
        else if (collider.gameObject.tag == "Veneno")
        {
            //REINICIO ESTADO VENENO
            if (estadoVeneno)
            {
                StopCoroutine("afectacionEstadoVeneno");
            }
            //SE DISPARA AL TENER YA UN ESTADO ELEMENTAL ACTIVO
            else if (counterEstados > 0)
            {
                counterEstados += 100;
                if (venenoObj != null) Destroy(venenoObj);
                if (fuegoObj != null) Destroy(fuegoObj);
                if (vientoObj != null) Destroy(vientoObj);
                StartCoroutine("combinacionesElementales");
                return;
            }

            //SE ESTABLECE EL ESTADO DE VENENO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoVeneno = true;
            counterEstados = 100;
            StartCoroutine("afectacionEstadoVeneno");
        }

    }

}
