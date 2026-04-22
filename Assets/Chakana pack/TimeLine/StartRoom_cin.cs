using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Playables; // Asegúrate de tener TextMeshPro en tu proyecto
using UnityEngine.Localization.Settings;

public class StartRoom_cin : MonoBehaviour
{
    [Header("Objetos de Escena")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject kinde; // El segundo personaje
    [SerializeField] GameObject tuto; // El segundo personaje

    [Header("UI de Diálogo")]
    [SerializeField] GameObject panelDialogo; // El objeto de la burbuja/panel
    [SerializeField] TextMeshPro textoDialogo; // El componente de texto
    [SerializeField] string[] lineasDialogoES; // Escribe los diálogos en el Inspector
    [SerializeField] string[] lineasDialogoEN; // Escribe los diálogos en el Inspector

    private Rigidbody2D rb;
    private int indiceDialogo = 0;
    private bool esperandoInput = false;
    private PlayableDirector timeline;
    private string localizationL;

    private void Awake()
    {
        rb = player.GetComponent<Rigidbody2D>();
        timeline = GetComponent<PlayableDirector>();

        localizationL = LocalizationSettings.SelectedLocale.Identifier.Code;
        print(localizationL);

        if (PlayerPrefs.HasKey("inicio01"))
        {
            CargarEstadoFinal();
        }
        else
        {
            // Bloqueamos al jugador al iniciar
            player.GetComponent<Hoyustus>().enabled = false;
            //rb.constraints = RigidbodyConstraints2D.FreezeAll;

            StartCoroutine(IniciarSecuencia());
        }
    }

    private void Update()
    {
        // Si estamos en la parte de los diálogos y el jugador presiona una tecla (ej. Espacio o Click)
        if (esperandoInput && Input.GetButtonDown("Interact"))
        {
            AvanzarDialogo();
        }
    }

    IEnumerator IniciarSecuencia()
    {
        // Pequeña espera inicial antes de que Kinde hable
        yield return new WaitForSeconds(1.0f);

        panelDialogo.SetActive(true);
        MostrarLinea();
    }

    void MostrarLinea()
    {
        string newLine = "";
        if (localizationL == "es") newLine = lineasDialogoES[indiceDialogo];
        else if (localizationL == "en") newLine = lineasDialogoEN[indiceDialogo];

        textoDialogo.text = newLine;
        esperandoInput = true;
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
            // Ya no hay más texto, empezamos la cinemática física
            esperandoInput = false;
            panelDialogo.SetActive(false);
            StartCoroutine(PlayScene());
        }
    }

    IEnumerator PlayScene()
    {
        PlayerPrefs.SetString("inicio01", "si");

        timeline.Play();

        // Aquí sucede lo que tenías antes: la puerta se activa y Sinchi se prepara
        yield return new WaitForSeconds(0.5f);

        timeline.Play();

        player.GetComponent<Animator>().SetBool("Grounded", true);

        yield return new WaitForSeconds(1.2f);

        // Liberamos el control del jugador
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