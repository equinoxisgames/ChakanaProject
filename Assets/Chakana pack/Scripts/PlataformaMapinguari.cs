using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaMapinguari : MonoBehaviour
{
    public int plataforma;
    private Mapianguari boss;

    private void Start()
    {
        boss = GameObject.Find("Mapinguari").GetComponent<Mapianguari>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.gameObject.transform.position.y > transform.position.y) {
            boss.nuevaPlataforma = plataforma;
        }
    }
}
