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
        sliderSelected.Select();
    }

    public void MusicController(float musicSlider)
    {
        soundAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider) * 20);
        sliderSelected.Select();
    }

    public void SFXController(float SFXSlider)
    {
        soundAudioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXSlider) * 20);
        sliderSelected.Select();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            sliderMasterSelected.Select();
        }
    }
}
