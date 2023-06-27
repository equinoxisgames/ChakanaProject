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
    [SerializeField] private GameObject explosion;
    [SerializeField] private float tiempoVolteo;
    [SerializeField] private float maxTiempoVolteo;
    [SerializeField] private float direction;
    [SerializeField] private bool siguiendo;
    [SerializeField] private bool atacando;
    [SerializeField] private bool ataqueDisponible = true;
    [SerializeField] private bool entroRangoAtaque;
    [SerializeField] private GameObject garras;
    [SerializeField] private GameObject detectorPared;
    [SerializeField] private GameObject hoyustus;
    [SerializeField] private LayerMask pared;


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
        explosionInvulnerable = "ExplosionEnemy";
        layerObject = transform.gameObject.layer;
        direction = transform.localScale.x;
        explosion = Resources.Load<GameObject>("Explosion");
        detectorPared = transform.GetChild(transform.childCount - 3).GameObject();
        garras = transform.GetChild(transform.childCount - 2).GameObject();
        hoyustus = GameObject.FindGameObjectWithTag("Player");
        objetivo = transform.position;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        /*navMesh = GetComponent<NavMeshAgent>();
        navMesh.updateRotation = navMesh.updateUpAxis = false;
        objetivoX = transform;*/
        //StartCoroutine(SetDestinationDelayed());
    }

    /*private IEnumerator SetDestinationDelayed()
    {
        yield return new WaitUntil(() => navMesh.isActiveAndEnabled);
        navMesh.SetDestination(player.position);
    }*/

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


    void Update()
    {
        /*distancia = Vector3.Distance(transform.position, player.position);

        if (!atacando && distancia <= 2.5f)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
        else if (distancia <= 5.5f)
        {
            entroRangoAtaque = true;
            //if (ataqueDisponible)
            //StartCoroutine(Ataque());
        }
        else if (distancia <= 10f)
        {
            siguiendo = true;
            objetivoX = player.transform;
        }
        else if(distancia > 10f){
            if (!entroRangoAtaque)
            {
                objetivoX = this.gameObject.transform;
            }
            else {
                //DESAPARICION
                Vector3 aux = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
                objetivoX = player.transform;
                objetivoX.transform.position = new Vector3(aux.x, aux.y, aux.z);
                Debug.Log("Desparicion");
            }
        }


        navMesh.SetDestination(objetivoX.position);*/
        tiempoVolteo += Time.deltaTime;
        Muerte();

        if (maxTiempoVolteo < tiempoVolteo && !siguiendo && !atacando)
        {
            Flip();
            //direction = -direction;
            //transform.localScale = new Vector3(direction, 1, 1);
            //tiempoVolteo = 0;
        }

        if (siguiendo && !atacando)
        {
            Move();
        }
    }


    void Flip() {
        if (transform.position.x <= objetivo.x)
            direction = -direction;
        else if(transform.position.x > objetivo.x)
            direction = -direction;
        /*if (transform.position.x < player.position.x)
            direction = 1;
        else
            direction = -1;*/

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

            StartCoroutine(cooldownRecibirDanio(direccion, 1));
            if (collider.transform.parent != null)
            {
                collider.transform.parent.parent.GetComponent<Hoyustus>().cargaLanza();
                recibirDanio(collider.transform.parent.parent.GetComponent<Hoyustus>().getAtaque());
            }
        }
        else if (collider.gameObject.layer == 11)
        {
            rb.velocity = Vector2.zero;
        }

        if (!collider.name.Contains("Enemy"))
        {
            triggerElementos_1_1_1(collider);
        }
    }


    private void OnTriggerStay2D(Collider2D collider)
    {

        if (collider.gameObject.layer == 11)
        {
            //jugadorDetectado = true;
            objetivo = collider.transform.position;

            if (!atacando && Vector3.Distance(transform.position, collider.transform.position) <= 2.5f)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }
            else if (Vector3.Distance(transform.position, collider.transform.position) <= 8f && ataqueDisponible)
            {
                detectorPared.transform.position = hoyustus.transform.position;
                if (entroRangoAtaque && 
                !Physics2D.OverlapCircle(detectorPared.transform.position, 4f, pared))
                {
                    //DESAPARECER
                    //ANALIZAR LA ORIENTACION
                    Debug.Log("Pan");
                }
                else if (!entroRangoAtaque) {
                    entroRangoAtaque = true;
                    StartCoroutine(Ataque());
                }
            }
            else
            {
                siguiendo = true;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11 && !entroRangoAtaque)
        {
            rb.velocity = Vector3.zero;
            siguiendo = false;
            tiempoVolteo = 0;
        }
        else if (collision.gameObject.layer == 11 && entroRangoAtaque) { 
            //DESAPARECER
        }
    }


    private IEnumerator Ataque() {
        ataqueDisponible = false;
        atacando = true;

        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        for (int i = 0; i < 4; i++) {
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


    private IEnumerator combinacionesElementales()
    {
        if (counterEstados == 11)
        {
            //VIENTO - FUEGO
            estadoViento = false;
            afectacionViento = 0;
            counterEstados = 10;
            aumentoFuegoPotenciado = 3;
            ataque = ataqueMax * 0.75f;
            StopCoroutine("afectacionEstadoFuego");
            StartCoroutine("afectacionEstadoFuego");
        }
        else if (counterEstados == 101)
        {
            //VENENO - VIENTO
            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoViento");
            rb.velocity = Vector3.zero;
            counterEstados = 0;
            estadoVeneno = false;
            estadoViento = false;
            playable = false;
            aumentoDanioParalizacion = 1.5f;
            yield return new WaitForSeconds(2f);
            playable = true;
            aumentoDanioParalizacion = 1f;
            //StartCoroutine(setParalisis());

        }
        else if (counterEstados == 110)
        {
            //FUEGO - VENENO
            StopCoroutine("afectacionEstadoVeneno");
            StopCoroutine("afectacionEstadoFuego");
            counterEstados = 0;
            explosion.GetComponent<ExplosionBehaviour>().modificarValores(3, 45, 6, 12, "Untagged", "ExplosionPlayer");
            Instantiate(explosion, transform.position, Quaternion.identity);
            estadoVeneno = false;
            estadoFuego = false;
        }
        yield return new WaitForEndOfFrame();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectorPared.transform.position, 4);
    }
}