using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Chontacuro1 : CharactersBehaviour
{
    private Hoyustus hoyustusPlayerCotroller;

    [SerializeField] public float movementSpeed;
    [SerializeField] public float seguimientoSpeed;
    [SerializeField] public float detectionRadius = 3;

    [SerializeField] private float speed;
    [SerializeField] private float direction = 1;
    [SerializeField] private bool siguiendo = false;

    [SerializeField] private Vector3 limit1, limit2, objetivo;
    [SerializeField] private float posY;
    [SerializeField] int deteccion = 1;

    int pasos = 0;

    [SerializeField] Animator anim;

    [SerializeField] Transform groundDetector;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool hayPiso = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        fuerzaRecoil = 1;
        inmuneDash = true;
        explosionInvulnerable = "ExplosionEnemy";
        hoyustusPlayerCotroller = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
        speed = movementSpeed;

        limit1 = transform.GetChild(0).gameObject.transform.position;
        limit2 = transform.GetChild(1).gameObject.transform.position;
        objetivo = limit1;
        layerObject = transform.gameObject.layer;
        posY = transform.localPosition.y;
    }

    private void ajustarLimites()
    {
        /*if (transform.localPosition.y <= posY - 3) {
            limit1 = transform.GetChild(0).gameObject.transform.position;
            limit2 = transform.GetChild(1).gameObject.transform.position;
        }*/

        if ((int)limit1.x == (int)limit2.x) {
            limit1 = transform.GetChild(0).gameObject.transform.position;
            limit2 = transform.GetChild(1).gameObject.transform.position;
        }

        if (limit1.x > limit2.x){
            limit1 = transform.GetChild(0).gameObject.transform.position;
            limit2 = transform.GetChild(1).gameObject.transform.position;
            Vector3 aux = limit1;
            limit1 = limit2;
            limit2 = aux;
        }

    }


    private void Falling() {
        rb.velocity -= Vector2.up * Time.deltaTime * -Physics2D.gravity * 4.5f;
    }

    private void Muerte() {
        Destroy(this.gameObject);
    }


    private void FixedUpdate()
    {
        //Move();
        if (transform.position.x + 1.3f < objetivo.x || transform.position.x - 1.3f > objetivo.x) {
            Flip();
        }

        detectarPiso();
        if (playable)
        {
            Move();            
        }

        if (rb.velocity.y < 0)
        {
            Falling();
        }

        if (vida <= 0)
        {
            Muerte();
        }

        ajustarLimites();
    }


    private void OnTriggerStay2D(Collider2D collider)
    {

        if (collider.gameObject.tag == "Player")
        {
            if (!detectarPiso())
            {
                speed = 0;
            }
            else {

                speed = seguimientoSpeed;
                siguiendo = true;
                objetivo = collider.transform.position;
            }
        }
    }

    public bool detectarPiso() {
        if (!Physics2D.OverlapCircle(groundDetector.position, 0.2f, groundLayer)) {
            if (direction == - 1) {
                limit1 = transform.position + Vector3.right * 0.1f;
            }
            else if(direction == 1)
            {
                limit2 = transform.position - Vector3.right * 0.1f;
            }
            return false;
        }
        return true;
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player") {
            siguiendo = false;
            speed = movementSpeed;
            deteccion = 1;
            if (transform.position.x >= collider.transform.position.x) {
                if (transform.position.x < limit1.x)
                {
                    objetivo = limit1;
                }
                else {
                    objetivo = limit2;
                }
            }
            else if (transform.position.x < collider.transform.position.x) {
                objetivo = limit2;
                if (transform.position.x > limit2.x)
                {
                    objetivo = limit2;
                }
                else
                {
                    objetivo = limit1;
                }
            }


            /*if (Vector3.Distance(transform.position, limit2) > Vector3.Distance(transform.position, limit1)) {
                objetivo = limit1;
            }
            else
            {
                objetivo = limit2;
            }*/
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 16)
        {
            if (objetivo == limit1) {
                limit1 = transform.position + Vector3.right;
                objetivo = limit1;
                direction = 1;
            }
            else if(objetivo == limit2) 
            {
                limit2 = transform.position - Vector3.right;
                objetivo = limit2;
                direction = -1;
            }


            if (siguiendo)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                speed = 0;
            }
            else {
                speed = movementSpeed;
            }
        }
    }


    protected override void Recoil(int direccion, float fuerzaRecoil)
    {
        playable = false; //EL OBJECT ESTARIA SIENDO ATACADO Y NO PODRIA ATACAR-MOVERSE COMO DE COSTUMBRE
        rb.AddForce(new Vector2(direccion * 10, rb.gravityScale * 4), ForceMode2D.Impulse);
        //EstablecerInvulnerabilidades(layerObject);
    }


    private void Move()
    {
        rb.velocity = new Vector2(direction * speed * (1 - afectacionViento) * deteccion, rb.velocity.y);

        if (!siguiendo) { 
            if (transform.position.x <= limit1.x)
            {
                objetivo = limit2;
            }
            else if (transform.position.x >= limit2.x)
            {
                objetivo = limit1;
            }
        }
    }

    private void Flip() {
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

        if (collider.gameObject.tag == "Viento")
        {
            if (estadoViento)
            {
                StopCoroutine("afectacionEstadoViento");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 1;
                //StartCoroutine("combinacionesElementales");
                combinacionesElementales();
                return;
            }
            estadoViento = true;
            counterEstados = 1;
            StartCoroutine("afectacionEstadoViento");
        }
        //DETECCIONS DE TRIGGERS DE OBJETOS TAGUEADOS COMO FUEGO
        else if (collider.gameObject.tag == "Fuego")
        {
            if (estadoFuego)
            {
                StopCoroutine("afectacionEstadoFuego");
            }
            else if (counterEstados > 0)
            {
                counterEstados += 10;
                //StartCoroutine("combinacionesElementales");
                combinacionesElementales();
                return;
            }
            estadoFuego = true;
            counterEstados = 10;
            StartCoroutine("afectacionEstadoFuego");
        }
    }


    private void combinacionesElementales()
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
        //yield return new WaitForEndOfFrame();
    }

}