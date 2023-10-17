using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXAudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer sfxAudioMixer;

    public void MusicController(float musicSlider)
    {
        sfxAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider) * 20);
    }
}
