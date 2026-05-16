using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Hoyustus : CharactersBehaviour
{
    [Header("Movimiento Base")]
    [Tooltip("Velocidad máxima al caminar por el suelo.")]
    [SerializeField] float walkSpeedGround = 9f;
    [Tooltip("Factor de reducción de velocidad horizontal mientras se está en el aire.")]
    [SerializeField] float resistenciaAire = 0.3f;
    [SerializeField] float walkSpeed = 12f;
    [SerializeField] private bool isWalking = false;
    [Space(5)]

    [Header("Salto (Hollow Knight Style)")]
    [Tooltip("Altura objetivo del primer salto (multiplicador basado en el tamaño del cuerpo).")]
    [SerializeField] private float alturaPrimerSalto = 5f;
    [Tooltip("Altura adicional que alcanza el doble salto.")]
    [SerializeField] private float alturaDobleSalto = 3f;
    [Tooltip("Reducción de velocidad horizontal mientras saltas (0 = no se mueve, 1 = igual que en suelo).")]
    [SerializeField] private float multiplicadorAire = 0.7f;
    [Tooltip("Multiplicador de velocidad que se aplica al soltar el botón de salto prematuramente (microsaltos).")]
    [SerializeField] private float jumpCutMultiplier = 0.25f;
    [Tooltip("Tiempo de gracia para saltar después de dejar una plataforma.")]
    [SerializeField] private float coyoteTime = 0.15f;
    [Tooltip("Margen de tiempo para registrar un salto antes de tocar el suelo.")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [Tooltip("Escala de gravedad normal del personaje.")]
    [SerializeField] private float defaultGravityScale = 5f;
    [Tooltip("Multiplicador de gravedad aplicado únicamente cuando el personaje está cayendo. Aumentar para caer más rápido.")]
    [SerializeField] private float fallGravityMultiplier = 2.8f;
    [Tooltip("Multiplicador de gravedad extra aplicado cerca del tope del salto (velocidad Y baja). Da el efecto 'pop' de Hollow Knight.")]
    [SerializeField] private float peakGravityMultiplier = 4f;
    [Tooltip("Umbral de velocidad Y (valor absoluto) para considerar que el personaje está en el tope del arco.")]
    [SerializeField] private float peakSpeedThreshold = 3f;
    [Tooltip("Multiplicador directo sobre la velocidad inicial del salto. Más de 1 = sube más rápido. La altura máxima sigue siendo la parametrizada.")]
    [SerializeField] private float jumpImpulseMultiplier = 1f;
    [Tooltip("Prefab del efecto visual para el salto inicial.")]
    [SerializeField] private GameObject jumpVFXPrefab;
    [Tooltip("Duración en segundos del efecto visual del salto inicial.")]
    [SerializeField] private float jumpVFXDuration = 1f;
    [Tooltip("Prefab del efecto visual para el doble salto.")]
    [SerializeField] private GameObject doubleJumpVFXPrefab;
    [Tooltip("Duración en segundos del efecto visual del doble salto.")]
    [SerializeField] private float doubleJumpVFXDuration = 1f;

    private float coyoteTimeCount = 0f;
    private float jumpBufferCount = 0f;
    private bool doubleJumpQueued = false;

    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool isSecondJump = false;
    [SerializeField] private float correctorSalto = 19;
    [SerializeField] private bool firstJump = true;
    [SerializeField] private bool secondJump = false;
    [SerializeField] private bool saltoEspecial = false;
    [SerializeField] private float extraSalto = 10;
    [Space(5)]

    [Header("Falling")]
    [SerializeField] private float fuerzaCaida = 0f;
    [Space(5)]

    [Header("Ground Checking")]
    [SerializeField] public float groundCheckRadius;
    [SerializeField] Transform groundTransform;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask platformLayer;
    [Space(5)]

    [SerializeField] Animator anim;

    [Header("Audio")]
    AudioSource playerAudio;
    [SerializeField] AudioSource jumpAudio;
    [SerializeField] AudioClip AudioWalking;
    [SerializeField] AudioClip AudioJump;
    [SerializeField] AudioClip AudioHurt;
    [SerializeField] AudioClip AudioDashVariant;
    [SerializeField] AudioClip AudioDashOriginal;
    [SerializeField] AudioClip AudioSkill02;

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
    [SerializeField] ParticleSystem AttackVFX = null;
    [SerializeField] ParticleSystem Attack2VFX = null;
    private Quaternion attackVFXRotacionOriginal;

    [SerializeField] private GameObject menuMuerte;

    //TESTING DE ESCENAS
    [SerializeField] private GameObject controladorTesting;
    [SerializeField] private GameObject pantallaCanvas;

    [Header("Variables Player")]
    [SerializeField] private float maxVida = 1000;
    [SerializeField] private float tiempoInvulnerabilidad = 2f;
    [SerializeField] private float timeAir = 1.2f;
    [SerializeField] private float currentTimeAir = 0f;
    [Space(5)]


    [Header("Estados Player")]
    [SerializeField] private int tocandoPared = 1;
    [Space(5)]


    [Header("Dash")]
    [SerializeField] private float timeDashCooldown = 0.6f;
    [SerializeField] private float velocidadDash = 45f;
    [SerializeField] private bool dashAvailable = true;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private GameObject dashVFX;
    [Space(5)]


    [Header("Ataque")]
    [SerializeField] private bool atacando = false;
    [SerializeField] private int codigoAtaque = 0;
    [SerializeField] private float tiempoCooldownAtaque = 0.2f;
    [Tooltip("Duración de la animación de ataque antes de activar la lanza.")]
    [SerializeField] private float tiempoAnimacionAtaque = 0.2f;
    [Tooltip("Tiempo que la lanza permanece activa durante el ataque.")]
    [SerializeField] private float tiempoLanzaActiva = 0.2f;
    [SerializeField] private bool ataqueAvailable = true;
    [SerializeField] private GameObject[] lanzas;
    [SerializeField] private float valorAtaqueNormal = 50;
    [SerializeField] private float valorAtaqueHabilidadCondor = 100;
    [SerializeField] private float valorAtaqueHabilidadLanza = 150;

    [Header("Combo")]
    [Tooltip("Número de golpes para activar el combo.")]
    [SerializeField] private int golpesParaCombo = 3;
    [Tooltip("Tiempo máximo entre golpes para mantener el combo activo.")]
    [SerializeField] private float ventanaTiempoCombo = 1.5f;
    [Tooltip("Pausa adicional después del último golpe del combo antes de poder atacar de nuevo.")]
    [SerializeField] private float pausaDespuesCombo = 0.4f;
    [Tooltip("Multiplicador de daño del golpe de combo.")]
    [SerializeField] private float multiplicadorDanioCombo = 3f;
    [Tooltip("Escala del VFX en el golpe de combo.")]
    [SerializeField] private float escalaVFXCombo = 2.5f;
    [Tooltip("Color del VFX en el golpe de combo.")]
    [SerializeField] private Color colorVFXCombo = new Color(1f, 0.4f, 0f);
    [Tooltip("Duración del hitstop (pausa de impacto) en el golpe de combo.")]
    [SerializeField] private float duracionHitstop = 0.06f;
    [Tooltip("Duración del slow motion en el golpe de combo.")]
    [SerializeField] private float duracionSlowMotion = 0.12f;
    [Tooltip("Escala de tiempo durante el slow motion (0.1 = muy lento).")]
    [SerializeField] private float escalaSlowMotion = 0.15f;
    [Tooltip("Duración en segundos de la rueda giratoria del combo.")]
    [SerializeField] private float duracionRuedaCombo = 0.5f;
    [Tooltip("Velocidad de rotación de la rueda (grados por segundo).")]
    [SerializeField] private float velocidadRuedaCombo = 1800f;
    [Tooltip("Escala de la rueda giratoria del combo.")]
    [SerializeField] private float escalaRuedaCombo = 3f;
    [Tooltip("Color de la rueda giratoria del combo.")]
    [SerializeField] private Color colorRuedaCombo = new Color(1f, 1f, 0f);
    [Tooltip("Activa o desactiva la rueda giratoria del combo.")]
    [SerializeField] private bool ruedaComboActiva = true;
    [Tooltip("Duración de la vibración del mando al conectar el golpe de combo (segundos).")]
    [SerializeField] private float duracionVibracionCombo = 0.15f;
    [Tooltip("Si está activo, reproduce un audio diferente en el último golpe del combo.")]
    [SerializeField] private bool cambiarAudioCombo = true;
    [Tooltip("Audio que suena en el último golpe del combo.")]
    [SerializeField] private AudioClip audioCombo;
    [Tooltip("Audios aleatorios para los golpes 1 y 2 del combo.")]
    [SerializeField] private AudioClip[] audiosAtaqueNormal;
    [Tooltip("Si está activo, el VFX del combo tiene un pequeño giro en X.")]
    [SerializeField] private bool girarVFXCombo = true;
    [Tooltip("Ángulo de deformación en X del VFX del combo.")]
    [SerializeField] private float anguloGiroVFXCombo = 10f;
    [Tooltip("Si está activo, el VFX del combo rota en Z.")]
    [SerializeField] private bool girarZVFXCombo = false;
    [Tooltip("Ángulo de giro en Z del VFX del combo (grados).")]
    [SerializeField] private float anguloGiroZVFXCombo = 0f;
    [Tooltip("Multiplicador de achatado en Y del VFX del combo (menor a 1 = más achatado, mayor a 1 = más estirado).")]
    [SerializeField] private float deformacionYVFXCombo = 1f;
    [Tooltip("Si está activo, el VFX rota 90 grados para ataques hacia arriba y abajo.")]
    [SerializeField] private bool vfxDireccionActivo = true;
    [Tooltip("Si está activo, el golpe 2 del combo se ve en sentido contrario al golpe 1.")]
    [SerializeField] private bool invertirGolpe2 = true;
    [SerializeField] private int contadorCombo = 0;
    [SerializeField] private float timerCombo = 0f;
    private bool comboActivo = false;
    private bool esGolpeComboVFX = false;
    private int golpeActualVFX = 0;
    private int idRestauracion = 0;
    private int indexAtaqueActual = 0;
    [Space(5)]


    [Header("Habilidades")]
    [SerializeField] float cargaHabilidadCondor;
    [SerializeField] float cargaHabilidadSerpiente;
    [SerializeField] float cargaHabilidadLanza;
    [SerializeField] float cargaCuracion;
    [SerializeField] float aumentoBarraSalto = 10;
    [SerializeField] float aumentoBarraDash = 15;
    [SerializeField] float aumentoBarraAtaque = 15;
    [SerializeField] float danioExplosionCombinacionFuego_Veneno = 35;
    [Space(5)]


    [Header("PREFABS")]
    [SerializeField] private GameObject bolaVeneno;
    [SerializeField] private Transform wallPoint;

    [SerializeField] private float botonCuracion = 0f;
    [SerializeField] private bool aplastarBotonCuracion = false;

    [SerializeField] private bool realizandoHabilidadLanza = false;

    [SerializeField] private bool curando = false;

    [SerializeField] private int SSTEPS = 60;
    [SerializeField] private int CSTEPS = 0;
    private float maxHabilidad_Curacion = 100f;

    private IEnumerator recoil;
    [SerializeField] GameObject skillObj01;
    [SerializeField] GameObject skillObj02;
    [SerializeField] GameObject skillObj03;
    [SerializeField] GameObject skillObj04;
    [SerializeField] GameObject deathFX;

    private bool playerDie = false;
    private bool weaponEquip = false;
    private bool enableSkill01, enableSkill02, enableSkill03;

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

    public void setCargaHabilidades(int e)
    {
        switch (e)
        {
            case 0:
                cargaHabilidadCondor = 100;
                enableSkill01 = true;
                break;
            case 1:
                cargaHabilidadSerpiente = 100;
                enableSkill02 = true;
                break;
            case 2:
                cargaHabilidadLanza = 100;
                enableSkill03 = true;
                break;
        }
    }

    public void setCargaCuracion(int e)
    {
        cargaCuracion += e;

        GameObject tuto = GameObject.Find("tuto4");
        if (tuto != null) tuto.GetComponent<TutorialRoute>().StartHealTuto();
    }

    public void CurarCompletamente()
    {
        vida = maxVida;
    }

    public void UpdatePU(int e)
    {
        if (e == 1)
        {
            valorAtaqueHabilidadCondor *= 1.25f;
            valorAtaqueHabilidadLanza *= 1.25f;
        }
        else if (e == 2)
        {
            maxVida *= 1.5f;
        }
        else
        {
            ataqueMax *= 1.25f;
            ataque *= 1.25f;
        }
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Boost01"))
        {
            valorAtaqueHabilidadCondor *= 1.25f;
            valorAtaqueHabilidadLanza *= 1.25f;
        }

        if (PlayerPrefs.HasKey("Boost02"))
        {
            maxVida *= 1.5f;
        }

        enableSkill01 = PlayerPrefs.HasKey("condorSkill");
        enableSkill02 = PlayerPrefs.HasKey("snakeSkill");
        enableSkill03 = PlayerPrefs.HasKey("spearSkill");

        // Guardar rotación original del AttackVFX para restaurarla exactamente
        if (AttackVFX != null)
            attackVFXRotacionOriginal = AttackVFX.transform.localRotation;

        LoadData();
    }

    public void EnableWeapon()
    {
        weaponEquip = true;
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
            if (PlayerPrefs.HasKey("respawn") && PlayerPrefs.GetInt("scenePos") == 0)
            {
                vida = maxVida;
            }
            cargaHabilidadCondor = playerData.getCondor();
            cargaHabilidadSerpiente = playerData.getSerpiente();
            cargaHabilidadLanza = playerData.getLanza();
            cargaCuracion = playerData.getCuracion();
        }
        else
        {
            SaveManager.SavePlayerData(GetComponent<Hoyustus>());
        }

        if (PlayerPrefs.HasKey("Boost03"))
        {
            ataqueMax *= 1.25f;
            ataque *= 1.25f;
        }

        weaponEquip = true;
    }

    public void SavePlayerData()
    {
        SaveManager.SavePlayerData(vida, gold, cargaHabilidadCondor, cargaHabilidadSerpiente, cargaHabilidadLanza, cargaCuracion, ataque);
    }


    void Start()
    {
        Application.targetFrameRate = 60;

        Physics2D.IgnoreLayerCollision(11, 14, true);
        Physics2D.IgnoreLayerCollision(13, 12, true);
        Physics2D.IgnoreLayerCollision(13, 14, true);
        Physics2D.IgnoreLayerCollision(13, 15, true);

        invulnerable = false;
        explosionInvulnerable = "ExplosionPlayer";
        layerObject = this.gameObject.layer;
        aumentoDanioParalizacion = 1f;
        lanzas = new GameObject[transform.GetChild(this.transform.childCount - 1).childCount];
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        anim = this.gameObject.GetComponent<Animator>();
        flash = GetComponent<DamageFlash>();
        ataque = ataqueMax;
        ataqueMax = ataque;

        for (int i = 0; i < lanzas.Length; i++)
        {
            lanzas[i] = transform.GetChild(this.transform.childCount - 1).GetChild(i).gameObject;
        }

        playerAudio = GetComponent<AudioSource>();
        explosion = Resources.Load<GameObject>("Explosion");
        bolaVeneno = Resources.Load<GameObject>("BolaVeneno");

        if (PlayerPrefs.HasKey("respawn") && PlayerPrefs.GetInt("scenePos") == 0)
        {
            StartCoroutine(ResurectPlayer());
        }

        rb.gravityScale = defaultGravityScale;
        QuitarInvulnerabilidades(layerObject);
    }


    void Update()
    {
        cargaHabilidades();
        TocarPared();

        // Timer del combo: si pasa el tiempo sin atacar, se resetea
        if (contadorCombo > 0)
        {
            timerCombo -= Time.deltaTime;
            if (timerCombo <= 0f)
            {
                contadorCombo = 0;
                comboActivo = false;
            }
        }

        if (playable)
        {
            Falling();
            if (weaponEquip) AtaqueLanza();
            Dash();

            // Detección de Jump Button
            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCount = jumpBufferTime;
                if (!Grounded() && secondJump) doubleJumpQueued = true;
            }
            else jumpBufferCount -= Time.deltaTime;

            // coyote time y estados de salto
            if (Grounded())
            {
                coyoteTimeCount = coyoteTime;
                firstJump = true;
                secondJump = false;
                isJumping = false;
                doubleJumpQueued = false;
                walkSpeed = walkSpeedGround;
            }
            else
            {
                coyoteTimeCount -= Time.deltaTime;
                walkSpeed = walkSpeedGround * (1 - resistenciaAire);

                if (coyoteTimeCount <= 0f && firstJump)
                {
                    firstJump = false;
                    secondJump = true;
                }
            }

            jump();
        }

        if (!curando && Input.GetAxis("Skill01") == 1 && cargaHabilidadCondor >= maxHabilidad_Curacion && playable && enableSkill01)
        {
            cargaHabilidadCondor = 0f;
            StartCoroutine("habilidadCondor");
            return;
        }
        if (!curando && Input.GetAxis("Skill02") == 1 && cargaHabilidadSerpiente >= maxHabilidad_Curacion && playable && enableSkill02)
        {
            cargaHabilidadSerpiente = 0f;
            StartCoroutine("habilidadSerpiente");
            return;
        }
        if (!curando && !atacando && Input.GetButtonDown("Skill03") && cargaHabilidadLanza >= maxHabilidad_Curacion && playable && enableSkill03)
        {
            cargaHabilidadLanza = 0;
            transform.parent = null;
            invulnerable = true;
            playable = false;
            StartCoroutine(habilidadLanza());
            return;
        }
        if (cargaCuracion >= maxHabilidad_Curacion && Input.GetButtonDown("Skill04") && playable)
        {
            curando = true;
            cargaCuracion = 0;
            playable = false;
            StartCoroutine("Curacion");
            return;
        }
    }

    public bool Grounded()
    {
        if (rb.linearVelocity.y > 0.1f) return false;

        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer) ||
            Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, platformLayer))
        {
            anim.SetBool("Grounded", true);
            return true;
        }
        else
        {
            anim.SetBool("Grounded", false);
            return false;
        }
    }


    private void jump()
    {
        // MICROSALTO (Jump Cut) — corte agresivo para saltos cortos tipo Hollow Knight
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            // Forzar gravedad de caída inmediatamente para que el corte se sienta brusco
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
            isJumping = false;
        }

        // SALTO INICIAL
        if (jumpBufferCount > 0 && coyoteTimeCount > 0 && !isJumping && firstJump)
        {
            playerAudio.Stop();
            playerAudio.loop = false;
            jumpAudio.Play();
            anim.Play("Saltar");

            // VFX del salto inicial
            if (jumpVFXPrefab != null)
            {
                GameObject vfx = Instantiate(jumpVFXPrefab, transform.position - new Vector3(0, 1.5f, 0), Quaternion.identity);
                Destroy(vfx, jumpVFXDuration);
            }

            // Gravedad base para calcular la velocidad (sin multiplicadores de caída)
            float gravity = -Physics2D.gravity.y * defaultGravityScale;
            float jumpVelBase = Mathf.Sqrt(2 * gravity * alturaPrimerSalto);
            // jumpImpulseMultiplier hace que suba más rápido aplicando más velocidad inicial,
            // pero compensamos aumentando la gravedad de ascenso para que la altura máxima
            // siga siendo alturaPrimerSalto sin importar cuánto impulso se aplique.
            float jumpVel = jumpVelBase * jumpImpulseMultiplier;
            // Gravedad de ascenso compensada: escala cuadráticamente con el impulso
            // para que h = v² / (2g) = alturaPrimerSalto siempre se cumpla
            rb.gravityScale = defaultGravityScale * (jumpImpulseMultiplier * jumpImpulseMultiplier);
            // Forzar velocidad Y limpia para que el impulso se sienta instantáneo
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);

            isJumping = true;
            firstJump = false;
            secondJump = true;
            jumpBufferCount = 0;
            coyoteTimeCount = 0;
            cargaHabilidadCondor += aumentoBarraSalto;
        }

        // DOBLE SALTO
        else if (doubleJumpQueued && secondJump && !Grounded() && !isTouchingRoof())
        {
            playerAudio.loop = false;
            playerAudio.Stop();
            jumpAudio.Play();
            anim.Play("Doble Salto");

            // Instanciar VFX para el doble salto
            if (doubleJumpVFXPrefab != null)
            {
                GameObject vfx = Instantiate(doubleJumpVFXPrefab, transform.position - new Vector3(0, 0.5f, 0), Quaternion.identity);
                Destroy(vfx, doubleJumpVFXDuration);
            }

            float gravity = -Physics2D.gravity.y * defaultGravityScale;
            float jumpVel = Mathf.Sqrt(2 * gravity * alturaDobleSalto) * jumpImpulseMultiplier;
            rb.gravityScale = defaultGravityScale * (jumpImpulseMultiplier * jumpImpulseMultiplier);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);

            isJumping = true;
            secondJump = false;
            doubleJumpQueued = false;
            cargaHabilidadCondor += aumentoBarraSalto;
        }

        if (rb.linearVelocity.y < -0.1f && !Grounded() && !atacando)
            anim.Play("Caer");
    }

    void FixedUpdate()
    {
        if (playable && !atacando) Walk();

        if (vida <= 0 && !playerDie)
        {
            StartCoroutine(Muerte());
            playerDie = true;
        }

        anim.SetBool("Walking", rb.linearVelocity.x != 0);
        anim.SetBool("Grounded", Grounded());
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetFloat("XVelocity", rb.linearVelocity.x);
        anim.SetFloat("Vida", vida);
        anim.SetFloat("Ataque", ataque);
        anim.SetInteger("Gold", gold);
        anim.SetBool("Dashing", isDashing);
        anim.SetBool("Atacando", atacando);
        anim.SetInteger("CA", codigoAtaque);
        anim.SetBool("SecondJump", secondJump);
        anim.SetBool("Jumping", isJumping);

    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false;
        if (isJumping)
        {
            rb.linearVelocity = Vector3.zero;
            rb.gravityScale = defaultGravityScale;
            rb.AddForce(new Vector2(direccion * 2.6f * fuerzaRecoil, rb.gravityScale), ForceMode2D.Impulse);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(new Vector2(direccion * 4 * fuerzaRecoil, rb.gravityScale * 2), ForceMode2D.Impulse);
        }

        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        Physics2D.IgnoreLayerCollision(layerObject, 19, true);
        EstablecerInvulnerabilidades(layerObject);
    }


    protected override sealed void QuitarInvulnerabilidades(int layerObject)
    {
        invulnerable = false;
        Physics2D.IgnoreLayerCollision(3, layerObject, false);
        Physics2D.IgnoreLayerCollision(layerObject, 12, false);
        Physics2D.IgnoreLayerCollision(layerObject, 15, false);
        Physics2D.IgnoreLayerCollision(layerObject, 19, false);

    }


    private IEnumerator Curacion()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Destroy(Instantiate(skillObj04, transform.position, Quaternion.identity), 1.5f);
        yield return new WaitForSeconds(0.5f);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playable = true;
        vida += 350;
        if (vida > maxVida) vida = maxVida;
        curando = false;
    }


    private IEnumerator habilidadCondor()
    {
        anim.SetInteger("Skill", 2);
        playable = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        cargaCuracion += 35;
        yield return new WaitForSeconds(0.35f);
        GameObject extraExplosion = Instantiate(explosion, transform.position + Vector3.up * 1f, Quaternion.identity);
        extraExplosion.GetComponent<ExplosionBehaviour>().modificarValores(15, valorAtaqueHabilidadCondor, 15, 12, "Viento", explosionInvulnerable, false);
        extraExplosion.name += "Player";
        Destroy(Instantiate(skillObj02, transform.position, Quaternion.identity), 2f);
        yield return new WaitForSeconds(0.5f);
        playable = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }


    private IEnumerator habilidadSerpiente()
    {
        anim.SetInteger("Skill", 1);
        playable = false;
        dashAvailable = false;
        cargaCuracion += 35;
        yield return new WaitForSeconds(0.05f);
        anim.SetInteger("Skill", 0);
        yield return new WaitForSeconds(0.25f);
        GameObject bolaVenenoGenerada = Instantiate(bolaVeneno, transform.position + Vector3.up, Quaternion.identity);
        yield return new WaitForEndOfFrame();
        bolaVenenoGenerada.GetComponent<BolaVeneno>().AniadirFuerza(-transform.localScale.x, 14);
        yield return new WaitForEndOfFrame();
        dashAvailable = true;
        playable = true;
        vidaMax = 1000;
    }


    private IEnumerator habilidadLanza()
    {
        anim.SetInteger("Skill", 3);
        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        Physics2D.IgnoreLayerCollision(layerObject, 19, true);
        EstablecerInvulnerabilidades(layerObject);
        realizandoHabilidadLanza = true;
        playable = false;
        invulnerable = true;
        cargaCuracion += 35;
        atacando = true;
        codigoAtaque = 3;
        cargaHabilidadLanza = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        ataque = valorAtaqueHabilidadLanza;
        lanzas[3].SetActive(true);
        yield return new WaitForSeconds(0.05f);
        anim.SetInteger("Skill", 0);
        Destroy(Instantiate(skillObj01, transform.position, Quaternion.identity, transform), 1f);
        IEnumerator movimientoHabilidadLanza()
        {
            rb.AddForce(new Vector2(transform.localScale.x * 40, 0), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.35f);
            rb.linearVelocity = Vector2.zero;
            realizandoHabilidadLanza = false;
        }
        StartCoroutine(movimientoHabilidadLanza());
        yield return new WaitUntil(() => (tocandoPared == 0 || !realizandoHabilidadLanza));
        anim.SetInteger("Skill", 0);
        atacando = false;
        codigoAtaque = 0;
        QuitarInvulnerabilidades(layerObject);
        realizandoHabilidadLanza = false;
        playable = true;
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
        ataque = valorAtaqueNormal;
        ataque = ataqueMax;
        lanzas[3].SetActive(false);
    }

    public void cargaHabilidades() { }

    public void setPlayable(bool state) { playable = state; }
    public void setGold(int e) { gold += e; }
    public int GetGold() { return gold; }
    public void setAumentoDanioParalizacion(float value) { aumentoDanioParalizacion = value; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 18 || collision.gameObject.layer == 19)
        {
            try
            {
                if (!invulnerable && collision.gameObject.transform.parent.name == "-----ENEMIES")
                {
                    invulnerable = true;
                    RecibirDanio(collision.gameObject.GetComponent<CharactersBehaviour>().getAtaque());
                    recoil = cooldownRecibirDanio((int)-Mathf.Sign(collision.transform.position.x - transform.position.x),
                        collision.gameObject.GetComponent<CharactersBehaviour>().fuerzaRecoil);
                    StartCoroutine(recoil);
                    CollisionElementos_1_1_1(collision);
                    return;
                }
            }
            catch (Exception) { }
            if (!invulnerable) CollisionElementos_1_1_1(collision);
        }
    }

    protected override sealed IEnumerator cooldownRecibirDanio(int direccion, float fuerzaRecoil)
    {
        Recoil(direccion, fuerzaRecoil);
        if (vida <= 0) yield break;
        playerAudio.loop = false;
        playerAudio.Stop();
        playerAudio.clip = AudioHurt;
        playerAudio.Play();
        yield return new WaitForSeconds(0.5f);
        playable = true;
        yield return new WaitForSeconds(0.7f);
        QuitarInvulnerabilidades(layerObject);
    }

    public bool IsInvulnerable() { return invulnerable; }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 18 || collision.gameObject.layer == 19)
        {
            try
            {
                if (!invulnerable && collision.gameObject.transform.parent.name == "-----ENEMIES")
                {
                    RecibirDanio(collision.gameObject.GetComponent<CharactersBehaviour>().getAtaque());
                    recoil = cooldownRecibirDanio((int)-Mathf.Sign(collision.transform.position.x - transform.position.x),
                        collision.gameObject.GetComponent<CharactersBehaviour>().fuerzaRecoil);
                    StartCoroutine(recoil);
                    CollisionElementos_1_1_1(collision);
                }
            }
            catch (Exception) { }
        }
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        if (collider.gameObject.layer == 3 || collider.gameObject.layer == 18 || collider.gameObject.layer == 19)
        {
            int direccion = (collider.transform.position.x > gameObject.transform.position.x) ? -1 : 1;
            try
            {
                if (collider.gameObject.transform.parent == null) TriggerElementos_1_1_1(collider);
                else if (!invulnerable && (collider.gameObject.transform.parent.parent.name == "-----ENEMIES" && (collider.gameObject.layer == 3 || collider.gameObject.layer == 19)))
                {
                    RecibirDanio(collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().getAtaque());
                    recoil = cooldownRecibirDanio(direccion, collider.gameObject.transform.parent.GetComponent<CharactersBehaviour>().fuerzaRecoil);
                    StartCoroutine(recoil);
                    TriggerElementos_1_1_1(collider);
                }
                return;
            }
            catch (Exception) { }
        }
        if (!invulnerable && !collider.gameObject.name.Contains("Player")) TriggerElementos_1_1_1(collider);
    }

    private bool isTouchingRoof()
    {
        if (Physics2D.OverlapCircle(groundTransform.position + Vector3.up * 2.75f, groundCheckRadius, groundLayer) || Physics2D.OverlapCircle(groundTransform.position + Vector3.up * 2.75f, groundCheckRadius, wallLayer))
        {
            isJumping = false;
            return true;
        }
        return false;
    }

    private void TocarPared()
    {
        tocandoPared = (Physics2D.OverlapArea(wallPoint.position + Vector3.right * transform.localScale.x * 0.5f +
            Vector3.up * 1.25f, wallPoint.position + Vector3.right * transform.localScale.x * 0.1f - Vector3.up * 1.25f, wallLayer)) ? 0 : 1;
    }

    private IEnumerator Muerte()
    {
        playable = false;
        GetComponent<AudioSource>().enabled = false;
        GameObject.Find("HUDMenu").GetComponent<HudManager>().SetVibration();
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = Vector2.zero;
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = 0;
        Physics2D.IgnoreLayerCollision(0, 3, true);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        anim.Play("MuerteHoyustus");
        yield return new WaitForSeconds(0.4f);
        Instantiate(deathFX, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        GetComponent<SpriteRenderer>().enabled = false;
        SavePlayerData();
        SaveManager.SavePlayerData(GetComponent<Hoyustus>());
        Time.timeScale = 0;
        menuMuerte.SetActive(true);
    }

    private void Walk()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (h >= -0.10 && h <= 0.10)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            isWalking = false;
            if (playerAudio.clip == AudioWalking) playerAudio.Stop();
            return;
        }
        else if (h < -0.10) transform.localScale = new Vector3(-1, 1, 1);
        else if (h > 0.10) transform.localScale = Vector3.one;

        isWalking = true;
        if (!isJumping)
        {
            if (playerAudio.clip != AudioWalking) playerAudio.Stop();
            if (!playerAudio.isPlaying && Grounded())
            {
                playerAudio.loop = true;
                playerAudio.clip = AudioWalking;
                playerAudio.Play();
            }
        }
        else playerAudio.Stop();

        float currentHorizontalSpeed = Grounded() ? walkSpeed : walkSpeed * multiplicadorAire;
        rb.linearVelocity = new Vector2(h * currentHorizontalSpeed * (1 - afectacionViento) * tocandoPared, rb.linearVelocity.y);
    }

    void Falling()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Caída normal: gravedad multiplicada para bajar rápido
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else if (isJumping && Mathf.Abs(rb.linearVelocity.y) < peakSpeedThreshold)
        {
            // Cerca del tope del arco: gravedad extra para el efecto "pop" de Hollow Knight
            rb.gravityScale = defaultGravityScale * peakGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void AtaqueLanza()
    {
        if (vida <= 0) return;
        if (Input.GetButtonDown("Atacar"))
        {
            if (ataqueAvailable && playable)
            {
                atacando = true;
                int index = 0;
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                if (v == 0) { anim.Play("Lanza Lateral"); codigoAtaque = 4; }
                else if (v > 0) { index = 1; codigoAtaque = 5; }
                else if (v < 0 && !Grounded()) { index = 2; codigoAtaque = 6; }
                else { anim.Play("Lanza Lateral"); codigoAtaque = 4; }
                ataqueAvailable = false;
                indexAtaqueActual = index;
                StartCoroutine(lanzaCooldown(index));
            }
        }
    }

    private IEnumerator lanzaCooldown(int index)
    {
        // Actualizar contador de combo
        contadorCombo++;
        timerCombo = ventanaTiempoCombo;
        bool esGolpeCombo = (contadorCombo >= golpesParaCombo);
        golpeActualVFX = contadorCombo;

        // Resetear contador inmediatamente si es el último golpe — evita doble combo
        if (esGolpeCombo)
        {
            contadorCombo = 0;
            timerCombo = 0f;
            esGolpeComboVFX = true; // flag para PlayAttackVFX
        }

        atacando = true;
        yield return new WaitForSeconds(tiempoAnimacionAtaque);
        atacando = false;
        codigoAtaque = 0;

        // Aplicar daño de combo ANTES de activar la lanza para que pegue con el valor correcto
        if (esGolpeCombo)
        {
            ataque = valorAtaqueNormal * multiplicadorDanioCombo;
            comboActivo = true;

            // Audio diferente si está configurado
            if (cambiarAudioCombo && audioCombo != null)
                jumpAudio.PlayOneShot(audioCombo);
        }
        else if (audiosAtaqueNormal != null && audiosAtaqueNormal.Length > 0)
        {
            AudioClip clipAleatorio = audiosAtaqueNormal[UnityEngine.Random.Range(0, audiosAtaqueNormal.Length)];
            if (clipAleatorio != null)
                jumpAudio.PlayOneShot(clipAleatorio);
        }

        lanzas[index].SetActive(true);
        yield return new WaitForSeconds(tiempoLanzaActiva);
        lanzas[index].SetActive(false);
        playable = true;

        if (esGolpeCombo)
        {
            StartCoroutine(EfectosVisualCombo());
            comboActivo = false;
            ataque = valorAtaqueNormal;
        }

        yield return new WaitForSeconds(tiempoCooldownAtaque);
        if (esGolpeCombo) yield return new WaitForSeconds(pausaDespuesCombo);
        golpeActualVFX = 0;
        esGolpeComboVFX = false;
        indexAtaqueActual = 0;
        ataqueAvailable = true;
    }

    // VFX del combo — siempre se ejecuta al tercer golpe
    private IEnumerator EfectosVisualCombo()
    {
        if (ruedaComboActiva) StartCoroutine(RuedaCombo());
        yield break;
    }

    // Hitstop + slow motion + vibración — solo si el tercer golpe conectó
    public void NotificarHitCombo()
    {
        if (comboActivo)
        {
            comboActivo = false; // guardia — evita múltiples llamadas por el mismo combo
            GameObject.Find("HUDMenu")?.GetComponent<HudManager>()?.SetVibration(duracionVibracionCombo);
            StartCoroutine(EfectosImpactoCombo());
        }
    }

    private IEnumerator EfectosImpactoCombo()
    {
        // No ejecutar si el jugador está muerto
        if (vida <= 0) yield break;

        // Hitstop — pausa de impacto
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duracionHitstop);

        // Si el jugador murió durante el hitstop, no restaurar — Muerte() maneja el timeScale
        if (vida <= 0) yield break;

        // Slow motion
        Time.timeScale = escalaSlowMotion;
        yield return new WaitForSecondsRealtime(duracionSlowMotion);

        // Si el jugador murió durante el slow motion, no restaurar
        if (vida <= 0) yield break;

        // Restaurar tiempo normal — siempre a 1f
        Time.timeScale = 1f;
    }

    private IEnumerator RuedaCombo()
    {
        if (AttackVFX == null) yield break;

        // Crear copia del VFX como hija del jugador para que se mueva con él
        GameObject rueda = Instantiate(AttackVFX.gameObject, transform.position, Quaternion.identity);
        rueda.transform.SetParent(transform);
        rueda.transform.localPosition = AttackVFX.transform.localPosition;
        rueda.transform.localScale = new Vector3(-1f * escalaRuedaCombo, escalaRuedaCombo, escalaRuedaCombo);

        // Aplicar giro en X si está activo
        if (girarVFXCombo)
            rueda.transform.localRotation = Quaternion.Euler(anguloGiroVFXCombo, 0f, 0f);

        // Aplicar color independiente de la rueda
        var mainModule = rueda.GetComponent<ParticleSystem>().main;
        mainModule.startColor = colorRuedaCombo;
        rueda.GetComponent<ParticleSystem>().Play();

        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionRuedaCombo)
        {
            rueda.transform.Rotate(0f, 0f, velocidadRuedaCombo * Time.deltaTime);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        Destroy(rueda);
    }

    private void Dash()
    {
        if (Input.GetButtonDown("Dash") && dashAvailable && tocandoPared != 0)
        {
            transform.parent = null;
            invulnerable = true;
            playable = false;
            dashAvailable = false;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            StartCoroutine(dashCooldown());
        }
    }

    private IEnumerator dashCooldown()
    {
        if (vida <= 0) yield break;
        dashVFX.SetActive(true);
        dashVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        int numeroRandom = UnityEngine.Random.Range(1, 101);
        if (numeroRandom >= 50) dashVFX.GetComponent<AudioSource>().clip = AudioDashVariant;
        else dashVFX.GetComponent<AudioSource>().clip = AudioDashOriginal;

        dashVFX.GetComponent<AudioSource>().Play();
        isDashing = true;
        Physics2D.IgnoreLayerCollision(3, layerObject, true);
        Physics2D.IgnoreLayerCollision(layerObject, 19, true);
        EstablecerInvulnerabilidades(layerObject);
        anim.Play("Dash");
        cargaHabilidadSerpiente += aumentoBarraDash;
        IEnumerator movimientoDash()
        {
            rb.AddForce(new Vector2(transform.localScale.x * velocidadDash, 0), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.2f);
            rb.gravityScale = defaultGravityScale;
            isDashing = false;
        }
        StartCoroutine(movimientoDash());
        yield return new WaitUntil(() => (tocandoPared == 0 || isDashing == false));
        dashVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        playable = true;
        isJumping = false;
        yield return new WaitForSeconds(0.1f);
        QuitarInvulnerabilidades(layerObject);
        yield return new WaitForSeconds(timeDashCooldown);
        dashAvailable = true;
    }

    public void cargaLanza() { cargaHabilidadLanza += aumentoBarraAtaque; }

    public void danioExterno(int direccion, float fuerza)
    {
        if (!realizandoHabilidadLanza)
        {
            recoil = cooldownRecibirDanio(direccion, fuerza);
            StartCoroutine(recoil);
        }
    }
    public float getMaxVida() { return maxVida; }
    IEnumerator ResurectPlayer()
    {
        anim.SetBool("Resurect", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("Resurect", false);
    }
    public void PlayParticles() { ParticleTestParticleTest.Play(); }
    public void PlayAttackVFX()
    {
        // Cancelar cualquier restauración pendiente incrementando el id
        idRestauracion++;

        // Leer y resetear flags — así el siguiente golpe empieza limpio
        bool fueGolpeCombo = esGolpeComboVFX;
        int fueGolpeActual = golpeActualVFX;
        esGolpeComboVFX = false;
        golpeActualVFX = 0;

        // Forzar restauración de alignment y rotación al inicio de cada ataque — por si quedó mal de un combo anterior
        AttackVFX.GetComponent<ParticleSystemRenderer>().alignment = ParticleSystemRenderSpace.Local;
        if (Attack2VFX != null)
            Attack2VFX.GetComponent<ParticleSystemRenderer>().alignment = ParticleSystemRenderSpace.Local;
        AttackVFX.transform.localRotation = attackVFXRotacionOriginal;

        if ((fueGolpeCombo || fueGolpeActual > 0) && AttackVFX != null)
        {
            var mainModule = AttackVFX.main;
            if (fueGolpeCombo)
                mainModule.startColor = colorVFXCombo;

            float escalaX = (invertirGolpe2 && fueGolpeActual == 2) ? 1f * escalaVFXCombo : -1f * escalaVFXCombo;
            float escalaY = escalaVFXCombo * deformacionYVFXCombo;
            float escalaZ = escalaVFXCombo;

            if (girarVFXCombo)
            {
                escalaY = escalaVFXCombo * deformacionYVFXCombo * (1f + anguloGiroVFXCombo / 45f);
                escalaZ = escalaVFXCombo * (1f - anguloGiroVFXCombo / 90f);
            }

            AttackVFX.transform.localScale = new Vector3(escalaX, escalaY, escalaZ);

            // Ataque hacia arriba — rotar 90 grados para que el VFX se vea vertical
            if (vfxDireccionActivo && indexAtaqueActual == 1)
                AttackVFX.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            else if (girarZVFXCombo)
                AttackVFX.transform.localRotation = Quaternion.Euler(0f, 0f, anguloGiroZVFXCombo);
            else
                AttackVFX.transform.localRotation = attackVFXRotacionOriginal;

            AttackVFX.Play();
            Attack2VFX.Play();
            StartCoroutine(RestaurarVFXCombo(mainModule.duration, idRestauracion));
        }
        else
        {
            AttackVFX.Play();
            Attack2VFX.Play();
        }
    }

    private IEnumerator RestaurarVFXCombo(float delay, int id)
    {
        yield return new WaitForSeconds(delay);
        if (id != idRestauracion) yield break;
        var mainModule = AttackVFX.main;
        mainModule.startColor = Color.white;
        AttackVFX.transform.localScale = new Vector3(-1f, 1f, 1f);
        AttackVFX.transform.localRotation = attackVFXRotacionOriginal;
    }
}