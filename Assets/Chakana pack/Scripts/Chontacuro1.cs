using System.Collections;
using UnityEngine;

public class Chontacuro1 : Enemy
{
    [Header("Configuración de Patrulla")]
    [SerializeField] public float speed = 2f;
    private int direction = 1;

    [Header("Estado de Combate")]
    [SerializeField] private float tiempoAturdimiento = 0.4f;
    private bool aturdido = false;

    [Header("Referencias")]
    [SerializeField] AudioClip audioHurt;
    [SerializeField] GameObject goldObj;

    private AudioSource charAudio;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        charAudio = GetComponent<AudioSource>();
        vidaMax = vida;
        flash = GetComponent<DamageFlash>();

        if (healthBar != null)
        {
            bar = Instantiate(healthBar).GetComponent<EnemyHealthBar>();
            bar.SetFocus(transform);
        }
    }

    void Start()
    {
        fuerzaRecoil = 1;
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = transform.gameObject.layer;

        // Asignamos la orientación inicial basada en la escala del prefab
        direction = (int)Mathf.Sign(transform.localScale.x);
    }

    private void FixedUpdate()
    {
        if (vida <= 0)
        {
            Muerte();
            return;
        }

        if (bar != null)
        {
            bar.SetHealthValue(vida / vidaMax);
        }

        if (rb.linearVelocity.y < 0)
        {
            Falling();
        }

        // Solo se mueve y detecta entorno si NO está aturdido por un golpe
        if (!aturdido)
        {
            DetectarEntorno();
            Move();
        }
    }

    #region Lógica de Movimiento y Entorno

    private void DetectarEntorno()
    {
        // 1. Detección de precipicio: Si el detector de suelo NO toca la capa groundLayer
        bool haySuelo = Physics2D.OverlapCircle(groundDetector.position, 0.3f, groundLayer);

        // 2. Detección de pared: Si el detector frontal SÍ toca una pared o el suelo frente a él
        bool chocaPared = Physics2D.OverlapCircle(wallDetector.position, 0.3f, wallLayer) ||
                          Physics2D.OverlapCircle(wallDetector.position, 0.3f, groundLayer);

        // Si se acaba el piso o hay un muro, se da la vuelta
        if (!haySuelo || chocaPared)
        {
            Flip();
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(direction * speed * (1 - afectacionViento), rb.linearVelocity.y);
    }

    private void Flip()
    {
        // Invertimos la dirección matemática
        direction *= -1;

        // Volteamos visualmente el sprite y los detectores
        transform.localScale = new Vector3(direction, 1, 1);
    }

    private void Falling()
    {
        rb.linearVelocity -= Vector2.up * Time.deltaTime * -Physics2D.gravity.y * 4.5f;
    }

    #endregion

    #region Sistema de Daño y Aturdimiento

    protected override void Recoil(int direccionGolpe, float fuerzaRecoilMod)
    {
        StopCoroutine("RutinaAturdido");
        StartCoroutine(RutinaAturdido(direccionGolpe));
    }

    private IEnumerator RutinaAturdido(int direccionGolpe)
    {
        aturdido = true;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        rb.AddForce(new Vector2(-direccionGolpe * 10f, rb.gravityScale * 4f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(tiempoAturdimiento);

        aturdido = false;
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14)
        {
            int dirRecoil = (int)Mathf.Sign(collider.transform.position.x - transform.position.x);
            StartCoroutine(cooldownRecibirDanio(dirRecoil, 1));

            if (collider.transform.parent != null)
            {
                var jugador = collider.transform.parent.parent.GetComponent<Hoyustus>();
                if (jugador != null)
                {
                    jugador.cargaLanza();
                    RecibirDanio(jugador.getAtaque());
                }
            }

            charAudio.loop = false;
            charAudio.Stop();
            charAudio.clip = audioHurt;
            charAudio.Play();
        }

        if (collider.gameObject.CompareTag("Viento") && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoViento) StopCoroutine("afectacionEstadoViento");
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                this.CombinacionesElementales();
                return;
            }
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");
        }
        else if (collider.gameObject.CompareTag("Fuego") && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoFuego) StopCoroutine("afectacionEstadoFuego");
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                CombinacionesElementales();
                return;
            }
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
        }
    }

    private void CombinacionesElementales()
    {
        if (counterEstados == 11)
        {
            if (combObj01 == null) combObj01 = Instantiate(combFX01, transform.position, Quaternion.identity, transform);

            estadoViento = false;
            afectacionViento = 0;
            counterEstados = 10;
            aumentoFuegoPotenciado = 3;
            ataque = ataqueMax * 0.75f;
            StopCoroutine("afectacionEstadoFuego");
            estadoFuego = true;
            StartCoroutine("afectacionEstadoFuego");
        }
    }

    #endregion

    #region Muerte

    private void Muerte()
    {
        if (deathFX != null) Instantiate(deathFX, transform.position, Quaternion.identity);
        if (goldObj != null) Instantiate(goldObj, transform.position, Quaternion.identity);

        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (Collider2D collider in objetos)
        {
            Rigidbody2D rb2D = collider.GetComponent<Rigidbody2D>();
            if (rb2D != null && collider.gameObject != this.gameObject)
            {
                Vector2 direccion = collider.transform.position - transform.position;
                float distancia = 1 + direccion.magnitude;
                float fuerza = 200 / distancia;
                rb2D.AddForce(direccion * fuerza);
            }
        }

        var spawner = GameObject.Find("-----ENEMIES");
        if (spawner != null) spawner.GetComponent<EnemyRespawn>().EnemyDeath();

        if (bar != null) Destroy(bar.gameObject);
        Destroy(this.gameObject);
    }

    #endregion
}