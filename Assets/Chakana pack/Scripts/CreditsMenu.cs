using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

public class CreditsMenu : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("LoadPanel")]
    [SerializeField] GameObject loadPanel;
    [SerializeField] Slider loadBar;

    public TMP_Dropdown dropdownWindowMode;

    public Button btExit;

    private bool corutinaIniciada = false;
    private bool sceneChange = false;
    string escena;

    void Start()
    {
        LoadSettings();


        //btExit.Select();
        escena = SceneManager.GetActiveScene().name;

        if (escena == "00- Intro Start Game")
        {
            Invoke("OpenStartRoom", 22f);
        }

    }

    public void LoadSettings()
    {
        int FullScreenKeyValue = PlayerPrefs.GetInt("FullScreenKeyValue", 0);
        dropdownWindowMode.value = FullScreenKeyValue;

        //Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        //return;
        //Debug.Log("LoadSettings() : FullScreenKeyValue> "+ FullScreenKeyValue);

        //if (FullScreenKeyValue == 0)
        //{
        //    Resolution[] resolutions = Screen.resolutions;
        //    Resolution maxResolution = resolutions[resolutions.Length - 1];
        //    int maxWidthResolution = maxResolution.width;
        //    int maxHeightResolution = maxResolution.height;
        //    Screen.SetResolution(maxWidthResolution, maxHeightResolution, FullScreenMode.FullScreenWindow, maxResolution.refreshRateRatio);
        //    Screen.fullScreen = true;
        //    Debug.Log("Resolucion Full Screen");
        //}
        //else
        //{
        //    Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        //    Debug.Log("Resolucion 1280x720");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //btExit.Select();

        //if (Input.GetMouseButtonDown(0))
        //{
        //    btExit.Select();
        //}


        //if (Input.GetButtonDown("Cancel"))
        //{
        //    OpenMainMenu();
        //}

        //if (Input.GetButtonDown("Fire2"))
        //{
        //    OpenMainMenu();
        //}

    }
    public void OpenMainMenu()
    {
        

        if (sceneChange) return;
        sceneChange = true;

        loadPanel.SetActive(true);

        //if (!corutinaIniciada)
        //{
            StartCoroutine(LoadAsyncScene(1));
            //corutinaIniciada = true;
        //}

       
    }

    public void OpenStartRoom()
    {
        

        if (sceneChange) return;
        sceneChange = true;

        loadPanel.SetActive(true);

        //if (!corutinaIniciada)
        //{
        StartCoroutine(LoadAsyncScene(2));
        //corutinaIniciada = true;
        //}


    }

    public void OpenMainRoom()
    {
        

        if (sceneChange) return;
        sceneChange = true;

        loadPanel.SetActive(true);

        StartCoroutine(LoadAsyncScene(1));
    }

    IEnumerator LoadAsyncScene(int sceneIndex)
    {

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        //SceneManager.LoadScene(9 + 1);
        //yield return new WaitForSeconds(1f);
        //while(!asyncOperation.isDone)
        //{
        //    Debug.Log("Progres: "+asyncOperation.progress);
        //    loadBar.value = asyncOperation.progress+0.05f;
        //    yield return null;
        //}

        for (int i = 0; i < 20; i++)
        {
            //Debug.Log("Progres: "+asyncOperation.progress);
            //loadBar.value = asyncOperation.progress+0.05f;
            if (i < 15)
            {
                loadBar.value = i * 0.01f;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                loadBar.value = i * 0.09f + 0.16f;
                yield return new WaitForSeconds(1.5f);
            }
        }

        //yield break;
    }
}
