using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    private int limitRadio = 3;
    private CircleCollider2D collider;
    private float indiceExplosion = 6;
    public readonly float danioExplosion = 45;

    void Update()
    {
        this.gameObject.transform.localScale += Vector3.one * Time.deltaTime * indiceExplosion;
        //collider.radius += indiceExplosion * Time.deltaTime;
        if (transform.localScale.x >= 3) { 
            Destroy(this.gameObject);
        }
    }

}
