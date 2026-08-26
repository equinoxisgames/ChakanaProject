using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Michsky.UI.Dark;

public class LocalizationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HorizontalSelector languageSelector;

    [Header("Settings")]
    [SerializeField] private string playerPrefKey = "SelectedLanguage";
    [SerializeField] private int defaultLanguageIndex = 0;

    void Start()
    {
        ApplySavedLanguage();
    }

    /// <summary>
    /// Convierte el valor string de SelectedLanguage a su índice correspondiente.
    /// "en" → 0, "es" → 1
    /// </summary>
    private int LanguageCodeToIndex(string code)
    {
        switch (code.ToLower())
        {
            case "en": return 0;
            case "es": return 1;
            default:
                Debug.LogWarning($"<b>[LocalizationController]</b> Código de idioma desconocido '{code}'. Se usará el índice por defecto ({defaultLanguageIndex}).");
                return defaultLanguageIndex;
        }
    }

    /// <summary>
    /// Lee el idioma guardado en PlayerPrefs ("en" o "es") y lo aplica al HorizontalSelector.
    /// </summary>
    public void ApplySavedLanguage()
    {
        if (languageSelector == null)
        {
            Debug.LogError("<b>[LocalizationController]</b> HorizontalSelector no asignado en el Inspector.", this);
            return;
        }

        if (languageSelector.itemList == null || languageSelector.itemList.Count == 0)
        {
            Debug.LogWarning("<b>[LocalizationController]</b> El HorizontalSelector no tiene ítems configurados.", this);
            return;
        }

        string savedLanguage = PlayerPrefs.GetString(playerPrefKey, "en");
        int resolvedIndex = LanguageCodeToIndex(savedLanguage);

        SetLanguage(resolvedIndex);
    }

    /// <summary>
    /// Establece el idioma en el HorizontalSelector por índice y guarda la selección en PlayerPrefs.
    /// </summary>
    /// <param name="index">Índice del ítem de idioma a seleccionar.</param>
    public void SetLanguage(int index)
    {
        if (languageSelector == null) return;

        if (index < 0 || index >= languageSelector.itemList.Count)
        {
            Debug.LogError($"<b>[LocalizationController]</b> Índice {index} inválido.", this);
            return;
        }

        languageSelector.defaultIndex = index;
        languageSelector.index = index;
        languageSelector.SetupSelector();

        SaveLanguage(index);

        Debug.Log($"<b>[LocalizationController]</b> Idioma aplicado: {languageSelector.itemList[index].itemTitle} (índice {index}).");
    }

    /// <summary>
    /// Establece el idioma buscando por nombre del ítem en el HorizontalSelector.
    /// </summary>
    /// <param name="languageName">Nombre del idioma tal como está definido en el ítem del selector.</param>
    public void SetLanguageByName(string languageName)
    {
        if (languageSelector == null) return;

        int foundIndex = languageSelector.itemList.FindIndex(item =>
            item.itemTitle.Equals(languageName, System.StringComparison.OrdinalIgnoreCase));

        if (foundIndex == -1)
        {
            Debug.LogWarning($"<b>[LocalizationController]</b> No se encontró el idioma '{languageName}' en el selector.", this);
            return;
        }

        SetLanguage(foundIndex);
    }

    /// <summary>
    /// Guarda el código de idioma en PlayerPrefs según el índice. 0 → "en", 1 → "es".
    /// </summary>
    public void SaveLanguage(int index)
    {
        string code = index == 0 ? "en" : "es";
        PlayerPrefs.SetString(playerPrefKey, code);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Retorna el código de idioma guardado en PlayerPrefs ("en" o "es").
    /// </summary>
    public string GetSavedLanguageCode()
    {
        return PlayerPrefs.GetString(playerPrefKey, "en");
    }

    /// <summary>
    /// Retorna el índice correspondiente al idioma guardado en PlayerPrefs.
    /// </summary>
    public int GetSavedLanguageIndex()
    {
        return LanguageCodeToIndex(GetSavedLanguageCode());
    }

    /// <summary>
    /// Retorna el nombre del ítem de idioma actualmente activo en el selector.
    /// </summary>
    public string GetSavedLanguageName()
    {
        int savedIndex = GetSavedLanguageIndex();

        if (languageSelector == null || languageSelector.itemList == null ||
            savedIndex >= languageSelector.itemList.Count)
            return string.Empty;

        return languageSelector.itemList[savedIndex].itemTitle;
    }
}