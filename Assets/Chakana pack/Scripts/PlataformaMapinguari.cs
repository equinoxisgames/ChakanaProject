using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class PlataformaMapinguari : MonoBehaviour
{
    public int plataforma;
    public float minX;
    public float maxX;
    private Mapianguari boss;

    private void Start()
    {
        boss = GameObject.Find("Mapinguari").GetComponent<Mapianguari>();
        minX = this.gameObject.transform.GetChild(0).position.x;
        maxX = this.gameObject.transform.GetChild(1).position.x;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.gameObject.transform.position.y -0.1f > transform.position.y) {
            boss.nuevaPlataforma = plataforma;
            boss.minX= minX;
            boss.maxX= maxX;
        }
    }
}
