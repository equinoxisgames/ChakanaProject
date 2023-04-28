using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuMuerte : MonoBehaviour
{
    public void changeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
        correccionLogicas();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void changeScene()
    {
        PlayerData data = SaveManager.LoadPlayerData();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        correccionLogicas();
        if (data != null)
        {
            SceneManager.LoadScene(data.getSceneName());
            //PlayerPrefs.SetFloat("nextPositionXPrefsName", data.getX());
            //PlayerPrefs.SetFloat("nextPositionYPrefsName", data.getY());
        }
        else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void correccionLogicas() {
        Physics2D.IgnoreLayerCollision(0, 3, false);
        Physics2D.IgnoreLayerCollision(11, 3, false);
        //this.gameObject.tag = "Player";
        //this.gameObject.layer = 11;
    }

}
