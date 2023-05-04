using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{

    public RectTransform pauseMenu;
    public RectTransform exitMenu;
    public RectTransform confirmQuitMenu;
    //public RectTransform exitMenu;
    public Transform hoyustusGameObject;
    public Button btContinue;
    public Button btExitGame;
    public Button btLoadSlot;


    string escena;

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
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
           
            escena = SceneManager.GetActiveScene().name;
            
            //Debug.Log("Entra Update");

            //exitMenu.gameObject.SetActive(false);

            if (escena != "00- Main Menu 0")
            {

                //Debug.Log("Entra Update y escena es diferente de Main Menu");

                if (!pauseMenu.gameObject.activeSelf && !confirmQuitMenu.gameObject.activeSelf)
                {
                    //Debug.Log("Entra Update y escena es diferente de Main Menu y Pause Menu esta deshabilitado y Confirm Menu esta deshabilitado");

                    pauseMenu.gameObject.SetActive(true);
                    btContinue.Select();
                    Time.timeScale = 0f;
                    //hoyustusGameObject.gameObject.SetActive(false);

                    

                }
                else
                {
                    //Debug.Log("Entra Update y escena es diferente de Main Menu y Pause Menu esta habilitado O Confirm Menu habilitado");
                    Time.timeScale = 1f;
                    if (pauseMenu.gameObject.activeSelf)
                        {
                        pauseMenu.gameObject.SetActive(false);
                        //hoyustusGameObject.gameObject.SetActive(true);
                    }
                    if (confirmQuitMenu.gameObject.activeSelf)
                    {
                        confirmQuitMenu.gameObject.SetActive(false);
                        //hoyustusGameObject.gameObject.SetActive(true);
                    }
                    
                }
            }
            
            btContinue.Select();


        }




    }

    public void Escape()
    {
        
        escena = SceneManager.GetActiveScene().name;

            if (escena != "00- Main Menu 0")
            {


                if (!pauseMenu.gameObject.activeSelf && !confirmQuitMenu.gameObject.activeSelf)
                {

                Debug.Log("Entra Escape al IF PauseMenu Deshabilitado y ConfirmQuitMenu Deshabilitado");
                    //pauseMenu.gameObject.SetActive(true);
                    btContinue.Select();
                Time.timeScale = 0f;
                //hoyustusGameObject.gameObject.SetActive(false);

                //Time.timeScale = 0f;

            }
                else
                {
                Time.timeScale = 1f;
                if (pauseMenu.gameObject.activeSelf)
                    {
                    Debug.Log("Entra al IF Else PauseMenu Habilitado");
                        pauseMenu.gameObject.SetActive(false);
                        //hoyustusGameObject.gameObject.SetActive(true);
                    }
                    if (confirmQuitMenu.gameObject.activeSelf)
                    {
                    Debug.Log("Entra al IF Else ConfirmQuitMenu Habilitado");
                        confirmQuitMenu.gameObject.SetActive(false);
                        //hoyustusGameObject.gameObject.SetActive(true);
                    }
                }
            
        }

        
         btContinue.Select();





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
        if(active)
        Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

}
