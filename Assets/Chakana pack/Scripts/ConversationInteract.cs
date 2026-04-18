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


        //Valida la conversación que va a presentar dependiendo de los palyer prefs
        ValidateAndRunConversation();

    }

    /// <summary>
    /// Evalúa inputs, estado de interacción y prioridades de PlayerPrefs para ejecutar la conversación.
    /// </summary>
    public void ValidateAndRunConversation()
    {
        var gamepad = Gamepad.current;

        bool pressedE = Input.GetKeyDown(KeyCode.E);
        bool pressedY = gamepad != null && gamepad.buttonNorth.wasPressedThisFrame;

        if (pressedE || pressedY)
            Debug.Log($"[ConversationInteract] Input detectado — E: {pressedE}, Y: {pressedY}, _inConversation: {_inConversation}");

        // --- DEBUG PREVIO: Para ver por qué entra o no al bloque ---
        Debug.Log($"[DEBUG Check] _inConversation: {!_inConversation} | pressedE: {pressedE} | pressedY: {pressedY} | _interactBtn: {_interactBtn}");


        // --- 1. DEBUG DE INPUTS Y ESTADO ---
        // Ayuda a identificar si el problema es de input o de flags lógicos.
        if (pressedE || pressedY)
        {
            Debug.Log($"[Check Pre-Condición] _inConversation: {_inConversation} | _interactBtn: {_interactBtn} | E: {pressedE} | Y: {pressedY}");
        }

        // --- 2. CONDICIÓN DE ENTRADA ---
        if (!_inConversation && (pressedE || pressedY) && _interactBtn)
        {
            Debug.Log("<color=green><b>[VALIDATION PASSED]</b></color> Iniciando lógica de prioridades...");

            // --- 3. LECTURA DE DATOS ---
            string conversationID = "1"; // Valor por defecto
            int ukukuM = PlayerPrefs.GetInt("ukukuM", 0);
            int conv1 = PlayerPrefs.GetInt("conv1", 0);

            Debug.Log($"<color=yellow>[DEBUG PlayerPrefs]</color> ukukuM: {ukukuM} | conv1: {conv1}");

            // --- 4. ÁRBOL DE PRIORIDADES ---

            // Prioridad 1: Tienda desbloqueada permanentemente
            if (conv1 == 2)
            {
                conversationID = "OPEN_STORE";
                Debug.Log("<color=white>-> Prioridad Seleccionada: Tienda Permanente (conv1 == 2)</color>");
            }
            // Prioridad 2: Condición especial (Misión completa + Tienda sin abrir)
            else if (ukukuM == 3 && conv1 == 0)
            {
                conversationID = "OPEN_STORE";
                Debug.Log("<color=white>-> Prioridad Seleccionada: Apertura Tienda Post-Misión (ukukuM == 3 & conv1 == 0)</color>");
            }
            // Prioridad 3: No ha empezado la misión
            else if (ukukuM == 0)
            {
                conversationID = "START_MISSION";
                Debug.Log("<color=white>-> Prioridad Seleccionada: Inicio de Misión (ukukuM == 0)</color>");
            }
            // Prioridad 4: Misión completada
            else if (ukukuM == 3)
            {
                conversationID = "MISSION_COMPLETE";
                Debug.Log("<color=white>-> Prioridad Seleccionada: Misión Finalizada (ukukuM == 3)</color>");
            }
            // Prioridad 5: Misión en curso (1 o 2)
            else if (ukukuM > 0 && ukukuM < 3)
            {
                conversationID = "INCOMPLETE_MISSION";
                Debug.Log($"<color=white>-> Prioridad Seleccionada: Misión en progreso (ukukuM: {ukukuM})</color>");
            }
            else
            {
                Debug.LogWarning("[DEBUG Logic] Ninguna condición de prioridad encajó. Se usará ID default: " + conversationID);
            }

            // --- 5. EJECUCIÓN FINAL ---
            Debug.Log($"<color=cyan><b>[EXECUTION]</b></color> Enviando ID final: <b>{conversationID}</b> al dialogueController.");

            _inConversation = true;

            if (dialogueController != null)
            {
                dialogueController.StartConversation(conversationID);
            }
            else
            {
                Debug.LogError("<b>[ERROR]</b> ¡dialogueController no está asignado en el script!");
            }

            StartConversation(); // Llamada al método que gestiona el inicio visual/lógico
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