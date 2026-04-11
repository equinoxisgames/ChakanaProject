using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class PlataformaMapinguari : MonoBehaviour
{
    public int plataforma;
    private Vector3 minX;
    private Vector3 maxX;
    public Vector3 position;
    public Mapianguari boss;
    private GameObject nubeVeneno;

    private void Start()
    {
        minX = this.gameObject.transform.GetChild(0).position;
        maxX = this.gameObject.transform.GetChild(1).position;
        nubeVeneno = this.gameObject.transform.GetChild(2).gameObject;

        for (int i = 0; i < 5; i++) {
            nubeVeneno.transform.GetChild(i).gameObject.SetActive(false);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.transform.position.y -0.1f > transform.position.y) {
            boss.NewTeleport(minX, maxX, plataforma);
        }
    }

    public Vector3 GetMinX()
    {
        return minX;
    }
    
    public Vector3 GetMaxX()
    {
        return maxX;
    }
}
