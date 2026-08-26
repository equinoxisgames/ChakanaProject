using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.Localization.Settings;

public class StartRoom_cin : MonoBehaviour
{
    [Header("Objetos de Escena")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject kinde;
    [SerializeField] GameObject tuto;

    [Header("UI de Diálogo")]
    [SerializeField] GameObject panelDialogo;
    [SerializeField] TextMeshPro textoDialogo;
    [SerializeField] string[] lineasDialogoES;
    [SerializeField] string[] lineasDialogoEN;

    [Header("Audio por Palabra")]
    [SerializeField] List<AudioClip> clipsPalabra;
    [SerializeField] AudioSource fuenteAudioPalabra;
    [SerializeField] float pausaMinPalabra = 0.05f;
    [SerializeField] float pausaMaxPalabra = 0.12f;
    [SerializeField] float pitchMin = 0.9f;
    [SerializeField] float pitchMax = 1.1f;

    [Header("Configuración de Texto")]
    [SerializeField] float velocidadTexto = 0.03f;

    [Header("Fade del Panel")]
    [SerializeField] float duracionFadeIn = 0.4f;
    [SerializeField] float duracionFadeOut = 0.3f;

    private Rigidbody2D rb;
    private int indiceDialogo = 0;
    private bool esperandoInput = false;
    private PlayableDirector timeline;
    private string localizationL;

    private bool estaEscribiendo = false;
    private string lineaCompletaActual = "";
    private Coroutine corrutinaEscritura;

    private CanvasGroup canvasGroupPanel;

    private void Awake()
    {
        rb = player.GetComponent<Rigidbody2D>();
        timeline = GetComponent<PlayableDirector>();

        localizationL = LocalizationSettings.SelectedLocale.Identifier.Code;

        canvasGroupPanel = panelDialogo.GetComponent<CanvasGroup>();
        if (canvasGroupPanel == null)
            canvasGroupPanel = panelDialogo.AddComponent<CanvasGroup>();

        // Forzamos estado oculto desde el primer frame sin importar la escena
        canvasGroupPanel.alpha = 0f;
        panelDialogo.SetActive(false);

        if (PlayerPrefs.HasKey("inicio01"))
        {
            CargarEstadoFinal();
        }
        else
        {
            player.GetComponent<Hoyustus>().enabled = false;
            StartCoroutine(IniciarSecuencia());
        }
    }

    private void Update()
    {
        if (esperandoInput && Input.GetButtonDown("Submit"))
        {
            if (estaEscribiendo)
            {
                if (corrutinaEscritura != null)
                    StopCoroutine(corrutinaEscritura);

                textoDialogo.text = lineaCompletaActual;
                estaEscribiendo = false;
            }
            else
            {
                AvanzarDialogo();
            }
        }
    }

    IEnumerator IniciarSecuencia()
    {
        yield return new WaitForSeconds(1.0f);

        // Reseteamos texto y alpha antes de activar para evitar el flash
        textoDialogo.text = "";
        canvasGroupPanel.alpha = 0f;
        panelDialogo.SetActive(true);

        yield return StartCoroutine(FadePanel(0f, 1f, duracionFadeIn));

        MostrarLinea();
    }

    void MostrarLinea()
    {
        if (localizationL == "es")
            lineaCompletaActual = lineasDialogoES[indiceDialogo];
        else if (localizationL == "en")
            lineaCompletaActual = lineasDialogoEN[indiceDialogo];
        else
            lineaCompletaActual = lineasDialogoEN[indiceDialogo];

        textoDialogo.text = "";
        esperandoInput = true;

        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        corrutinaEscritura = StartCoroutine(EscribirLinea());
    }

    IEnumerator EscribirLinea()
    {
        estaEscribiendo = true;
        textoDialogo.text = "";

        char[] caracteres = lineaCompletaActual.ToCharArray();

        for (int i = 0; i < caracteres.Length; i++)
        {
            char letra = caracteres[i];
            textoDialogo.text += letra;

            yield return new WaitForSeconds(velocidadTexto);

            bool esFinDePalabra = (letra == ' ') || (i == caracteres.Length - 1);

            if (esFinDePalabra)
            {
                ReproducirClipAleatorio();
                float pausa = Random.Range(pausaMinPalabra, pausaMaxPalabra);
                yield return new WaitForSeconds(pausa);
            }
        }

        estaEscribiendo = false;
    }

    void ReproducirClipAleatorio()
    {
        if (clipsPalabra == null || clipsPalabra.Count == 0) return;
        if (fuenteAudioPalabra == null) return;

        int idx = Random.Range(0, clipsPalabra.Count);
        fuenteAudioPalabra.pitch = Random.Range(pitchMin, pitchMax);
        fuenteAudioPalabra.PlayOneShot(clipsPalabra[idx]);
    }

    void AvanzarDialogo()
    {
        indiceDialogo++;

        if (indiceDialogo < lineasDialogoES.Length)
        {
            MostrarLinea();
        }
        else
        {
            esperandoInput = false;
            StartCoroutine(CerrarPanelYContinuar());
        }
    }

    IEnumerator CerrarPanelYContinuar()
    {
        yield return StartCoroutine(FadePanel(1f, 0f, duracionFadeOut));
        panelDialogo.SetActive(false);
        yield return StartCoroutine(PlayScene());
    }

    IEnumerator PlayScene()
    {
        PlayerPrefs.SetString("inicio01", "si");

        timeline.Play();

        yield return new WaitForSeconds(0.5f);

        player.GetComponent<Animator>().SetBool("Grounded", true);

        yield return new WaitForSeconds(1.2f);

        tuto.SetActive(true);
        player.GetComponent<Hoyustus>().enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
    }

    void CargarEstadoFinal()
    {
        player.GetComponent<Hoyustus>().enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        gameObject.SetActive(false);
    }

    IEnumerator FadePanel(float alphaInicio, float alphaFin, float duracion)
    {
        float tiempo = 0f;
        canvasGroupPanel.alpha = alphaInicio;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            canvasGroupPanel.alpha = Mathf.Lerp(alphaInicio, alphaFin, tiempo / duracion);
            yield return null;
        }

        canvasGroupPanel.alpha = alphaFin;
    }
}