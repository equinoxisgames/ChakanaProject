using UnityEngine;
using Michsky.UI.Dark;

/// <summary>
/// Controlador del selector de idioma.
/// Detecta el idioma del sistema, lo persiste en PlayerPrefs y sincroniza
/// el HorizontalSelector sin aplicar el cambio de idioma en ese momento.
/// El cambio real de idioma ocurre sólo a través de los métodos públicos.
/// </summary>
public class LanguageSelectorController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constantes
    // -------------------------------------------------------------------------

    /// <summary>Clave usada en PlayerPrefs para persistir el código BCP-47.</summary>
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private HorizontalSelector languageSelector;

    // -------------------------------------------------------------------------
    // Datos de idiomas soportados
    // -------------------------------------------------------------------------

    /// <summary>
    /// Índices del HorizontalSelector:
    ///   0 → Español  ("es")
    ///   1 → English  ("en")
    /// Amplía este array si agregas más idiomas al selector.
    /// </summary>
    private static readonly string[] SupportedCodes = { "en", "es" };

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        InitializeLanguage();
    }

    // -------------------------------------------------------------------------
    // Inicialización
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determina el idioma a usar (PlayerPrefs → sistema) y setea el selector.
    /// No aplica ningún cambio de idioma en el sistema de localización.
    /// </summary>
    private void InitializeLanguage()
    {
        string codeToUse;

        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            // Idioma previamente guardado
            codeToUse = PlayerPrefs.GetString(LANGUAGE_PREF_KEY);
            Debug.Log($"[LanguageSelectorController] Idioma guardado encontrado: {codeToUse}");
        }
        else
        {
            // Primera ejecución: detectar idioma del sistema
            codeToUse = DetectAndSaveSystemLanguage();
        }

        // Sólo actualiza el HorizontalSelector, sin cambiar el idioma activo
        SetSelectorToCode(codeToUse);
    }

    /// <summary>
    /// Lee el código BCP-47 guardado en PlayerPrefs bajo <see cref="LANGUAGE_PREF_KEY"/>.
    /// Si no existe, usa el idioma del sistema como fallback, lo guarda y lo devuelve.
    /// </summary>
    /// <returns>Código BCP-47 guardado en PlayerPrefs, o fallback del sistema.</returns>
    private string DetectAndSaveSystemLanguage()
    {
        Debug.Log($"[LanguageSelectorController] Código leído de PlayerPrefs ({LANGUAGE_PREF_KEY})");

        string savedCode;

        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            savedCode = PlayerPrefs.GetString(LANGUAGE_PREF_KEY);
            Debug.Log($"[LanguageSelectorController] Código leído de PlayerPrefs ({LANGUAGE_PREF_KEY}): {savedCode}");
            return savedCode;
        }

        //prueba para ver que tiene LANGUAGE_PREF_KEY

        savedCode = PlayerPrefs.GetString(LANGUAGE_PREF_KEY);
        Debug.Log($"[LanguageSelectorController] Código leído de PlayerPrefs ({LANGUAGE_PREF_KEY}): {savedCode}");

        // Fallback: primera vez que se ejecuta y no hay nada guardado
        string fallbackCode = SystemLanguageToBcp47(Application.systemLanguage);
        Debug.Log($"[LanguageSelectorController] No hay PlayerPref guardado. Fallback del sistema: {fallbackCode}");
        SaveLanguageCode(fallbackCode);
        return fallbackCode;
    }

    // -------------------------------------------------------------------------
    // Selector UI
    // -------------------------------------------------------------------------

    /// <summary>
    /// Asigna el índice correcto al HorizontalSelector según el código BCP-47
    /// y llama a SetupSelector(). No dispara ningún cambio de idioma.
    /// </summary>
    /// <param name="code">Código BCP-47 (ej: "es", "en").</param>
    private void SetSelectorToCode(string code)
    {
        int index = CodeToIndex(code);

        if (languageSelector == null)
        {
            Debug.LogWarning("[LanguageSelectorController] languageSelector no está asignado.");
            return;
        }

        languageSelector.defaultIndex = index;
        languageSelector.index = index;
        languageSelector.SetupSelector();

        Debug.Log($"[LanguageSelectorController] Selector actualizado → índice {index} ({code})");
    }

    // -------------------------------------------------------------------------
    // Métodos públicos de cambio de idioma
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cambia el idioma activo mediante código BCP-47.
    /// Guarda la preferencia en PlayerPrefs y actualiza el selector.
    /// Conecta aquí con tu sistema de localización (ej: LocalizationSettings).
    /// </summary>
    /// <param name="code">Código BCP-47 (ej: "es", "en").</param>
    public void ChangeLanguageByCode(string code)
    {
        if (!IsCodeSupported(code))
        {
            Debug.LogWarning($"[LanguageSelectorController] Código '{code}' no soportado. Operación cancelada.");
            return;
        }

        SaveLanguageCode(code);
        SetSelectorToCode(code);
        ApplyLanguage(code);

        Debug.Log($"[LanguageSelectorController] Idioma cambiado a: {code}");
    }

    /// <summary>
    /// Cambia el idioma activo mediante el índice del HorizontalSelector.
    /// Guarda la preferencia en PlayerPrefs y actualiza el selector.
    /// </summary>
    /// <param name="index">
    /// Índice del selector:
    ///   0 → Español ("es")
    ///   1 → English ("en")
    /// </param>
    public void ChangeLanguageByIndex(int index)
    {
        if (index < 0 || index >= SupportedCodes.Length)
        {
            Debug.LogWarning($"[LanguageSelectorController] Índice {index} fuera de rango.");
            return;
        }

        string code = SupportedCodes[index];
        SaveLanguageCode(code);
        SetSelectorToCode(code);
        ApplyLanguage(code);

        Debug.Log($"[LanguageSelectorController] Idioma cambiado a índice {index} ({code})");
    }

    // -------------------------------------------------------------------------
    // Aplicación real del idioma
    // -------------------------------------------------------------------------

    /// <summary>
    /// Punto de integración con el sistema de localización del proyecto.
    /// Reemplaza el cuerpo de este método según tu solución:
    ///   - Unity Localization Package  → LocalizationSettings.SelectedLocale
    ///   - I2 Localization             → LocalizationManager.CurrentLanguage
    ///   - Sistema propio              → tu API aquí
    /// </summary>
    /// <param name="code">Código BCP-47 del idioma a activar.</param>
    private void ApplyLanguage(string code)
    {
        // ----------------------------------------------------------------
        // EJEMPLO con Unity Localization Package:
        // ----------------------------------------------------------------
        // var locale = LocalizationSettings.AvailableLocales.Locales
        //     .Find(l => l.Identifier.Code == code);
        // if (locale != null)
        //     LocalizationSettings.SelectedLocale = locale;
        // ----------------------------------------------------------------

        Debug.Log($"[LanguageSelectorController] ApplyLanguage({code}) → integra tu sistema de localización aquí.");
    }

    // -------------------------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------------------------

    /// <summary>Persiste el código en PlayerPrefs y hace flush inmediato.</summary>
    private static void SaveLanguageCode(string code)
    {
        PlayerPrefs.SetString(LANGUAGE_PREF_KEY, code);
        PlayerPrefs.Save();
    }

    /// <summary>Devuelve el índice del selector para el código dado (fallback → 0).</summary>
    private static int CodeToIndex(string code)
    {
        for (int i = 0; i < SupportedCodes.Length; i++)
        {
            if (SupportedCodes[i] == code)
                return i;
        }

        Debug.LogWarning($"[LanguageSelectorController] Código '{code}' no mapeado. Usando índice 0 (en).");
        return 0;
    }

    /// <summary>Indica si el código está en la lista de soportados.</summary>
    private static bool IsCodeSupported(string code)
    {
        foreach (string c in SupportedCodes)
            if (c == code) return true;
        return false;
    }

    /// <summary>
    /// Convierte <see cref="SystemLanguage"/> al código BCP-47 de dos letras.
    /// Amplía el switch para los idiomas adicionales que soporte tu juego.
    /// </summary>
    private static string SystemLanguageToBcp47(SystemLanguage lang)
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
            case SystemLanguage.Chinese: return "zh";
            case SystemLanguage.Japanese: return "ja";
            case SystemLanguage.Korean: return "ko";
            case SystemLanguage.Arabic: return "ar";
            default: return "en"; // fallback
        }
    }
}