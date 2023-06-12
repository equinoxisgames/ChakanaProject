using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinchos02 : MonoBehaviour
{
    Vector3 pos;
    bool isActive = false;
    [SerializeField] GameObject exVFX;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isActive)
        {
            isActive = true;
            StartCoroutine(DelayDmg(collision.gameObject.GetComponent<Hoyustus>()));
        }
    }

    public void NewPos(Vector3 e)
    {
        if (isActive) return;
        pos = e;
    }

    IEnumerator DelayDmg(Hoyustus player)
    {
        player.recibirDanio(250);

        if(player.getVida() <= 0)
        {
            Instantiate(exVFX, transform.position, Quaternion.identity);
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 12, ForceMode2D.Impulse);
        }
        else
        {
            Instantiate(exVFX, transform.position, Quaternion.identity);
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 12, ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.44f);

            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            pos.z = player.transform.position.z;
            player.transform.position = pos;
            isActive = false;
        }
    }
}
