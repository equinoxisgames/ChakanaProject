using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boraro : Enemy
{
    [SerializeField] private float tiempoVolteo;
    [SerializeField] private float maxTiempoVolteo;
    [SerializeField] private float t1;
    [SerializeField] private float t2;
    [SerializeField] private float enfriamientoAtaque; 
    [SerializeField] private float tiempoAtaque;
    [SerializeField] private float fuerzaDesplazamientoAtaque;
    [SerializeField] private float enfriamientoCombo;
    [SerializeField] private float direction;
    [SerializeField] private bool siguiendo;
    [SerializeField] private bool playerDetectado;
    [SerializeField] private bool atacando;
    [SerializeField] private bool visible = true;
    [SerializeField] private bool ataqueDisponible = true;
    [SerializeField] private bool entroRangoAtaque;
    [SerializeField] private GameObject garras;
    [SerializeField] private GameObject detectorPared;
    [SerializeField] private GameObject detectorPiso;
    [SerializeField] private CapsuleCollider2D cuerpo;
    [SerializeField] private GameObject hoyustus;
    [SerializeField] private LayerMask pared;
    [SerializeField] private bool teletransportandose;
    [SerializeField] private List<GameObject> componentesBoraro = new List<GameObject>();
    [SerializeField] private Transform objetivoX;
    [SerializeField] AudioClip audioHurt;
    [SerializeField] GameObject goldObj;
    [SerializeField] GameObject disapearVFX;

    AudioSource charAudio;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        charAudio = GetComponent<AudioSource>();
    }

    void Start()
    {
        vidaMax = vida;
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = transform.gameObject.layer;
        direction = transform.localScale.x;
        explosion = Resources.Load<GameObject>("Explosion");
        detectorPared = transform.GetChild(transform.childCount - 3).GameObject();
        garras = transform.GetChild(transform.childCount - 2).GameObject();
        hoyustus = GameObject.FindGameObjectWithTag("Player");
        objetivo = transform.position;
        rangoVision += 1;
        rangoPreparacion += 1;
        rangoAtaque += 1;
        cuerpo = GetComponent<CapsuleCollider2D>();

        for (int i = 0; i < transform.childCount; i++)
        {
            componentesBoraro.Add(transform.GetChild(i).gameObject);
        }

    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 5.5f, rb.gravityScale * 2), ForceMode2D.Impulse);
    }


    private void Muerte()
    {
        if (vida <= 0)
        {
            Instantiate(deathFX, transform.position, Quaternion.identity);

            Instantiate(goldObj, transform.position, Quaternion.identity);

            Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, 3);

            foreach (Collider2D collider in objetos)
            {
                Rigidbody2D rb2D = collider.GetComponent<Rigidbody2D>();
                if (rb2D != null)
                {
                    Vector2 direccion = collider.transform.position - transform.position;
                    float distancia = 1 + direccion.magnitude;
                    float fuerza = 200 / distancia;
                    rb2D.AddForce(direccion * fuerza);
                }
            }

            Destroy(this.gameObject);
        }
    }


    private void FixedUpdate()
    {
        if (!atacando) {
            detectorPared.transform.position = hoyustus.transform.position - Vector3.right * hoyustus.transform.localScale.x * 2f;
            objetivo = hoyustus.transform.position;
            detectorPiso.transform.position = detectorPared.transform.position + Vector3.down * 1f;
        }

    }


    void Update()
    {
        anim.SetBool("Atacando", atacando);
        anim.SetBool("Siguiendo", siguiendo);
        anim.SetBool("Playable", playable);
        anim.SetBool("Dentro Rango", entroRangoAtaque);
        comportamiento();
        tiempoVolteo += Time.deltaTime;
        Muerte();

        if (!atacando)
        {
            if(!siguiendo && maxTiempoVolteo < tiempoVolteo && (!entroRangoAtaque || ataqueDisponible))
                Flip();
            if (detectarPiso())
            {
                if (siguiendo && !teletransportandose && playable && !entroRangoAtaque)
                    Move();
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (hoyustus.transform.position.x < transform.position.x)
                    transform.localScale = new Vector3(-1, 1, 1);
                else if (hoyustus.transform.position.x > transform.position.x)
                        transform.localScale = Vector3.one;
            }
        }
    }

    
    void Flip() {
        if (transform.position.x <= objetivo.x)
            direction = -direction;
        else if(transform.position.x > objetivo.x)
            direction = -direction;

        transform.localScale = new Vector3(direction, 1, 1);
        tiempoVolteo = 0;
    }

    public bool detectarPiso()
    {
        if (!Physics2D.OverlapCircle(new Vector3(1.8f * transform.localScale.x, -2.75f, 0) + transform.position, 0.2f, groundLayer))
        {
            return false;
        }
        return true;
    }


    void Move()
    {
        rb.velocity = new Vector2(direction * speed * (1 - afectacionViento), rb.velocity.y);

        if (transform.position.x <= objetivo.x)
        {
            direction = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            direction = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }


    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14)
        {

            int direccion = -(int)OrientacionDeteccionPlayer(collider.transform.position.x);

            TriggerElementos_1_1_1(collider);
            if (collider.transform.parent != null)
            {
                StartCoroutine(cooldownRecibirDanio(direccion, 1));
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                RecibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());
                charAudio.loop = false;
                charAudio.Stop();
                charAudio.clip = audioHurt;
                charAudio.Play();
            }
            return;
        }
        else if (collider.gameObject.layer == 11 && collider.gameObject.CompareTag("Untagged"))
        {
            rb.velocity = Vector2.zero;
            if (objetivo.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);
            return;
        }

        if (!collider.name.Contains("Enemy") && collider.gameObject.layer != 3 && collider.gameObject.layer != 18 && collider.gameObject.layer != 19)
        {
            TriggerElementos_1_1_1(collider);
        }
    }


    void comportamiento() {

        objetivo = hoyustus.transform.position;

        if (ataqueDisponible)
        {
            float playerDistance = Vector3.Distance(transform.position, hoyustus.transform.position);
            if (entroRangoAtaque)
            {
                ataqueDisponible = false;
                StartCoroutine(Teletransportacion());
            }
            else if (playerDistance <= rangoPreparacion && !entroRangoAtaque)
            {
                ataqueDisponible = false;
                entroRangoAtaque = true;
                StartCoroutine(Ataque());
            }
            else if (playerDistance <= rangoVision)
            {
                siguiendo = true;
                playerDetectado = true;
            }
            else if (playerDistance > rangoVision + 1.5f && playerDetectado)
            {
                ataqueDisponible = false;
                siguiendo = false;
                StartCoroutine(Teletransportacion());
            }          
        }
    }


    private IEnumerator Teletransportacion() {

        if (visible)
        {
            Desaparecer();
            //TIEMPO DE DESAPARICIÓN/TELETRANSPORTACIÓN
            yield return new WaitForSeconds(1);
        }

        detectorPared.transform.position = hoyustus.transform.position - Vector3.right * hoyustus.transform.localScale.x * 2.5f;
        objetivo = hoyustus.transform.position;
        detectorPiso.transform.position = detectorPared.transform.position + Vector3.down * 1f;
        if (!Physics2D.OverlapArea(detectorPared.transform.position + Vector3.left + Vector3.up,
            detectorPared.transform.position + Vector3.right + Vector3.down, pared) &&
            Physics2D.OverlapCircle(detectorPiso.transform.position, 1f, groundLayer))
        {

            //CAMBIO LA ORIENTACIÓN
            //ANALIZO LA ORIENTACIÓN
            float aux = hoyustus.transform.localScale.x;
            transform.position = objetivo - Vector3.right * aux;
            Aparecer();
            if (hoyustus.transform.position.x < transform.position.x)
            {
                direction = -1;
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                direction = 1;
                transform.localScale = new Vector3(1, 1, 1);
            }

            StartCoroutine(Ataque());
        }
        else { 
            yield return new WaitForSeconds(0.3f);
            ataqueDisponible = true;
        }
    }

    private void Desaparecer()
    {
        Instantiate(disapearVFX, transform.position, Quaternion.identity);

        visible = false;
        teletransportandose = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        cuerpo.enabled = false;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        foreach(GameObject g in componentesBoraro) {
            g.SetActive(false);
        }
    }

    private void Aparecer()
    {
        visible = true;
        teletransportandose = false;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        foreach (GameObject g in componentesBoraro)
        {
            g.SetActive(true);
        }
        rb.isKinematic = false;
        cuerpo.enabled = true;
        objetivo = detectorPared.transform.position = hoyustus.transform.position;
        detectorPiso.transform.position = objetivo - Vector3.down * 1f;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.name.Contains("Enemy"))
            CollisionElementos_1_1_1(collision);
    }


    private IEnumerator Ataque() {
        //TIEMPO DE PREPARACION
        playable = false;
        atacando = true;
        entroRangoAtaque = true;
        yield return new WaitForSeconds(t1);
        //ATAQUE
        rb.velocity = new Vector2(0f, rb.velocity.y);
        detectorPiso.transform.position = transform.position + Vector3.down * 2 + Vector3.right * transform.localScale.x * 3f;
        for (int i = 0; i < 4; i++) {
            if (!Physics2D.OverlapCircle(detectorPiso.transform.position, 1f, groundLayer))
            {
                /*anim.Play("BoraroIdle");
                anim.speed = 1;*/
                rb.velocity = Vector3.zero;
                atacando = false;
                tiempoVolteo = 0;
                yield return new WaitForSeconds(t2);
                ataqueDisponible = true;
                yield break;
            }
            if (i % 2 == 0)
            {
                //anim.Play("Attack", -1, 0);
            }

            if (hoyustus.transform.position.x < transform.position.x)
            {
                direction = -1;
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                direction = 1;
                transform.localScale = new Vector3(1, 1, 1);
            }

            //anim.speed = 1;
            rb.AddForce(new Vector2(fuerzaDesplazamientoAtaque * direction, 0), ForceMode2D.Impulse);
            garras.SetActive(true);
            yield return new WaitForSeconds(tiempoAtaque);
            //anim.speed = 0;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            garras.SetActive(false);
            yield return new WaitForSeconds(enfriamientoAtaque);
        }
        //anim.Play("BoraroIdle");
        atacando = false;
        //anim.speed = 1;
        tiempoVolteo = 0;
        //TIEMPO POSTERIOR AL ATAQUE
        yield return new WaitForSeconds(t2);
        tiempoVolteo = 0;
        ataqueDisponible = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(detectorPared.transform.position, new Vector3(2, 4, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectorPiso.transform.position, 1f);
    }
}