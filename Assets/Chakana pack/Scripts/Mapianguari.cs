using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using static System.Random;
using static Unity.Burst.Intrinsics.X86;

public class Mapianguari : CharactersBehaviour
{
    public bool persiguiendo = true, atacando = false, cambiandoPlataforma;
    public float rangoAtaqueCuerpo;
    public float tiempoDentroRango, tiempoFueraRango;
    public float minX, maxX;
    public float xObjetivo;
    public bool ataqueDisponible;
    private BoxCollider2D ataqueCuerpo, campoVision;
    private CapsuleCollider2D cuerpo;
    [SerializeField] private float movementVelocity = 6;
    [SerializeField] private float valorAtaqueBasico;
    [SerializeField] private float valorAtaqueEspecial;
    [SerializeField] private float coolDownAtaque;
    //private float maxVida;
    private bool segundaEtapa = false;

    [SerializeField] private GameObject bolaVeneno;
    //[SerializeField] private GameObject explosion;
    [SerializeField] public int nuevaPlataforma;
    [SerializeField] public int plataformaActual;
    [SerializeField] private GameObject charcoVeneno;
    [SerializeField] private bool usandoAtaqueEspecial = false;
    [SerializeField] private bool ataqueEspecialDisponible = true;
    [SerializeField] private bool cambioPlataformaDisponible = true;
    [SerializeField] private float timerAtaqueEspecial = 0f;
    [SerializeField] private LiquidBar lifeBar;
    [SerializeField] AudioClip audioHurt;
    [SerializeField] AudioClip audioAtk;
    [SerializeField] AudioClip audioScream;

    AudioSource charAudio;
    //[SerializeField] private GameObject combFX01;

    //private GameObject combObj01;

    private float xCharco = 10f;
    private float yCharco = 1.0f;
    private float escala = 3f;
    private float distanciaAtaqueBasico = 6f;
    private float distanciaAtaqueAturdimiento = 12f;
    private float reduccionTiempoAtaqueDistancia = 0;
    void Start()
    {
        fuerzaRecoil = 4f;
        plataformaActual = 1;
        nuevaPlataforma = 1;

        charAudio = GetComponent<AudioSource>();

        //INICIALIZACION VARIABLES
        explosionInvulnerable = "ExplosionEnemy";
        //vida = 200;
        vidaMax = vida;
        ataqueMax = valorAtaqueBasico;
        ataque = ataqueMax;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        counterEstados = 0;
        layerObject = transform.gameObject.layer;
        ataqueDisponible = true;
        cuerpo = transform.GetComponent<CapsuleCollider2D>();
        campoVision = transform.GetChild(0).GetComponent<BoxCollider2D>();
        ataqueCuerpo = transform.GetChild(1).GetComponent<BoxCollider2D>();

        ataqueCuerpo.enabled = false;
        layerObject = transform.gameObject.layer;

        //SE DESACTIVAN LAS COLISIONES DEL CUERPO DEL BOSS CON EL DASHBODY DE HOYUSTUS Y SU CUERPO ESTANDAR
        //Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        //Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").transform.GetChild(0).GetComponent<BoxCollider2D>());


        //CARGA DE PREFABS
        //bolaVeneno = Resources.Load<GameObject>("BolaVeneno");
        explosion = Resources.Load<GameObject>("Explosion");
    }

    private void UpdateLife()
    {
        lifeBar.targetFillAmount = (vida / vidaMax);
    }

    private void FixedUpdate()
    {
        UpdateLife();

        if (vida <= vidaMax / 2) {
            movementVelocity = 12;
            segundaEtapa = true;
            reduccionTiempoAtaqueDistancia = 5;
            distanciaAtaqueAturdimiento = 15;
        }
        if (vida <= 0) {
            StartCoroutine(Muerte());
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE MUERTE
    //***************************************************************************************************
    private IEnumerator Muerte() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        campoVision.enabled = false;
        xObjetivo = transform.position.x;
        //TIEMPO ANIMACION DEL Boss
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }


    void Update()
    {
        timerAtaqueEspecial += Time.deltaTime;

        if (!usandoAtaqueEspecial && nuevaPlataforma != plataformaActual) {
            if (segundaEtapa && timerAtaqueEspecial > 5)
            {
                System.Random aux = new System.Random();
                int posibilidadAtaqueEspecial = aux.Next(0, 2);
                Debug.Log(posibilidadAtaqueEspecial);
                if (posibilidadAtaqueEspecial == 1)
                {
                    //ATAQUE ESPECIAL
                    StartCoroutine(ataqueEspecial());
                }
            }
            else if(cambioPlataformaDisponible){
                cambioPlataformaDisponible = false;
                StartCoroutine(cambioPlataforma());
                //plataformaActual = nuevaPlataforma;
                //transform.position = new Vector3(transform.position.x, -99.8f + plataformaActual * 8.3f, 0);
            }           
        }
        //MODIFICACION DE POSICION A SEGUIR AL PLAYER AL ESTAR EN LA MISMA PLATAFORMA
        if (!usandoAtaqueEspecial && xObjetivo >= minX && xObjetivo <= maxX && !atacando) {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(xObjetivo, transform.position.y, transform.position.z)/*Vector3.right * xObjetivo*/, movementVelocity * (1 - afectacionViento) * Time.deltaTime);
        }
    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE

        rb.AddForce(new Vector2(direccion * 10, rb.gravityScale * 4), ForceMode2D.Impulse);
        //EstablecerInvulnerabilidades(layerObject);
    }


    //***************************************************************************************************
    //DETECCION DE TRIGGERS
    //***************************************************************************************************
    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        if (collider.gameObject.layer == 14) {
            int direccion = 1;
            if (collider.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }

            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            if (collider.transform.parent != null)
            {
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                recibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());
                charAudio.loop = false;
                charAudio.Stop();
                charAudio.clip = audioHurt;
                charAudio.Play();
            }
        }
        //DETECCIONS DE TRIGGERS DE OBJETOS CON LAYER EXPLOSION O ARMA_PLAYER

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
        if (collider.gameObject.tag == "Viento" && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoViento)
            {
                StopCoroutine("afectacionEstadoViento");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                //StartCoroutine("combinacionesElementales");
                combinacionesElementales();
                return;

            }
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");
        }
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
        else if (collider.gameObject.tag == "Fuego" && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoFuego)
            {
                StopCoroutine("afectacionEstadoFuego");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                //StartCoroutine("combinacionesElementales");
                combinacionesElementales();
                return;

            }
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
        }
    }


    //***************************************************************************************************
    //COMBINACIONES ELEMENTALES
    //***************************************************************************************************
    private new void combinacionesElementales()
    {
        if (counterEstados == 11)
        {
            //VIENTO - FUEGO

            if (combObj01 == null) combObj01 = Instantiate(combFX01, transform.position, Quaternion.identity, transform);

            estadoViento = false;
            afectacionViento = 0;
            counterEstados = 10;
            aumentoFuegoPotenciado = 3;
            ataque = ataqueMax * 0.75f;
            StopCoroutine("afectacionEstadoFuego");
            StartCoroutine("afectacionEstadoFuego");
        }
        //yield return new WaitForEndOfFrame();
    }


    private void OnTriggerStay2D(Collider2D collider){

        //SE EJECUTA SOLO SI MAPINGUARI NO SE ENCUENTRA REALIZANDO EL ATAQUE ESPECIAL
        if (!usandoAtaqueEspecial && collider.gameObject.tag == "Player")
        {
            xObjetivo = collider.transform.position.x;
            float distanciaPlayer = 0;

            if (!atacando) { 
                //CAMBIO DE ORIENTACION
                if (xObjetivo < transform.position.x)
                {
                    transform.localScale = new Vector3(-escala, escala, 1);
                }
                else if (xObjetivo > transform.position.x)
                {
                    transform.localScale = new Vector3(escala, escala, 1);
                }

                distanciaPlayer = Mathf.Abs(transform.position.x - collider.transform.position.x);

                //HOYUSTUS DENTRO DEL RANGO DE ATAQUE DEL BOSS
                if (distanciaPlayer <= distanciaAtaqueAturdimiento)
                {
                    tiempoFueraRango = 0;
                    tiempoDentroRango += Time.deltaTime;
                }
                //HOYUSTUS FUERA DEL RANGO DE ATAQUE DEL BOSS
                else
                {
                    tiempoDentroRango = 0;
                    tiempoFueraRango += Time.deltaTime;
                }
            }

            if (ataqueDisponible && distanciaPlayer <= distanciaAtaqueBasico && tiempoDentroRango > 2 && tiempoDentroRango < 5)
            {
                StartCoroutine(ataqueCuerpoCuerpo());               
            }
            else if (ataqueDisponible && distanciaPlayer <= distanciaAtaqueAturdimiento && tiempoDentroRango > 5) {
                StartCoroutine(ataqueAturdimiento());
            }
            else if (ataqueDisponible && distanciaPlayer > distanciaAtaqueAturdimiento && tiempoFueraRango >= 10 - reduccionTiempoAtaqueDistancia){
                StartCoroutine(ataqueDistancia());
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


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE DE ATURDIMIENTO
    //***************************************************************************************************
    private IEnumerator ataqueAturdimiento() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        charAudio.Stop();
        charAudio.clip = audioScream;
        charAudio.Play();
        atacando = true;
        ataqueDisponible = false;
        //GameObject Hoyustus = GameObject.
        //TIEMPO PARA LA ANIMACION
        Debug.Log("Preparando ataque inmovilizador");
        yield return new WaitForSeconds(1.5f);

        //GENERACION DEL CHARCO DE VENENO
        if (segundaEtapa) {
            GameObject charcoGenerado = Instantiate(charcoVeneno, transform.position + Vector3.down * 2.8f, Quaternion.identity);
            charcoGenerado.name = "CharcoVenenoEnemy";
            //charcoGenerado.layer = 3;
            StartCoroutine(destruirCharco(charcoGenerado));
        }

        //SE EVALUA SI HOYUSTUS ESTA EN EL RANGO DEL ATAQUE
        if (Mathf.Abs(transform.position.x - GameObject.FindObjectOfType<Hoyustus>().GetComponent<Transform>().position.x) <= 15) {

            StartCoroutine(aturdirPlayer());
            Debug.Log("Te inmovilizo");
            yield return new WaitForSeconds(0.5f);
            tiempoDentroRango = 0;
        }

        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = true;
        atacando = false;
    }


    //***************************************************************************************************
    //CORRUTINA EN LA QUE SE PARALIZA Y SE QUITA LA PARALISIS DE HOYUSTUS
    //***************************************************************************************************
    private IEnumerator aturdirPlayer() {
        GameObject.FindObjectOfType<Hoyustus>().setParalisis();
        tiempoDentroRango = 0;
        yield return new WaitForSeconds(3f);
        GameObject.FindObjectOfType<Hoyustus>().quitarParalisis();
    }


    //***************************************************************************************************
    //CORRUTINA DE DESTRUCCION DE CHARCO DE VENENO
    //***************************************************************************************************
    private IEnumerator destruirCharco(GameObject charco) {
        charco.SetActive(true);
        yield return new WaitForSeconds(4f);
        Destroy(charco);
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE CUERPO A CUERPO
    //***************************************************************************************************
    private IEnumerator ataqueCuerpoCuerpo(){

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        charAudio.Stop();
        charAudio.clip = audioAtk;
        charAudio.Play();

        ataqueDisponible = false;
        ataqueCuerpo.enabled = true;
        atacando = true;
        //EXTENDER UN POCO LA DIMENSION DEL BOXCOLLIDER
        //EL TIEMPO DEPENDERA DE LA ANIMACION
        yield return new WaitForSeconds(0.3f);
        ataqueCuerpo.enabled = false;

        //DASH TRAS ATAQUE EN LA SEGUNDA ETAPA
        if (segundaEtapa && !((transform.position.x < minX + 3 && transform.localScale.x > 1) || (transform.position.x > maxX - 3 && transform.localScale.x < 1))) {
            //transform.position = transform.position + Vector3.up * 0.1f;
            rb.gravityScale = 0;
            rb.velocity = new Vector2(7f * -transform.localScale.x, 0f);
            yield return new WaitForSeconds(0.35f);
            rb.gravityScale = 5;
            rb.velocity = Vector2.zero;
        }
        atacando = false;
        yield return new WaitForSeconds(coolDownAtaque);
        ataqueDisponible = true;
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE A DISTANCIA
    //***************************************************************************************************
    private IEnumerator ataqueDistancia() {
        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        atacando = true;
        ataqueDisponible = false;
        //CORREGIR POR EL TIEMPO DE LA ANIMACION
        yield return new WaitForSeconds(1f);
        if (!segundaEtapa)
        {
            float auxDisparo = -10f;
            for (int i = 0; i < 10; i++)
            {
                GameObject bolaVenenoGenerada = Instantiate(bolaVeneno, transform.position, Quaternion.identity);
                bolaVenenoGenerada.AddComponent<BolaVeneno>();
                bolaVenenoGenerada.name += "Enemy";
                yield return new WaitForEndOfFrame();
                bolaVenenoGenerada.GetComponent<BolaVeneno>().aniadirFuerza(-transform.localScale.x, layerObject, 5, 20 + auxDisparo * 1.5f, explosion);
                auxDisparo++; 
                yield return new WaitForSeconds(0.05f);
            }
        }
        else {
            for (int i = 0; i < 10; i++)
            {
                System.Random aux = new System.Random();
                int direccion = aux.Next(0,2);
                Debug.Log(direccion);
                if (direccion == 0) {
                    direccion = -1;
                }
                GameObject bolaVenenoGenerada = Instantiate(bolaVeneno, transform.position, Quaternion.identity);
                bolaVenenoGenerada.AddComponent<BolaVeneno>();
                bolaVenenoGenerada.name += "Enemy";
                yield return new WaitForEndOfFrame();
                bolaVenenoGenerada.GetComponent<BolaVeneno>().aniadirFuerza(-transform.localScale.x * direccion, layerObject, 8, aux.Next(5, 22), explosion);
                yield return new WaitForSeconds(0.05f);
            }
        }


        //REINICIO DE VARIABLES RELACIONADAS A LA DETECCION Y EL ATAQUE
        atacando = false;
        ataqueDisponible = true;
        tiempoDentroRango = 0f;
        tiempoFueraRango = 0f;
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE ESPECIAL
    //***************************************************************************************************
    private IEnumerator ataqueEspecial() {
        usandoAtaqueEspecial = true;
        ataque = valorAtaqueEspecial;
        ataqueMax = ataque;
        atacando = true;

        //DESAPARICION BOSS
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        campoVision.enabled = false;
        transform.position = transform.position + Vector3.right * 100;

        //LANZAMIENTO BOLAS DE VENENO
        for (int i = 1; i <= 8; i++) {
            //POSICIONES ALEATORIAS

            //y -99 -72 x -58 -12
            System.Random aux = new System.Random();
            GameObject bolaVenenoGenerada = null;

            //EL VALOR CORRESPONDE A LA PLATAFORMA EN LA QUE SE GENERARA
            switch (aux.Next(0, 4))
            {
                case 0:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(aux.Next(-36, -7), aux.Next(-99, -96), 0), Quaternion.identity);
                    break;
                case 1:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(aux.Next(-38, -10), aux.Next(-91, -88), 0), Quaternion.identity);
                    break;
                case 2:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(aux.Next(-59, -31), aux.Next(-83, -80), 0), Quaternion.identity);
                    break;
                case 3:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(aux.Next(-38, -10), aux.Next(-75, -72), 0), Quaternion.identity);
                    break;
            }
            bolaVenenoGenerada.name += "Enemy";
            bolaVenenoGenerada.AddComponent<BolaVenenoArbolMapinguari>();
            yield return new WaitForEndOfFrame();
            bolaVenenoGenerada.GetComponent<BolaVenenoArbolMapinguari>().instanciarValores(explosion);
            yield return new WaitForSeconds(1f);
            Debug.Log($"Lanzamiento de bola de veneno {i}");
        }
        yield return new WaitForSeconds(0.3f);

        //EMBESTIDAS
        transform.localScale = new Vector3(-escala, escala, 1);
        for (int i = 1; i <= 3; i++) {
            //DESPLAZAMIENTO A LA PLATAFORMA
            switch (nuevaPlataforma) {
                case 0:
                    transform.position = new Vector3(-14, -99.8f, 0);
                    break;
                case 1:
                    transform.position = new Vector3(-14, -91.5f, 0);
                    break;
                case 2:
                    transform.position = new Vector3(-30, -83.2f, 0);
                    break;
                case 3:
                    transform.position = new Vector3(-14, -74.9f, 0);
                    break;
            }
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

            //MOVIMIENTO DE EXTREMO A EXTREMO
            this.rb.velocity = new Vector2(-26.5f, 0f);
            ataqueCuerpo.enabled = true;
            float extraDashTime = 0f;
            if (nuevaPlataforma == 0) {
                extraDashTime += 0.4f;
            }
            yield return new WaitForSeconds(1f + extraDashTime);
            ataqueCuerpo.enabled = false;

            //DESAPARICION TRAS EMBESTIDA
            rb.velocity = Vector2.zero;
            //OCULTAMIENTO
            if (i != 3) {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                transform.position = transform.position + Vector3.right * 100;
                yield return new WaitForSeconds(0.7f);
            }
            atacando = false;
        }
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        //STUN
        yield return new WaitForSeconds(2.8f);
        //tiempoDentroRango = 0;
        //tiempoFueraRango = 0;
        ataque = valorAtaqueBasico;
        ataqueMax = ataque;
        //RETORNO A VALORES DE JUEGO NORMAL
        campoVision.enabled = true;
        //plataformaActual = nuevaPlataforma;
        usandoAtaqueEspecial = false;
        timerAtaqueEspecial = 0f;

    }


    //***************************************************************************************************
    //CORRUTINA DE CAMBIO DE PLATAFORMA
    //***************************************************************************************************
    private IEnumerator cambioPlataforma() {
        //SE ESCONDE
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = false;
        usandoAtaqueEspecial = true;
        yield return new WaitForSeconds(0.5f);
        //SE DESPLAZA 
        plataformaActual = nuevaPlataforma;
        System.Random aux = new System.Random();
        int posicionTeletransporteX = aux.Next((int)minX, (int)maxX) + 1;
        transform.position = new Vector3(posicionTeletransporteX, -99.8f + plataformaActual * 8.3f, 0);
        yield return new WaitForSeconds(0.5f); 
        //REAPARECE "SALE DE LOS ARBOLES"
        //SE REACTIVA SU MOVIMIENTO
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = true;
        usandoAtaqueEspecial = false;
        cambioPlataformaDisponible = true;
    }


    //***************************************************************************************************
    //DETECCION DE COLISIONES
    //***************************************************************************************************
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //AL TOCAR UNA PLATAFORMA SE ESTABLECEN SUS LIMITES DE MOVIMIENTO EN X
        if ((collision.gameObject.layer == 6 || collision.gameObject.layer == 17) && collision.gameObject.name.StartsWith("Plataforma")) {
            minX = collision.gameObject.GetComponent<PlataformaMapinguari>().minX;
            maxX = collision.gameObject.GetComponent<PlataformaMapinguari>().maxX;
            plataformaActual = collision.gameObject.GetComponent<PlataformaMapinguari>().plataforma;
        }
    }
}
