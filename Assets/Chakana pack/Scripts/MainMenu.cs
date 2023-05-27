using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public RectTransform homeMenu;
    public RectTransform pauseMenu;
    public RectTransform exitMenu;
    public RectTransform confirmQuitMenu;


    //public RectTransform exitMenu;
    //public Transform hoyustusGameObject;
    public Button btContinue;
    public Button btCancelExitGame;
    public Button btExitGame;
    public Button btLoadSlot;


    string escena;
    bool boolHomeMenuActive = true;

    private bool mouseMovido = false;
    private bool gamePadConectado = false;

    public float joystickThreshold = 0.2f; // Umbral de sensibilidad del joystick

    private bool joystickIzquierdoMovido = false;
    private bool botonGamePadPress = false;
    private bool AnyKeyPress = false;

    void Start()
    {
        btContinue.Select();
        Time.timeScale = 1f;

        if (Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]))
        {
            gamePadConectado = true;
            Debug.Log("Start Gamepad conectado");
        }else
            mouseMovido = true;

    }

    void OnGUI()
    {
        Event e = Event.current;

        if (e.isKey)
        {
            AnyKeyPress = true;
            Debug.Log("Se ha presionado una tecla en el teclado: " + e.keyCode);
        }
    }

    void Update()
    {
        if (AnyKeyPress && !joystickIzquierdoMovido)
        {
            mouseMovido = true;
            joystickIzquierdoMovido = false;
            Debug.Log("Se ha aplastado el teclado por tanto el mouse se ha movido.");
        }

        // Validar movimiento del mouse
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            mouseMovido = true;
            joystickIzquierdoMovido = false;
            Debug.Log("El mouse se ha movido.");
        }
        // Validar movimiento del joystick izquierdo del gamepad
        float joystickX = Input.GetAxis("Horizontal");
        float joystickY = Input.GetAxis("Vertical");
        if (Mathf.Abs(joystickX) > joystickThreshold || Mathf.Abs(joystickY) > joystickThreshold)
        {
            joystickIzquierdoMovido = true;
            //mouseMovido = false;
            //Debug.Log("Joystick izquierdo del gamepad se ha movido.");
        }
        Escape();
    }
    public void Escape()
    {
        escena = SceneManager.GetActiveScene().name;
        if (Input.GetButtonDown("Cancel"))
        {
            if (escena != "00- Main Menu 0")
            {
                if (!pauseMenu.gameObject.activeSelf && !confirmQuitMenu.gameObject.activeSelf)
                {
                    pauseMenu.gameObject.SetActive(true);
                    btContinue.Select();
                    Time.timeScale = 0f;

                }
                else
                {
                    Time.timeScale = 1f;
                    if (pauseMenu.gameObject.activeSelf)
                    {
                        pauseMenu.gameObject.SetActive(false);
                    }
                    if (confirmQuitMenu.gameObject.activeSelf)
                    {
                        confirmQuitMenu.gameObject.SetActive(false);
                    }

                }

            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape) && mouseMovido)
                {
                    Debug.Log("Se ha presionado la tecla Escape en el teclado.");
                    EscapeHomeMenu();
                }
                else
                {
                    Debug.Log("Se ha presionado el botón Escape (start) del gamepad");
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire2") && joystickIzquierdoMovido)
            {
                if (escena != "00- Main Menu 0")
                {
                    Debug.Log("escena != 00 - Main Menu 0");
                }
                else
                {
                    Debug.Log("Se ha presionado la tecla Escape en el gamepad.");
                    EscapeHomeMenu();
                }
            }
            else
            {
                Debug.Log("Algun booleano en false BT FIRE, joystickIzquierdoMovido-->"+ joystickIzquierdoMovido);
            }


        }
    }
    public void EscapeHomeMenu()
    {

        if (boolHomeMenuActive)
        {
            homeMenu.gameObject.SetActive(false);
            boolHomeMenuActive = false;
            ActivateExitGame();
            Debug.Log("DESACTIVAR HOME MENU");
            btExitGame.Select();
        }
        else
        {
            homeMenu.gameObject.SetActive(true);
            boolHomeMenuActive = true;
            ActivateHomeMenu();
            Debug.Log("ACTIVAR HOME MENU");
        }
    }
    public void ActivateSlot1()
    {

        btLoadSlot.Select();
    }
    public void ActivateStart()
    {

        btContinue.Select();
    }
    public void ActivateExitGame()
    {
        btExitGame.Select();

    }
    public void ActivateCancelExitGame()
    {
        btCancelExitGame.Select();

    }
    public void ActivateHomeMenu()
    {
        homeMenu.gameObject.SetActive(true);
        boolHomeMenuActive = true;
        btContinue.Select();

    }
    public void DeActivateHomeMenu()
    {
        homeMenu.gameObject.SetActive(false);
        boolHomeMenuActive = false;
        

    }
    public void Continue()
    {
        //hoyustusGameObject.gameObject.SetActive(true);
        //btYes.Select();
        //btContinue.Select();


    }
    public void PlayGame()
    {
        SceneManager.LoadScene("00- StartRoom 1");
    }
    public void OpenMainMenu()
    {
        SceneManager.LoadScene("00- Main Menu 0");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ActivePlayer(bool active)
    {

        //hoyustusGameObject.gameObject.SetActive(active);
        if (active)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

}