using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.UI;
using Assets.FantasyInventory.Scripts.Enums;
using Assets.FantasyInventory.Scripts.GameData;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using Assets.FantasyInventory.Scripts.Interface;

public class ConversationInteract : MonoBehaviour
{
    [SerializeField] Hoyustus player;
    [SerializeField] DialogueSystemTrigger data;
    [SerializeField] GameObject interactBtn;
    [SerializeField] GameObject shop;
    [SerializeField] Transform shopList;
    public Inventory inventory;
    GameObject canvas;

    private GameObject canvasUI;
    private GameObject keyObj, joyObj;
    private bool joystick;
    private GameObject cam;
    private bool shopEnable = false;
    private bool shopping = false;
    
    void Start()
    {
        canvas = GameObject.Find("Dialogue Manager").transform.GetChild(0).gameObject;

        canvasUI = GameObject.Find("HUDMenu");

        if (PlayerPrefs.GetInt("ukukuM") == 3 && PlayerPrefs.GetInt("conv01") != 2)
        {
            data.conversation = "Ukuku02";
            shopEnable = true;
        }
        else if(PlayerPrefs.GetInt("conv01") == 1) data.conversation = "Ukuku03";
        else if(PlayerPrefs.GetInt("conv01") == 2)
        {
            data.conversation = "Ukuku04";
            shopEnable = true;
        }

        keyObj = interactBtn.transform.GetChild(0).gameObject;
        joyObj = interactBtn.transform.GetChild(1).gameObject;

        cam = transform.GetChild(0).gameObject;

        keyObj.SetActive(true);
        joyObj.SetActive(false);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (joystick)
            {
                joystick = false;

                keyObj.SetActive(true);
                joyObj.SetActive(false);
            }
        }

        if (Input.GetButtonDown("JoystickButton") || Input.GetAxis("HorizontalJ") != 0f || Input.GetAxis("VerticalJ") != 0f)
        {
            if (!joystick)
            {
                joystick = true;

                keyObj.SetActive(false);
                joyObj.SetActive(true);
            }
        }

        if (shopping)
        {
            if (interactBtn.activeSelf) EnableBtn(false);
            if (Input.GetButton("Cancel"))
            {
                CloseShop();
            }
        }
    }


    public void StartConversation()
    {
        player.enabled = false;
        cam.SetActive(true);

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        player.GetComponent<AudioSource>().Stop();

        player.GetComponent<Animator>().SetBool("Walking", false);
        player.GetComponent<Animator>().SetBool("Dashing", false);
        player.GetComponent<Animator>().SetBool("Atacando", false);
        player.GetComponent<Animator>().SetBool("Jumping", false);
        player.GetComponent<Animator>().SetBool("Grounded", true);

        if (!PlayerPrefs.HasKey("conv01"))
        {
            PlayerPrefs.SetInt("conv01", 1);
        }
        
        if (PlayerPrefs.GetInt("ukukuM") == 3)
        {
            PlayerPrefs.SetInt("conv01", 2);
            inventory.NewInventory();
        }
    }

    public void SelectFirstItem()
    {
        StartCoroutine(SelectItem());
    }

    IEnumerator SelectItem()
    {
        yield return new WaitForSeconds(0.5f);
        shopList.GetChild(0).GetComponent<Button>().Select();
    }

    public void StopConversation()
    {
        if (shopEnable && !PlayerPrefs.HasKey("TiendaVacia"))
        {
            shop.SetActive(true);
            canvasUI.SetActive(false);
            shopping = true;
            canvas.SetActive(false);
            shopList.GetChild(0).GetComponent<Button>().Select();
            shopList.GetChild(0).GetComponent<InventoryItem>().OnPress();
            GetComponent<Usable>().enabled = false;
            return;
        }

        player.enabled = true;
        cam.SetActive(false);

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        player.GetComponent<AudioSource>().Stop();

        if (PlayerPrefs.GetInt("ukukuM") == 4 && PlayerPrefs.GetInt("conv01") != 2) data.conversation = "Ukuku02";
        else if (PlayerPrefs.GetInt("conv01") == 1) data.conversation = "Ukuku03";
        else if (PlayerPrefs.GetInt("conv01") == 2) data.conversation = "Ukuku04";
    }

    public void EnableBtn(bool t)
    {
        interactBtn.SetActive(t);
    }

    public void CloseShop()
    {
        if (!shopping) return;

        canvas.SetActive(true);
        shop.SetActive(false);
        canvasUI.SetActive(true);
        shopping = false;
        player.enabled = true;
        cam.SetActive(false);
        GetComponent<Usable>().enabled = true;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        player.GetComponent<AudioSource>().Stop();

        if (PlayerPrefs.GetInt("ukukuM") == 4 && PlayerPrefs.GetInt("conv01") != 2) data.conversation = "Ukuku02";
        else if (PlayerPrefs.GetInt("conv01") == 1) data.conversation = "Ukuku03";
        else if (PlayerPrefs.GetInt("conv01") == 2) data.conversation = "Ukuku04";
        
        EnableBtn(true);
    }
}
