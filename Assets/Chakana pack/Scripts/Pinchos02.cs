using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinchos02 : MonoBehaviour
{
    Vector3 pos;
    [SerializeField] GameObject exVFX;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine(DelayDmg(collision.gameObject.GetComponent<Hoyustus>()));
        }
    }

    public void NewPos(Vector3 e)
    {
        pos = e;
    }

    IEnumerator DelayDmg(Hoyustus player)
    {
        player.recibirDanio(10);
        Instantiate(exVFX, transform.position, Quaternion.identity);

        player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 12.5f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.44f);
        pos.z = player.transform.position.z;

        player.transform.position = pos;
    }
}
