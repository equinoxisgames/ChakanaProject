using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanzaHoyustus : MonoBehaviour
{
    private int danio = 5;
    void Start()
    {
        Physics2D.IgnoreLayerCollision(14, 12, true);
    }

    public int getDanioArma() {
        return danio;
    }

}
