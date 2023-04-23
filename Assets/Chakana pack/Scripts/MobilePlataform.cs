using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobilePlataform : MonoBehaviour
{
    [SerializeField] private Transform sP, eP;
    [SerializeField] private float velocity;

    private Vector3 destino;

    // Start is called before the first frame update
    void Start()
    {
        destino = eP.position;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, destino, velocity * Time.deltaTime);

        if(transform.position == sP.position)
        {
            destino = eP.position;
        }
        
        if(transform.position == eP.position)
        {
            destino = sP.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            collision.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            collision.transform.parent = null;
        }
    }
}
