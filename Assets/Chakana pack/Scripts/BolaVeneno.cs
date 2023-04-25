using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolaVeneno : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.Sleep();
        
    }

    public void aniadirFuerza(float direccion) {
        rb.WakeUp();
        rb.AddForce(new Vector3(12f * -direccion, 18f, 0f), ForceMode2D.Impulse);
    }
}
