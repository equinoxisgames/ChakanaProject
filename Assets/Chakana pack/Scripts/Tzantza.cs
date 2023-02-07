using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Tzantza : MonoBehaviour
{
    //variables

    private CinemachineVirtualCamera cm;
    private SpriteRenderer sp;
    private Rigidbody2D rb;

    private Hoyustus hoyustusPlayerCotroller;
    private bool applyForce;

    public float movementSpeed = 3;
    public float detectionRadius = 15;
    public LayerMask playerLayer;

    public Vector2 tzantzaHeadPossition;
    public bool inTzantzaHead;
    public int tzantzaLives;

    public string tzantzaName;

    private void Awake()
    {
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        hoyustusPlayerCotroller = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = tzantzaName; 
    }

    // Update is called once per frame
    void Update()
    {

        detectionRadius = 15;
        movementSpeed = 7;

        Vector2 direction = hoyustusPlayerCotroller.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, hoyustusPlayerCotroller.transform.position);

        Debug.Log("distance: " + distance + " // detectionRadius: " + detectionRadius);


        if (distance <= detectionRadius)
        {
            rb.velocity = direction.normalized * movementSpeed;
            //TzantzaFlip(direction.normalized.x);
        }
        else {
            rb.velocity = direction.normalized * -1;
        }
    }


}
