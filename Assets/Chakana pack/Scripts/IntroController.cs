using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[System.Serializable]
public struct SubtitleData
{
    public float startTime;
    public float endTime;
    // El texto ya no se edita aquí en el Inspector; viene de la localización.
    // Se mantiene el campo para compatibilidad con el Inspector si se necesita fallback.
    [TextArea(2, 3)] public string text;
}

public class IntroController : MonoBehaviour
{
    [Header("Referencias de UI (TextMeshPro)")]
    public TextMeshProUGUI prologueText;
    public TextMeshProUGUI subtitleText;

    [Header("Referencias de Video")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu";

    [Header("Tiempos del Prólogo (Pantalla Negra)")]
    public float prologueReadingTime = 4f;
    public float textFadeDuration = 1.5f;

    [Header("Audio del Prólogo")]
    public AudioSource prologueAudio;
    public float audioFadeDuration = 1.5f;

    [Header("Configuración de Subtítulos")]
    public SubtitleData[] subtitles;

    [Header("Opciones Adicionales")]
    public bool canSkipIntro = true;

    // Claves de localización
    private const string TABLE_NAME = "ChakanaGameText"; // <-- Cambia esto al nombre de tu tabla
    private const string KEY_PROLOGUE = "INTRO_PROLOGUE_TEXT";
    private const string KEY_SUBTITLE_1 = "INTRO_SUBTITLE_TEXT_1";
    private const string KEY_SUBTITLE_2 = "INTRO_SUBTITLE_TEXT_2";
    private const string KEY_SUBTITLE_3 = "INTRO_SUBTITLE_TEXT_3";
    private const string KEY_SUBTITLE_4 = "INTRO_SUBTITLE_TEXT_4";

    private bool isSkipping = false;
    private bool isVideoFinished = false;

    // VARIABLES PARA DEBUG
    private float scriptStartTime;
    private string lastSubtitleShown = "";

    private void Start()
    {
        scriptStartTime = Time.time;
        Debug.Log($"<color=cyan>[IntroController] [0.00s]</color> INICIO DEL SCRIPT. Preparando UI.");

        subtitleText.text = "";
        SetTextAlpha(prologueText, 0f);
        prologueText.text = "";

        if (prologueAudio != null)
        {
            Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Dando Play al Audio del Prólogo.");
            prologueAudio.Play();
        }

        videoPlayer.loopPointReached += EndReached;

        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        if (canSkipIntro && !isSkipping)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log($"<color=yellow>[IntroController] [{GetTime()}]</color> SALTO DETECTADO por teclado.");
                SkipIntro();
            }
        }
    }

    private string GetTime()
    {
        return (Time.time - scriptStartTime).ToString("F2") + "s";
    }

    private void EndReached(VideoPlayer vp)
    {
        Debug.Log($"<color=red>[IntroController] [{GetTime()}]</color> EVENTO LoopPointReached disparado. Tiempo: {vp.time}s");
        isVideoFinished = true;
    }

    // ─────────────────────────────────────────────
    //  LOCALIZACIÓN
    // ─────────────────────────────────────────────

    /// <summary>
    /// Aplica el idioma guardado en PlayerPrefs ("SelectedLanguage") al sistema
    /// de localización de Unity, y espera a que el cambio sea efectivo.
    /// </summary>
    private IEnumerator ApplySelectedLanguage()
    {
        // Esperar a que el sistema de localización esté listo
        yield return LocalizationSettings.InitializationOperation;

        string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "en");
        Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Idioma guardado en PlayerPrefs: '{savedLanguage}'");

        if (!string.IsNullOrEmpty(savedLanguage))
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            UnityEngine.Localization.Locale matchedLocale = null;

            foreach (var locale in locales)
            {
                // Comparamos contra el código ISO (e.g. "es", "en") y el nombre completo
                if (locale.Identifier.Code.Equals(savedLanguage, System.StringComparison.OrdinalIgnoreCase) ||
                    locale.Identifier.CultureInfo?.Name.Equals(savedLanguage, System.StringComparison.OrdinalIgnoreCase) == true ||
                    locale.name.Equals(savedLanguage, System.StringComparison.OrdinalIgnoreCase))
                {
                    matchedLocale = locale;
                    break;
                }
            }

            if (matchedLocale != null)
            {
                Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Aplicando locale: {matchedLocale.Identifier.Code}");
                LocalizationSettings.SelectedLocale = matchedLocale;

                // Esperar a que el cambio de locale termine de cargar
                while (LocalizationSettings.SelectedLocale != matchedLocale)
                    yield return null;
            }
            else
            {
                Debug.LogWarning($"[IntroController] No se encontró un locale que coincida con '{savedLanguage}'. Se usará el locale por defecto.");
            }
        }
        else
        {
            Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> No hay idioma guardado. Se usa el locale por defecto del sistema.");
        }
    }

    /// <summary>
    /// Obtiene un string localizado de forma síncrona (una vez que el locale ya está activo).
    /// </summary>
    private string GetLocalizedString(string key)
    {
        var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TABLE_NAME, key);
        // Como el locale ya está inicializado y cargado, la operación debería estar lista de inmediato.
        if (op.IsDone)
        {
            return op.Result;
        }
        // Fallback: espera bloqueante (no recomendado en runtime, pero es seguro aquí
        // porque llamamos solo después de que el locale esté completamente cargado).
        op.WaitForCompletion();
        return op.Result;
    }

    /// <summary>
    /// Carga los textos localizados y los aplica al prólogo y a los subtítulos.
    /// </summary>
    private void ApplyLocalizedTexts()
    {
        // Prólogo
        string localizedPrologue = GetLocalizedString(KEY_PROLOGUE);
        prologueText.text = localizedPrologue;
        Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Prólogo localizado: '{localizedPrologue}'");

        // Subtítulos: mapeamos las 4 claves a los elementos del array
        string[] subtitleKeys = { KEY_SUBTITLE_1, KEY_SUBTITLE_2, KEY_SUBTITLE_3, KEY_SUBTITLE_4 };

        for (int i = 0; i < subtitleKeys.Length; i++)
        {
            if (i < subtitles.Length)
            {
                subtitles[i].text = GetLocalizedString(subtitleKeys[i]);
                Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Subtítulo [{i}] localizado: '{subtitles[i].text}'");
            }
            else
            {
                Debug.LogWarning($"[IntroController] El array 'subtitles' no tiene elemento en índice {i} para la clave '{subtitleKeys[i]}'.");
            }
        }
    }

    // ─────────────────────────────────────────────
    //  SECUENCIA PRINCIPAL
    // ─────────────────────────────────────────────

    private IEnumerator PlayIntroSequence()
    {
        // 1. Aplicar idioma ANTES de mostrar cualquier texto
        yield return ApplySelectedLanguage();

        // 2. Cargar textos localizados
        ApplyLocalizedTexts();

        Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Iniciando fundido de entrada del texto (Fade In).");
        yield return FadeText(prologueText, 1f, textFadeDuration);

        Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Esperando tiempo de lectura del prólogo ({prologueReadingTime}s).");
        yield return new WaitForSeconds(prologueReadingTime);

        Debug.Log($"<color=cyan>[IntroController] [{GetTime()}]</color> Iniciando Fade Out del texto y audio.");
        if (prologueAudio != null)
            StartCoroutine(FadeOutAudio(prologueAudio, audioFadeDuration));

        yield return FadeText(prologueText, 0f, textFadeDuration);
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"<color=orange>[IntroController] [{GetTime()}]</color> Preparando VideoPlayer en memoria...");
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        Debug.Log($"<color=green>[IntroController] [{GetTime()}]</color> Video PREPARADO. Duración: {videoPlayer.length}s. Dando Play...");
        videoPlayer.Play();

        while (!isVideoFinished)
        {
            UpdateSubtitles((float)videoPlayer.time);
            yield return null;
        }

        Debug.Log($"<color=orange>[IntroController] [{GetTime()}]</color> Video terminado. Pausa final de 1.5s.");
        yield return new WaitForSeconds(1.5f);

        if (!isSkipping)
        {
            Debug.Log($"<color=green>[IntroController] [{GetTime()}]</color> Cargando escena: {nextSceneName}");
            LoadNextScene();
        }
    }

    // ─────────────────────────────────────────────
    //  SUBTÍTULOS, FADE, AUDIO, ESCENA
    // ─────────────────────────────────────────────

    private void UpdateSubtitles(float currentTime)
    {
        string currentText = "";
        foreach (var sub in subtitles)
        {
            if (currentTime >= sub.startTime && currentTime <= sub.endTime)
            {
                currentText = sub.text;
                break;
            }
        }

        if (currentText != lastSubtitleShown)
        {
            lastSubtitleShown = currentText;
            subtitleText.text = currentText;
            if (!string.IsNullOrEmpty(currentText))
                Debug.Log($"<color=white>[IntroController] [VideoTime: {currentTime:F2}s]</color> Subtítulo: '{currentText}'");
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI textUI, float targetAlpha, float duration)
    {
        float startAlpha = textUI.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            SetTextAlpha(textUI, Mathf.Lerp(startAlpha, targetAlpha, time / duration));
            yield return null;
        }

        SetTextAlpha(textUI, targetAlpha);
    }

    private void SetTextAlpha(TextMeshProUGUI textUI, float alpha)
    {
        Color c = textUI.color;
        c.a = alpha;
        textUI.color = c;
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private void SkipIntro()
    {
        isSkipping = true;
        Debug.Log($"<color=yellow>[IntroController] [{GetTime()}]</color> Saltando intro...");
        StopAllCoroutines();

        if (prologueAudio != null) prologueAudio.Stop();
        if (videoPlayer != null) videoPlayer.Stop();

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log($"<color=green>[IntroController] [{GetTime()}]</color> === CARGANDO ESCENA: {nextSceneName} ===");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= EndReached;
    }
}