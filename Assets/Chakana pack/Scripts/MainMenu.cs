using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("00- StartRoom 1");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
