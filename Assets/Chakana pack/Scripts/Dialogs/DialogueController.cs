using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// ─────────────────────────────────────────────────────────────────────────────
// ESTRUCTURAS DE DATOS
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Modo de reproducción de sonido durante el tipeo.
/// </summary>
public enum TypingSoundMode
{
    /// <summary>Reproduce un sonido aleatorio por cada carácter escrito (con pausa extra tras cada espacio).</summary>
    PerCharacter,
    /// <summary>Reproduce un sonido aleatorio una vez por cada palabra completa.</summary>
    PerWord
}

/// <summary>
/// Representa una línea individual de diálogo dentro de una conversación.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Key de localización en la tabla 'ChakanaGameText'. El texto real se obtiene en runtime.")]
    public string localizationKey;

    [Tooltip("Índice del actor que pronuncia esta línea. Referencia a la lista actorImages del controlador.")]
    public int actorIndex;

    [Tooltip("AudioClip específico para esta línea. Si es null se usan los sonidos de tipeo globales de la conversación.")]
    public AudioClip audioClip;
}

/// <summary>
/// Representa una conversación completa compuesta por varias líneas de diálogo.
/// </summary>
[System.Serializable]
public class Conversation
{
    [Tooltip("Identificador único de esta conversación. Se usa para activarla por código o desde el inspector.")]
    public string conversationID;

    [Tooltip("Si es true, esta conversación es un monólogo y solo se mostrará la imagen del actor indicado en monologueActorIndex.")]
    public bool isMonologue;

    [Tooltip("Índice del actor único visible durante el monólogo. Solo aplica si isMonologue es true.")]
    public int monologueActorIndex;

    [Tooltip("Modo de reproducción de sonido de tipeo para esta conversación.")]
    public TypingSoundMode typingSoundMode;

    [Tooltip("Lista de líneas de diálogo que componen esta conversación.")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Tooltip("Evento opcional que se invoca al terminar la conversación. Si tiene listeners, se ejecuta antes de cerrar el panel.")]
    public UnityEvent onFinished;
}

// ─────────────────────────────────────────────────────────────────────────────
// CONTROLADOR PRINCIPAL
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// DialogueController — Controla la visualización de diálogos y monólogos para Unity 6.
/// Soporta múltiples conversaciones configurables desde el Inspector, localización via
/// Unity Localization Package, efecto de tipeo con sonidos, y transiciones visuales de actores.
/// </summary>
public class DialogueController : MonoBehaviour
{
    // ── UI ────────────────────────────────────────────────────────────────────

    [Header("── Referencias de UI ──────────────────────────────────")]

    [Tooltip("Panel raíz que contiene toda la interfaz de diálogo.")]
    public GameObject dialogPanel;

    [Tooltip("Lista de imágenes de los actores en pantalla. El nombre del GameObject de cada imagen se usa como nombre del actor.")]
    public List<Image> actorImages = new List<Image>();

    [Tooltip("TextMeshPro donde se muestra el nombre del actor activo.")]
    public TextMeshProUGUI actorNameText;

    [Tooltip("TextMeshPro donde se muestra el texto del diálogo con efecto de tipeo.")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Botón para avanzar al siguiente diálogo o completar el tipeo actual.")]
    public Button continueButton;

    // ── Audio ─────────────────────────────────────────────────────────────────

    [Header("── Audio ───────────────────────────────────────────────")]

    [Tooltip("Lista de sonidos de tipeo aleatorios. Se selecciona uno al azar en cada carácter o palabra según el modo activo.")]
    public List<AudioClip> typingSounds = new List<AudioClip>();

    [Tooltip("AudioSource utilizado para reproducir los sonidos de tipeo. Usa PlayOneShot para no cortar sonidos previos.")]
    public AudioSource audioSource;

    // ── Tipeo ─────────────────────────────────────────────────────────────────

    [Header("── Configuración de Tipeo ─────────────────────────────")]

    [Tooltip("Tiempo en segundos entre cada carácter revelado.")]
    public float typingSpeed = 0.04f;

    [Tooltip("Pausa adicional en segundos después de cada espacio (solo aplica en modo PerCharacter).")]
    public float wordPauseDuration = 0.1f;

    // ── Actores ───────────────────────────────────────────────────────────────

    [Header("── Configuración de Actores ──────────────────────────")]

    [Tooltip("Alpha del actor que está hablando actualmente.")]
    [Range(0f, 1f)] public float activeActorAlpha = 1f;

    [Tooltip("Alpha de los actores que no están hablando.")]
    [Range(0f, 1f)] public float inactiveActorAlpha = 0.4f;

    [Tooltip("Escala uniforme del actor activo.")]
    public float activeActorScale = 1.1f;

    [Tooltip("Escala uniforme de los actores inactivos.")]
    public float inactiveActorScale = 0.9f;

    [Tooltip("Duración en segundos de la transición suave de alpha y escala entre actores.")]
    public float actorTransitionDuration = 0.25f;

    // ── Conversaciones ────────────────────────────────────────────────────────

    [Header("── Conversaciones ──────────────────────────────────────")]

    [Tooltip("Lista de todas las conversaciones configuradas para este controlador.")]
    public List<Conversation> conversations = new List<Conversation>();

    [Tooltip("ID de la conversación que se inicia automáticamente en Start(). Dejar vacío para no auto-iniciar.")]
    public string startConversationID;

    // ── Estado interno ────────────────────────────────────────────────────────

    private Conversation _activeConversation;
    private int _currentLineIndex;
    private bool _isTyping;
    private Coroutine _typingCoroutine;
    private List<Coroutine> _transitionCoroutines = new List<Coroutine>();
    private string _currentLocale;
    private string _currentLineText;
    private Coroutine _subTypingCoroutine;
    private List<GameObject> _activatedParents = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    // Callback invocado al terminar cualquier conversación (suscribirse desde código externo)
    public System.Action OnConversationFinished;

    private void Awake()
    {
        // Ocultar el panel al inicio
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private void Start()
    {
        // Suscribir el botón Continuar
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonPressed);

        // Auto-iniciar conversación si se especificó un ID
        //if (!string.IsNullOrEmpty(startConversationID))
        //    StartConversation(startConversationID);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // API PÚBLICA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia la conversación con el ID indicado.
    /// Puede llamarse desde código externo o desde el Inspector vía UnityEvent.
    /// </summary>
    /// <param name="conversationID">ID único de la conversación a iniciar.</param>
    public void StartConversation(string conversationID)
    {
        Debug.Log($"[DialogueController] StartConversation llamado con ID: '{conversationID}'");

        // Evitar re-iniciar si ya hay una conversación activa
        if (_activeConversation != null)
        {
            Debug.LogWarning($"[DialogueController] Ya hay una conversación activa: '{_activeConversation.conversationID}'. Ignorando.");
            return;
        }

        Conversation conv = conversations.Find(c => c.conversationID == conversationID);

        if (conv == null)
        {
            Debug.LogError($"[DialogueController] No se encontró la conversación con ID: '{conversationID}'. IDs disponibles: {string.Join(", ", conversations.ConvertAll(c => c.conversationID))}");
            return;
        }

        if (conv.lines == null || conv.lines.Count == 0)
        {
            Debug.LogWarning($"[DialogueController] La conversación '{conversationID}' no tiene líneas configuradas.");
            return;
        }

        // Leer idioma desde PlayerPrefs y configurar locale
        _currentLocale = PlayerPrefs.GetString("SelectedLanguage", "en");
        ApplyLocale(_currentLocale);

        _activeConversation = conv;
        _currentLineIndex = 0;

        // Mostrar el panel
        if (dialogPanel != null)
        {
            // Activar toda la cadena de padres para garantizar visibilidad, recordando cuáles activamos
            _activatedParents.Clear();
            Transform t = dialogPanel.transform.parent;
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                {
                    Debug.Log($"[DialogueController] Activando padre inactivo: {t.gameObject.name}");
                    t.gameObject.SetActive(true);
                    _activatedParents.Add(t.gameObject);
                }
                t = t.parent;
            }
            dialogPanel.SetActive(true);
            string hierarchy = dialogPanel.name;
            Transform th = dialogPanel.transform.parent;
            while (th != null) { hierarchy = th.gameObject.name + (th.gameObject.activeSelf ? "" : "[INACTIVO]") + " > " + hierarchy; th = th.parent; }
            Debug.Log($"[DialogueController] dialogPanel activado. Es visible: {dialogPanel.activeInHierarchy}. Jerarquía: {hierarchy}");
        }
        else
        {
            Debug.LogError("[DialogueController] dialogPanel es NULL. No está asignado en el Inspector.");
        }

        // Configurar visibilidad de actores según monólogo/diálogo
        SetupActorVisibility();

        // Mostrar la primera línea
        ShowCurrentLine();
    }

    /// <summary>
    /// Detiene la conversación activa, cancela coroutines y oculta el panel.
    /// Puede llamarse desde código externo en cualquier momento.
    /// </summary>
    public void StopDialogue()
    {
        StopAllTypingAndTransitions();
        RestoreActorVisibility();

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // Restaurar padres que fueron activados por nosotros
        foreach (var parent in _activatedParents)
        {
            if (parent != null)
                parent.SetActive(false);
        }
        _activatedParents.Clear();

        _activeConversation = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOCALIZACIÓN
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Configura el locale activo en el sistema de localización de Unity
    /// usando el código de idioma obtenido desde PlayerPrefs.
    /// </summary>
    private void ApplyLocale(string localeCode)
    {
        if (LocalizationSettings.AvailableLocales == null)
        {
            Debug.LogWarning("[DialogueController] LocalizationSettings no está inicializado.");
            return;
        }

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var targetLocale = locales.Find(l => l.Identifier.Code == localeCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
        }
        else
        {
            Debug.LogWarning($"[DialogueController] No se encontró el locale '{localeCode}'. Se usará el locale por defecto.");
        }
    }

    /// <summary>
    /// Obtiene el texto localizado desde la tabla 'ChakanaGameText' usando la key indicada.
    /// Retorna la key literal como fallback si no se encuentra el texto.
    /// </summary>
    private string GetLocalizedText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[DialogueController] La localizationKey está vacía.");
            return string.Empty;
        }

        try
        {
            string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("ChakanaGameText", key);

            if (string.IsNullOrEmpty(localizedText))
            {
                Debug.LogWarning($"[DialogueController] La key '{key}' retornó texto vacío. Se usará la key como fallback.");
                return key;
            }

            return localizedText;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DialogueController] Error al obtener la key '{key}': {e.Message}. Se usará la key como fallback.");
            return key;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FLUJO DE DIÁLOGO
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra la línea actual de diálogo según el índice interno.
    /// </summary>
    private void ShowCurrentLine()
    {
        if (_activeConversation == null) return;

        DialogueLine line = _activeConversation.lines[_currentLineIndex];
        Debug.Log($"[DialogueController] ShowCurrentLine — índice {_currentLineIndex}, actorIndex: {line.actorIndex}, key: '{line.localizationKey}'");

        // Validar actorIndex
        if (!IsActorIndexValid(line.actorIndex)) return;

        // Actualizar nombre del actor (sin localización)
        if (actorNameText != null)
            actorNameText.text = actorImages[line.actorIndex].gameObject.name;

        // Aplicar efecto visual de actores (solo en modo diálogo)
        if (!_activeConversation.isMonologue)
            StartActorTransitions(line.actorIndex);

        // Obtener y cachear el texto localizado antes de iniciar el tipeo
        _currentLineText = GetLocalizedText(line.localizationKey);

        // Iniciar coroutine de tipeo
        StopTypingCoroutine();
        _typingCoroutine = StartCoroutine(TypeText(_currentLineText, line));
    }

    /// <summary>
    /// Callback del botón Continuar.
    /// Si hay tipeo en progreso: lo completa. Si no: avanza a la siguiente línea.
    /// </summary>
    private void OnContinueButtonPressed()
    {
        if (_activeConversation == null) return;

        if (_isTyping)
        {
            // Completar el texto instantáneamente
            CompleteCurrentLine();
        }
        else
        {
            // Avanzar a la siguiente línea
            AdvanceToNextLine();
        }
    }

    /// <summary>
    /// Detiene la coroutine de tipeo y muestra el texto completo de inmediato.
    /// </summary>
    private void CompleteCurrentLine()
    {
        StopTypingCoroutine();

        if (_activeConversation == null) return;

        if (dialogueText != null)
            dialogueText.text = _currentLineText;

        _isTyping = false;
    }

    /// <summary>
    /// Avanza al siguiente diálogo o finaliza la conversación si era el último.
    /// </summary>
    private void AdvanceToNextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _activeConversation.lines.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            FinishConversation();
        }
    }

    /// <summary>
    /// Ejecuta la lógica de cierre al terminar todos los diálogos.
    /// </summary>
    private void FinishConversation()
    {
        Debug.Log("[DialogueController] FinishConversation llamado.");
        StopAllTypingAndTransitions();
        RestoreActorVisibility();

        // Limpiar estado ANTES de invocar callbacks para evitar re-entrada inconsistente
        var conv = _activeConversation;
        _activeConversation = null;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        bool hasListeners = conv.onFinished != null &&
                            conv.onFinished.GetPersistentEventCount() > 0;

        if (hasListeners)
            conv.onFinished.Invoke();

        OnConversationFinished?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COROUTINE DE TIPEO
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Coroutine que revela el texto carácter por carácter con efecto de tipeo.
    /// </summary>
    private IEnumerator TypeText(string fullText, DialogueLine line)
    {
        _isTyping = true;
        Debug.Log($"[DialogueController] TypeText iniciado. Texto: '{fullText?.Substring(0, Mathf.Min(30, fullText?.Length ?? 0))}...' Panel activo en jerarquía: {dialogPanel?.activeInHierarchy}");

        if (dialogueText != null)
            dialogueText.text = string.Empty;
        else
            Debug.LogError("[DialogueController] dialogueText es NULL.");

        // Si la línea tiene un AudioClip específico, reproducirlo al inicio
        if (line.audioClip != null && audioSource != null)
            audioSource.PlayOneShot(line.audioClip);

        bool useLineAudio = line.audioClip != null;

        if (_activeConversation.typingSoundMode == TypingSoundMode.PerCharacter)
        {
            _subTypingCoroutine = StartCoroutine(TypePerCharacter(fullText, useLineAudio));
            yield return _subTypingCoroutine;
        }
        else
        {
            _subTypingCoroutine = StartCoroutine(TypePerWord(fullText, useLineAudio));
            yield return _subTypingCoroutine;
        }

        _isTyping = false;
    }

    /// <summary>
    /// Tipeo carácter por carácter con pausa extra tras cada palabra.
    /// </summary>
    private IEnumerator TypePerCharacter(string fullText, bool suppressTypingSounds)
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            dialogueText.text += c;

            if (c == ' ')
            {
                // Pausa extra tras espacio (fin de palabra)
                yield return new WaitForSeconds(wordPauseDuration);
            }
            else
            {
                // Reproducir sonido de tipeo aleatorio (solo si no hay audio de línea)
                if (!suppressTypingSounds)
                    PlayRandomTypingSound();

                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    /// <summary>
    /// Tipeo que reproduce un sonido por cada palabra completa al detectar espacio o fin de texto.
    /// </summary>
    private IEnumerator TypePerWord(string fullText, bool suppressTypingSounds)
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            dialogueText.text += c;

            bool isEndOfWord = (c == ' ' || i == fullText.Length - 1);

            if (isEndOfWord && !suppressTypingSounds)
                PlayRandomTypingSound();

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    /// <summary>
    /// Reproduce un AudioClip aleatorio de la lista typingSounds usando PlayOneShot.
    /// </summary>
    private void PlayRandomTypingSound()
    {
        if (typingSounds == null || typingSounds.Count == 0 || audioSource == null) return;

        AudioClip clip = typingSounds[Random.Range(0, typingSounds.Count)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ACTORES — VISIBILIDAD Y TRANSICIONES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Configura la visibilidad inicial de los actores al comenzar una conversación.
    /// En monólogo: solo el actor indicado en monologueActorIndex queda activo.
    /// En diálogo: todos los actores están visibles.
    /// </summary>
    private void SetupActorVisibility()
    {
        if (_activeConversation == null) return;

        if (_activeConversation.isMonologue)
        {
            int monoIndex = _activeConversation.monologueActorIndex;

            if (!IsActorIndexValid(monoIndex))
            {
                Debug.LogWarning($"[DialogueController] monologueActorIndex '{monoIndex}' fuera de rango. Se mostrarán todos los actores.");
                return;
            }

            for (int i = 0; i < actorImages.Count; i++)
            {
                if (actorImages[i] != null)
                    actorImages[i].gameObject.SetActive(i == monoIndex);
            }
        }
        else
        {
            // Diálogo: todos visibles
            foreach (var img in actorImages)
            {
                if (img != null)
                    img.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Restaura la visibilidad de todos los actores al terminar o detener una conversación.
    /// </summary>
    private void RestoreActorVisibility()
    {
        foreach (var img in actorImages)
        {
            if (img != null)
                img.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Inicia las coroutines de transición suave de alpha y escala para todos los actores.
    /// El actor con el índice indicado pasa a estado "activo"; los demás a "inactivo".
    /// </summary>
    private void StartActorTransitions(int activeIndex)
    {
        // Cancelar transiciones previas
        foreach (var c in _transitionCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        _transitionCoroutines.Clear();

        for (int i = 0; i < actorImages.Count; i++)
        {
            if (actorImages[i] == null) continue;

            bool isActive = (i == activeIndex);
            float targetAlpha = isActive ? activeActorAlpha : inactiveActorAlpha;
            float targetScale = isActive ? activeActorScale : inactiveActorScale;

            Coroutine co = StartCoroutine(TransitionActor(actorImages[i], targetAlpha, targetScale));
            _transitionCoroutines.Add(co);
        }
    }

    /// <summary>
    /// Coroutine que interpola suavemente el alpha y la escala de un actor.
    /// </summary>
    private IEnumerator TransitionActor(Image actorImage, float targetAlpha, float targetScale)
    {
        float elapsed = 0f;
        Color startColor = actorImage.color;
        Vector3 startScale = actorImage.rectTransform.localScale;

        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        Vector3 endScale = Vector3.one * targetScale;

        while (elapsed < actorTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / actorTransitionDuration);

            actorImage.color = Color.Lerp(startColor, targetColor, t);
            actorImage.rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // Asegurar valores finales exactos
        actorImage.color = targetColor;
        actorImage.rectTransform.localScale = endScale;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UTILIDADES INTERNAS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Detiene la coroutine de tipeo activa si existe.
    /// </summary>
    private void StopTypingCoroutine()
    {
        if (_subTypingCoroutine != null)
        {
            StopCoroutine(_subTypingCoroutine);
            _subTypingCoroutine = null;
        }
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _isTyping = false;
    }

    /// <summary>
    /// Detiene el tipeo y todas las transiciones de actores activas.
    /// </summary>
    private void StopAllTypingAndTransitions()
    {
        StopTypingCoroutine();

        foreach (var c in _transitionCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        _transitionCoroutines.Clear();
    }

    /// <summary>
    /// Valida que el índice de actor esté dentro del rango de actorImages.
    /// Loguea un error sin lanzar excepción si está fuera de rango.
    /// </summary>
    private bool IsActorIndexValid(int index)
    {
        if (actorImages == null || index < 0 || index >= actorImages.Count)
        {
            Debug.LogError($"[DialogueController] actorIndex '{index}' fuera de rango. actorImages tiene {actorImages?.Count ?? 0} elementos.");
            return false;
        }
        return true;
    }
}