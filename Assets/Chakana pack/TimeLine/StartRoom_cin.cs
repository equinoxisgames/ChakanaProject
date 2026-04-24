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

    [Header("Configuración de Texto")]
    [SerializeField] float velocidadTexto = 0.03f; // Tiempo entre cada letra

    private Rigidbody2D rb;
    private int indiceDialogo = 0;
    private bool esperandoInput = false;
    private PlayableDirector timeline;
    private string localizationL;

    // --- Nuevas variables para el Typewriter ---
    private bool estaEscribiendo = false;
    private string lineaCompletaActual = "";
    private Coroutine corrutinaEscritura;

    private void Awake()
    {
        rb = player.GetComponent<Rigidbody2D>();
        timeline = GetComponent<PlayableDirector>();

        localizationL = LocalizationSettings.SelectedLocale.Identifier.Code;

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
                // Si está escribiendo, interrumpimos la corrutina y autocompletamos la línea
                if (corrutinaEscritura != null)
                {
                    StopCoroutine(corrutinaEscritura);
                }
                textoDialogo.text = lineaCompletaActual;
                estaEscribiendo = false;
            }
            else
            {
                // Si ya terminó de escribir, pasamos a la siguiente línea
                AvanzarDialogo();
            }
        }
    }

    IEnumerator IniciarSecuencia()
    {
        yield return new WaitForSeconds(1.0f);

        panelDialogo.SetActive(true);
        MostrarLinea();
    }

    void MostrarLinea()
    {
        // 1. Definimos cuál es el texto completo que debe mostrarse según el idioma
        if (localizationL == "es")
            lineaCompletaActual = lineasDialogoES[indiceDialogo];
        else if (localizationL == "en")
            lineaCompletaActual = lineasDialogoEN[indiceDialogo];
        else
            lineaCompletaActual = lineasDialogoEN[indiceDialogo]; // Fallback por defecto

        textoDialogo.text = "";
        esperandoInput = true;

        kinde.GetComponent<AudioSource>().Play();

        // 2. Iniciamos el efecto de máquina de escribir
        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        corrutinaEscritura = StartCoroutine(EscribirLinea());
    }

    // Corrutina que añade letra por letra
    IEnumerator EscribirLinea()
    {
        estaEscribiendo = true;
        textoDialogo.text = "";

        // Convertimos el string completo en un arreglo de caracteres y lo iteramos
        foreach (char letra in lineaCompletaActual.ToCharArray())
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadTexto);
        }

        // Cuando termina el bucle, significa que la línea se completó naturalmente
        estaEscribiendo = false;
    }

    void AvanzarDialogo()
    {
        indiceDialogo++;

        // Asumimos que los arreglos de ES y EN tienen la misma longitud
        if (indiceDialogo < lineasDialogoES.Length)
        {
            MostrarLinea();
        }
        else
        {
            esperandoInput = false;
            panelDialogo.SetActive(false);
            StartCoroutine(PlayScene());
        }
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
}