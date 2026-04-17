using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.UI;
using Assets.FantasyInventory.Scripts.Enums;
using Assets.FantasyInventory.Scripts.GameData;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using Assets.FantasyInventory.Scripts.Interface;
using UnityEngine.InputSystem;

public class ConversationInteract : MonoBehaviour
{
    [SerializeField] Hoyustus player;
    [SerializeField] DialogueSystemTrigger data;
    [SerializeField] GameObject interactBtn;
    [SerializeField] GameObject shop;
    [SerializeField] Transform shopList;
    public Inventory inventory;
    public DialogueController dialogueController;
    GameObject canvas;

    private GameObject canvasUI;
    private GameObject keyObj, joyObj;
    private bool joystick;
    private GameObject cam;
    private bool shopEnable = false;
    private bool shopping = false;
    private bool _inConversation = false;
    private bool _interactBtn = false;

    void Start()
    {
        canvas = GameObject.Find("Dialogue Manager").transform.GetChild(0).gameObject;
        Debug.Log($"[ConversationInteract] canvas (Dialogue Manager hijo): {(canvas != null ? canvas.name : "NULL")}");

        canvasUI = GameObject.Find("HUDMenu");
        Debug.Log($"[ConversationInteract] canvasUI (HUDMenu): {(canvasUI != null ? canvasUI.name : "NULL")}");

        if (dialogueController != null)
            Debug.Log("[ConversationInteract] Suscrito a OnConversationFinished.");
        else
            Debug.LogError("[ConversationInteract] dialogueController es NULL. No está asignado en el Inspector.");

        if (PlayerPrefs.GetInt("ukukuM") == 3 && PlayerPrefs.GetInt("conv01") != 2)
        {
            data.conversation = "Ukuku02";
            shopEnable = true;
        }
        else if (PlayerPrefs.GetInt("conv01") == 1) data.conversation = "Ukuku03";
        else if (PlayerPrefs.GetInt("conv01") == 2)
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

    private void OnDestroy()
    {
        // Sin suscripciones por código que limpiar
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


        var gamepad = Gamepad.current;

        bool pressedE = Input.GetKeyDown(KeyCode.E);
        bool pressedY = gamepad != null && gamepad.buttonNorth.wasPressedThisFrame;

        if (pressedE || pressedY)
            Debug.Log($"[ConversationInteract] Input detectado — E: {pressedE}, Y: {pressedY}, _inConversation: {_inConversation}");

        if (!_inConversation && (pressedE || pressedY) && _interactBtn)
        {
            Debug.Log("[ConversationInteract] Abriendo conversación...");

            // 1. Determinar qué diálogo cargar según los PlayerPrefs
            string conversationID = "1"; // Valor por defecto
            int ukukuM = PlayerPrefs.GetInt("ukukuM", 0); // El segundo parámetro es el valor si no existe
            int conv1 = PlayerPrefs.GetInt("conv1", 0);

            // Lógica de prioridad
            if (conv1 == 2)
            {
                conversationID = "OPEN_STORE";
            }
            else if (ukukuM == 0)
            {
                conversationID = "START_MISSION";
            }
            else if (ukukuM == 3)
            {
                conversationID = "MISSION_COMPLETE";
            }
            else if (ukukuM > 0 && ukukuM < 3)
            {
                conversationID = "INCOMPLETE_MISSION";
            }

            // 2. Ejecutar la conversación con el ID seleccionado
            _inConversation = true;
            dialogueController.StartConversation(conversationID);
            StartConversation();
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

        if (shopList != null && shopList.childCount > 0)
        {
            Transform firstChild = shopList.GetChild(0);

            if (firstChild != null)
            {
                Button button = firstChild.GetComponent<Button>();

                if (button != null)
                {
                    button.Select();
                }
            }
        }
    }


    public void StopConversation()
    {
        Debug.Log("[ConversationInteract] StopConversation llamado.");
        _inConversation = false;

        if (shopEnable && !PlayerPrefs.HasKey("TiendaVacia"))
        {
            shop.SetActive(true);
            //canvasUI.SetActive(false);
            shopping = true;
            //canvas.SetActive(false);
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
        if (interactBtn != null)
        {
            interactBtn.SetActive(t);
            _interactBtn = t;
        }
    }

    public void CloseShop()
    {
        if (!shopping) return;

        _inConversation = false;
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