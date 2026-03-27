using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static System.Random;
using UnityEngine.SceneManagement;

public class Mapianguari : Enemy
{
    [SerializeField] private bool persiguiendo = true, atacando = false, cambiandoPlataforma;
    [SerializeField] private float t1;
    [SerializeField] private float t2;

    [SerializeField] private float tiempoDentroRango, tiempoFueraRango;
    [SerializeField] public float minX, maxX;
    [SerializeField] private float xObjetivo;
    [SerializeField] private bool ataqueDisponible = true;
    [SerializeField] private BoxCollider2D ataqueCuerpo, campoVision;
    [SerializeField] private CapsuleCollider2D cuerpo;
    [SerializeField] private float movementVelocity = 6;
    [SerializeField] private float valorAtaqueBasico;
    [SerializeField] private float valorAtaqueEspecial;
    [SerializeField] private float coolDownAtaque;
    private bool segundaEtapa = false;
    public Vector3 positionPlat = new Vector3();

    [SerializeField] private GameObject bolaVeneno;
    [SerializeField] public int nuevaPlataforma;
    [SerializeField] public int plataformaActual;
    [SerializeField] private GameObject charcoVeneno;
    [SerializeField] private GameObject plantaVeneno;
    [SerializeField] private GameObject humo;
    [SerializeField] private GameObject gotg;
    [SerializeField] private GameObject explosion2;
    [SerializeField] private ManagerPeleaMapinguari levelController;
    [SerializeField] private bool usandoAtaqueEspecial = false;
    [SerializeField] private bool realizandoAB, realizandoAT, pruebaAtaqueEspecial, isDead, iddel;
    [SerializeField] private bool ataqueEspecialDisponible = true;
    [SerializeField] private LiquidBar lifeBar;
    [SerializeField] AudioClip audioHurt;
    [SerializeField] AudioClip audioAtk;
    [SerializeField] AudioClip audioScream;
    [SerializeField] AudioClip audioWalk;
    [SerializeField] private float danioPlantaVeneno = 0f;
    [SerializeField] private float tiempoAnimacionAtaqueNormal = 0.4f;
    [SerializeField] private Sprite spriteStatic;
    System.Random triggerProbabilidad = new System.Random();

    AudioSource charAudio;
    [SerializeField] private float rangoCercania = 15f;
    [SerializeField] private float reduccionTiempoAtaqueDistancia = 0;

    private int totalPlants = 0;
    [SerializeField] private AudioSource hurtAudio;
    [SerializeField] private AudioSource hurtAudio2;
    [SerializeField] private AudioSource hurtAudio3;
    [SerializeField] private AudioSource hurtAudio4;

    private System.Random random = new System.Random();
    private bool hurtSound = false;

    //variables para el manejo de la pantalla de victoria

    [SerializeField] private GameObject boss;  // Referencia al jefe
    [SerializeField] private GameObject player;  // Referencia al jugador
    [SerializeField] private Material silhouetteMaterial;  // Material de silueta (negro)
    [SerializeField] private AudioSource victorySound;  // Clip de audio para la victoria
    [SerializeField] private GameObject quadPrefab;  // Prefab del Quad negro que actuar� como fondo
    private GameObject bossSilhouette, playerSilhouette, blackBackground;


    void Start()
    {
        anim = GetComponent<Animator>();
        rangoPreparacion = 6f;
        fuerzaRecoil = 4f;
        plataformaActual = 0;
        nuevaPlataforma = 0;

        charAudio = GetComponent<AudioSource>();

        //INICIALIZACION VARIABLES
        explosionInvulnerable = "ExplosionEnemy";
        vidaMax = vida;
        ataqueMax = valorAtaqueBasico;
        ataque = ataqueMax;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        counterEstados = 0;
        layerObject = transform.gameObject.layer;
        cuerpo = transform.GetComponent<CapsuleCollider2D>();
        campoVision = transform.GetChild(0).GetComponent<BoxCollider2D>();
        ataqueCuerpo = transform.GetChild(1).GetComponent<BoxCollider2D>();
        ataqueCuerpo.enabled = false;
        layerObject = transform.gameObject.layer;
        //SE DESACTIVAN LAS COLISIONES DEL CUERPO DEL BOSS CON EL DASHBODY DE HOYUSTUS Y SU CUERPO ESTANDAR
        //Physics2D.IgnoreCollision(cuerpo, GameObject.Find("Hoyustus Solicitud Prefab").GetComponent<CapsuleCollider2D>());
        explosion = Resources.Load<GameObject>("Explosion");
        ataqueDisponible = true;
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
            rangoCercania = 17;
            danioPlantaVeneno = 100;
        }
        if (vida <= 0 && !isDead) {
            isDead = true;
            GameObject.FindObjectOfType<Hoyustus>().QuitarParalisis();
            StopAllCoroutines();
            
            StartCoroutine(Muerte());
        }
    }

    IEnumerator ShowVictoryScreen()
    {

        GameObject.Find("HUDMenu").GetComponent<HudManager>().SetVibrationBossDeath();

        victorySound.Stop();
        victorySound.Play();

        // 1. Desactivar los personajes originales
        //boss.SetActive(false);
        //player.SetActive(false);

        // 2. Crear las siluetas de los personajes
        Vector3 bossPosition = boss.transform.position;
        Vector3 playerPosition = player.transform.position;

        bossSilhouette = Instantiate(boss, bossPosition, Quaternion.identity);
        playerSilhouette = Instantiate(player, playerPosition, Quaternion.identity);

        // Asignar el material de silueta
        bossSilhouette.GetComponent<SpriteRenderer>().material = silhouetteMaterial;
        playerSilhouette.GetComponent<SpriteRenderer>().material = silhouetteMaterial;

        // 3. Crear el Quad negro como fondo detr�s de los personajes
        // Ajustar la posici�n del Quad en el eje Z (detr�s de los personajes)
        blackBackground = Instantiate(quadPrefab, new Vector3(0, 0, -1), Quaternion.identity);
        blackBackground.transform.localScale = new Vector3(500f, 300f, 0f);  // Escalar el Quad para cubrir la pantalla
        //blackBackground.GetComponent<MeshRenderer>().material.color = Color.black;

        // Colocar las siluetas en la misma posici�n X e Y, pero con Z ajustado a -1
        bossSilhouette.transform.position = new Vector3(bossPosition.x, bossPosition.y, -1);
        playerSilhouette.transform.position = new Vector3(playerPosition.x, playerPosition.y, -1);

        Time.timeScale = 1;  // Pausar el tiempo si lo hab�as pausado antes
        // 4. Pausa de retroalimentaci�n
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 0;  // Reanudar el tiempo si lo hab�as pausado antes
        yield return new WaitForSecondsRealtime(1f);
        blackBackground.transform.localScale = new Vector3(0f, 0f, 0f);
        Time.timeScale = 1;  // Pausar el tiempo si lo hab�as pausado antes
        // 5. Desactivar el fondo negro y las siluetas
        Destroy(bossSilhouette);
        Destroy(playerSilhouette);
        Destroy(blackBackground);

        bossSilhouette.SetActive(false);
        playerSilhouette.SetActive(false);
        blackBackground.SetActive(false);
        

        // 6. Continuar con el juego (transici�n o siguiente nivel)
        Time.timeScale = 1;  // Reanudar el tiempo si lo hab�as pausado antes
    }

    //***************************************************************************************************
    //CORRUTINA DE MUERTE
    //***************************************************************************************************
    private IEnumerator Muerte() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        


        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<CapsuleCollider2D>().enabled = false;
        anim.enabled = true;
        anim.SetBool("Muerto", true);
        campoVision.enabled = false;
        xObjetivo = transform.position.x;
        atacando = true;
        ataqueDisponible = false;
        levelController.EliminarLogicaPlataformas();
        GetComponent<SpriteRenderer>().material = playerMat;
        //TIEMPO ANIMACION DEL Boss

        StartCoroutine(ShowVictoryScreen());

        yield return new WaitForSeconds(5f);
        

        Instantiate(gotg, transform.position, Quaternion.identity);

        Destroy(GetComponent<Mapianguari>());
    }


    void Update()
    {
        anim.SetFloat("Vida", vida);
        anim.SetBool("AB", realizandoAB);
        anim.SetBool("AT", realizandoAT);
        anim.SetBool("AE", pruebaAtaqueEspecial);
        anim.SetBool("Iddel", iddel);

        if (!usandoAtaqueEspecial && nuevaPlataforma != plataformaActual && !isDead && !atacando) {
            StartCoroutine(CambioPlataforma());       
        }
        //MODIFICACION DE POSICION A SEGUIR AL PLAYER AL ESTAR EN LA MISMA PLATAFORMA
        if (!usandoAtaqueEspecial && xObjetivo >= minX && xObjetivo <= maxX && !atacando) {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(xObjetivo, transform.position.y, transform.position.z)/*Vector3.right * xObjetivo*/, movementVelocity * (1 - afectacionViento) * Time.deltaTime);
            if(charAudio.clip != audioWalk)
            {
                charAudio.clip = audioWalk;
                charAudio.loop = true;
                charAudio.Play();
            }
            else if (!charAudio.isPlaying)
            {
                charAudio.loop = true;
                charAudio.Play();
            }
        }
    }

    IEnumerator HurtSound()
    {
        hurtSound = true;

        hurtAudio.Stop();
        hurtAudio.Play();

        // Genera un n�mero aleatorio entre 0 y 1
        int randomIndex = random.Next(0, 2);  // System.Random genera 0 o 1

        // Detiene ambos audios antes de reproducir uno
        hurtAudio2.Stop();
        hurtAudio3.Stop();
        hurtAudio4.Stop();

        // Reproduce el audio aleatorio
        if (randomIndex == 0)
        {
            hurtAudio2.Play();
        }
        else if (randomIndex == 1)
        {
            hurtAudio3.Play();
        }
        else
        {
            hurtAudio4.Play();
        }

        yield return new WaitForSeconds(0.4f);

        hurtSound = false;
    }

    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 10, rb.gravityScale * 4), ForceMode2D.Impulse);
    }


    //***************************************************************************************************
    //DETECCION DE TRIGGERS
    //***************************************************************************************************
    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        if (collider.gameObject.layer == 14) {

            int direccion = -(int)OrientacionDeteccionPlayer(collider.transform.position.x);

            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            if (collider.transform.parent != null)
            {
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                RecibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());

                if (!hurtSound)
                {
                    StartCoroutine(HurtSound());
                }
            }
        }
        //DETECCIONS DE TRIGGERS DE OBJETOS CON LAYER EXPLOSION O ARMA_PLAYER

        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO VIENTO
        if (collider.gameObject.CompareTag("Viento") && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoViento)
            {
                StopCoroutine("afectacionEstadoViento");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                CombinacionesElementales();
                return;

            }
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");
            if(segundaEtapa && triggerProbabilidad.Next(0, 2) == 0 && vida >= 250)
            {
                StartCoroutine(AtaqueEspecial());
            }
        }
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
        else if (collider.gameObject.CompareTag("Fuego") && !collider.gameObject.name.Contains("Enemy"))
        {
            if (estadoFuego)
            {
                StopCoroutine("afectacionEstadoFuego");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                CombinacionesElementales();
                return;

            }
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
            if (segundaEtapa && triggerProbabilidad.Next(0, 2) == 0 && vida >= 250)
            {
                StartCoroutine(AtaqueEspecial());
            }
        }
    }


    //***************************************************************************************************
    //COMBINACIONES ELEMENTALES
    //***************************************************************************************************
    private void CombinacionesElementales()
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
            StopCoroutine("afectacionEstadoViento");
            StopCoroutine("afectacionEstadoFuego");
            StartCoroutine("afectacionEstadoFuego");
        }
    }


    private void OnTriggerStay2D(Collider2D collider){

        //SE EJECUTA SOLO SI MAPINGUARI NO SE ENCUENTRA REALIZANDO EL ATAQUE ESPECIAL
        if (!usandoAtaqueEspecial && collider.gameObject.CompareTag("Player") && ataqueDisponible) 
        {
            xObjetivo = collider.transform.position.x;
            float distanciaPlayer = 0;

            if (!atacando) { 
                //CAMBIO DE ORIENTACION
                if (xObjetivo < transform.position.x)
                    transform.localScale = new Vector3(-1, 1, 1);
                else if (xObjetivo > transform.position.x)
                    transform.localScale = Vector3.one;

                distanciaPlayer = Mathf.Abs(transform.position.x - collider.transform.position.x);

                if (distanciaPlayer <= rangoCercania)
                {
                    tiempoFueraRango = 0;
                    tiempoDentroRango += Time.deltaTime;
                }
                else
                {
                    tiempoDentroRango = 0;

                    tiempoFueraRango += Time.deltaTime;
                }
            }

            if (distanciaPlayer <= rangoPreparacion && tiempoDentroRango > 0.75 && tiempoDentroRango <= 5)
                StartCoroutine(AtaqueCuerpoCuerpo());
            else if (distanciaPlayer <= rangoCercania && tiempoDentroRango > 5)
                StartCoroutine(AtaqueAturdimiento());
            else if (distanciaPlayer > rangoCercania && tiempoFueraRango >= 10 - reduccionTiempoAtaqueDistancia)
                StartCoroutine(AtaqueDistancia());
        }
    }


    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player")) {
            tiempoDentroRango = 0;
            tiempoFueraRango = 0;
        }
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE DE ATURDIMIENTO
    //***************************************************************************************************
    private IEnumerator AtaqueAturdimiento() {

        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        charAudio.loop = false;
        charAudio.Stop();
        charAudio.clip = audioScream;
        charAudio.Play();
        atacando = true;
        realizandoAT = true;
        ataqueDisponible = false;
        //TIEMPO PARA LA ANIMACION
        yield return new WaitForSeconds(0.5f);
        realizandoAT = false;
        Debug.Log("Preparando ataque inmovilizador");
        yield return new WaitForSeconds(0.4f);
        //GENERACION DEL CHARCO DE VENENO
        /*if (segundaEtapa) {
            GameObject charcoGenerado = Instantiate(charcoVeneno, transform.position + Vector3.down * 2.8f, Quaternion.identity);
            charcoGenerado.name = "CharcoVenenoEnemy";
            StartCoroutine(DestruirCharco(charcoGenerado));
        }*/

        //SE EVALUA SI HOYUSTUS ESTA EN EL RANGO DEL ATAQUE
        if (Mathf.Abs(transform.position.x - GameObject.FindObjectOfType<Hoyustus>().GetComponent<Transform>().position.x) <= 15) {

            StartCoroutine(AturdirPlayer());
            Debug.Log("Te inmovilizo");
            yield return new WaitForSeconds(0.1f);
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
    private IEnumerator AturdirPlayer() {
        GameObject.FindObjectOfType<Hoyustus>().SetParalisis();
        tiempoDentroRango = 0;
        yield return new WaitForSeconds(3f);
        GameObject.FindObjectOfType<Hoyustus>().QuitarParalisis();
    }


    //***************************************************************************************************
    //CORRUTINA DE DESTRUCCION DE CHARCO DE VENENO
    //***************************************************************************************************
    private IEnumerator DestruirCharco(GameObject charco) {
        charco.SetActive(true);
        yield return new WaitForSeconds(4f);
        Destroy(charco);
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE CUERPO A CUERPO
    //***************************************************************************************************
    private IEnumerator AtaqueCuerpoCuerpo(){
        //SE MODIFICAN ESTAS VARIABLES PARA NO INTERFERIR EL TIEMPO DE ACCION DE LA CORRUTINA
        charAudio.Stop();
        charAudio.clip = audioAtk;

        ataqueDisponible = false;
        atacando = true;
        iddel = true;
        anim.Play("Iddel Mapinguari");
        //PREPARACION DEL ATAQUE
        yield return new WaitForSeconds(t1);
        iddel = false;
        anim.Play("Ataque Normal Mapinguari", -1, 0);
        realizandoAB = true;
        yield return new WaitUntil(() => iddel);
        charAudio.Play();
        //yield return new WaitForSecondsRealtime(tiempoAnimacionAtaqueNormal);
        //DASH TRAS ATAQUE EN LA SEGUNDA ETAPA
        //iddel = true;
        realizandoAB = false;
        if (segundaEtapa && !((transform.position.x < minX + 3 && transform.localScale.x > 1) || (transform.position.x > maxX - 3 && transform.localScale.x < 1))) {
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(12f * -transform.localScale.x, 0f);
            yield return new WaitForSeconds(0.25f);
            rb.gravityScale = 5;
            rb.linearVelocity = Vector2.zero;
        }
        //DETENIMIENTO TRAS ATAQUE
        yield return new WaitForSeconds(t2);
        atacando = false;
        iddel = false;

        yield return new WaitForSeconds(0.1f);
        ataqueDisponible = true;

        if (triggerProbabilidad.Next(0, 4) == 0)
        {
            StartCoroutine(AtaqueDistancia());
        }
    }

    private void setIddel() {
        iddel = true;
    }


    //***************************************************************************************************
    //CORRUTINA DE ATAQUE A DISTANCIA
    //***************************************************************************************************
    private IEnumerator AtaqueDistancia() {
        atacando = true;
        ataqueDisponible = false;
        realizandoAT = true;

        charAudio.loop = false;
        charAudio.Stop();
        charAudio.clip = audioScream;
        charAudio.Play();

        yield return new WaitForSeconds(0.1f);
        realizandoAT = false;
        //TIEMPO ANIMACION
        yield return new WaitForSeconds(0.7f);
        if (segundaEtapa)
        {
            for (int i = 0; i < 3; i++) {
                System.Random aux = new System.Random();

                if (totalPlants >= 2) break;
                else totalPlants++;

                switch (aux.Next(0, 4))
                {
                    case 0:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -7), -98, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 1:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -10), -92, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 2:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-59, -31), -84, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 3:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -10), -76, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                }
            }
        }
        else {

            if (totalPlants <= 1)
            {
                totalPlants++;
                System.Random aux = new System.Random();
                switch (aux.Next(0, 4))
                {
                    case 0:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -7), -98, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 1:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -10), -92, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 2:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-59, -31), -84, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                    case 3:
                        Instantiate(plantaVeneno, new Vector3(aux.Next(-38, -10), -76, 0), Quaternion.identity).GetComponent<PlantaVeneno>().setDanio(danioPlantaVeneno, gameObject);
                        break;
                }
            }   
        }
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(0.5f);
        atacando = false;
        ataqueDisponible = true;
        tiempoDentroRango = 0f;
        tiempoFueraRango = 0f;
    }

    public void PlantDestroy()
    {
        totalPlants--;
        if (totalPlants < 0) totalPlants = 0;
    }

    //***************************************************************************************************
    //CORRUTINA DE ATAQUE ESPECIAL
    //***************************************************************************************************
    private IEnumerator AtaqueEspecial() {
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
            triggerProbabilidad = new System.Random();
            GameObject bolaVenenoGenerada = null;

            //EL VALOR CORRESPONDE A LA PLATAFORMA EN LA QUE SE GENERARA
            switch (triggerProbabilidad.Next(0, 4))
            {
                case 0:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(triggerProbabilidad.Next(-38, -7), triggerProbabilidad.Next(-99, -96), 0), Quaternion.identity);
                    break;
                case 1:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(triggerProbabilidad.Next(-38, -10), triggerProbabilidad.Next(-91, -88), 0), Quaternion.identity);
                    break;
                case 2:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(triggerProbabilidad.Next(-59, -31), triggerProbabilidad.Next(-83, -80), 0), Quaternion.identity);
                    break;
                case 3:
                    bolaVenenoGenerada = Instantiate(bolaVeneno, new Vector3(triggerProbabilidad.Next(-38, -10), triggerProbabilidad.Next(-75, -72), 0), Quaternion.identity);
                    break;
            }
            bolaVenenoGenerada.name += "Enemy";
            bolaVenenoGenerada.AddComponent<BolaVenenoArbolMapinguari>();
            yield return new WaitForEndOfFrame();
            bolaVenenoGenerada.GetComponent<BolaVenenoArbolMapinguari>().InstanciarValores(explosion, explosion2);
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(0.3f);
        pruebaAtaqueEspecial = true;
        //EMBESTIDAS
        transform.localScale = new Vector3(-1, 1, 1);
        for (int i = 1; i <= 3; i++) {
            //DESPLAZAMIENTO A LA PLATAFORMA
            switch (nuevaPlataforma) {
                case 0:
                    transform.position = new Vector3(-14, -99.5f, 0);
                    break;
                case 1:
                    transform.position = new Vector3(-14, -91.2f, 0);
                    break;
                case 2:
                    transform.position = new Vector3(-30, -83f, 0);
                    break;
                case 3:
                    transform.position = new Vector3(-14, -74.7f, 0);
                    break;
            }
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

            //MOVIMIENTO DE EXTREMO A EXTREMO
            this.rb.linearVelocity = new Vector2(-35f, 0f);
            float extraDashTime = 0f;
            if (nuevaPlataforma == 0) {
                extraDashTime += 0.4f;
            }
            yield return new WaitForSeconds(1f + extraDashTime);

            //DESAPARICION TRAS EMBESTIDA
            rb.linearVelocity = Vector2.zero;
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
        ataque = valorAtaqueBasico;
        ataqueMax = ataque;
        pruebaAtaqueEspecial = false;
        iddel = true;
        transform.position = new Vector3(-42.82f, -98.99f, 0);
        //STUN
        yield return new WaitForSeconds(5f);
        iddel = false;
        //RETORNO A VALORES DE JUEGO NORMAL
        usandoAtaqueEspecial = false;
        campoVision.enabled = true;
    }


    //***************************************************************************************************
    //CORRUTINA DE CAMBIO DE PLATAFORMA
    //***************************************************************************************************
    private IEnumerator CambioPlataforma() {
        //SE ESCONDE

        charAudio.Stop();
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = false;
        usandoAtaqueEspecial = true;
        anim.enabled = false;
        Destroy(Instantiate(humo, transform.position, Quaternion.identity), 1);
        //GetComponent<SpriteRenderer>().sortingOrder = -6;
        yield return new WaitForSeconds(0.5f);
        //SE DESPLAZA 
        plataformaActual = nuevaPlataforma;
        triggerProbabilidad = new System.Random();
        int posicionTeletransporteX = triggerProbabilidad.Next((int)minX, (int)maxX) + 1;
        transform.position = positionPlat;
        Destroy(Instantiate(humo, transform.position, Quaternion.identity), 1);
        yield return new WaitForSeconds(0.5f);
        //REAPARECE "SALE DE LOS ARBOLES"
        //GetComponent<SpriteRenderer>().sortingOrder = 8;
        anim.enabled = true;
        //SE REACTIVA SU MOVIMIENTO
        tiempoDentroRango = 0;
        tiempoFueraRango = 0;
        ataqueDisponible = true;
        usandoAtaqueEspecial = false;
    }


    //***************************************************************************************************
    //DETECCION DE COLISIONES
    //***************************************************************************************************
    private void OnCollisionEnter2D(Collision2D collision)
    {
        try
        {
            //AL TOCAR UNA PLATAFORMA SE ESTABLECEN SUS LIMITES DE MOVIMIENTO EN X
            if ((collision.gameObject.layer == 6 || collision.gameObject.layer == 17) && collision.gameObject.name.StartsWith("Plataforma"))
            {
                minX = collision.gameObject.GetComponent<PlataformaMapinguari>().minX;
                maxX = collision.gameObject.GetComponent<PlataformaMapinguari>().maxX;
                positionPlat = collision.gameObject.GetComponent<PlataformaMapinguari>().position;
                plataformaActual = collision.gameObject.GetComponent<PlataformaMapinguari>().plataforma;
            }
        }
        catch (Exception) { }

    }


    public void ActivacionGarras(int estado) {
        if (estado == 0)
            ataqueCuerpo.enabled = false;
        else
            ataqueCuerpo.enabled = true;
    }

}
