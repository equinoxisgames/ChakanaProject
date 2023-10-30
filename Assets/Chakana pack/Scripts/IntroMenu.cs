using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("LoadPanel")]
    [SerializeField] GameObject loadPanel;
    [SerializeField] Slider loadBar;

   

    private bool corutinaIniciada = false;

    void Start()
    {
        Screen.fullScreen = true;

        PlayerPrefs.DeleteAll();
        Resolution[] resolutions = Screen.resolutions;
        Resolution maxResolution = resolutions[resolutions.Length - 1];
        int maxWidthResolution = maxResolution.width;
        int maxHeightResolution = maxResolution.height;

        Screen.SetResolution(maxWidthResolution, maxHeightResolution, FullScreenMode.ExclusiveFullScreen, maxResolution.refreshRateRatio);

        Debug.Log("maxWidthResolution: " + maxWidthResolution + " / maxHeightResolution: " + maxHeightResolution + " / maxResolution.refreshRate :" + maxResolution.refreshRateRatio.ToString());

        //Screen.fullScreen = true;

        PlayerPrefs.SetInt("FullScreenKeyValue", 0);
        //PlayerPrefs.Save();

    }

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        
    }

    // Update is called once per frame
    void Update()
    {
        

        




    }
    public void OpenMainMenu()
    {
        PlayerPrefs.DeleteAll();
        
        loadPanel.SetActive(true);

        //if (!corutinaIniciada)
        //{
            StartCoroutine(LoadAsyncScene(1));
            //corutinaIniciada = true;
        //}

       
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
