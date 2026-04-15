using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mapianguari : Enemy
{
    public enum EstadoMapinguari { Intro, Acechando, Teletransportando, AtacandoCuerpo, InvocandoPlantas, TransicionFuria, AsedioRodante, Muerto }

    [Header("Máquina de Estados & Fases")]
    [SerializeField] private EstadoMapinguari estadoActual = EstadoMapinguari.Acechando;
    private bool enFuria = false;

    [Header("Configuración de Movimiento y Combate")]
    [SerializeField] private float movementVelocity = 6f;
    [SerializeField] private float rangoAtaqueZarpazo = 3.5f;
    [SerializeField] private float cooldownInvocacion = 8f;
    [SerializeField] private float valorAtaqueBasico = 20f;
    [SerializeField] private float valorAtaqueEspecial = 40f;

    private float temporizadorInvocacion;
    private Transform playerTransform;
    private Hoyustus scriptJugador;
    private int plataformaActual = 0;

    [Header("Mecánica: Invocación")]
    [SerializeField] private GameObject plantaVeneno;
    [SerializeField] private List<Transform> posPlantas;
    [SerializeField] private float danioPlantaVeneno = 10f;

    [Header("Efectos y Referencias Extra")]
    [SerializeField] private GameObject humo;
    [SerializeField] private GameObject gotg;
    [SerializeField] private LiquidBar lifeBar;
    [SerializeField] private ManagerPeleaMapinguari levelController;
    [SerializeField] private BoxCollider2D ataqueCuerpo;
    [SerializeField] private Transform embestidaPos;

    [Header("Audio")]
    [SerializeField] private AudioClip audioAtk;
    [SerializeField] private AudioClip audioScream;
    [SerializeField] private AudioClip audioWalk;
    [SerializeField] private AudioSource hurtAudio, hurtAudio2, hurtAudio3, hurtAudio4, shockAudio;
    [SerializeField] private AudioSource victorySound;
    private AudioSource charAudio;
    private bool hurtSound = false;

    [Header("Pantalla Victoria")]
    [SerializeField] private GameObject bossSilhouettePrefab;
    [SerializeField] private GameObject playerSilhouettePrefab;
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private GameObject quadPrefab;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        charAudio = GetComponent<AudioSource>();
        flash = GetComponent<DamageFlash>();

        vidaMax = vida;
        ataqueMax = valorAtaqueBasico;
        ataque = ataqueMax;
        fuerzaRecoil = 4f;
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = gameObject.layer;

        ataqueCuerpo.enabled = false;
        temporizadorInvocacion = cooldownInvocacion;

        // Referencia directa al jugador para optimizar rendimiento
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            scriptJugador = playerObj.GetComponent<Hoyustus>();
        }

        estadoActual = EstadoMapinguari.Acechando;
    }

    private void Update()
    {
        if (estadoActual == EstadoMapinguari.Muerto) return;

        ActualizarVida();

        if (vida <= 0)
        {
            StopAllCoroutines();
            estadoActual = EstadoMapinguari.Muerto;
            StartCoroutine(RutinaMuerte());
            return;
        }

        // Transición a Fase Furia (<50% HP)
        if (!enFuria && vida <= vidaMax / 2 && estadoActual == EstadoMapinguari.Acechando)
        {
            StartCoroutine(EntrarEnFuria());
            return;
        }

        // Si está en medio de una acción atómica (como teletransporte o ataque), no evaluamos la IA de movimiento
        if (estadoActual != EstadoMapinguari.Acechando) return;

        EvaluarComportamientoAcecho();
    }

    #region Máquina de Estados (IA)

    private void EvaluarComportamientoAcecho()
    {
        if (playerTransform == null || !playable) return;

        float distanciaHorizontal = Mathf.Abs(transform.position.x - playerTransform.position.x);

        // 2. Manejo de Cooldowns de Habilidades
        temporizadorInvocacion -= Time.deltaTime;

        // Mirar hacia el jugador
        MirarHacia(playerTransform.position.x);

        // 3. Selección de Ataques
        if (distanciaHorizontal <= rangoAtaqueZarpazo)
        {
            StartCoroutine(AtaqueZarpazo());
        }
        else if (temporizadorInvocacion <= 0f)
        {
            StartCoroutine(InvocacionDePlantas());
        }
        else
        {
            // Movimiento de acecho
            Vector2 direccion = new Vector2(Mathf.Sign(playerTransform.position.x - transform.position.x), 0);
            rb.linearVelocity = new Vector2(direccion.x * movementVelocity * (1 - afectacionViento), rb.linearVelocity.y);

            ManejarAudioCaminar(true);
        }
    }

    private void MirarHacia(float targetX)
    {
        if (targetX < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = Vector3.one;
    }

    public void NewTeleport(Vector3 posMin, Vector3 posMax, int e)
    {
        if (plataformaActual == e || estadoActual == EstadoMapinguari.AsedioRodante || estadoActual == EstadoMapinguari.TransicionFuria) return;
        plataformaActual = e;
        StopAllCoroutines();
        anim.SetBool("Iddel", false);
        anim.SetBool("AB", false);
        StartCoroutine(TeletransporteAlJugador(posMin, posMax));
    }

    #endregion

    #region Mecánicas de Combate (Corrutinas)

    private IEnumerator TeletransporteAlJugador(Vector3 posMin, Vector3 posMax)
    {
        estadoActual = EstadoMapinguari.Teletransportando;
        ManejarAudioCaminar(false);
        rb.linearVelocity = Vector2.zero;

        // Rugido Ligero de aviso
        ReproducirAudio(audioScream);
        anim.ResetControllerState();
        anim.SetBool("AT", true);
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("AT", false);

        // Desaparecer con humo
        yield return new WaitForSeconds(0.4f);
        Instantiate(humo, transform.position, Quaternion.identity);
        GetComponent<SpriteRenderer>().enabled = false;
        ataqueCuerpo.enabled = false;

        yield return new WaitForSeconds(0.5f);

        // --- LÓGICA TÁCTICA DE REAPARICIÓN ---

        float distanciaSegura = 7f; // Distancia mínima que mantendrá con Sinchi
        float margenBorde = 1.5f;   // Margen para no aparecer con medio cuerpo fuera de la plataforma
        float playerX = playerTransform.position.x;

        // Calculamos dos posibles puntos de reaparición: a la izquierda y a la derecha del jugador
        float puntoIzquierda = playerX - distanciaSegura;
        float puntoDerecha = playerX + distanciaSegura;

        // Comprobamos cuáles de estos puntos están dentro de los límites de la plataforma
        bool izqValido = puntoIzquierda >= (posMin.x + margenBorde);
        bool derValido = puntoDerecha <= (posMax.x - margenBorde);

        float nuevoX = playerX; // Valor de seguridad por defecto

        if (izqValido && derValido)
        {
            // Si hay espacio en ambos lados, aparece en el lado donde haya más terreno libre
            float espacioIzq = playerX - posMin.x;
            float espacioDer = posMax.x - playerX;
            nuevoX = (espacioIzq > espacioDer) ? puntoIzquierda : puntoDerecha;
        }
        else if (izqValido)
        {
            nuevoX = puntoIzquierda;
        }
        else if (derValido)
        {
            nuevoX = puntoDerecha;
        }
        else
        {
            // Caso extremo: Si la plataforma es muy pequeña, aparece lo más lejos posible sin salirse
            nuevoX = (playerX > (posMin.x + posMax.x) / 2f) ? posMin.x + margenBorde : posMax.x - margenBorde;
        }

        // Para la Y, usamos posMax.y (o posMin.y, dependiendo de cómo estén configurados tus límites). 
        // Esto garantiza que aparezca a nivel del suelo, ignorando si el jugador estaba saltando.
        // Opcional: Sumar un pequeño offset (+ 0.5f) si el punto de pivote (pivot) de tu sprite no está en los pies.
        float nuevoY = posMax.y;

        transform.position = new Vector3(nuevoX, nuevoY, 0);

        // Obligamos al jefe a mirar al jugador instantáneamente antes de aparecer
        MirarHacia(playerX);

        // Reaparecer
        Instantiate(humo, transform.position, Quaternion.identity);
        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        estadoActual = EstadoMapinguari.Acechando;
    }

    private IEnumerator AtaqueZarpazo()
    {
        estadoActual = EstadoMapinguari.AtacandoCuerpo;
        ManejarAudioCaminar(false);
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("AB", true); // Ataque Básico
        anim.SetBool("Iddel", true);

        // El evento de animación debería activar "ActivacionGarras(1)" y luego "(0)"
        yield return new WaitForSeconds(0.4f); // Ajusta según la duración de tu animación de ataque

        ReproducirAudio(audioAtk);
        anim.SetBool("AB", false);

        yield return new WaitForSeconds(0.2f);

        ataqueCuerpo.enabled = true;

        yield return new WaitForSeconds(0.2f); // Pequeño descanso tras atacar

        ataqueCuerpo.enabled = false;

        yield return new WaitForSeconds(0.5f);

        anim.SetBool("Iddel", false);
        estadoActual = EstadoMapinguari.Acechando;
    }

    private IEnumerator InvocacionDePlantas()
    {
        estadoActual = EstadoMapinguari.InvocandoPlantas;
        temporizadorInvocacion = cooldownInvocacion;
        ManejarAudioCaminar(false);
        rb.linearVelocity = Vector2.zero;

        // Rugido de Invocación
        ReproducirAudio(audioScream);
        anim.SetBool("AT", true);

        yield return new WaitForSeconds(0.5f);
        anim.SetBool("AT", false);

        // --- LÓGICA DE APARICIÓN SIN REPETIR ---

        // 1. Definimos cuántas plantas van a salir
        int cantidadAInvocar = enFuria ? 3 : 2;

        // Seguridad: Evitar error si hay menos posiciones en la lista que plantas a invocar
        cantidadAInvocar = Mathf.Min(cantidadAInvocar, posPlantas.Count);

        if (cantidadAInvocar > 0)
        {
            // 2. Creamos una copia temporal de la lista para ir eliminando las posiciones ya usadas
            List<Transform> posicionesDisponibles = new List<Transform>(posPlantas);

            for (int i = 0; i < cantidadAInvocar; i++)
            {
                // 3. Elegimos un índice al azar de las posiciones que AÚN están disponibles
                int indiceAleatorio = UnityEngine.Random.Range(0, posicionesDisponibles.Count);
                Transform puntoElegido = posicionesDisponibles[indiceAleatorio];

                // 4. Instanciamos la planta en la posición exacta del Transform elegido
                GameObject planta = Instantiate(plantaVeneno, puntoElegido.position, Quaternion.identity);

                // Configuramos su daño
                var scriptPlanta = planta.GetComponent<PlantaVeneno>();
                if (scriptPlanta != null) scriptPlanta.setDanio(danioPlantaVeneno, gameObject);

                // 5. Eliminamos esta posición de la lista temporal para que no se repita
                posicionesDisponibles.RemoveAt(indiceAleatorio);
            }
        }

        yield return new WaitForSeconds(0.9f);

        estadoActual = EstadoMapinguari.Acechando;
    }

    #endregion

    #region Fase de Furia y Especial

    private IEnumerator EntrarEnFuria()
    {
        enFuria = true;
        estadoActual = EstadoMapinguari.TransicionFuria;
        ManejarAudioCaminar(false);
        rb.linearVelocity = Vector2.zero;

        // Rugido Masivo
        ReproducirAudio(audioScream);
        anim.SetBool("AT", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("AT", false);

        yield return new WaitForSeconds(0.3f);
        // Knockback Global al jugador
        if (scriptJugador != null)
        {
            Vector2 direccionKnockback = (playerTransform.position - transform.position).normalized;
            scriptJugador.GetComponent<Rigidbody2D>().AddForce(direccionKnockback * 250f, ForceMode2D.Impulse);
            // Aquí puedes llamar a un método de Sinchi para aturdirlo temporalmente si lo deseas
        }

        yield return new WaitForSeconds(1f);

        // Mejoras de Furia
        movementVelocity = 12f;
        danioPlantaVeneno = 100f;

        StartCoroutine(AsedioRodante());
    }

    private IEnumerator AsedioRodante()
    {
        estadoActual = EstadoMapinguari.AsedioRodante;
        ataque = valorAtaqueEspecial;

        // 1. Preparación y Teletransporte inicial
        anim.SetBool("AE", true); // Inicia animación de bola
        Instantiate(humo, transform.position, Quaternion.identity); // Humo al desaparecer

        // Teletransporte a la posición de inicio configurada
        transform.position = embestidaPos.position;
        Instantiate(humo, transform.position, Quaternion.identity); // Humo al aparecer

        yield return new WaitForSeconds(0.5f); // Pequeña pausa para que el jugador reaccione

        int choquesTotales = 0;
        float direccionX = -1f; // Empezamos hacia la izquierda según tu petición
        float velocidadEmbestida = 30f;

        // 2. Bucle de movimiento basado en choques
        while (choquesTotales < 4)
        {
            // Aplicamos la velocidad constantemente
            rb.linearVelocity = new Vector2(direccionX * velocidadEmbestida, rb.linearVelocity.y);

            // Girar el sprite según la dirección
            transform.localScale = new Vector3(direccionX * -1, 1, 1);

            // Verificamos si el wallDetector está tocando algo
            if (DetectarPared())
            {
                choquesTotales++;
                direccionX *= -1f; // Invertimos la dirección (Rebote)
                yield return new WaitForSeconds(0.5f);

                // Pequeño Shake de cámara en cada choque para dar impacto
                if (CameraShakeManager.Instance != null)
                {
                    CameraShakeManager.Instance.ShakeDanio();
                    shockAudio.Play();
                }

                // Esperamos un frame para evitar múltiples detecciones en el mismo choque
                yield return new WaitForFixedUpdate();

                // Si ya llegamos al 4to choque, salimos del bucle
                if (choquesTotales >= 4) break;
            }

            yield return null; // Esperar al siguiente frame
        }

        // 3. Finalización del ataque
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("AE", false);
        ataque = valorAtaqueBasico;

        // Queda aturdido tras el esfuerzo
        anim.SetBool("Iddel", true);
        yield return new WaitForSeconds(3f);
        anim.SetBool("Iddel", false);

        estadoActual = EstadoMapinguari.Acechando;
    }

    // Función auxiliar para detectar la pared mediante el wallDetector
    private bool DetectarPared()
    {
        // Usamos un CircleCast o Linecast en la posición del objeto wallDetector
        // Ajusta el radio (0.5f) y las capas según tu proyecto
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 2f, wallLayer);
        return hit != null;
    }

    #endregion

    #region Daño, Interacciones y Muerte

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        // Recibir daño del arma del jugador
        if (collider.gameObject.layer == 14)
        {
            int direccion = -(int)Mathf.Sign(collider.transform.position.x - transform.position.x);
            StartCoroutine(cooldownRecibirDanio(direccion, 1));

            if (collider.transform.parent != null)
            {
                var arma = collider.transform.parent.parent.GetComponent<Hoyustus>();
                if (arma != null)
                {
                    arma.cargaLanza();
                    RecibirDanio(arma.getAtaque());

                    if (!hurtSound) StartCoroutine(ReproducirSonidoDanioAleatorio());
                }
            }
        }
        // Elementos (Fuego/Viento) se mantienen igual que en tu base
    }

    protected override void Recoil(int direccion, float fuerzaRecoilMod)
    {
        // En jefes masivos, el recoil no suele empujarlos tanto, pero mantenemos tu lógica
        playable = false;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direccion * 10f, rb.gravityScale * 4f), ForceMode2D.Impulse);
    }

    private IEnumerator ReproducirSonidoDanioAleatorio()
    {
        hurtSound = true;
        hurtAudio.Stop(); hurtAudio.Play();

        System.Random rnd = new System.Random();
        int randomIndex = rnd.Next(0, 3);

        hurtAudio2.Stop(); hurtAudio3.Stop(); hurtAudio4.Stop();

        if (randomIndex == 0) hurtAudio2.Play();
        else if (randomIndex == 1) hurtAudio3.Play();
        else hurtAudio4.Play();

        yield return new WaitForSeconds(0.4f);
        hurtSound = false;
    }

    private IEnumerator RutinaMuerte()
    {
        if (scriptJugador != null) scriptJugador.QuitarParalisis();
        CameraShakeManager.Instance.ShakeMuerteEnemigo();
        GetComponent<AudioSource>().Stop();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<CapsuleCollider2D>().enabled = false;
        ataqueCuerpo.enabled = false;

        anim.SetBool("Muerto", true);
        if (levelController != null) levelController.EliminarLogicaPlataformas();

        StartCoroutine(ShowVictoryScreen());

        yield return new WaitForSeconds(5f);

        if (gotg != null) Instantiate(gotg, transform.position, Quaternion.identity);
        Destroy(this); // Destruye el script, mantiene el cadáver
    }

    IEnumerator ShowVictoryScreen()
    {
        // ... (Tu lógica de pantalla de victoria intacta) ...
        GameObject hudMenu = GameObject.Find("HUDMenu");
        if (hudMenu != null) hudMenu.GetComponent<HudManager>().SetVibrationBossDeath();

        victorySound.Stop(); 
        victorySound.Play();

        /*GameObject bossSilhouette = Instantiate(bossSilhouettePrefab, transform.position, Quaternion.identity);
        bossSilhouette.GetComponent<SpriteRenderer>().material = silhouetteMaterial;
        bossSilhouette.transform.position = new Vector3(transform.position.x, transform.position.y, -1);*/

        yield return new WaitForSecondsRealtime(1f);

        //Destroy(bossSilhouette);
    }

    #endregion

    #region Utilidades

    private void ActualizarVida()
    {
        if (lifeBar != null) lifeBar.targetFillAmount = (vida / vidaMax);
    }

    private void ReproducirAudio(AudioClip clip)
    {
        charAudio.loop = false;
        charAudio.Stop();
        charAudio.clip = clip;
        charAudio.Play();
    }

    private void ManejarAudioCaminar(bool caminando)
    {
        if (caminando)
        {
            if (charAudio.clip != audioWalk || !charAudio.isPlaying)
            {
                charAudio.clip = audioWalk;
                charAudio.loop = true;
                charAudio.Play();
            }
        }
        else if (charAudio.clip == audioWalk)
        {
            charAudio.Stop();
        }
    }

    // Evento de animación llamado desde Unity
    public void ActivacionGarras(int estado)
    {
        ataqueCuerpo.enabled = (estado != 0);
    }

    #endregion
}