using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer soundAudioMixer;
    public Slider sliderSelected;
    public Slider sliderMasterSelected;


    public void MasterController(float masterSlider)
    {
        soundAudioMixer.SetFloat("MasterVolume", Mathf.Log10(masterSlider) * 20);
        PlayerPrefs.SetFloat("MasterAudioKeyValue", masterSlider);
        PlayerPrefs.Save();
        sliderSelected.Select();
    }

    public void MusicController(float musicSlider)
    {
        soundAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider) * 20);
        PlayerPrefs.SetFloat("MusicAudioKeyValue", musicSlider);
        PlayerPrefs.Save();
        sliderSelected.Select();
    }

    public void SFXController(float SFXSlider)
    {
        soundAudioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXSlider) * 20);
        PlayerPrefs.SetFloat("SFXAudioKeyValue", SFXSlider);
        PlayerPrefs.Save();
        sliderSelected.Select();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            sliderMasterSelected.Select();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q))
        {
            sliderMasterSelected.Select();
        }

        if (Input.GetButtonDown("Jump"))
        {
            sliderMasterSelected.Select();
        }
        

    }
    
}
