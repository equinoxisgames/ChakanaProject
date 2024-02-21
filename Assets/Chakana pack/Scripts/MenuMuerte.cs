using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MenuMuerte : MonoBehaviour
{
    bool isDead;
    int sceneLoad;

    [SerializeField] GameObject loadScenePanel;

    private void Start()
    {
        isDead = true;
    }

    private void Update()
    {
        if (Input.anyKeyDown && isDead)
        {
            RespawnContinue();
            isDead = false;
        }
    }

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
            //SceneManager.LoadScene(data.getSceneName());
            //PlayerPrefs.SetFloat("nextPositionXPrefsName", data.getX());
            //PlayerPrefs.SetFloat("nextPositionYPrefsName", data.getY());
        }
        else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RespawnContinue()
    {
        PlayerPrefs.SetInt("scenePos", 0);
        Time.timeScale = 1;

        if (!PlayerPrefs.HasKey("respawn"))
        {
            sceneLoad = 2;
        }
        else sceneLoad = PlayerPrefs.GetInt("respawn");

        //StartCoroutine(LoadNextScene());

        StartCoroutine(LoadAsyncScene(sceneLoad));
    }

    IEnumerator LoadAsyncScene(int sceneIndex)
    {

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        yield return new WaitForSeconds(1f);
        //SceneManager.LoadScene(9 + 1);
        //yield return new WaitForSeconds(1f);
        //while(!asyncOperation.isDone)
        //{
        //    Debug.Log("Progres: "+asyncOperation.progress);
        //    loadBar.value = asyncOperation.progress+0.05f;
        //    yield return null;
        //}

        //for (int i = 0; i < 20; i++)
        //{
        //    //Debug.Log("Progres: "+asyncOperation.progress);
        //    //loadBar.value = asyncOperation.progress+0.05f;
        //    if (i < 15)
        //    {
        //        loadBar.value = i * 0.01f;
        //        yield return new WaitForSeconds(1f);
        //    }
        //    else
        //    {
        //        loadBar.value = i * 0.09f + 0.16f;
        //        yield return new WaitForSeconds(1.5f);
        //    }
        //}

        //yield break;
    }
    private void correccionLogicas() {
        Physics2D.IgnoreLayerCollision(0, 3, false);
        Physics2D.IgnoreLayerCollision(11, 3, false);
        //this.gameObject.tag = "Player";
        //this.gameObject.layer = 11;
    }

    IEnumerator LoadNextScene()
    {
        loadScenePanel.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneLoad);

        while (asyncLoad.isDone == false)
        {
            yield return null;
        }
    }
}
