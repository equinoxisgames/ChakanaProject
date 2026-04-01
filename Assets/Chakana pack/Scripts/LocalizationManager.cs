using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LocalizationManager: Gestiona el idioma de la aplicación utilizando
/// el paquete de Localización de Unity. Detecta el idioma del sistema,
/// lo guarda en PlayerPrefs y permite cambiarlo en tiempo de ejecución.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // CONSTANTES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clave con la que se guarda el idioma seleccionado en PlayerPrefs.
    /// </summary>
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

    // ─────────────────────────────────────────────────────────────────────────
    // REFERENCIAS (asignar desde el Inspector)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("UI - Texto de idioma en pantalla")]
    [Tooltip("Texto TextMeshPro donde se mostrará el idioma activo.")]
    [SerializeField] private TextMeshProUGUI languageDisplayText;

    public TextMeshProUGUI txtButtonContinueNormal;
    public TextMeshProUGUI txtButtonContinueHighL;
    public Button btNewGame;

    // Si no usas TextMeshPro, descomenta la siguiente línea y comenta la de arriba:
    // [SerializeField] private Text languageDisplayText;

    // ─────────────────────────────────────────────────────────────────────────
    // VARIABLES PRIVADAS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Indica si el sistema de localización ya terminó de inicializarse.
    /// </summary>
    private bool _isReady = false;

    // ─────────────────────────────────────────────────────────────────────────
    // CICLO DE VIDA DE UNITY
    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Iniciamos la coroutine que espera a que el sistema de localización
        // esté completamente listo antes de operar sobre él.
        StartCoroutine(InitializeLocalization());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Espera a que LocalizationSettings termine de inicializarse y luego
    /// aplica el idioma guardado (o detecta el del sistema si es la primera vez).
    /// </summary>
    private IEnumerator InitializeLocalization()
    {
        // Esperamos a que el paquete de localización de Unity esté listo.
        // InitializationOperation es una AsyncOperationHandle que debemos awaitar.
        yield return LocalizationSettings.InitializationOperation;

        _isReady = true;
        Debug.Log("[LocalizationManager] Sistema de localización listo.");

        // Comprobamos si ya existe un idioma guardado en PlayerPrefs.
        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            // Si existe, lo recuperamos y lo aplicamos.
            string savedCode = PlayerPrefs.GetString(LANGUAGE_PREF_KEY);
            Debug.Log($"[LocalizationManager] Idioma guardado encontrado: {savedCode}");
            ApplyLanguageByCode(savedCode);
        }
        else
        {
            // Si no existe, detectamos el idioma del sistema operativo.
            DetectAndSaveSystemLanguage();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DETECCIÓN DEL IDIOMA DEL SISTEMA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Detecta el idioma configurado en el sistema operativo (Application.systemLanguage),
    /// lo convierte a un código de idioma compatible con Unity Localization (ej: "es", "en")
    /// y lo guarda en PlayerPrefs.
    /// </summary>
    private void DetectAndSaveSystemLanguage()
    {
        // Unity expone el idioma del SO a través de Application.systemLanguage,
        // que devuelve un valor del enum SystemLanguage.
        SystemLanguage systemLang = Application.systemLanguage;

        // Convertimos el enum SystemLanguage a un código BCP-47 (ej: "es", "en", "fr").
        // Puedes ampliar este switch con los idiomas que soporte tu juego.
        string languageCode = ConvertSystemLanguageToCode(systemLang);

        Debug.Log($"[LocalizationManager] Idioma del sistema detectado: {systemLang} → código: {languageCode}");

        // Guardamos el código en PlayerPrefs para que persista entre sesiones.
        SaveLanguage(languageCode);

        // Aplicamos el idioma al sistema de localización de Unity.
        ApplyLanguageByCode(languageCode);
    }

    /// <summary>
    /// Convierte un valor de SystemLanguage al código de locale BCP-47 correspondiente.
    /// Si el idioma no está en la lista, se usa inglés ("en") como fallback.
    /// </summary>
    /// <param name="lang">Idioma detectado por Unity desde el sistema operativo.</param>
    /// <returns>Código de idioma en formato BCP-47 (ej: "es", "en", "pt").</returns>
    private string ConvertSystemLanguageToCode(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Spanish: return "es";
            case SystemLanguage.English: return "en";
            case SystemLanguage.French: return "fr";
            case SystemLanguage.German: return "de";
            case SystemLanguage.Italian: return "it";
            case SystemLanguage.Portuguese: return "pt";
            case SystemLanguage.Russian: return "ru";
            case SystemLanguage.Japanese: return "ja";
            case SystemLanguage.Korean: return "ko";
            case SystemLanguage.Chinese: return "zh";
            case SystemLanguage.Arabic: return "ar";
            case SystemLanguage.Dutch: return "nl";
            case SystemLanguage.Polish: return "pl";
            case SystemLanguage.Turkish: return "tr";
            // Agrega más casos según los idiomas que soporte tu juego.

            default:
                // Si el idioma no está soportado, usamos inglés como fallback.
                Debug.LogWarning($"[LocalizationManager] Idioma '{lang}' no mapeado. Se usará inglés como fallback.");
                return "en";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // APLICAR IDIOMA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Busca en las locales disponibles del proyecto la que coincida con el código
    /// dado y la establece como idioma activo en Unity Localization.
    /// </summary>
    /// <param name="code">Código BCP-47 del idioma deseado (ej: "es", "en").</param>
    public void ApplyLanguageByCode(string code)
    {
        if (!_isReady)
        {
            Debug.LogWarning("[LocalizationManager] El sistema de localización aún no está listo.");
            return;
        }

        // Recorremos todas las locales disponibles en el proyecto
        // (configuradas en Edit → Project Settings → Localization).
        ILocalesProvider localesProvider = LocalizationSettings.AvailableLocales;

        foreach (Locale locale in localesProvider.Locales)
        {
            // Comparamos el identificador de la locale con el código buscado.
            // locale.Identifier.Code devuelve el código BCP-47 (ej: "es", "en-US").
            if (locale.Identifier.Code.StartsWith(code, System.StringComparison.OrdinalIgnoreCase))
            {
                // Establecemos la locale seleccionada como activa.
                LocalizationSettings.SelectedLocale = locale;

                Debug.Log($"[LocalizationManager] Idioma aplicado: {locale.LocaleName} ({locale.Identifier.Code})");

                // Actualizamos el texto en pantalla.
                UpdateLanguageDisplay(locale);

                return;
            }
        }

        // Si no encontramos la locale, lo notificamos.
        Debug.LogWarning($"[LocalizationManager] No se encontró una locale para el código '{code}'. " +
                         "Verifica que esté agregada en Project Settings → Localization.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GUARDAR IDIOMA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Guarda el código de idioma en PlayerPrefs para que persista entre sesiones.
    /// </summary>
    /// <param name="code">Código BCP-47 a guardar (ej: "es", "en").</param>
    public void SaveLanguage(string code)
    {
        PlayerPrefs.SetString(LANGUAGE_PREF_KEY, code);

        // PlayerPrefs.Save() fuerza la escritura en disco inmediatamente.
        // En la mayoría de los casos Unity lo hace automático al cerrar,
        // pero es buena práctica llamarlo explícitamente.
        PlayerPrefs.Save();

        Debug.Log($"[LocalizationManager] Idioma guardado en PlayerPrefs: {LANGUAGE_PREF_KEY} = {code}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI - MOSTRAR IDIOMA EN PANTALLA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza el texto en pantalla con el nombre e identificador de la locale activa.
    /// </summary>
    /// <param name="locale">Locale actualmente seleccionada.</param>
    private void UpdateLanguageDisplay(Locale locale)
    {
        if (languageDisplayText == null)
        {
            Debug.LogWarning("[LocalizationManager] No se asignó un texto de UI para mostrar el idioma.");
            return;
        }

        // Mostramos el nombre completo del idioma y su código.
        // Ejemplo: "Español (es)"
        languageDisplayText.text = $"Idioma activo: {locale.LocaleName} ({locale.Identifier.Code})";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // API PÚBLICA (para usar desde otros scripts o botones de UI)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Cambia el idioma en tiempo de ejecución, lo guarda en PlayerPrefs y
    /// actualiza la UI. Puedes conectar este método a un botón en el Inspector.
    /// </summary>
    /// <param name="code">Código BCP-47 del idioma deseado (ej: "es", "en").</param>
    public void ChangeLanguage(string code)
    {
        SaveLanguage(code);
        ApplyLanguageByCode(code);
        //ValidateNewGameLocalizationText();
    }

    /// <summary>
    /// Devuelve el código del idioma actualmente guardado en PlayerPrefs.
    /// Útil para inicializar dropdowns o menús de selección de idioma.
    /// </summary>
    /// <returns>Código BCP-47 guardado, o string vacío si no hay ninguno.</returns>
    public string GetSavedLanguageCode()
    {
        return PlayerPrefs.GetString(LANGUAGE_PREF_KEY, string.Empty);
    }

    /// <summary>
    /// Elimina el idioma guardado en PlayerPrefs y vuelve a detectar el del sistema.
    /// Útil para un botón de "Restablecer configuración de idioma".
    /// </summary>
    public void ResetToSystemLanguage()
    {
        PlayerPrefs.DeleteKey(LANGUAGE_PREF_KEY);
        Debug.Log("[LocalizationManager] Preferencia de idioma eliminada. Detectando idioma del sistema...");
        DetectAndSaveSystemLanguage();
    }

    public void ValidateNewGameLocalizationText()
    {
        int flagGameSaved = 0;
        flagGameSaved = PlayerPrefs.GetInt("GameSaved", 0);
        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> flagGameSaved: {flagGameSaved}");


        string language = PlayerPrefs.GetString("SelectedLanguage", "en");
        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> SelectedLanguage: '{language}'");

        string continueText = language == "es" ? "CONTINUAR" : "CONTINUE";
        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> continueText resuelto: '{continueText}'");

        txtButtonContinueNormal.text = continueText;
        txtButtonContinueHighL.text = continueText;
        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> Textos actualizados en UI.");

        btNewGame.gameObject.SetActive(true);
        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> btNewGame activado.");


        Debug.Log($"<b>[ValidateNewGameLocalizationText]</b> flagGameSaved es 0, no se modifica la UI.");

    }
}