using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] [Range(0f, 1f)] private float backgroundVolume = 0.5f; // ajusta este valor en el inspector

    private AudioSource backgroundMusicSource;
    private Coroutine activeFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        backgroundMusicSource = GetComponent<AudioSource>();
        if (backgroundMusicSource == null)
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();

        if (backgroundClip != null)
        {
            backgroundMusicSource.clip = backgroundClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.volume = backgroundVolume;
            backgroundMusicSource.Play();
        }
    }

    public void CrossfadeToCombat(AudioSource combatSource, AudioClip combatClip, float targetCombatVolume = 0.7f)
    {
        if (activeFadeCoroutine != null)
            StopCoroutine(activeFadeCoroutine);

        activeFadeCoroutine = StartCoroutine(
            CrossfadeRoutine(
                fadeOut: backgroundMusicSource,
                fadeIn: combatSource,
                clipToPlay: combatClip,
                targetVolumeIn: targetCombatVolume
            )
        );
    }

    public void CrossfadeToBackground(AudioSource combatSource)
    {
        if (activeFadeCoroutine != null)
            StopCoroutine(activeFadeCoroutine);

        activeFadeCoroutine = StartCoroutine(
            CrossfadeRoutine(
                fadeOut: combatSource,
                fadeIn: backgroundMusicSource,
                clipToPlay: null,
                targetVolumeIn: backgroundVolume  // restaura al volumen original, no a 1f
            )
        );
    }

    private IEnumerator CrossfadeRoutine(AudioSource fadeOut, AudioSource fadeIn,
                                         AudioClip clipToPlay, float targetVolumeIn)
    {
        float startVolumeOut = fadeOut.volume;
        float startVolumeIn = fadeIn.volume;
        float elapsed = 0f;

        if (clipToPlay != null)
        {
            fadeIn.clip = clipToPlay;
            fadeIn.volume = 0f;
            fadeIn.loop = true;
            fadeIn.Play();
        }
        else
        {
            fadeIn.volume = 0f;
            if (!fadeIn.isPlaying)
                fadeIn.UnPause();
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
            fadeIn.volume = Mathf.Lerp(0f, targetVolumeIn, t);

            yield return null;
        }

        fadeOut.volume = 0f;
        fadeIn.volume = targetVolumeIn;

        fadeOut.Pause();

        activeFadeCoroutine = null;
    }
}