using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicAudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer musicAudioMixer;

    public void MusicController(float musicSlider)
    {
        musicAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider)*20);
    }

}
