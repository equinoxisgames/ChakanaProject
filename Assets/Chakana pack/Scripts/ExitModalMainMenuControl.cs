using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Assets.FantasyInventory.Scripts.Interface.Elements;

public class ExitModalMainMenuControl : MonoBehaviour
{
    //variables
    public Button   btContinueExitGame;
    public bool     activateBtContinueExitGame = true;

    public Button btContinueGame;
    public bool activateBtContinueGame = true;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Validar movimiento del mouse
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            activateBtContinueExitGame = true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();

            if (leftStick != Vector2.zero && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) &&
                !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                Debug.Log("El joystick izquierdo del gamepad se ha movido ");
                if (btContinueExitGame != null && btContinueExitGame.IsInteractable() && btContinueExitGame.gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                {
                    Debug.Log("El botón está seleccionado y habilitado.");
                    // Realiza aquí las acciones correspondientes cuando el botón está seleccionado y habilitado.
                }
                else
                {
                    Debug.Log("El botón no está seleccionado y/o no está habilitado.");
                    // Realiza aquí las acciones correspondientes cuando el botón no está seleccionado o no está habilitado.
                    if (activateBtContinueExitGame)
                    {
                        btContinueExitGame.Select();
                        activateBtContinueExitGame = false;
                    }
                }

                if (btContinueGame != null && btContinueGame.IsInteractable() && btContinueGame.gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                {
                    Debug.Log("El botón está seleccionado y habilitado.");
                    // Realiza aquí las acciones correspondientes cuando el botón está seleccionado y habilitado.
                }
                else
                {
                    Debug.Log("El botón no está seleccionado y/o no está habilitado.");
                    // Realiza aquí las acciones correspondientes cuando el botón no está seleccionado o no está habilitado.
                    if (activateBtContinueGame)
                    {
                        btContinueGame.Select();
                        activateBtContinueGame = false;
                    }
                }
            }
        }
    }
}
