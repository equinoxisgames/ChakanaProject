using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

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

    [SerializeField] private float movementSpeed = 3;
    [SerializeField] private float detectionRadius = 3;

    [SerializeField] Animator anim;

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
    }

    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 3.5f, rb.gravityScale * 2), ForceMode2D.Impulse);
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
        tiempoVolteo += Time.deltaTime;
        Muerte();

        if (maxTiempoVolteo < tiempoVolteo && !siguiendo)
        {
            Flip();
        }

        if (siguiendo)
        {
            Move();
        }
        else { 
            rb.velocity = Vector3.zero;
        }

    }


    void Flip() { 
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

            if (Vector3.Distance(transform.position, collider.transform.position) <= 2.5f)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }
            else if (Vector3.Distance(transform.position, collider.transform.position) <= 3.5f)
            {
                //ATAQUE
            }
            else
            {
                siguiendo = true;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            }
        }
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
}