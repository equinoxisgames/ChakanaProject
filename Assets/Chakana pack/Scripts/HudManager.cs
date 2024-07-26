using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HudManager : MonoBehaviour
{
    Hoyustus player;
    [SerializeField] LiquidBar lifeBar;
    [SerializeField] LiquidBar manaBar;
    [SerializeField] LiquidBar condorBar;
    [SerializeField] LiquidBar snakeBar;
    [SerializeField] LiquidBar weaponBar;
    [SerializeField] Text goldTxt;

    [Header ("UI_Extras")]
    [SerializeField] List<CanvasGroup> btnKeys = new List<CanvasGroup>();
    [SerializeField] List<CanvasGroup> btnJoys = new List<CanvasGroup>();

    [SerializeField] LiquidBar condorLiq;
    [SerializeField] LiquidBar snakeLiq;
    [SerializeField] LiquidBar weaponLiq;

    [ColorUsage(true, true)]
    [SerializeField] Color colorCondor1, colorSnake1, colorWeapon1;

    [ColorUsage(true, true)]
    [SerializeField] Color colorCondor2, colorSnake2, colorWeapon2;

    private bool manaB, condorB, snakeB, weaponB;

    float lifeMax;
    float life;
    float mana;
    float condor;
    float snake;
    float weapon;
    float maxValue;
    float gold;

    private bool isVibration = false;

    private void Awake()
    {
        maxValue = 100;
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
            lifeMax = player.getMaxVida();
        }

        if (lifeMax == 1500)
        {
            LifePlus();
        }

        life = player.getVida();
        mana = player.getCargaCuracion();
        condor = player.getCargaHabilidadCondor();
        snake = player.getCargaHabilidadSerpiente();
        weapon = player.getCargaHabilidadLanza();
        gold = player.getGold();

        lifeBar.currentFillAmount = (life / lifeMax);
        lifeBar.targetFillAmount = (life / lifeMax);

        manaBar.currentFillAmount = (mana / maxValue);
        manaBar.targetFillAmount = (mana / maxValue);

        condorBar.currentFillAmount = (condor / maxValue);
        condorBar.targetFillAmount = (condor / maxValue);

        snakeBar.currentFillAmount = (snake / maxValue);
        snakeBar.targetFillAmount = (snake / maxValue);

        weaponBar.currentFillAmount = (weapon / maxValue);
        weaponBar.targetFillAmount = (weapon / maxValue);
        goldTxt.text = player.getGold().ToString();
    }

    void Update()
    {
        UpdateData();
        MaxCharge();
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

        if (player.getCargaCuracion() != mana)
        {
            mana = player.getCargaCuracion();

            manaBar.targetFillAmount = (mana / maxValue);
        }

        if(player.getCargaHabilidadCondor() != condor)
        {
            condor = player.getCargaHabilidadCondor();

            condorBar.targetFillAmount = (condor / maxValue);
        }

        if (player.getCargaHabilidadSerpiente() != snake)
        {
            snake = player.getCargaHabilidadSerpiente();

            snakeBar.targetFillAmount = (snake / maxValue);
        }

        if (player.getCargaHabilidadLanza() != weapon)
        {
            weapon = player.getCargaHabilidadLanza();

            weaponBar.targetFillAmount = (weapon / maxValue);
        }
    }

    private void MaxCharge()
    {
        if (maxValue <= mana && !manaB)
        {
            manaB = true;

            btnJoys[0].alpha = 1;
            btnKeys[0].alpha = 1;
        }
        else if(maxValue > mana && manaB)
        {
            manaB = false;

            btnJoys[0].alpha = 0.5f;
            btnKeys[0].alpha = 0.5f;
        }

        if (maxValue <= condor && !condorB)
        {
            condorB = true;

            btnJoys[1].alpha = 1;
            btnKeys[1].alpha = 1;

            condorLiq.barColor = colorCondor2;
        }
        else if(maxValue > condor && condorB)
        {
            condorB = false;

            btnJoys[1].alpha = 0.5f;
            btnKeys[1].alpha = 0.5f;

            condorLiq.barColor = colorCondor1;
        }

        if (maxValue <= snake && !snakeB)
        {
            print("aparece");
            snakeB = true;

            btnJoys[2].alpha = 1;
            btnKeys[2].alpha = 1;

            snakeLiq.barColor = colorSnake2;
        }
        else if(maxValue > snake && snakeB)
        {
            snakeB = false;

            btnJoys[2].alpha = 0.5f;
            btnKeys[2].alpha = 0.5f;

            snakeLiq.barColor = colorSnake1;
        }

        if (maxValue == weapon && !weaponB)
        {
            weaponB = true;

            btnJoys[3].alpha = 1;
            btnKeys[3].alpha = 1;

            weaponLiq.barColor = colorWeapon2;
        }
        else if(maxValue > weapon && weaponB)
        {
            weaponB = false;

            btnJoys[3].alpha = 0.5f;
            btnKeys[3].alpha = 0.5f;

            weaponLiq.barColor = colorWeapon1;
        }
    }

    public void LifePlus()
    {
        lifeBar.GetComponent<RectTransform>().localPosition = new Vector3(62, -45, 0);
        lifeBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 650);

    }

    public float GetCondor()
    {
        return condor;
    }

    public float GetSnake()
    {
        return snake;
    }

    public float GetWeapon()
    {
        return weapon;
    }

    public float GetCuracion()
    {
        return mana;
    }

    public void SetVibration()
    {
        if (!isVibration)
        {
            StartCoroutine(StartVibration(1,1));
            isVibration = true;
        }
    }

    IEnumerator StartVibration(float e, float i)
    {
        int playerIndex = 0;
        float vibrationDuration = e;
        float intensity = i;

        var gamepads = Gamepad.all;

        if (gamepads.Count > 0)
        {
            Gamepad gamepad = Gamepad.all[playerIndex];
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(intensity, intensity);
                yield return new WaitForSeconds(vibrationDuration);
                gamepad.SetMotorSpeeds(0f, 0f);
                isVibration = false;
            }
        }
    }
}
