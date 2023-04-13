using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Chontacuro : MonoBehaviour
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

    public Vector2 ChontacuroHeadPossition;
    public bool inChontacuroHead;
    public int ChontacuroLives;

    public string ChontacuroName;

    private float speed;
    
    private Vector3 limit1, limit2, objetivo;

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
        gameObject.name = ChontacuroName;

        speed = 4f;
        limit1 = new Vector3(-20f, 0f, 0f);
        limit2 = new Vector3(20f, 0f, 0f);
        objetivo = limit1;
        
    }

    // Update is called once per frame
    void Update()
    {

        detectionRadius = 15;
        movementSpeed = 7;

        Vector2 direction = hoyustusPlayerCotroller.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hoyustusPlayerCotroller.transform.position);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        //Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);

        if (distance <= detectionRadius)
        {
            rb.velocity = direction.normalized * movementSpeed;
            ChontacuroFlip(direction.normalized.x);
            anim.SetBool("ChontacuroWalk", true);
        }
        else {
            rb.velocity = direction.normalized * -1f;
            
           // Move();

            if (distance <= detectionRadius +1)
                transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            //ChontacuroFlipBack(direction.normalized.x);
            //ChontacuroFlip(direction.normalized.x);
            anim.SetBool("ChontacuroWalk", false);
        }
        
    }
    private void FixedUpdate()
    {
        //Move();
    }
    private void ChontacuroFlip(float xDirection)
    {
        //Debug.Log("ChontacuroFlip xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x);

        if (xDirection<0 && transform.localScale.x >0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        }else if (xDirection > 0 && transform.localScale.x < 0)
        {

            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }
    private void ChontacuroFlipBack(float xDirection)
    {

        

        // if (xDirection < 0 && transform.localScale.x > 0)
        if (xDirection > 0 &&  transform.localScale.x < 0)
        {

            Debug.Log("Entra if 1 .. ChontacuroFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: " + hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3((-1*hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        //else if (xDirection > 0 && transform.localScale.x < 0)
        else if (xDirection < 0 && transform.localScale.x > 0)
        {

            Debug.Log("Entra if 2 .. ChontacuroFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: "+ hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            

        }

    }
    private void Move()
    {

        Debug.Log("Entra a Move: transform.position es: "+ transform.position+" objetivo es :" + objetivo);

        transform.position = Vector2.MoveTowards(transform.position, objetivo, 5f * Time.deltaTime);

        

        if (transform.position.x <= limit1.x)
        {
            Debug.Log("entra al if transform.position.x es:"+ transform.position.x+ " limit1.x:"+ limit1.x+ "objetivo:" + objetivo);

            objetivo = limit2;

            Debug.Log("despues de objetivo = limit2; transform.position.x es:" + transform.position.x + " limit1.x:" + limit1.x + "objetivo:" + objetivo);

        }
        else if (transform.position.x >= limit2.x)
        {
            Debug.Log("entra al else transform.position.x es:" + transform.position.x + " limit1.x:" + limit1.x);

            objetivo = limit1;
        }
    }
}