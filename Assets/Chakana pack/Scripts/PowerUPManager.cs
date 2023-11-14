using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUPManager : MonoBehaviour
{
    [SerializeField] GameObject pUP01;
    [SerializeField] GameObject pUP02;
    [SerializeField] GameObject pUP03;

    [SerializeField] Transform player;

    public void ShowVFX(int e)
    {
        transform.position = player.position;

        StartCoroutine(VFXAnimation(e));
    }

    IEnumerator VFXAnimation(int e)
    {
        if(e == 1)
        {
            pUP01.SetActive(true);
        }
        else if(e == 2)
        {
            pUP02.SetActive(true);
        }
        else
        {
            pUP03.SetActive(true);
        }

        yield return new WaitForSeconds(2.5f);

        if (e == 1)
        {
            pUP01.SetActive(false);
        }
        else if (e == 2)
        {
            pUP02.SetActive(false);
        }
        else
        {
            pUP03.SetActive(false);
        }
    }
}
