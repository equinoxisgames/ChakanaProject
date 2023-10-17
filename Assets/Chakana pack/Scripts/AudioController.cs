using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer soundAudioMixer;


    public void MasterController(float masterSlider)
    {
        soundAudioMixer.SetFloat("MasterVolume", Mathf.Log10(masterSlider) * 20);
    }

    public void MusicController(float musicSlider)
    {
        soundAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider) * 20);
    }

    public void SFXController(float SFXSlider)
    {
        soundAudioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXSlider) * 20);
    }
}
