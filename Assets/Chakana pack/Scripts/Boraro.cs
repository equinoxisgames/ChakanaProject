using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Boraro : CharactersBehaviour
{

    [SerializeField] private bool applyForce;
    [SerializeField] private Vector3 objetivo;
    //[SerializeField] private GameObject explosion;
    [SerializeField] private float tiempoVolteo;
    [SerializeField] private float maxTiempoVolteo;
    [SerializeField] private float direction;
    [SerializeField] private bool siguiendo;
    [SerializeField] private bool atacando;
    [SerializeField] private bool ataqueDisponible = true;
    [SerializeField] private bool entroRangoAtaque;
    [SerializeField] private GameObject garras;
    [SerializeField] private GameObject detectorPared;
    [SerializeField] private GameObject detectorPiso;
    [SerializeField] private GameObject campoVision;
    [SerializeField] private GameObject hoyustus;
    [SerializeField] private LayerMask pared;
    [SerializeField] private LayerMask piso;
    [SerializeField] private bool teletransportandose;


    [SerializeField] private float movementSpeed = 3;
    [SerializeField] private float detectionRadius = 3;
    [SerializeField] private NavMeshAgent navMesh;
    [SerializeField] private float distancia;
    [SerializeField] private Transform objetivoX;



    [SerializeField] Animator anim;

    Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
        campoVision = transform.GetChild(transform.childCount - 1).GameObject();
        hoyustus = GameObject.FindGameObjectWithTag("Player");
        objetivo = transform.position;
        //player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 5.5f, rb.gravityScale * 2), ForceMode2D.Impulse);
        //EstablecerInvulnerabilidades(layerObject);
    }


    private void Muerte()
    {
        if (vida <= 0)
        {
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
        comportamiento();
        tiempoVolteo += Time.deltaTime;
        Muerte();

        if (maxTiempoVolteo < tiempoVolteo && !siguiendo && !atacando)
        {
            Flip();
        }

        if (siguiendo && !atacando && !teletransportandose && playable)
        {
            Move();
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


    void Move()
    {
        rb.velocity = new Vector2(direction * movementSpeed * (1 - afectacionViento), rb.velocity.y);

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
            int direccion = 1;
            if (collider.transform.position.x > gameObject.transform.position.x)
            {
                direccion = -1;
            }
            else
            {
                direccion = 1;
            }

            triggerElementos_1_1_1(collider);
            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            if (collider.transform.parent != null)
            {
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                recibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());
            }
            return;
        }
        else if (collider.gameObject.layer == 11)
        {
            rb.velocity = Vector2.zero;
            //objetivo = hoyustus.transform.position;
            if (objetivo.x < transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            return;
        }

        if (!collider.name.Contains("Enemy") && collider.gameObject.layer != 3 && collider.gameObject.layer != 18)
        {
            triggerElementos_1_1_1(collider);
        }
    }


    void comportamiento() {

        objetivo = hoyustus.transform.position;

        if (ataqueDisponible)
        {
            if (Vector3.Distance(transform.position, hoyustus.transform.position) <= 8f && !entroRangoAtaque)
            {
                ataqueDisponible = false;
                StartCoroutine(Teletransportacion());
            }
            else if (Vector3.Distance(transform.position, hoyustus.transform.position) <= 18f && !entroRangoAtaque)
            {
                siguiendo = true;
            }
            else if(entroRangoAtaque)
            {
                ataqueDisponible = false;
                StartCoroutine(Teletransportacion());
            }
            
        }
    }


    private IEnumerator Teletransportacion() {
                
        if (entroRangoAtaque)
        {
            //DESAPAREZCO
            void Desaparecer(){
                teletransportandose = true;
                campoVision.SetActive(false);
                rb.velocity = Vector3.zero;
                rb.Sleep();
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }

            void Aparecer() {
                teletransportandose = false;
                campoVision.SetActive(true);
                objetivo = detectorPared.transform.position = hoyustus.transform.position;
                detectorPiso.transform.position = objetivo - Vector3.down * 1f;
                rb.WakeUp();
                this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }

            Desaparecer();

            //TIEMPO DE DESAPARICIÓN/TELETRANSPORTACIÓN
            yield return new WaitForSeconds(1);

            detectorPared.transform.position = hoyustus.transform.position - Vector3.right * hoyustus.transform.localScale.x * 2.5f;
            objetivo = hoyustus.transform.position;
            detectorPiso.transform.position = detectorPared.transform.position + Vector3.down * 1f;
            if (!Physics2D.OverlapArea(detectorPared.transform.position + Vector3.left + Vector3.up, 
                detectorPared.transform.position + Vector3.right + Vector3.down, pared) &&
                Physics2D.OverlapCircle(detectorPiso.transform.position, 1f, piso))
            {

                //CAMBIO MI ORIENTACIÓN
                //ANALIZO LA ORIENTACIÓN
                float aux = hoyustus.transform.localScale.x;
                transform.position = objetivo - Vector3.right * aux;
                Aparecer();
                if (hoyustus.transform.position.x < transform.position.x)
                {
                    direction = -1;
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else {
                    direction = 1;
                    transform.localScale = new Vector3(1, 1, 1);
                }

                //ME MUEVO A ESE PUNTO
                //APAREZCO JUNTO AL JUGADOR 
                StartCoroutine(Ataque());
            }
            else {
                //APAREZCO EN LA MISMA POSICIÓN

                Aparecer();
                ataqueDisponible = false;
                yield return new WaitForSeconds(1.5f);
                ataqueDisponible = true;
            }
        }
        else
        {
            ataqueDisponible = false;
            entroRangoAtaque = true;
            StartCoroutine(Ataque());
        }
        yield return null;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.name.Contains("Enemy"))
        {
            collisionElementos_1_1_1(collision);
        }
    }


    private IEnumerator Ataque() {
        ataqueDisponible = false;
        atacando = true;

        rb.velocity = new Vector2(0f, rb.velocity.y);
        detectorPiso.transform.position = transform.position + Vector3.down * 2 + Vector3.right * transform.localScale.x * 3f;
        for (int i = 0; i < 4; i++) {
            if (!Physics2D.OverlapCircle(detectorPiso.transform.position, 1f, piso)) {
                //rb.velocity = Vector3.zero;
                //rb.AddForce(new Vector2(-transform.localScale.x * 2f, 0f));
                //objetivo = transform.position;
                i = 4;
                break;
            }
            rb.AddForce(new Vector2(6f * direction, 0), ForceMode2D.Impulse);
            garras.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            rb.velocity = new Vector2(0f, rb.velocity.y);
            garras.SetActive(false);
            yield return new WaitForSeconds(0.3f);
        }

        atacando = false;
        yield return new WaitForSeconds(2.5f);
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