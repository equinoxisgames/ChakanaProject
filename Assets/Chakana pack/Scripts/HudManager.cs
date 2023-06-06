using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    Hoyustus player;
    [SerializeField] LiquidBar lifeBar;
    [SerializeField] LiquidBar manaBar;
    [SerializeField] Text goldTxt;

    float lifeMax;
    float life;
    float gold;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hoyustus>();
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("primerDato"))
        {
            PlayerPrefs.SetFloat("vidaM", player.getVida());
            lifeMax = PlayerPrefs.GetFloat("vidaM");
            PlayerPrefs.SetInt("primerDato", 1);
        }
        else
        {
            lifeMax = PlayerPrefs.GetFloat("vidaM");
        }

        life = player.getVida();
        gold = player.getGold();

        lifeBar.currentFillAmount = (life / lifeMax);
        lifeBar.targetFillAmount = (life / lifeMax);
        goldTxt.text = player.getGold().ToString();

        string[] joystickNames = Input.GetJoystickNames();

        if (joystickNames.Length > 0 && !string.IsNullOrEmpty(joystickNames[0]))
        {
            Debug.Log("Hay un mando conectado.");
        }
    }

    void Update()
    {
        UpdateData();
    }

    private void UpdateData()
    {
        if(player.getVida() != life)
        {
            life = player.getVida();

            lifeBar.targetFillAmount = (life / lifeMax);
        }

        if(player.getGold() != gold)
        {
            gold = player.getGold();

            goldTxt.text = player.getGold().ToString();
        }
    }
}
