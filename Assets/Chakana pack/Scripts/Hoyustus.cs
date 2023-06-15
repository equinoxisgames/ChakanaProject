using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Unity.VisualScripting;
using System;
using System.Runtime.InteropServices.WindowsRuntime;


public class Hoyustus : CharactersBehaviour
{

    [Header("CineMachine")]
    [SerializeField] CinemachineVirtualCamera myVirtualCamera;
    [SerializeField] float myCameraSensivity = 0.006f;

    public PlayerStateList pState;

    [Header("X Axis Movement")]
    [SerializeField] float walkSpeedGround = 9f;
    [SerializeField] float resistenciaAire = 0.3f;
    [SerializeField] float walkSpeed = 12f;

    [Space(5)]

    [Header("Y Axis Movement")]
    [SerializeField] float jumpSpeed = 45;
    [SerializeField] float fallSpeed = 45;
    [SerializeField] int jumpSteps = 20;
    [SerializeField] int jumpThreshold = 7;
    [Space(5)]

    [Header("Ground Checking")]
    [SerializeField] Transform groundTransform;
    [SerializeField] float groundCheckY = 0.2f;
    [SerializeField] float groundCheckX = 1;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask transitionLayer;
    [SerializeField] LayerMask transitionLayer1;
    [SerializeField] LayerMask transitionLayer2;
    [SerializeField] LayerMask transitionLayer3;
    [Space(5)]

    [Header("Roof Checking")]
    [SerializeField] Transform roofTransform;
    [SerializeField] float roofCheckY = 0.2f;
    [SerializeField] float roofCheckX = 1;
    [Space(5)]

    float grabity;
    int stepsJumped = 0;

    public float groundCheckRadius;


    [SerializeField] Animator anim;



    [Header("Audio")]
    [SerializeField] AudioSource AudioWalking;
    [SerializeField] AudioSource AudioJump;
    [SerializeField] AudioSource AudioFall;
    [SerializeField] AudioSource AmbientFlute;

    [SerializeField] AudioSource AudioStep1;
    [SerializeField] AudioSource AudioStep2;
    [SerializeField] AudioSource AudioStep3;
    [SerializeField] AudioSource AudioStep4;
    [SerializeField] AudioSource AudioStep5;
    [SerializeField] AudioSource AudioStep6;
    [SerializeField] AudioSource AudioStep7;
    [SerializeField] AudioSource AudioStep8;

    [SerializeField] AudioSource GameplayIntro;
    [SerializeField] AudioSource GameplayLoop;

    [SerializeField] ParticleSystem ParticleTestParticleTest = null;

    [Header("Movement")]
    public bool doubleJump = false;


    string firstRunPrefsName = "firstRun";
    string flipFlagPrefsName = "flipFlag";

    string escena;


    [SerializeField] private GameObject menuMuerte;
    private BoxCollider2D dashBodyTESTING;


    //Variables relacionadas al recibir danio y la muerte del personaje.
    //public float vida = 100;
    [Header("Variables Player")]
    [SerializeField] private float maxVida = 1000;
    [SerializeField] private float tiempoInvulnerabilidad = 2f;
    //[SerializeField] private const int maxStepsImpulso = 13;
    [SerializeField] private float timeAir = 1.2f;
    [SerializeField] private float currentTimeAir = 0f;
    [SerializeField] private int currentStepsImpulso = 0;
    [Space(5)]


    //[SerializeField] private float multiplicadorGravedad;
    //[SerializeField] private float escalaGravedad;
    //public bool botonSaltoArriba = true;
    [Header("Estados Player")]
    [SerializeField] private bool firstJump = true;
    [SerializeField] private bool secondJump = false;
    [SerializeField] private int tocandoPared = 1;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool isWalking = false;
    [Space(5)]


    [Header("Dash")]
    [SerializeField] private bool dashAvailable = true;
    [SerializeField] private GameObject bodyHoyustus;
    [SerializeField] private GameObject dashBody;
    [SerializeField] private CapsuleCollider2D body;
    [Space(5)]


    [Header("Ataque")]
    [SerializeField] private bool ataqueAvailable = true;
    [SerializeField] private GameObject[] lanzas;
    [Space(5)]


    [Header("Habilidades")]
    [SerializeField] float cargaHabilidadCondor;
    [SerializeField] float cargaHabilidadSerpiente;
    [SerializeField] float cargaHabilidadLanza;
    [SerializeField] float cargaCuracion;
    [Space(5)]


    [Header("PREFABS")]
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject bolaVeneno;
    //MODIFICAR TRAS PRUEBAS
    [SerializeField] private Transform wallPoint;
    [SerializeField] private Transform roofPoint;


    private float posYAntesSalto = 0f;


    [SerializeField] private float botonCuracion = 0f;
    [SerializeField] private bool aplastarBotonCuracion = false;

    private bool realizandoHabilidadLanza = false;

    [SerializeField] private bool curando = false;

    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool atacando = false;
    [SerializeField] private int codigoAtaque = 0;
    [SerializeField] private int SSTEPS = 60;
    [SerializeField] private int CSTEPS = 0;
    private float maxHabilidad_Curacion = 100f;

    float limitY = 0f;

    [SerializeField] private float correctorSalto = 19;

    private bool saltoEspecial = false;

    public void isTocandoPared(int value)
    {
        tocandoPared = value;
    }

    public float getCargaHabilidadCondor()
    {
        return cargaHabilidadCondor;
    }

    public float getCargaHabilidadSerpiente()
    {
        return cargaHabilidadSerpiente;
    }

    public float getCargaHabilidadLanza()
    {
        return cargaHabilidadLanza;
    }

    public float getCargaCuracion()
    {
        return cargaCuracion;
    }


    private void Awake()
    {
        //CARGAR DATA
        LoadData();
        //Debug.Log(gold);

        // Establecer el m nimo de FPS
        //Application.targetFrameRate = 30; // Por ejemplo, 30 FPS

        // Establecer el m ximo de FPS
        //QualitySettings.vSyncCount = 0; // Desactivar el VSync
        //Application.targetFrameRate = 90; // Por ejemplo, 60 FPS

    }


    private void LoadData()
    {
        if (!PlayerPrefs.HasKey("iniciado"))
        {
            PlayerPrefs.SetInt("iniciado", 1);
            SaveManager.SavePlayerData(GetComponent<Hoyustus>());
        }

        PlayerData playerData = SaveManager.LoadPlayerData();
        if (playerData != null)
        {
            gold = playerData.getGold();
            ataque = playerData.getAtaque();
            vida = playerData.getVida();
            if (playerData.getVida() <= 0) vida = maxVida;
            cargaHabilidadCondor = playerData.getCondor();
            cargaHabilidadSerpiente = playerData.getSerpiente();
            cargaHabilidadLanza = playerData.getLanza();
            cargaCuracion = playerData.getCuracion();
        }
        else
        {
            SaveManager.SavePlayerData(GetComponent<Hoyustus>());
        }
    }

    public void SavePlayerData()
    {
        SaveManager.SavePlayerData(vida, gold, cargaHabilidadCondor, cargaHabilidadSerpiente, cargaHabilidadLanza, cargaCuracion);
    }


    void Start()
    {
        //ESTABLECER FRAME RATE
        Application.targetFrameRate = 70;

        limitY = transform.position.y + 2;

        //IGNORACION DE COLISIONES A LO LARGO DE LA ESCENA --> DEBERIA IR EN UN GAMEMANAGER OBJECT
        Physics2D.IgnoreLayerCollision(11, 14, true);
        Physics2D.IgnoreLayerCollision(13, 12, true);
        Physics2D.IgnoreLayerCollision(13, 14, true);
        Physics2D.IgnoreLayerCollision(13, 15, true);

        //INICIALIZACION VARIABLES 
        invulnerable = false;
        explosionInvulnerable = "ExplosionPlayer";
        layerObject = this.gameObject.layer;
        aumentoDanioParalizacion = 1f;
        lanzas = new GameObject[transform.GetChild(this.transform.childCount - 1).childCount];
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        anim = this.gameObject.GetComponent<Animator>();
        grabity = rb.gravityScale;
        escena = SceneManager.GetActiveScene().name;
        ataqueMax = 5;
        ataque = ataqueMax;
        dashBodyTESTING = this.gameObject.GetComponent<BoxCollider2D>();
        dashBodyTESTING.enabled = false;

        //INICIALIZACION DE LOS GAMEOBJECTS DE LAS LANZAS
        for (int i = 0; i < lanzas.Length; i++)
        {
            lanzas[i] = transform.GetChild(this.transform.childCount - 1).GetChild(i).gameObject;
        }


        //INICIALIZACION DE body Y bodyDash
        //bodyHoyustus = this.transform.GetChild(0).gameObject;
        dashBody = this.transform.GetChild(0).gameObject;
        body = this.gameObject.GetComponent<CapsuleCollider2D>();
        //dashBody.SetActive(false);


        //AudioWalking.Play();
        if (pState == null)
        {
            pState = GetComponent<PlayerStateList>();
        }


        //escalaGravedad = rb.gravityScale;


        if (escena == "00- StartRoom 1" || escena == "05 - Room GA1" || escena == "05 - Room GA1")
        {
            //StartCoroutine(PlayGamePlayLoop());
        }

        //Debug.Log("Escena: " + escena);

        //Lee datos de memoria

        pState.firstRunFlag = PlayerPrefs.GetInt(firstRunPrefsName, 1);
        pState.flipFlag = PlayerPrefs.GetInt(flipFlagPrefsName, 0);

        /*if (pState.firstRunFlag == 0)
        {
            pState.x = PlayerPrefs.GetFloat(nextPositionXPrefsName, 0);
            pState.y = PlayerPrefs.GetFloat(nextPositionYPrefsName, 0);
            PlayerPrefs.SetInt(firstRunPrefsName, 1);
            rb.transform.position = new Vector2(pState.x, pState.y);
           // Debug.Log("pState.flipFlag : " + pState.flipFlag);
            if (pState.flipFlag == 1) transform.localScale = new Vector2(-1, transform.localScale.y);
        }*/

        //Debug.Log(maxStepsImpulso);


        //CARGA DE PREFABS
        explosion = Resources.Load<GameObject>("Explosion");
        bolaVeneno = Resources.Load<GameObject>("BolaVeneno");


        SSTEPS = 55;
        maxVida = vida;
    }


    void Update()
    {
        //AudioWalking.Play();
        //GetInputs();
        //WalkingControl();
        //Flip();
        //Walk(xAxis);
        tocarPared();

        if(transform.parent != null)
        {
            limitY = transform.position.y + 8.5f;
        }

        //HABILIDADES ELEMENTALES
        //Debug.Log(CSTEPS);
        //Debug.Log(rb.velocity.y);
        if (botonCuracion >= 0.3f)
        {
            botonCuracion = 0f;
            aplastarBotonCuracion = false;
        }

        if (aplastarBotonCuracion)
        {
            botonCuracion += Time.deltaTime;

            if (!curando && Input.GetButton("Jump") && cargaHabilidadCondor >= maxHabilidad_Curacion)
            {
                StartCoroutine("habilidadCondor");
            }
            if (!curando && Input.GetButton("Dash") && cargaHabilidadSerpiente >= maxHabilidad_Curacion)
            {
                dashAvailable = false;
                StartCoroutine("habilidadSerpiente");
            }
            if (!curando && Input.GetButton("Atacar") && cargaHabilidadLanza >= maxHabilidad_Curacion)
            {
                StartCoroutine("habilidadLanza");
            }
        }

        if (Input.GetButtonDown("Activador_Habilidades"))
        {

            aplastarBotonCuracion = true;
            //ACTIVACION DE LA CURACION
            if (cargaCuracion >= maxHabilidad_Curacion && aplastarBotonCuracion && botonCuracion > 0f && botonCuracion < 0.3f)
            {
                curando = true;
                cargaCuracion = 0;
                playable = false;
                aplastarBotonCuracion = false;
                botonCuracion = 0f;
                StartCoroutine("Curacion");
                return;
            }


            /*if (!curando && Input.GetButton("Jump") && cargaHabilidadCondor >= maxHabilidad_Curacion)
            {
                StartCoroutine("habilidadCondor");
            }
            if (!curando && Input.GetButton("Dash") && cargaHabilidadSerpiente >= maxHabilidad_Curacion)
            {
                dashAvailable = false;
                StartCoroutine("habilidadSerpiente");
            }
            if (!curando && Input.GetButton("Atacar") && cargaHabilidadLanza >= maxHabilidad_Curacion)
            {
                StartCoroutine("habilidadLanza");
            }*/

            return;
        }

        if (playable)
        {
            //Walk();
            Falling();
            ataqueLanza();
            Dash();
            //Jump(0.1f, 0.1f);
            //jumpPrueba();


            /*if (Input.GetButton("Jump") && !isJumping && !doubleJump)
            {
                Jump();
            }

            if (Input.GetButton("Jump") && isJumping && !doubleJump)
            {
                isJumping = false;
            }

            if (Input.GetButton("Jump") && !isJumping && doubleJump)
            {
                DoubleJump();
            }*/
        }
        //Recoil();
        //Attack();
    }


    /*private void Jump()
    {
        rb.AddForce(new Vector2(0f, 5), ForceMode2D.Impulse);
        isJumping = true;
        //timeAir = 0f;
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(0f, 5), ForceMode2D.Impulse);
        doubleJump = true;
    }*/


    private void jumpPrueba()
    {
        if (Input.GetButtonUp("Jump"))
        {
            if(!atacando)
                anim.Play("Caer");
            rb.velocity = new Vector2(rb.velocity.x, 0);
            isJumping = false;
            CSTEPS = 0;

            if (firstJump)
            {
                secondJump = true;
                firstJump = false;
                currentTimeAir = 0;
                return;
            }
            else
            {
                secondJump = false;
                return;
            }
        }

        if (firstJump && !secondJump && !isTouchingRoof() && CSTEPS < SSTEPS)
        {
            if (Input.GetButtonUp("Jump") || /*currentTimeAir > timeAir ||*/ CSTEPS >= SSTEPS || transform.position.y >= limitY || isTouchingRoof())// || transform.position.y - posYAntesSalto > limitSaltoUno)
            {
                anim.Play("Caer");
                secondJump = true;
                isJumping = false;
                firstJump = false;
                CSTEPS = 0;
                rb.velocity = new Vector2(rb.velocity.x, 0);
                currentTimeAir = 0;
                //limitY += 7;
                //Debug.Log("Alto");
                return;
            }

            if (Input.GetButtonDown("Jump") && Grounded())
            {
                anim.Play("Saltar");
                saltoEspecial = false;
                isJumping = true;
                secondJump = false;
                //currentStepsImpulso = 0;
                rb.AddForce(new Vector2(0, 6f), ForceMode2D.Impulse);
                //Debug.Log("Salto");
                isJumping = true;
                //cargaHabilidadCondor += 0.05f;
                cargaHabilidadCondor += 20;
                //posYAntesSalto = transform.position.y;
                CSTEPS++;
                limitY = transform.position.y + 8.5f;
                //Debug.Log(limitY);

            }
            else if (Input.GetButton("Jump") && isJumping && transform.position.y < limitY/*&& currentTimeAir <= timeAir*/)//&&  transform.position.y - posYAntesSalto <= limitSaltoUno)
            {
                //Seria mejor subir la fuerza inicial e impulso de salto pero reducir a costa el tiempo limite de esta mecanica
                //MODIFICANDO VELOCIDADES
                //rb.velocity += Vector2.up * -Physics2D.gravity * (1.5f /*- 0.5f * currentStepsImpulso/maxStepsImpulso)*/ ) * Time.deltaTime;
                //AGREGANDO FUERZAS
                isJumping = true;
                //rb.velocity = new Vector2(rb.velocity.x, ((7 + 0.5f * 18f * ((35 - CSTEPS) * (35 - CSTEPS) )/20)/(35 - CSTEPS)/20));
                rb.AddForce(new Vector2(0, ((7 + 0.5f * correctorSalto * ((SSTEPS - CSTEPS) * (SSTEPS - CSTEPS)) / 35) / (SSTEPS - CSTEPS) / 35)), ForceMode2D.Impulse);
                ///rb.AddForce(new Vector2(0, 0.8f - (0.022f * CSTEPS)), ForceMode2D.Impulse);
                currentTimeAir += Time.fixedDeltaTime;
                //Debug.Log("Impulso");
                //cargaHabilidadCondor += 0.005f;
                CSTEPS++;
            }

        }
        //DOBLE SALTO
        else if (!firstJump && secondJump && !isTouchingRoof() && CSTEPS < SSTEPS)
        {

            if (Input.GetButtonDown("Jump") && CSTEPS == 0)
            {
                anim.Play("Doble Salto");
                CSTEPS = 1;
                //currentStepsImpulso = 0;
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                rb.AddForce(new Vector2(0, -rb.velocity.y + 18), ForceMode2D.Impulse);
                Debug.Log("Salto 2");
                isJumping = true;
                secondJump = true;
                limitY = transform.position.y + 8.5f;
                //cargaHabilidadCondor += 0.05f;
                cargaHabilidadCondor += 20;
            }
            else if (Input.GetButton("Jump") && isJumping && transform.position.y < limitY/*&& currentTimeAir <= timeAir - 0.2f*/)// && transform.position.y - posYAntesSalto <= limitSaltoDos)
            {
                //Seria mejor subir la fuerza inicial e impulso de salto pero reducir a costa el tiempo limite de esta mecanica
                //MODIFICANDO VELOCIDADES
                //rb.velocity += Vector2.up * -Physics2D.gravity * (2f /*- 0.5f * currentStepsImpulso/maxStepsImpulso)*/ ) * Time.deltaTime;
                //AGREGANDO FUERZAS
                CSTEPS++;
                rb.AddForce(new Vector2(0, 1.15f - (limitY - transform.position.y) / 10), ForceMode2D.Impulse);
                currentTimeAir += Time.fixedDeltaTime;
                isJumping = true;
                //Debug.Log("Impulso 2");
                //cargaHabilidadCondor += 0.005f;

            }


            if (Input.GetButtonUp("Jump") || transform.position.y >= limitY/*|| currentTimeAir > timeAir - 0.2f */ || CSTEPS > SSTEPS || isTouchingRoof())// || transform.position.y - posYAntesSalto > limitSaltoUno)
            {
                if (!atacando)
                    anim.Play("Caer");
                CSTEPS = 0;
                isJumping = false;
                secondJump = false;
                rb.velocity = new Vector2(rb.velocity.x, 0);
                //Debug.Log("Alto 2");
                return;
            }

        }

    }

    private void LateUpdate()
    {
        if (playable)
        {
            jumpPrueba();
        }
    }


    void FixedUpdate()
    {
        /*if (isJumping && currentTimeAir < timeAir)
        {
            // Incrementar la fuerza del salto en funci n del tiempo que se mantiene presionado el bot n
            rb.AddForce(new Vector2(0f, 5f * (timeAir - currentTimeAir)), ForceMode2D.Impulse);
            currentTimeAir += Time.fixedDeltaTime;
        }*/

        if (playable && !atacando)
        {
            /*if (!saltoEspecial)
            {
                jumpPrueba();
            }
            else {
                saltoEspecialSalto();
            }*/
            //jumpPrueba();
            Walk();
        }
        /*if (rb.velocity.y < 0)
        {
            //Falling();
            rb.gravityScale = 4;
        }
        else {
            rb.gravityScale = 2;
        }*/

        if (vida <= 0)
        {
            StartCoroutine(Muerte());
        }
        /*if (pState.recoilingX == true && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY == true && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (Grounded())
        {
            StopRecoilY();
        }
        */
        if (EventTransition())
        {
            //Debug.Log("Disparar Transici n");
            LoadNextLevel();
        }
        //ImproveJump();
        //Jump();
        //Walk();
        //Walk(xAxis);
        //WalkingControl();
        //Jump(0.1f, 0.1f);
        anim.SetBool("Walking", rb.velocity.x != 0);
        anim.SetBool("Grounded", Grounded());
        anim.SetFloat("YVelocity", rb.velocity.y);
        anim.SetFloat("XVelocity", rb.velocity.x);
        anim.SetFloat("Vida", vida);
        anim.SetFloat("Ataque", ataque);
        anim.SetInteger("Gold", gold);
        anim.SetBool("Dashing", isDashing);
        anim.SetBool("Atacando", atacando);
        //anim.SetBool("Curando", curando);
        anim.SetInteger("CA", codigoAtaque);
        anim.SetBool("SecondJump", secondJump);
        anim.SetBool("Jumping", isJumping);

    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE

        if (rb.gravityScale == 0)
            rb.AddForce(new Vector2(direccion * 4 * fuerzaRecoil, 1), ForceMode2D.Impulse);
        else
            rb.AddForce(new Vector2(direccion * 4 * fuerzaRecoil, rb.gravityScale * 2), ForceMode2D.Impulse);
        EstablecerInvulnerabilidades(layerObject);
    }


    //***************************************************************************************************
    //CURACION DEL PLAYER
    //***************************************************************************************************
    private IEnumerator Curacion()
    {
        //CAMBIO A LA ANIMACION
        yield return new WaitForSeconds(1f);
        playable = true;
        vida += 350;
        curando = false;
        Debug.Log("Curado");
    }


    //***************************************************************************************************
    //HABILIDAD CONDOR
    //***************************************************************************************************
    private IEnumerator habilidadCondor()
    {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA HABILIDAD
        cargaHabilidadCondor = 0f;
        playable = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        cargaCuracion += 30;

        //SE MODIFICA EL GAMEOBJECT DEL PREFAB EXPLOSION Y SE LO INSTANCIA
        explosion.GetComponent<ExplosionBehaviour>().modificarValores(15, 1, 15, 12, "Viento", explosionInvulnerable);
        Instantiate(explosion, transform.position + Vector3.up * 1f, Quaternion.identity);

        //SE ESPERA HASTA QUE SE GENERE ESTA EXPLOSION
        yield return new WaitForSeconds(1.2f);

        //SE VUELVEN A ESTABLECER LOS VALORES DE JUEGO NORMAL
        playable = true;
        rb.gravityScale = 2;
    }


    private IEnumerator habilidadSerpiente()
    {
        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA HABILIDAD
        playable = false;
        dashAvailable = false;
        cargaHabilidadSerpiente = 0f;
        cargaCuracion += 30;

        //SE GENERA OTRO OBJETO A PARTIR DEL PREFAB BOLAVENENO Y SE LO MODIFICA
        GameObject bolaVenenoGenerada = Instantiate(bolaVeneno, transform.position + Vector3.up, Quaternion.identity);
        bolaVenenoGenerada.GetComponent<CircleCollider2D>().isTrigger = false;
        bolaVenenoGenerada.AddComponent<BolaVeneno>();
        yield return new WaitForEndOfFrame();
        bolaVenenoGenerada.GetComponent<BolaVeneno>().aniadirFuerza(transform.localScale.x, layerObject);
        yield return new WaitForEndOfFrame();

        //SE VUELVEN A ESTABLECER LOS VALORES DE JUEGO NORMAL
        dashAvailable = true;
        playable = true;

    }


    private IEnumerator habilidadLanza()
    {
        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        EstablecerInvulnerabilidades(layerObject);
        invulnerable = true;
        cargaCuracion += 30;

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA HABILIDAD
        atacando = true;
        codigoAtaque = 3;
        realizandoHabilidadLanza = true;
        cargaHabilidadLanza = 0f;
        playable = false;
        body.enabled = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        Physics2D.IgnoreLayerCollision(3, 11, true);

        //ACTIVACION Y MODIFICACION DE LA LANZA
        lanzas[0].tag = "Fuego";
        ataque = 15;
        lanzas[0].SetActive(true);


        IEnumerator movimientoHabilidadLanza()
        {
            rb.AddForce(new Vector2(transform.localScale.x * 20, 0), ForceMode2D.Impulse);
            yield return new WaitForSeconds(1);
            rb.velocity = Vector2.zero;
            realizandoHabilidadLanza = false;
        }
        StartCoroutine(movimientoHabilidadLanza());
        yield return new WaitUntil(() => (tocandoPared == 0 || realizandoHabilidadLanza == false));
        Physics2D.IgnoreLayerCollision(3, 11, true);
        atacando = false;
        codigoAtaque = 0;

        //SE VUELVEN A ESTABLECER LOS VALORES DE JUEGO NORMAL
        QuitarInvulnerabilidades(layerObject);
        invulnerable = false;
        body.enabled = true;
        realizandoHabilidadLanza = false;
        playable = true;
        rb.gravityScale = 2f;
        ataque = 5;

        //DESACTIVACION Y MODIFICACION DE LA LANZA
        lanzas[0].SetActive(false);
        lanzas[0].tag = "Untagged";
        lanzas[0].layer = 14;
    }


    public void setPlayable(bool state)
    {
        playable = state;
    }


    public void setAumentoDanioParalizacion(float value)
    {
        aumentoDanioParalizacion = value;
    }


    //***************************************************************************************************
    //DETECCION DE COLISIONES
    //***************************************************************************************************
    private void OnCollisionEnter2D(Collision2D collision)
    {

        //COLISIONES PARA OBJETOS TAGUEADOS COMO ENEMY
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 18)
        {
            //direccion nos dara la orientacion de recoil al sufrir danio
            int direccion = 1;
            if (collision.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }


            try
            {
                //DETECCION DE DEL CUERPO DEL ENEMIGO
                if (!invulnerable && collision.gameObject.transform.parent.name == "-----ENEMIES")
                {
                    //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
                    //hurtParticleSystem.Play();
                    recibirDanio(collision.gameObject.GetComponent<CharactersBehaviour>().getAtaque());
                    StartCoroutine(cooldownRecibirDanio(direccion, collision.gameObject.GetComponent<CharactersBehaviour>().fuerzaRecoil));
                    collisionElementos_1_1_1(collision);
                }

            }
            catch (Exception e)
            {

            }

            if (!invulnerable)
                collisionElementos_1_1_1(collision);
            /*
            //CONVERTIFLO A FUNCION
            if (collision.gameObject.tag == "Viento")
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
                    StartCoroutine("combinacionesElementales");
                    return;

                }

                //SE ESTABLECE EL ESTADO DE VIENTO Y SUS RESPECTIVOS COMO ACTIVOS
                estadoViento = true;
                counterEstados = 1;
                StartCoroutine("afectacionEstadoViento");
            }

            //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
            else if (collision.gameObject.tag == "Fuego")
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
                    StartCoroutine("combinacionesElementales");
                    return;
                }

                //SE ESTABLECE EL ESTADO DE FUEGO Y SUS RESPECTIVOS COMO ACTIVOS
                estadoFuego = true;
                counterEstados = 10;
                StartCoroutine("afectacionEstadoFuego");
            }

            //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VENENO
            else if (collision.gameObject.tag == "Veneno")
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
                    StartCoroutine("combinacionesElementales");
                    return;
                }

                //SE ESTABLECE EL ESTADO DE VENENO Y SUS RESPECTIVOS COMO ACTIVOS
                estadoVeneno = true;
                counterEstados = 100;
                StartCoroutine("afectacionEstadoVeneno");
            }*/
            //vida -= 20;
            //StartCoroutine(cooldownRecibirDanio(direccion));
            //recibirDanio(collision.gameObject.GetComponent<CharactersBehaviour>().getAtaque());
        }
    }



    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 18)
        {
            //direccion nos dara la orientacion de recoil al sufrir danio
            int direccion = 1;
            if (collision.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }


            try
            {
                //DETECCION DE DEL CUERPO DEL ENEMIGO
                if (!invulnerable && collision.gameObject.transform.parent.name == "-----ENEMIES")
                {
                    recibirDanio(collision.gameObject.GetComponent<CharactersBehaviour>().getAtaque());
                    StartCoroutine(cooldownRecibirDanio(direccion, collision.gameObject.GetComponent<CharactersBehaviour>().fuerzaRecoil));
                    collisionElementos_1_1_1(collision);
                }

            }
            catch (Exception e)
            {

            }
        }
    }
    //***************************************************************************************************
    //DETECCION DE TRIGGERS
    //***************************************************************************************************
    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);


        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO ENEMY
        if (collider.gameObject.layer == 3 || collider.gameObject.layer == 18)
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
            //Dentro de cada collision de los enemigos lo que se deberia hacer es reducir la vida y lanzar la corrutina por lo que esta deberia ser public

            try
            {

                //DETECCION DE OBJETOS HIJOS DEL ENEMIGO
                if (!invulnerable && collider.gameObject.transform.parent.parent.name == "-----ENEMIES" && collider.gameObject.layer == 3)
                {
                    //StartCoroutine(HurtParticlesPlayer());
                    recibirDanio(collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().getAtaque());
                    StartCoroutine(cooldownRecibirDanio(direccion, collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().fuerzaRecoil));
                    triggerElementos_1_1_1(collider);
                    return;
                }
                else if (collider.gameObject.transform.parent.parent.name == "-----ENEMIES" && collider.gameObject.layer == 18)
                {
                    //StartCoroutine(HurtParticlesPlayer());
                    recibirDanio(collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().getAtaque());
                    StartCoroutine(cooldownRecibirDanio(direccion, collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().fuerzaRecoil));
                    triggerElementos_1_1_1(collider);
                    return;
                }

            }
            catch (Exception e)
            {

            }
        }
        if (!invulnerable)
            triggerElementos_1_1_1(collider);

        /*
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
                StartCoroutine("combinacionesElementales");
                return;
            }

            //SE ESTABLECE EL ESTADO DE VENENO Y SUS RESPECTIVOS COMO ACTIVOS
            estadoVeneno = true;
            counterEstados = 100;
            StartCoroutine("afectacionEstadoVeneno");
        }*/
    }


    //***************************************************************************************************
    //EXIT DE TRIGGERS
    //***************************************************************************************************
    private void OnTriggerExit2D(Collider2D collider)
    {
    }


    //***************************************************************************************************
    //DETECCION SUELO
    //***************************************************************************************************
    public bool Grounded()
    {

        //if (Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(-groundCheckX, 0), Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(groundCheckX, 0), Vector2.down, groundCheckY, groundLayer))
        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer) || //|| Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, enemyLayer))
            Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, platformLayer))
        {
            if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, platformLayer) && rb.velocity.y > 0.1f) {
                return false;
            }

            anim.SetBool("Grounded", true);
            //isJumping = false;
            firstJump = true;
            secondJump = false;
            walkSpeed = walkSpeedGround;
            currentTimeAir = 0;
            saltoEspecial = false;
            CSTEPS = 0;
            //if(Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer))
                //limitY = transform.position.y + 8.5f;
            isJumping = false;
            return true;
        }
        else
        {
            anim.SetBool("Grounded", false);
            isJumping = true;
            walkSpeed = walkSpeedGround * (1 - resistenciaAire);
            saltoEspecial = true;
            return false;

        }

    }


    private bool isTouchingRoof()
    {
        if (Physics2D.OverlapCircle(groundTransform.position + Vector3.up * 2.5f, groundCheckRadius, groundLayer))
        {
            return true;
        }
        return false;
    }


    private void tocarPared()
    {
        //tocandoPared = (Physics2D.OverlapBox(wallPoint.position, Vector2.right * 2f, 0, wallLayer)) ? 0 : 1;
        tocandoPared = (Physics2D.OverlapArea(wallPoint.position + Vector3.right * transform.localScale.x * 0.5f +
            Vector3.up * 1.25f, wallPoint.position - Vector3.right * transform.localScale.x * 0.5f - Vector3.up * 1.25f, wallLayer)) ? 0 : 1;

    }


    //***************************************************************************************************
    //CORRUTINA DE MUERTE
    //***************************************************************************************************
    private IEnumerator Muerte()
    {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        playable = false;
        //Corregir los tiempos en relacion a la muerte por danio fisico y por estas afectaciones elementales
        yield return new WaitForSeconds(0.5f);

        rb.velocity = Vector2.zero;
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = 0;
        Physics2D.IgnoreLayerCollision(0, 3, true);
        //GUARDADO DE INFORMACION
        //gold = 100;
        //SaveManager.SavePlayerData(maxVida, gold, SceneManager.GetActiveScene().name);
        //Da inicio a la animacion
        //WaitForSeconds deberia tener el tiempo de la animacion para desplegar el menu
        yield return new WaitForSeconds(1f);
        //Desplegar el menu

        SaveManager.SavePlayerData(GetComponent<Hoyustus>());

        Time.timeScale = 0;
        menuMuerte.SetActive(true);
        //La correccion de las acciones tomadas al "revivir" se implementaran despues al contar con el resto de mecanicas implementadas
        //Es decir la posicion en el checkpoint, vida y gold
    }


    //***************************************************************************************************
    //CORRUTINA DE COMBINACIONES ELEMENTALES
    //***************************************************************************************************
    private IEnumerator combinacionesElementales()
    {
        if (counterEstados == 11)
        {
            //VIENTO - FUEGO
            estadoViento = false;
            afectacionViento = 0;
            counterEstados = 10;
            aumentoFuegoPotenciado = 3;
            ataque = ataqueMax * 0.75f;
            StopCoroutine("afectacionEstadoFuego");
            StartCoroutine("afectacionEstadoFuego");
        }
        else if (counterEstados == 101)
        {
            //VENENO - VIENTO
            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoViento");
            rb.velocity = Vector3.zero;
            counterEstados = 0;
            estadoVeneno = false;
            estadoViento = false;
            playable = false;
            aumentoDanioParalizacion = 1.5f;
            yield return new WaitForSeconds(2f);
            playable = true;
            aumentoDanioParalizacion = 1f;
            //StartCoroutine(setParalisis());

        }
        else if (counterEstados == 110)
        {
            //FUEGO - VENENO
            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoFuego");
            counterEstados = 0;
            explosion.GetComponent<ExplosionBehaviour>().modificarValores(3, 45, 6, 12, "Untagged", "ExplosionEnemy");
            Instantiate(explosion, transform.position, Quaternion.identity);
            estadoVeneno = false;
            estadoFuego = false;
        }
        yield return new WaitForEndOfFrame();
    }


    //***************************************************************************************************
    //MOVIMIENTO
    //***************************************************************************************************
    private void Walk()
    {
        float h = Input.GetAxis("Horizontal");

        if (h >= -0.10 && h <= 0.10)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isWalking = false;
            return;
        }
        else if (h < -0.10)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (h > 0.10)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        isWalking = true;

        //TOCANDO PARED PERMITIRA QUE SE DETENGA EL PLAYER SI TRATA DE CAMINAR Y SE TOCA UNA PARED
        //LA AFECTACION DEL VIENTO REDUCIRA SU VELOCIDAD AL ESTAR EN EL AIRE
        rb.velocity = new Vector2(h * walkSpeed * (1 - afectacionViento) * tocandoPared, rb.velocity.y);
    }


    //***************************************************************************************************
    //AUMENTO DE LA VELOCIDAD DE CAIDA DE LOS OBJETOS
    //***************************************************************************************************
    void Falling()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity -= Vector2.up * Time.deltaTime * -Physics2D.gravity * 9f;
        }
    }


    //***************************************************************************************************
    //Ataque Lanza
    //***************************************************************************************************
    private void ataqueLanza()
    {

        if (ataqueAvailable && Input.GetButtonDown("Atacar"))
        {
            atacando = true;
            //playable = false;
            int index = 0;  //SE REFIERE AL INDICE DE LOS HIJOS DEL OBJETO LANZA DE HOYUSTUS
            //VOLVERLAS VARIABLES GLOBALES
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (v == 0)
            {
                //Aniadir el pequenio impulso de movimiento
                anim.Play("Lanza Lateral");
                codigoAtaque = 4;
            }
            else if (v != 0 && h == 0)
            {
                //VERIFIFICAR QUE SOLO FUNCIONE AL ESTAR EN EL AIRE Y AGREGAR LAS POSICIONES VERTICALES DE ATAQUE.
                if (v > 0)
                {
                    index = 1;
                    codigoAtaque = 5;
                }
                else if (v <= 0 && !Grounded())
                {
                    index = 2;
                    codigoAtaque = 6;
                }
                else if (v <= 0 && Grounded())
                {
                    anim.Play("Lanza Lateral");
                    codigoAtaque = 4;
                }
            }
            else if (v != 0 && h != 0)
            {
                //VERIFIFICAR QUE SOLO FUNCIONE AL ESTAR EN EL AIRE Y AGREGAR LAS POSICIONES VERTICALES DE ATAQUE.
                if (Math.Abs(v) > Math.Abs(h))
                {
                    if (v > 0)
                    {
                        index = 1;
                        codigoAtaque = 5;
                    }
                    else if (v <= 0 && !Grounded())
                    {
                        index = 2;
                        codigoAtaque = 6;
                    }
                    else if (v <= 0 && Grounded())
                    {
                        anim.Play("Lanza Lateral");
                        codigoAtaque = 4;
                    }
                }
                else
                {
                    //Aniadir el pequenio impulso de movimiento
                    //lanza.SetActive(true);
                    anim.Play("Lanza Lateral");
                    codigoAtaque = 4;
                }
            }

            ataqueAvailable = false;
            //cargaHabilidadLanza += 0.5f;
            StartCoroutine(lanzaCooldown(index));
        }
    }


    //***************************************************************************************************
    //CooldownAtaque
    //***************************************************************************************************
    private IEnumerator lanzaCooldown(int index)
    {
        //rb.velocity = new Vector2(0, rb.velocity.y);
        lanzas[index].SetActive(true);
        atacando = true;
        yield return new WaitForSeconds(0.2f);
        atacando = false;
        playable = true;
        codigoAtaque = 0;
        lanzas[index].SetActive(false);
        yield return new WaitForSeconds(0.5f);
        ataqueAvailable = true;
    }


    //***************************************************************************************************
    //DASH
    //***************************************************************************************************
    private void Dash()
    {
        if (dashAvailable && Input.GetButton("Dash") && tocandoPared != 0)
        {
            invulnerable = true;
            playable = false;
            dashAvailable = false;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            StartCoroutine(dashCooldown());

        }
    }


    //***************************************************************************************************
    //COOLDOWN DASH
    //***************************************************************************************************
    private IEnumerator dashCooldown()
    {
        EstablecerInvulnerabilidades(layerObject);
        anim.Play("Dash");
        isDashing = true;

        body.isTrigger = true;
        //body.enabled = false; **********************************************
        //bodyHoyustus.SetActive(false);
        //dashBody.transform.position = transform.position + Vector3.up; *********************************
        //dashBody.SetActive(true); **********************************************************************
        //dashBodyTESTING.enabled = true; **************************************
        cargaHabilidadSerpiente += 15f;
        //rb.AddForce(new Vector2(transform.localScale.x * 45, 0), ForceMode2D.Impulse);
        //MODIFICAR EL TIEMPO QUE DURARIA EL DASH
        //yield return new WaitForSeconds(0.2f);

        IEnumerator movimientoDash()
        {
            rb.AddForce(new Vector2(transform.localScale.x * 45, 0), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.2f);
            //rb.velocity = Vector2.zero;
            isDashing = false;
        }
        StartCoroutine(movimientoDash());
        yield return new WaitUntil(() => (tocandoPared == 0 || isDashing == false));
        isDashing = false;
        //dashBody.SetActive(false);**********************************************************************
        //dashBodyTESTING.enabled = false; *********************************************
        //body.enabled = true; *******************************************************
        //bodyHoyustus.SetActive(true);
        playable = true;
        rb.gravityScale = 2;
        QuitarInvulnerabilidades(layerObject);
        body.isTrigger = false;
        invulnerable = false;
        yield return new WaitForSeconds(1f);
        dashAvailable = true;
    }


    public void cargaLanza()
    {
        cargaHabilidadLanza += 15f;
    }



    private void ImproveJump()
    {
        if (rb.velocity.y < 0)
        {
            //rb.velocity += Vector2.up * Physics2D.gravity.y * (3f - 1) * Time.deltaTime;
            rb.velocity += Vector2.up * Physics2D.gravity.y * (8f - 1) * Time.deltaTime;
            //Debug.Log("_rigidbody.velocity.y<0");
        }
        else if (rb.velocity.y > 0 && !Input.GetButtonDown("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (11.5f - 1) * Time.deltaTime;
            //rb.velocity += Vector2.up * Physics2D.gravity.y * (100.5f - 1) * Time.deltaTime;
            //Debug.Log("_rigidbody.velocity.y >0 && !Input.GetButtonDown()");
        }

    }


    public bool EventTransition()
    {
        //isGrounded = Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer);

        //if (Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(-groundCheckX, 0), Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(groundCheckX, 0), Vector2.down, groundCheckY, groundLayer))
        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer))
        {
            //Debug.Log("transitionLayer " + transitionLayer.ToString());
            return true;

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer1))
        {
            //Debug.Log("transitionLayer1 " + transitionLayer1.ToString());
            return true;

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer2))
        {
            // Debug.Log("transitionLayer2 " + transitionLayer2.ToString());
            return true;

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer3))
        {
            //Debug.Log("transitionLayer3 " + transitionLayer3.ToString());
            return true;

        }
        else return false;

    }

    public void ejecucionCorrutinaPrueba(int direccion, float fuerza) {
        StartCoroutine(cooldownRecibirDanio(direccion, fuerza));
    }


    public void LoadNextLevel()
    {

        string transitionLayerExit;

        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer))
        {
            //Debug.Log("transitionLayer " + transitionLayer.ToString());
            transitionLayerExit = "Transition";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer1))
        {
            //Debug.Log("transitionLayer1 " + transitionLayer1.ToString());
            transitionLayerExit = "Transition1";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer2))
        {
            //Debug.Log("transitionLayer2 " + transitionLayer2.ToString());
            transitionLayerExit = "Transition2";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer3))
        {
            //Debug.Log("transitionLayer3 " + transitionLayer3.ToString());
            transitionLayerExit = "Transition3";

        }
        else transitionLayerExit = "";

        //Debug.Log("transitionLayerExit " + transitionLayerExit);

        escena = SceneManager.GetActiveScene().name;
        //Debug.Log("Escena: " + escena);
        //Debug.Log("transitionLayerExit: " + transitionLayerExit);

        /*if (escena == "00- StartRoom 1")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -29.344f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(1 + 1);
            }

            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 29.576f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(5 + 1);
            }


        }
        if (escena == "01-Level 1")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(0 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -55.934f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(2 + 1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -137.45f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 41.468f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(3 + 1);
            }
            else if (transitionLayerExit == "Transition3")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -137.45f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(3 + 1);
            }
        }
        if (escena == "03-Room 3")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -56.341f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 0);
            SceneManager.LoadScene(1 + 1);
        }
        if (escena == "04-Level 2")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -133.46f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(1 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -132.69f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 41.422f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(1 + 1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.845f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -8.851f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(4 + 1);
            }
        }
        if (escena == "05-Room GA1")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -188.567f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(3 + 1);
        }
        if (escena == "06- Room 6")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.160f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(0 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -27.914f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 38.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(6 + 1);
            }


        }
        if (escena == "07-Room 7")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 100.690f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(5 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 63.045f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -4.417f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(7 + 1);

            }

        }
        if (escena == "08-Room 8")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 45.333f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -4.418f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(6 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.160f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(8 + 1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 119.625f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -26.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(12 + 1);
            }
        }
        if (escena == "09-Room 9")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 70.635f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -51.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(9 + 1);


            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 114.528f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -51.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(7 + 1);

            }

        }
        if (escena == "10-Room 10 - 11")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(8 + 1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -55.934f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(10 + 1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(11 + 1);
            }
        }
        if (escena == "12-Room 12")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -0.653f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -82.824f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 0);
            SceneManager.LoadScene(9 + 1);
        }
        if (escena == "13- SaveRoom")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 70.211f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -102.327f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(9 + 1);
            }

            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -64.304f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -103.677f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(13 + 1);
            }
        }
        if (escena == "13-Room 13")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, 151.409f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -26.827f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(7 + 1);
        }
        if (escena == "14-Boss Room")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.340f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(11 + 1);
        }*/
    }

    public bool Roofed()
    {

        if (Physics2D.Raycast(roofTransform.position, Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /*void GetInputs()
    {
        
        yAxis = Input.GetAxis("Vertical");
        xAxis = Input.GetAxis("Horizontal");

        
        if (yAxis > 0.25)
        {
            yAxis = 1;
        }
        else if (yAxis < -0.25)
        {
            yAxis = -1;
        }
        else
        {
            yAxis = 0;
        }

        if (xAxis > 0.25)
        {
            xAxis = 1;
        }
        else if (xAxis < -0.25)
        {
            xAxis = -1;
        }
        else
        {
            xAxis = 0;
        }

        anim.SetBool("Grounded", Grounded());
        anim.SetFloat("YVelocity", rb.velocity.y);

         
        //if (Input.GetButtonDown("Jump") && Grounded()) DNA 11/01/2022 SE AUMENTA VARIABLE DOUBLE JUMP 
            if (Input.GetButtonDown("Jump") && Grounded() || doubleJump==true)
        {
            //Debug.Log("Entra a Jumping :" + pState.jumping);
            pState.jumping = true;
            anim.SetTrigger("Jumping");
            anim.SetBool("Jump", Grounded());
            //AudioJump.Play();
        }

        if (!Input.GetButton("Jump") && stepsJumped < jumpSteps && stepsJumped > jumpThreshold && pState.jumping)
        {
            //Debug.Log("Entra a StopJumpQuick :" + pState.jumping);
            StopJumpQuick();
        }
        else if (!Input.GetButton("Jump") && stepsJumped < jumpThreshold && pState.jumping)
        {
            //Debug.Log("Entra a StopJumpSlow :" + pState.jumping);
            StopJumpSlow();
        }

    }

    void WalkingControl()
    {
        //Debug.Log("Entra a la secci n de get button");

        if (Input.GetButtonDown("Horizontal"))// && Grounded() )//&& pState.walking == true)
        {
            //Debug.Log("Entra a la secci n de get button Horizontal Input");
            if (Grounded())
            {
                //Debug.Log("Entra a la secci n de get button Horizontal Input Grounded");

                AudioWalking.Play();
            }
            else
            {

                //Debug.Log("Entra a la secci n de get button Horizontal Input Not Grounded");
                AudioWalking.Stop();
            }

        }
        else if (!Grounded())
        {
            if (Grounded())
            {
                //Debug.Log("Entra a la secci n de get button Horizontal Input !Grounded Grounded");

                AudioWalking.Play();
            }
            else
            {

                //Debug.Log("Entra a la secci n de get button Horizontal Input !Grounded !Grounded");
                AudioWalking.Stop();
            }
        }

        if (Input.GetButtonDown("Horizontal") && Grounded())
        {
            AudioWalking.Play();
        }

        if (Input.GetButtonUp("Horizontal"))
        {
            AudioWalking.Stop();
        }
        if (Input.GetButtonDown("Jump") && Grounded())
        {
            AudioWalking.Stop();
        }

    }*/

    void PlayJumpAudio()
    {
        AudioJump.Play();

    }
    void PlayFallAudio()
    {
        AudioFall.Play();

    }

    public float getMaxVida()
    {
        return maxVida;
    }



    IEnumerator PlayGamePlayLoop()
    {
        yield return new WaitForSeconds(59.2f);
        GameplayIntro.Stop();
        GameplayLoop.Play();
    }


    void PlayAudioStep1()
    {
        AudioStep1.Play();
    }
    void PlayAudioStep2()
    {
        AudioStep2.Play();
    }
    void PlayAudioStep3()
    {
        AudioStep3.Play();
    }
    void PlayAudioStep4()
    {
        AudioStep4.Play();
    }
    void PlayAudioStep5()
    {
        AudioStep5.Play();
    }
    void PlayAudioStep6()
    {
        AudioStep6.Play();
    }
    void PlayAudioStep7()
    {
        AudioStep7.Play();
    }
    void PlayAudioStep8()
    {
        AudioStep8.Play();
    }
    void PlayParticles()
    {
        ParticleTestParticleTest.Play();
    }

    /*IEnumerator HurtParticlesPlayer()
    {
        //hurtParticleSystem.loop = false;
        var ps = transform.GetChild(transform.childCount - 3).GetChild(0).GetComponent<ParticleSystem>();
        var psMain = ps.main;
        psMain.loop = false;
        hurtParticleSystem.Play();
        //hurtParticleSystem.Clear();
        hurtParticleSystem.Play();
        yield return new WaitForSeconds(3f);
        //psMain.loop = true;
        hurtParticleSystem.Clear();
        hurtParticleSystem.Play();
        //hurtParticleSystem.Clear();
    }*/

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(attackTransform.position, attackRadius);
        //Gizmos.DrawWireSphere(downAttackTransform.position, downAttackRadius);
        //Gizmos.DrawWireSphere(upAttackTransform.position, upAttackRadius);
        //Gizmos.DrawWireCube(groundTransform.position, new Vector2(groundCheckX, groundCheckY));
        //Gizmos.DrawCube(transform.position + Vector3.up, new Vector3);

        //Gizmos.DrawWireCube(wallPoint.position, Vector3.right * transform.localScale.x);
        //Gizmos.DrawCube(transform.position + Vector3.right * transform.localScale.x + Vector3.down, new Vector2(0.05f, 2.5f));


        Gizmos.DrawLine(groundTransform.position, groundTransform.position + new Vector3(0, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(-groundCheckX, 0), groundTransform.position + new Vector3(-groundCheckX, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(groundCheckX, 0), groundTransform.position + new Vector3(groundCheckX, -groundCheckY));

        Gizmos.DrawLine(roofTransform.position, roofTransform.position + new Vector3(0, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(-roofCheckX, 0), roofTransform.position + new Vector3(-roofCheckX, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(roofCheckX, 0), roofTransform.position + new Vector3(roofCheckX, roofCheckY));
    }
}