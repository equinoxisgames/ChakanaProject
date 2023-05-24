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

    void Start()
    {
        btContinue.Select();
        Time.timeScale = 1f;

        if (Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]))
        {
            gamePadConectado = true;
            joystickIzquierdoMovido = true;
            Debug.Log("Start Gamepad conectado");
        }else
            mouseMovido = true;

    }
    void Update()
    {
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
            mouseMovido = false;
            //Debug.Log("Joystick izquierdo del gamepad se ha movido.");
        }

        if (Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]))
        {
            gamePadConectado = true;

            //Debug.Log("Gamepad conectado");
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

                // Validar movimiento del joystick izquierdo del gamepad
                float joystickX = Input.GetAxis("Horizontal");
                float joystickY = Input.GetAxis("Vertical");

                if (Mathf.Abs(joystickX) > joystickThreshold || Mathf.Abs(joystickY) > joystickThreshold)
                {
                    //joystickIzquierdoMovido = true;
                    //mouseMovido = false;
                    //Debug.Log("Joystick izquierdo del gamepad se ha movido.");
                }
                else
                {
                    

                    if (mouseMovido && !joystickIzquierdoMovido)
                    {

                        //string[] gamepadButtons = new string[] { "Button7"};

                        
                        if (Input.GetButtonDown("Cancel"))
                        {
                            Debug.Log("Se ha presionado el botón del gamepad.");
                            botonGamePadPress = true;
                        }

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            Debug.Log("Se ha presionado la tecla Escape en el teclado.");
                            botonGamePadPress = false;
                        }


                        if (botonGamePadPress)
                        {
                            Debug.Log("Se ha presionado el botón Start del gamepad.");
                        }
                        else
                        {

                            Debug.Log("Evento de salto realizado con el teclado");

                            if (boolHomeMenuActive)
                            {
                                homeMenu.gameObject.SetActive(false);
                                boolHomeMenuActive = false;
                                ActivateExitGame();
                                Debug.Log("Evento de salto realizado con el teclado--BOOL HOME MENU ACTIVE = TRUE // DESACTIVAR HOME MENU");
                                btExitGame.Select();
                            }
                            else
                            {
                                homeMenu.gameObject.SetActive(true);
                                boolHomeMenuActive = true;
                                ActivateHomeMenu();
                                Debug.Log("Evento de salto realizado con el teclado--BOOL HOME MENU ACTIVE = FALSE // ACTIVAR HOME MENU");
                            }
                        }

                        
                    }
                    else
                    {
                        Debug.Log("Algun bool en false BT CANCEL , joystickIzquierdoMovido-->" + joystickIzquierdoMovido + " mouseMovido-->" + mouseMovido);
                    }
                }



            }
        }
        else
        {
            if (Input.GetButtonDown("Fire2") && joystickIzquierdoMovido && !mouseMovido)
            {
                if (escena != "00- Main Menu 0")
                {
                    Debug.Log("escena != 00 - Main Menu 0");
                }
                else
                {
                    if (boolHomeMenuActive)
                    {
                        homeMenu.gameObject.SetActive(false);
                        boolHomeMenuActive = false;
                        ActivateExitGame();
                        Debug.Log("Evento de salto realizado con el gamepad--BOOL HOME MENU ACTIVE = TRUE // DESACTIVAR HOME MENU");
                        btExitGame.Select();
                    }
                    else
                    {
                        homeMenu.gameObject.SetActive(true);
                        boolHomeMenuActive = true;
                        ActivateHomeMenu();
                        Debug.Log("Evento de salto realizado con el gamepad--BOOL HOME MENU ACTIVE = FALSE // ACTIVAR HOME MENU");
                    }
                }
            }
            else
            {
                //Debug.Log("Algun booleano en false BT FIRE, joystickIzquierdoMovido-->"+ joystickIzquierdoMovido+ " mouseMovido-->" + mouseMovido);
            }


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