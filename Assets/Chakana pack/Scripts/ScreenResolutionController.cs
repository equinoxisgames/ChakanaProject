using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenResolutionController : MonoBehaviour
{
    public TMP_Dropdown dropDownResolution;
    Resolution[] resolutionsList;

    // Start is called before the first frame update
    void Start()
    {
        FindResolutions();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FindResolutions()
    {
        resolutionsList = Screen.resolutions;
        dropDownResolution.ClearOptions();
        List<string> optionsDropDownList = new List<string>();
        int actualResolution = 0;

        Debug.Log("resolutionsList.Length:" + resolutionsList.Length);


        for (int i = 0; i < resolutionsList.Length; i++)
        {
            string optionDropDown = resolutionsList[i].width + " x " + resolutionsList[i].height + " @ " + (int)resolutionsList[i].refreshRateRatio.value + " Hz.";
            optionsDropDownList.Add(optionDropDown);

            if (Screen.fullScreen && resolutionsList[i].width == Screen.currentResolution.width &&
                resolutionsList[i].height == Screen.currentResolution.height)
            {
                actualResolution = i;
            }
        }



        dropDownResolution.AddOptions(optionsDropDownList);
        dropDownResolution.value = actualResolution;
        dropDownResolution.RefreshShownValue();

    }
    public void ChangeResolution(int resolutionIndex)
    {
        Resolution resolutionSelected = resolutionsList[resolutionIndex];
        Screen.SetResolution(resolutionSelected.width, resolutionSelected.height, Screen.fullScreen);
    }
}
