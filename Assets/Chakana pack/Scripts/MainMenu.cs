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

    private void Start()
    {
        btContinue.Select();
        Time.timeScale = 1f;

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
    void Update()
    {
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
                if (boolHomeMenuActive)
                {
                    homeMenu.gameObject.SetActive(false);
                    boolHomeMenuActive = false;
                    ActivateExitGame();
                    Debug.Log("--BOOL HOME MENU ACTIVE = TRUE // DESACTIVAR HOME MENU");
                    btExitGame.Select();
                }
                else
                {
                    homeMenu.gameObject.SetActive(true);
                    boolHomeMenuActive = true;
                    ActivateHomeMenu();
                    Debug.Log("--BOOL HOME MENU ACTIVE = FALSE // ACTIVAR HOME MENU");
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire2"))
            {
                if (escena != "00- Main Menu 0")
                {
                    Debug.Log("escena != 00 - Main Menu 0");
                }
                else
                {
                    
                }
            }
        }
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