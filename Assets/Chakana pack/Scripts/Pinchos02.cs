using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinchos02 : MonoBehaviour
{
    Vector3 pos;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Hoyustus>().recibirDanio(10);
            //StartCoroutine(DelayDmg(collision.gameObject.GetComponent<Hoyustus>()));

            pos.z = collision.transform.position.z;

            collision.transform.position = pos;
        }
    }

    public void NewPos(Vector3 e)
    {
        pos = e;
    }

    /*IEnumerator DelayDmg(Hoyustus player)
    {
        player.recibirDanio(10);
        player.setPlayable(false);
        player.GetComponent<Rigidbody2D>().isKinematic = true;

        yield return new WaitForSeconds(1f);

        player.setPlayable(true);
        player.GetComponent<Rigidbody2D>().isKinematic = false;
    }*/
}
