using System.Collections;
using UnityEngine;

public class ApallimayArco : Apallimay
{
    public enum EstadoArquero { Idle, Atacando }

    [Header("Estado y Visión")]
    [SerializeField] private EstadoArquero estadoActual = EstadoArquero.Idle;
    //[SerializeField] private float rangoVision = 10f;
    [SerializeField] private float cooldownCambioMirada = 3f;
    private float temporizadorMirada;

    [Header("Combate (Arco)")]
    [SerializeField] private GameObject flecha;
    [SerializeField] private float cooldownDisparoFlechas = 2f;
    //private bool ataqueDisponible = true;
    private bool atacando = false;
    private int codigoAtaque; // Para el Animator: 0=Frente, 1=Arriba, 2=Abajo

    [Header("Pasiva Especial (Teletransporte al 50% HP)")]
    [Tooltip("Asigna un Empty GameObject de la escena aquí. El enemigo huirá a esta posición.")]
    [SerializeField] private Transform puntoTeletransporte;
    [SerializeField] private GameObject dropObj; // El objeto que dejará al huir (goldObj)
    [SerializeField] private GameObject humoFX; // Opcional, para que el TP tenga feedback visual

    private Vector3 posicionEscape;
    private bool teleportUsado = false;

    [Header("Referencias")]
    [SerializeField] private AudioClip hurtSound;
    private AudioSource aud;
    private Transform playerTransform;
    private int direction = 1;

    void Start()
    {
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = gameObject.layer;
        fuerzaRecoil = 2f;
        vidaMax = vida;
        ataqueDisponible = true;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        aud = GetComponent<AudioSource>();

        if (healthBar != null)
        {
            bar = Instantiate(healthBar).GetComponent<EnemyHealthBar>();
            bar.SetFocus(transform);
        }

        // Buscar al jugador solo una vez al inicio ahorra mucho rendimiento
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // Guardamos la coordenada exacta al inicio. Aunque el puntoTeletransporte se mueva luego,
        // el enemigo recordará este Vector3 específico.
        if (puntoTeletransporte != null) posicionEscape = puntoTeletransporte.position;

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

        // --- PASIVA ESPECIAL: Revisar si la vida bajó al 50% o menos ---
        if (!teleportUsado && vida <= vidaMax * 0.5f)
        {
            EjecutarTeletransporte();
            return; // Detenemos el resto del código en este frame
        }

        // --- ACTUALIZAR ANIMACIONES ---
        anim.SetFloat("CA1", codigoAtaque);

        // Si está sufriendo recoil por un golpe o disparando, no evaluamos la IA
        if (!playable || atacando) return;

        ActualizarComportamiento();
    }

    private void ActualizarComportamiento()
    {
        float distancia = ObtenerDistanciaAlJugador();
        bool jugadorEnRango = distancia <= rangoVision && JugadorEnLineaDeVision(distancia);

        if (jugadorEnRango)
        {
            estadoActual = EstadoArquero.Atacando;
            MirarHacia(playerTransform.position.x);
            Apuntar();

            if (ataqueDisponible)
            {
                StartCoroutine(DispararFlecha());
            }
        }
        else
        {
            estadoActual = EstadoArquero.Idle;
            ComportamientoIdle();
        }
    }

    #region Estados y Mecánicas

    private void ComportamientoIdle()
    {
        // Se asegura de no moverse físicamente
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Rotar cada cierto tiempo
        temporizadorMirada -= Time.fixedDeltaTime;
        if (temporizadorMirada <= 0)
        {
            Flip();
            temporizadorMirada = cooldownCambioMirada;
        }
    }

    private void Apuntar()
    {
        if (playerTransform == null) return;

        Vector2 direccionJugador = playerTransform.position - transform.position;
        // Calculamos el ángulo basándonos hacia donde mira (transform.right * direction)
        float angulo = Vector2.Angle(transform.right * direction, direccionJugador);
        // Si el jugador está más abajo, el ángulo es negativo
        if (playerTransform.position.y < transform.position.y) angulo *= -1;

        if (angulo <= 30f && angulo >= -30f) codigoAtaque = 0; // Frente
        else if (angulo > 30f) codigoAtaque = 1; // Arriba
        else codigoAtaque = 2; // Abajo
    }

    private IEnumerator DispararFlecha()
    {
        ataqueDisponible = false;
        atacando = true;
        anim.SetBool("Atacando", true);

        // 1. Tiempo de preparación (Levantar el arco)
        yield return new WaitForSeconds(0.55f);
        anim.SetBool("Atacando", false);
        // 2. Disparo de la flecha
        if (playerTransform != null)
        {
            GameObject flechaGenerada = Instantiate(flecha, transform.position, Quaternion.identity);
            flechaGenerada.name += "Enemy";

            // Usamos Mathf.Atan2 para una rotación perfecta y sin fallos matemáticos hacia el objetivo
            Vector2 dirDisparo = playerTransform.position - transform.position;
            float anguloFlecha = Mathf.Atan2(dirDisparo.y, dirDisparo.x) * Mathf.Rad2Deg;
            flechaGenerada.transform.rotation = Quaternion.Euler(0, 0, anguloFlecha);

            flechaGenerada.GetComponent<ProyectilMovUniforme>().setDanio(ataque);
        }

        // 3. Finaliza animación de disparo
        yield return new WaitForSeconds(0.2f);
        atacando = false;

        // 4. Esperar el CD para volver a disparar
        yield return new WaitForSeconds(cooldownDisparoFlechas);
        ataqueDisponible = true;
    }

    private void EjecutarTeletransporte()
    {
        teleportUsado = true;

        // 1. Dejar el Drop
        if (dropObj != null) Instantiate(dropObj, transform.position, Quaternion.identity);

        // 2. Efecto visual (opcional)
        if (humoFX != null) Instantiate(humoFX, transform.position, Quaternion.identity);

        // 3. Mover instantáneamente al Vector3 guardado en Start
        transform.position = posicionEscape;

        // 4. Frenar físicas e interrumpir corrutinas para que no se teletransporte y dispare al mismo tiempo
        rb.linearVelocity = Vector2.zero;

        //estadoActual = EstadoArquero.Idle;
        //atacando = false;
        //ataqueDisponible = true; // Reseteamos por si estaba a medio ataque
    }

    #endregion

    #region Utilidades y Detección

    private float ObtenerDistanciaAlJugador()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    private bool JugadorEnLineaDeVision(float distancia)
    {
        if (playerTransform == null) return false;

        Vector2 direccionRayo = (playerTransform.position - transform.position).normalized;

        // Raycast ignora al jugador, solo busca si hay un muro o piso en medio del camino
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionRayo, distancia, wallLayer);

        return hit.collider == null; // Si no chocó con un muro, lo está viendo
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

    #endregion

    #region Sistema de Daño y Muerte

    protected override void Recoil(int direccionGolpe, float fuerzaRecoilMod)
    {
        playable = false;
        rb.AddForce(new Vector2(direccionGolpe * 2f, rb.gravityScale * 2f), ForceMode2D.Impulse);
        Invoke("RestaurarPlayable", 0.4f);
    }

    private void RestaurarPlayable()
    {
        playable = true;
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        // Layer 14 (Arma del jugador)
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

    private void Muerte()
    {
        if (deathFX != null) Instantiate(deathFX, transform.position, Quaternion.identity);

        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D obj in objetos)
        {
            Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
            if (rb2D != null && obj.gameObject != gameObject)
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