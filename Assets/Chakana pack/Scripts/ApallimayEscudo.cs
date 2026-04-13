using System.Collections;
using UnityEngine;

public class ApallimayEscudo : Enemy
{
    public enum EstadoEscudo { Idle, Persecucion, Atacando, Vulnerable, Huida }

    [Header("Máquina de Estados")]
    [SerializeField] private EstadoEscudo estadoActual = EstadoEscudo.Idle;
    [SerializeField] private float rangoVision = 8f;
    [SerializeField] private float rangoAtaque = 1.5f;

    [Header("Configuración Idle & Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float cooldownCambioMirada = 3f;
    [SerializeField] private float tiempoCaminarAtras = 2f;
    private float temporizadorMirada;
    private bool persecucionPausada; // Para el movimiento "cuidadoso"

    [Header("Combate & Tiempos")]
    [SerializeField] private float t1 = 0.5f; // Preparación ataque
    [SerializeField] private float t2 = 1.5f; // Tiempo de descanso/vulnerabilidad
    [SerializeField] private float cooldownAtaque = 1f;
    [SerializeField] private bool ataqueDisponible;

    [Header("Referencias Ojetos")]
    [SerializeField] private BoxCollider2D daga;
    [SerializeField] private GameObject escudo;
    [SerializeField] private GameObject shieldImpact;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private GameObject goldObj;

    private Transform playerTransform;
    private AudioSource aud;
    private int direction = 1;

    void Start()
    {
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = gameObject.layer;
        fuerzaRecoil = 2f;
        vidaMax = vida;
        ataqueDisponible = true;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aud = GetComponent<AudioSource>();
        flash = GetComponent<DamageFlash>();

        if (healthBar != null)
        {
            bar = Instantiate(healthBar).GetComponent<EnemyHealthBar>();
            bar.SetFocus(transform);
        }

        // Buscar al jugador al inicio es más eficiente
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        daga.enabled = false;
        escudo.SetActive(true);
        temporizadorMirada = cooldownCambioMirada;
        direction = (int)Mathf.Sign(transform.localScale.x);
    }

    void FixedUpdate()
    {
        if (vida <= 0)
        {
            Muerte();
            return;
        }

        if (bar != null) bar.SetHealthValue(vida / vidaMax);

        // Si el enemigo no es "playable" (ej. recibiendo daño pesado), no procesar la IA
        if (!playable) return;

        ProcesarIA();
    }

    private void ProcesarIA()
    {
        float distancia = ObtenerDistanciaAlJugador();
        bool viendoAlJugador = distancia <= rangoVision && JugadorEnLineaDeVision(distancia);

        switch (estadoActual)
        {
            case EstadoEscudo.Idle:
                ComportamientoIdle(viendoAlJugador);
                break;

            case EstadoEscudo.Persecucion:
                ComportamientoPersecucion(distancia, viendoAlJugador);
                break;

            case EstadoEscudo.Atacando:
            case EstadoEscudo.Vulnerable:
                // Durante estos estados, el control lo toma la Corrutina de Ataque
                break;

            case EstadoEscudo.Huida:
                // Durante la huida el control lo toma la Corrutina de Huida, 
                // pero aplicamos la física aquí
                MoverseHacia(-direction * (speed * 0.7f)); // Camina hacia atrás más lento
                break;
        }
    }

    #region Comportamientos de Estado

    private void ComportamientoIdle(bool viendoAlJugador)
    {
        DetenerMovimiento();

        if (viendoAlJugador)
        {
            estadoActual = EstadoEscudo.Persecucion;
            return;
        }

        temporizadorMirada -= Time.fixedDeltaTime;
        if (temporizadorMirada <= 0)
        {
            Flip();
            temporizadorMirada = cooldownCambioMirada;
        }
    }

    private void ComportamientoPersecucion(float distancia, bool viendoAlJugador)
    {
        if (!viendoAlJugador)
        {
            estadoActual = EstadoEscudo.Idle;
            return;
        }

        MirarAlJugador();

        if (distancia <= rangoAtaque && ataqueDisponible)
        {
            StartCoroutine(SecuenciaAtaqueYVulnerabilidad());
            return;
        }

        // Movimiento cuidadoso: avanza un poco, se detiene, avanza.
        if (!persecucionPausada)
        {
            anim.SetBool("Playable", true);
            MoverseHacia(direction * speed);
            // Probabilidad aleatoria de pausarse para dar la sensación de ir "con cuidado"
            if (Random.Range(0f, 100f) < 1f) StartCoroutine(PausarPersecucion());
        }
        else
        {
            DetenerMovimiento();
        }
    }

    private IEnumerator PausarPersecucion()
    {
        persecucionPausada = true;
        anim.SetBool("Playable", false);
        yield return new WaitForSeconds(0.5f);
        persecucionPausada = false;
        anim.SetBool("Playable", true);
    }

    private IEnumerator SecuenciaAtaqueYVulnerabilidad()
    {
        // 1. Inicia el Ataque
        estadoActual = EstadoEscudo.Atacando;
        ataqueDisponible = false;
        DetenerMovimiento();
        anim.SetBool("Playable", false);
        anim.SetBool("Atacando", true);

        escudo.SetActive(false); // Baja el escudo
        yield return new WaitForSeconds(t1);

        anim.SetBool("Atacando", false);
        daga.enabled = true;
        escudo.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        daga.enabled = false;

        // 2. Estado Vulnerable
        estadoActual = EstadoEscudo.Vulnerable;
        // Aquí no hacemos Flip(). Se queda mirando a donde atacó.
        yield return new WaitForSeconds(t2);

        // 3. Recupera el escudo y huye
        anim.SetBool("Playable", true);
        StartCoroutine(SecuenciaHuida());
    }

    private IEnumerator SecuenciaHuida()
    {
        estadoActual = EstadoEscudo.Huida;
        anim.SetBool("BackWalk", true);
        MirarAlJugador(); // Aseguramos que retroceda viendo al jugador

        yield return new WaitForSeconds(tiempoCaminarAtras);

        anim.SetBool("BackWalk", false);
        estadoActual = EstadoEscudo.Persecucion;

        yield return new WaitForSeconds(cooldownAtaque);
        ataqueDisponible = true;
    }

    #endregion

    #region Movimiento y Visión

    private void MoverseHacia(float velocidadX)
    {
        // Solo se mueve si no hay un vacío enfrente
        if (DetectarPiso())
        {
            rb.linearVelocity = new Vector2(velocidadX * (1 - afectacionViento), rb.linearVelocity.y);
        }
        else
        {
            DetenerMovimiento();
        }
    }

    private void DetenerMovimiento()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Playable", false);
    }

    private void MirarAlJugador()
    {
        if (playerTransform == null) return;
        int dirHaciaJugador = playerTransform.position.x > transform.position.x ? 1 : -1;

        if (direction != dirHaciaJugador)
        {
            direction = dirHaciaJugador;
            transform.localScale = new Vector3(direction, 1, 1);
        }
    }

    private void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    private float ObtenerDistanciaAlJugador()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    private bool JugadorEnLineaDeVision(float distancia)
    {
        if (playerTransform == null) return false;
        Vector2 direccionRayo = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionRayo, distancia, wallLayer | groundLayer);
        return hit.collider == null;
    }

    public bool DetectarPiso()
    {
        // Comprueba si hay piso un poco más adelante en la dirección que se mueve
        Vector2 puntoDeteccion = (Vector2)groundDetector.position + (Vector2.right * direction * 0.5f);
        return Physics2D.OverlapCircle(puntoDeteccion, 0.2f, groundLayer);
    }

    #endregion

    #region Daño y Colisiones

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14) // Capa del arma del jugador
        {
            // Calculamos desde dónde viene el ataque y a dónde mira el enemigo
            float dirAtaque = Mathf.Sign(collider.transform.position.x - transform.position.x);
            float miDireccion = Mathf.Sign(transform.localScale.x);

            // Si el ataque viene de frente Y el escudo está activo -> ¡BLOQUEO!
            if (dirAtaque == miDireccion && escudo.activeInHierarchy)
            {
                if (shieldImpact != null) Destroy(Instantiate(shieldImpact, escudo.transform.position, Quaternion.identity), 1.5f);

                // Pequeño empuje hacia atrás por golpear el escudo (opcional)
                rb.AddForce(new Vector2(-dirAtaque * 3f, 0), ForceMode2D.Impulse);
                return;
            }

            // Si llega aquí, es porque le dio por la espalda, el escudo estaba abajo, o estaba vulnerable
            TriggerElementos_1_1_1(collider);
            Recoil(-(int)dirAtaque, fuerzaRecoil);

            if (collider.transform.parent != null)
            {
                var scriptJugador = collider.transform.parent.parent.GetComponent<Hoyustus>();
                if (scriptJugador != null)
                {
                    scriptJugador.cargaLanza();
                    RecibirDanio(scriptJugador.getAtaque());

                    if (aud != null)
                    {
                        aud.Stop();
                        aud.clip = hurtSound;
                        aud.Play();
                    }
                }
            }
        }
        else if (!collider.name.Contains("Enemy") && collider.gameObject.layer != 3 && collider.gameObject.layer != 18)
        {
            TriggerElementos_1_1_1(collider);
        }
    }

    protected override void Recoil(int direccionGolpe, float fuerzaRecoilMod)
    {
        rb.linearVelocity = Vector2.zero;
        playable = false;
        rb.AddForce(new Vector2(direccionGolpe, rb.gravityScale) * fuerzaRecoilMod, ForceMode2D.Impulse);
        Invoke("RestaurarPlayable", 0.4f);
    }

    private void RestaurarPlayable()
    {
        playable = true;
    }

    private void Muerte()
    {
        if (deathFX != null) Instantiate(deathFX, transform.position, Quaternion.identity);
        if (goldObj != null) Instantiate(goldObj, transform.position, Quaternion.identity);

        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D obj in objetos)
        {
            Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
            if (rb2D != null && obj.gameObject != gameObject)
            {
                Vector2 direccionExp = obj.transform.position - transform.position;
                float distancia = 1 + direccionExp.magnitude;
                rb2D.AddForce(direccionExp * (200 / distancia));
            }
        }

        var spawner = GameObject.Find("-----ENEMIES");
        if (spawner != null) spawner.GetComponent<EnemyRespawn>().EnemyDeath();

        if (bar != null) Destroy(bar.gameObject);
        Destroy(gameObject);
    }
    #endregion
}