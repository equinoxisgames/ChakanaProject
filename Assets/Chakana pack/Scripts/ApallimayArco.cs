using StylizedWater2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApallimayArco : CharactersBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private Vector3 objetivo;
    [SerializeField] private float rangoAtaque;
    [SerializeField] private bool ataqueDisponible;
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject flecha;
    [SerializeField] private bool atacando;
    [SerializeField] private Vector3 limit1;
    [SerializeField] private Vector3 limit2;
    [SerializeField] private bool jugadorDetectado;
    [SerializeField] private float direction = 1;
    [SerializeField] private float detectionTime = 0;


    void Start()
    {
        layerObject = transform.gameObject.layer;
        fuerzaRecoil = 2f;
        ataqueDisponible = true;
        rb = GetComponent<Rigidbody2D>();
        explosion = Resources.Load<GameObject>("Explosion");
        flecha = Resources.Load<GameObject>("BolaVeneno");
        objetivo = limit2;
        limit1 = transform.GetChild(0).gameObject.transform.position;
        limit2 = transform.GetChild(1).gameObject.transform.position;
    }


    void Update()
    {
        Muerte();
        //Flip();

        if (transform.position.x < limit1.x || transform.position.x > limit2.x)
        {
            Flip();
        }

        if (!jugadorDetectado)
        {
            Move();
        }
    }

    private void Muerte()
    {
        if (vida <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void Move() {

        rb.velocity = new Vector2(direction * speed * (1 - afectacionViento), rb.velocity.y);

        if (transform.position.x <= limit1.x)
        {
            objetivo = limit2;
            //Flip();
        }
        else if (transform.position.x >= limit2.x)
        {
            objetivo = limit1;
            //Flip();
        }
    }

    private void Flip()
    {
        if (transform.position.x < objetivo.x)
        {
            direction = 1;
        }
        else if (transform.position.x > objetivo.x)
        {
            direction = -1;
        }
        transform.localScale = new Vector3(direction, 1, 0);
    }


    private IEnumerator Ataque(Vector3 objetivoAtaque) {
        //ataqueDisponible = false;
        GameObject flechaGenerada = Instantiate(flecha, transform.position, Quaternion.identity);
        flechaGenerada.SetActive(false);
        flechaGenerada.name += "Enemy";
        atacando = true;
        //playable = false;
        //rb.velocity = Vector2.zero;
        yield return new WaitForEndOfFrame();
        //CAMBIAR POR FLECHA
        flechaGenerada.AddComponent<BolaFuego>().instanciarValores(layerObject, objetivoAtaque);
        flechaGenerada.SetActive(true);

        //REVISAR SI ES IGUAL DE BUENO CON DOS DE ESTOS RETORNOS
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        flechaGenerada.GetComponent<BolaFuego>().aniadirFuerza();
        //TIEMPO DE ANIMACION
        yield return new WaitForSeconds(0.4f);
        atacando = false;
        //playable = true;
        //yield return new WaitForSeconds(2.3f);
        //ataqueDisponible = true;

    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 2, rb.gravityScale * 2), ForceMode2D.Impulse);
        //EstablecerInvulnerabilidades(layerObject);
    }

    private new void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == 14 && playable)
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
            jugadorDetectado = true;
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
            jugadorDetectado = true;
            detectionTime += Time.deltaTime;

            if (detectionTime >= 2.5f) {
                //DISPARO DE FLECHA
                detectionTime = 0;
                StartCoroutine(Ataque(collider.transform.position));
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11)
        {
            jugadorDetectado = false;
            detectionTime = 0;
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
