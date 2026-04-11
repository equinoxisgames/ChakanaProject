using System.Collections;
using UnityEngine;

public class Tzantza : Enemy
{
    public enum EstadoTzantza { Idle, Combate, Atacando }

    [Header("Comportamiento IA")]
    [SerializeField] private EstadoTzantza estadoActual = EstadoTzantza.Idle;
    [SerializeField] private float rangoVision = 8f;
    [SerializeField] private float rangoAtaque = 8f;
    [SerializeField] private float speed;

    [Tooltip("Distancia a la que empezará a retroceder si el jugador se acerca mucho")]
    [SerializeField] private float distanciaMinimaSegura = 3f;

    [Tooltip("Distancia a la que empezará a perseguir si el jugador se aleja")]
    [SerializeField] private float distanciaMaximaSegura = 5f;

    [SerializeField] private float distanciaSuelo = 1.5f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Ataque y Tiempos")]
    [SerializeField] private GameObject bolaFuego;
    [SerializeField] private float t1 = 0.5f; // Tiempo de preparación/casteo
    [SerializeField] private float t2 = 0.5f; // Tiempo de recuperación
    [SerializeField] private float cooldownAtaque = 2f;

    [Header("Objetos y Audio")]
    [SerializeField] private GameObject goldObj; // Aquí soltará el Spondylus
    [SerializeField] private AudioClip audioHurt;
    [SerializeField] private AudioClip audioAttack;

    private AudioSource charAudio;
    private bool ataqueDisponible = true;
    private Transform playerTransform;

    // Variables para el Idle
    private Vector2 puntoAnclaje;
    private Vector2 destinoAleatorio;
    private float temporizadorIdle;

    void Start()
    {
        charAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        vidaMax = vida;
        flash = GetComponent<DamageFlash>();

        explosionInvulnerable = "ExplosionEnemy";
        explosion = Resources.Load<GameObject>("Explosion");
        layerObject = transform.gameObject.layer;
        fuerzaRecoil = 2f;

        if (healthBar != null)
        {
            bar = Instantiate(healthBar).GetComponent<EnemyHealthBar>();
            bar.SetFocus(transform);
        }

        puntoAnclaje = transform.position;
        ElegirNuevoDestinoIdle();
    }

    void Update()
    {
        if (vida <= 0)
        {
            EjecutarMuerte();
            return;
        }

        BuscarJugador();
        bar.SetHealthValue(vida / vidaMax);

        switch (estadoActual)
        {
            case EstadoTzantza.Idle:
                ComportamientoIdle();
                break;
            case EstadoTzantza.Combate:
                ComportamientoCombate();
                break;
            case EstadoTzantza.Atacando:
                // La velocidad se detiene en seco al atacar
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    #region Lógica de Estados (IA)

    private void BuscarJugador()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }

        float distancia = Vector2.Distance(transform.position, playerTransform.position);

        if (estadoActual == EstadoTzantza.Idle && distancia <= rangoVision)
        {
            estadoActual = EstadoTzantza.Combate;
        }
    }

    private void ComportamientoIdle()
    {
        temporizadorIdle -= Time.deltaTime;

        if (Vector2.Distance(transform.position, destinoAleatorio) < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            if (temporizadorIdle <= 0) ElegirNuevoDestinoIdle();
        }
        else
        {
            MoverHacia(destinoAleatorio);
        }
    }

    private void ElegirNuevoDestinoIdle()
    {
        Vector2 direccionAleatoria = Random.insideUnitCircle * 3f;
        destinoAleatorio = puntoAnclaje + direccionAleatoria;
        temporizadorIdle = Random.Range(1f, 3f);
    }

    private void ComportamientoCombate()
    {
        float distancia = Vector2.Distance(transform.position, playerTransform.position);
        MirarHacia(playerTransform.position); // Siempre mira al jugador en combate

        // 1. Condición de pérdida de aggro
        if (distancia > rangoVision * 1.5f)
        {
            estadoActual = EstadoTzantza.Idle;
            puntoAnclaje = transform.position;
            return;
        }

        // 2. Condición de ataque
        // Asumiendo que 'rangoAtaque' viene de la clase base Enemy. 
        // Si es mayor que distanciaMaximaSegura, atacará desde lejos.
        if (ataqueDisponible && distancia <= rangoAtaque)
        {
            StartCoroutine(SecuenciaAtaque(playerTransform.position));
            return;
        }
        MantenerAltura();
        // 3. Lógica de "Zona Segura" (Movimiento)
        if (distancia < distanciaMinimaSegura)
        {
            // Huir (Retroceder)
            Vector2 direccionHuida = (transform.position - playerTransform.position).normalized;
            rb.linearVelocity = direccionHuida * speed * (1 - afectacionViento);
        }
        else if (distancia > distanciaMaximaSegura)
        {
            // Perseguir (Acercarse)
            Vector2 direccionPersecucion = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direccionPersecucion * speed * (1 - afectacionViento);
        }
        else
        {
            // En zona segura: frenar movimiento horizontal, mantener vertical (gravedad/flote)
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void MantenerAltura()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanciaSuelo, capaSuelo);
        if (hit.collider != null)
        {
            rb.AddForce(Vector2.up * 100f);
        }
    }

    private void MoverHacia(Vector2 destino)
    {
        Vector2 direccion = (destino - (Vector2)transform.position).normalized;
        rb.linearVelocity = direccion * speed * (1 - afectacionViento);
        MirarHacia(destino);

        MantenerAltura();
    }

    private void MirarHacia(Vector2 objetivoRef)
    {
        if (objetivoRef.x < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else if (objetivoRef.x > transform.position.x) transform.localScale = Vector3.one;
    }

    #endregion

    #region Ataque y Combate

    private IEnumerator SecuenciaAtaque(Vector3 objetivoAtaque)
    {
        estadoActual = EstadoTzantza.Atacando;
        ataqueDisponible = false;
        anim.SetBool("Atacando", true);

        // Preparación del ataque
        yield return new WaitForSeconds(t1);

        // Disparo
        charAudio.Stop();
        charAudio.clip = audioAttack;
        charAudio.Play();

        GameObject bolaFuegoGenerada = Instantiate(bolaFuego, transform.position, Quaternion.identity);
        bolaFuegoGenerada.name += "Enemy";
        bolaFuegoGenerada.GetComponent<ProyectilMovUniforme>().setDanio(ataque);

        // Termina animación y espera recuperación
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Atacando", false);
        yield return new WaitForSeconds(t2);

        // Vuelve al estado de combate para reposicionarse inmediatamente
        estadoActual = EstadoTzantza.Combate;

        // Cooldown en segundo plano
        yield return new WaitForSeconds(cooldownAtaque);
        ataqueDisponible = true;
    }

    protected override void Recoil(int direccion, float fuerzaRecoilMod)
    {
        // Al recibir daño, se interrumpe temporalmente su velocidad
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direccion * fuerzaRecoilMod, 0.5f), ForceMode2D.Impulse);

        // Si recibe un golpe, forzamos que entre en combate por si estaba en Idle
        if (estadoActual == EstadoTzantza.Idle) estadoActual = EstadoTzantza.Combate;
    }

    #endregion

    #region Físicas y Daño (Triggers)

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14)
        {
            int direccion = -(int)OrientacionDeteccionPlayer(collider.transform.position.x);

            TriggerElementos_1_1_1(collider);
            StartCoroutine(cooldownRecibirDanio(direccion, 1));

            if (collider.transform.parent != null)
            {
                var jugador = collider.transform.parent.parent.GetComponent<Hoyustus>();
                if (jugador != null)
                {
                    jugador.cargaLanza();
                    RecibirDanio(jugador.getAtaque());
                }

                charAudio.loop = false;
                charAudio.Stop();
                charAudio.clip = audioHurt;
                charAudio.Play();
            }
            return;
        }
        else if (collider.gameObject.layer == 11 && !collider.name.Contains("Sinchi Solicitud Prefab"))
        {
            return;
        }

        if (!collider.name.Contains("Enemy") && collider.gameObject.layer != 3 && collider.gameObject.layer != 18)
        {
            TriggerElementos_1_1_1(collider);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.name.Contains("Enemy"))
        {
            CollisionElementos_1_1_1(collision);
        }
    }

    private void EjecutarMuerte()
    {
        Instantiate(deathFX, transform.position, Quaternion.identity);
        Instantiate(goldObj, transform.position, Quaternion.identity);
        Destroy(bar.gameObject);

        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D col in objetos)
        {
            Rigidbody2D rb2D = col.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                Vector2 direccion = col.transform.position - transform.position;
                float distancia = 1 + direccion.magnitude;
                float fuerza = 200 / distancia;
                rb2D.AddForce(direccion * fuerza);
            }
        }

        var spawner = GameObject.Find("-----ENEMIES");
        if (spawner != null) spawner.GetComponent<EnemyRespawn>().EnemyDeath();

        Destroy(gameObject);
    }

    #endregion
}