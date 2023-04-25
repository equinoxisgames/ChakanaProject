using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolaVeneno : MonoBehaviour
{
    Rigidbody2D rb;
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector3(3f, 4f, 0f), ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
