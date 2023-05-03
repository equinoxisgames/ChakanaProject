using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Chontacuro1 : MonoBehaviour
{
    //variables

    private CinemachineVirtualCamera cm;
    private SpriteRenderer sp;
    private Rigidbody2D rb;

    private Hoyustus hoyustusPlayerCotroller;
    private bool applyForce;

    public float movementSpeed = 3;
    public float detectionRadius = 3;
    public LayerMask playerLayer;

    public Vector2 Chontacuro1HeadPossition; 
    public bool inChontacuro1Head;
    public int Chontacuro1Lives;

    public string Chontacuro1Name;

    private float speed;
    
    private Vector3 limit1, limit2, objetivo;

    int pasos = 0;

    [SerializeField] Animator anim;

    private void Awake()
    {
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        hoyustusPlayerCotroller = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
        anim = GetComponent<Animator>();
        anim.SetFloat("XVelocity", rb.velocity.x);

        Debug.Log("XVelocity: "+ rb.velocity.x);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = Chontacuro1Name;

        speed = 4f;
        limit1 = new Vector3(-60f, 0f, 0f);
        limit2 = new Vector3(60f, 0f, 0f);
        objetivo = limit1;
        
    }

    // Update is called once per frame
    void Update()
    {
        

    }
    private void FixedUpdate()
    {
        //Move();

        detectionRadius = 10;
        movementSpeed = 5f;

        Vector2 direction = hoyustusPlayerCotroller.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hoyustusPlayerCotroller.transform.position);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        if (distance <= detectionRadius)
        {
            rb.velocity = direction.normalized * movementSpeed;
            Chontacuro1Flip(direction.normalized.x);
            anim.SetBool("Chontacuro1Walk", true);
        }
        else
        {
            //rb.velocity = direction.normalized * -0f;



            //if (distance <= detectionRadius +1)
            //  transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            Move();
            //Chontacuro1FlipBack(direction.normalized.x);
            //Chontacuro1Flip(direction.normalized.x);
            anim.SetBool("Chontacuro1Walk", false);
        }
    }
    private void Chontacuro1Flip(float xDirection)
    {
        //Debug.Log("Chontacuro1Flip xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x);

        if (xDirection<0 && transform.localScale.x >0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        }else if (xDirection > 0 && transform.localScale.x < 0)
        {

            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }
    private void Chontacuro1FlipBack(float xDirection)
    {

        

        // if (xDirection < 0 && transform.localScale.x > 0)
        if (xDirection > 0 &&  transform.localScale.x < 0)
        {

            Debug.Log("Entra if 1 .. Chontacuro1FlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: " + hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3((-1*hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        //else if (xDirection > 0 && transform.localScale.x < 0)
        else if (xDirection < 0 && transform.localScale.x > 0)
        {

            Debug.Log("Entra if 2 .. Chontacuro1FlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: "+ hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            

        }

    }
    private void Move()
    {

       

        Debug.Log("Pasos: "+pasos);

        //transform.position = Vector2.MoveTowards(transform.position, objetivo, 5f * Time.deltaTime);
        if (pasos >= 0 && pasos <= 100)
        {
            Debug.Log("If pasos <= 10 -->" + pasos);
            
            transform.position = new Vector3(gameObject.transform.position.x + 0.04f, gameObject.transform.position.y, gameObject.transform.position.z);
            pasos = pasos + 1;
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            

            Debug.Log("Else If pasos > 10 -->" + pasos);
            transform.position = new Vector3(gameObject.transform.position.x - 0.04f, gameObject.transform.position.y, gameObject.transform.position.z);
            pasos = pasos + 1;
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);

            if (pasos == 200)
            {
                pasos = 0;
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            }
                
        }




        //if (transform.position.x <= limit1.x)
        //{
        //    Debug.Log("entra al if transform.position.x es:"+ transform.position.x+ " limit1.x:"+ limit1.x+ "objetivo:" + objetivo);

        //    objetivo = limit2;

        //    Debug.Log("despues de objetivo = limit2; transform.position.x es:" + transform.position.x + " limit1.x:" + limit1.x + "objetivo:" + objetivo);

        //}
        //else if (transform.position.x >= limit2.x)
        //{
        //    Debug.Log("entra al else transform.position.x es:" + transform.position.x + " limit1.x:" + limit1.x);

        //    objetivo = limit1;
        //}
    }
}