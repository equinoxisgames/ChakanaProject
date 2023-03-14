using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Apallimay : MonoBehaviour
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

    public Vector2 ApallimayHeadPossition;
    public bool inApallimayHead;
    public int ApallimayLives;

    public string ApallimayName;

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
        gameObject.name = ApallimayName; 
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
            ApallimayFlip(direction.normalized.x);
            anim.SetBool("ApallimayWalk", true);
        }
        else {
            rb.velocity = direction.normalized * -0.0f;

            if (distance <= detectionRadius +1)
                transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            //ApallimayFlipBack(direction.normalized.x);

            anim.SetBool("ApallimayWalk", false);
        }
        
    }

    private void ApallimayFlip(float xDirection)
    {
        //Debug.Log("ApallimayFlip xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x);

        if (xDirection<0 && transform.localScale.x >0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        }else if (xDirection > 0 && transform.localScale.x < 0)
        {

            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }
    private void ApallimayFlipBack(float xDirection)
    {

        

        // if (xDirection < 0 && transform.localScale.x > 0)
        if (xDirection > 0 &&  transform.localScale.x < 0)
        {

            Debug.Log("Entra if 1 .. ApallimayFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: " + hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3((-1*hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        //else if (xDirection > 0 && transform.localScale.x < 0)
        else if (xDirection < 0 && transform.localScale.x > 0)
        {

            Debug.Log("Entra if 2 .. ApallimayFlipBack xDirection: " + xDirection + " // transform.localScale.x: " + transform.localScale.x + " // hoyustusPlayerCotroller.transform.localScale.x: "+ hoyustusPlayerCotroller.transform.localScale.x);

            transform.localScale = new Vector3(-1 * (-1 * hoyustusPlayerCotroller.transform.localScale.x), transform.localScale.y, transform.localScale.z);
            

        }

    }
}