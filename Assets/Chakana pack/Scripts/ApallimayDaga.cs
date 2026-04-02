using System.Collections;
using UnityEngine;

public class ApallimayDaga : Apallimay
{
    public enum EstadoEnemigo { Idle, Persecucion, Ataque, Huida }

    [Header("Máquina de Estados")]
    [SerializeField] private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float speedIdle = 2f;
    [SerializeField] private float speedChase = 4f;
    [SerializeField] private float speedFlee = 4.5f;
    private int direction = 1;
    private float tiempoPatrullaAleatoria;

    [Header("Configuración de Combate")]
    //[SerializeField] private float rangoVision = 6f;
    //[SerializeField] private float rangoAtaque = 1.5f;
    [SerializeField] private float tiempoPreparacion = 0.5f; // t1
    [SerializeField] private float tiempoDescansoAtaque = 0.4f; // t2
    [SerializeField] private float cooldownAtaque = 3f;
    [SerializeField] private float fuerzaImpulsoAtaque = 12f;

    private float temporizadorCooldown;
    //private bool ataqueDisponible = true;
    private bool atacando = false;

    [Header("Referencias")]
    [SerializeField] private BoxCollider2D daga;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private GameObject goldObj; // Drop (Spondylus)

    private AudioSource aud;
    private Transform playerTransform;

    void Start()
    {
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = gameObject.layer;
        fuerzaRecoil = 3f;

        rb = GetComponent<Rigidbody2D>();
        aud = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        // Referencias de los hijos
        daga = transform.GetChild(2).GetComponent<BoxCollider2D>();
        groundDetector = transform.GetChild(4);
        wallDetector = transform.GetChild(5);

        daga.enabled = false;
        vidaMax = vida;

        if (healthBar != null)
        {
            bar = Instantiate(healthBar).GetComponent<EnemyHealthBar>();
            bar.SetFocus(transform);
        }

        // Buscar al jugador una sola vez al inicio
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        AsignarNuevoTiempoPatrulla();
    }

    void FixedUpdate()
    {
        if (vida <= 0)
        {
            Muerte();
            return;
        }

        if (bar != null) bar.SetHealthValue(vida / vidaMax);

        if (rb.linearVelocity.y < 0) Falling();

        // Si el enemigo está en modo jugable (no está aturdido por recoil) y no está en medio de la animación de ataque
        if (playable && !atacando)
        {
            ActualizarEstado();
        }
    }

    #region Lógica de Estados

    private void ActualizarEstado()
    {
        // Actualizar el CD del ataque si ya lo usamos
        if (!ataqueDisponible)
        {
            temporizadorCooldown -= Time.fixedDeltaTime;
            if (temporizadorCooldown <= 0)
            {
                ataqueDisponible = true;
                if (estadoActual == EstadoEnemigo.Huida) estadoActual = EstadoEnemigo.Persecucion;
            }
        }

        float distanciaAlJugador = ObtenerDistanciaAlJugador();
        bool jugadorVisible = JugadorEnLineaDeVision(distanciaAlJugador);

        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                ComportamientoIdle(jugadorVisible);
                break;

            case EstadoEnemigo.Persecucion:
                ComportamientoPersecucion(distanciaAlJugador, jugadorVisible);
                break;

            case EstadoEnemigo.Huida:
                ComportamientoHuida();
                break;
        }
    }

    private void ComportamientoIdle(bool jugadorVisible)
    {
        if (jugadorVisible && ataqueDisponible)
        {
            estadoActual = EstadoEnemigo.Persecucion;
            return;
        }

        // Patrulla aleatoria
        tiempoPatrullaAleatoria -= Time.fixedDeltaTime;
        if (tiempoPatrullaAleatoria <= 0)
        {
            Flip();
            AsignarNuevoTiempoPatrulla();
        }

        if (EntornoBloqueado())
        {
            Flip();
            AsignarNuevoTiempoPatrulla();
        }

        Mover(speedIdle);
    }

    private void ComportamientoPersecucion(float distancia, bool jugadorVisible)
    {
        if (!jugadorVisible)
        {
            estadoActual = EstadoEnemigo.Idle;
            return;
        }

        MirarHacia(playerTransform.position.x);

        if (distancia <= rangoAtaque && ataqueDisponible)
        {
            StartCoroutine(EjecutarAtaque());
        }
        else
        {
            if (!EntornoBloqueado()) Mover(speedChase);
            else rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void ComportamientoHuida()
    {
        // Corre en dirección opuesta al jugador
        int direccionHuida = playerTransform.position.x > transform.position.x ? -1 : 1;

        if (direction != direccionHuida)
        {
            direction = direccionHuida;
            ActualizarEscala();
        }

        // Si en su huida se topa con una pared o un abismo, se queda acorralado esperando el CD
        if (EntornoBloqueado())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            Mover(speedFlee);
        }
    }

    #endregion

    #region Movimiento y Detección

    private void Mover(float velocidadActual)
    {
        rb.linearVelocity = new Vector2(direction * velocidadActual * (1 - afectacionViento), rb.linearVelocity.y);
    }

    private void AsignarNuevoTiempoPatrulla()
    {
        tiempoPatrullaAleatoria = Random.Range(1.5f, 4f); // Se mueve entre 1.5 y 4 segundos antes de cambiar
    }

    private bool EntornoBloqueado()
    {
        bool haySuelo = Physics2D.OverlapCircle(groundDetector.position, 0.3f, groundLayer);
        bool chocaPared = Physics2D.OverlapCircle(wallDetector.position, 0.3f, wallLayer) ||
                          Physics2D.OverlapCircle(wallDetector.position, 0.3f, groundLayer);

        return !haySuelo || chocaPared;
    }

    private float ObtenerDistanciaAlJugador()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    private bool JugadorEnLineaDeVision(float distancia)
    {
        if (playerTransform == null || distancia > rangoVision) return false;

        // Comprueba la orientación: ¿Está mirando hacia el jugador?
        //float dirHaciaJugador = Mathf.Sign(playerTransform.position.x - transform.position.x);
        //if (dirHaciaJugador != direction) return false; 
        // Solo lo ve si está frente a él

        // Raycast para comprobar que no hay paredes en medio
        Vector2 direccionRayo = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionRayo, distancia, wallLayer | groundLayer);

        return hit.collider == null; // Retorna true si el rayo no chocó con ninguna pared
    }

    private void MirarHacia(float targetX)
    {
        int nuevaDireccion = targetX > transform.position.x ? 1 : -1;
        if (direction != nuevaDireccion)
        {
            direction = nuevaDireccion;
            ActualizarEscala();
        }
    }

    private void Flip()
    {
        direction *= -1;
        ActualizarEscala();
    }

    private void ActualizarEscala()
    {
        transform.localScale = new Vector3(direction, 1, 1);
    }

    private void Falling()
    {
        rb.linearVelocity -= Vector2.up * Time.deltaTime * -Physics2D.gravity.y * 4.5f;
    }

    #endregion

    #region Combate

    private IEnumerator EjecutarAtaque()
    {
        estadoActual = EstadoEnemigo.Ataque;
        atacando = true;
        ataqueDisponible = false;

        // 1. FRENAR (Preparación)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Atacando", true);

        yield return new WaitForSeconds(tiempoPreparacion);
        anim.SetBool("Atacando", false);
        // 2. ATAQUE (Dash con daga)
        if (aud != null)
        {
            aud.Stop();
            aud.clip = attackSound;
            aud.Play();
        }

        rb.AddForce(new Vector2(direction * fuerzaImpulsoAtaque, 0f), ForceMode2D.Impulse);
        daga.enabled = true;

        yield return new WaitForSeconds(0.4f); // Duración activa de la daga

        // 3. FINALIZAR ATAQUE
        daga.enabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(tiempoDescansoAtaque);

        // 4. TRANSICIÓN A HUIDA
        atacando = false;
        temporizadorCooldown = cooldownAtaque;
        estadoActual = EstadoEnemigo.Huida;
    }

    protected override void Recoil(int direccionGolpe, float fuerzaRecoilMod)
    {
        playable = false;
        rb.AddForce(new Vector2(direccionGolpe * 7f, rb.gravityScale * 4f), ForceMode2D.Impulse);
        Invoke("RestaurarPlayable", 0.5f); // Pequeño aturdimiento tras recibir daño
    }

    private void RestaurarPlayable()
    {
        playable = true;
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        // Daño del jugador
        if (collider.gameObject.layer == 14)
        {
            int dirGolpe = -(int)Mathf.Sign(collider.transform.position.x - transform.position.x);
            TriggerElementos_1_1_1(collider);
            StartCoroutine(cooldownRecibirDanio(dirGolpe, 1));

            if (collider.transform.parent != null)
            {
                var jugador = collider.transform.parent.parent.GetComponent<Hoyustus>();
                if (jugador != null)
                {
                    jugador.cargaLanza();
                    RecibirDanio(jugador.getAtaque());

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

    #endregion

    #region Muerte

    private void Muerte()
    {
        if (deathFX != null) Instantiate(deathFX, transform.position, Quaternion.identity);
        if (goldObj != null) Instantiate(goldObj, transform.position, Quaternion.identity);

        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D obj in objetos)
        {
            Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
            if (rb2D != null && obj.gameObject != this.gameObject)
            {
                Vector2 direccionExp = obj.transform.position - transform.position;
                float distancia = 1 + direccionExp.magnitude;
                float fuerza = 200 / distancia;
                rb2D.AddForce(direccionExp * fuerza);
            }
        }

        var spawner = GameObject.Find("-----ENEMIES");
        if (spawner != null) spawner.GetComponent<EnemyRespawn>().EnemyDeath();

        if (bar != null) Destroy(bar.gameObject);
        Destroy(gameObject);
    }

    #endregion
}