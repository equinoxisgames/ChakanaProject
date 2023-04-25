using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    private int limitRadio = 3;
    private float indiceExplosion = 6;
    public readonly float danioExplosion = 45;

    void Update()
    {
        this.gameObject.transform.localScale += Vector3.one * Time.deltaTime * indiceExplosion;
        if (transform.localScale.x >= 3) { 
            Destroy(this.gameObject);
        }
    }

}
