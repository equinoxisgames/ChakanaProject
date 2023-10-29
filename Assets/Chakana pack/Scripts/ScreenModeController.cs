using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenModeController : MonoBehaviour
{
    public TMP_Dropdown dropdownWindowMode;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateFullScreen()
    {
        if (dropdownWindowMode.value == 0)
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution maxResolution = resolutions[resolutions.Length - 1];
            int maxWidthResolution = maxResolution.width;
            int maxHeightResolution = maxResolution.height;

            Screen.SetResolution(maxWidthResolution, maxHeightResolution, FullScreenMode.FullScreenWindow, maxResolution.refreshRateRatio);

            Debug.Log("maxWidthResolution: " + maxWidthResolution + " / maxHeightResolution: " + maxHeightResolution + " / maxResolution.refreshRate :"+ maxResolution.refreshRateRatio.ToString());

            Screen.fullScreen = true;

            PlayerPrefs.SetInt("FullScreenKeyValue", 0);
            PlayerPrefs.Save();
        }
        

        if (dropdownWindowMode.value == 1)
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

            PlayerPrefs.SetInt("FullScreenKeyValue", 1);
            PlayerPrefs.Save();

            Debug.Log("maxWidthResolution: " + 1280 + " / maxHeightResolution: " + 720);

            //Screen.fullScreen = false;
        }

    }
}
