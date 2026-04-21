using Assets.FantasyInventory.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ElementalDoor : MonoBehaviour
{
    [SerializeField] string doorName;
    [SerializeField] Transform openPos, closePos, orbPos, playerTr;
    [SerializeField] TextMeshPro adTxt;

    [Header("Partículas")]
    [SerializeField] GameObject particle; // Partícula grande
    [SerializeField] GameObject part01;   // Minipartícula 1
    [SerializeField] GameObject part02;   // Minipartícula 2
    [SerializeField] GameObject part03;   // Minipartícula 3
    [SerializeField] GameObject lightDoor;
    [SerializeField] AudioClip musicFinal;

    [Header("Configuración de Animación")]
    [SerializeField] float particleSpeed = 10f;
    [SerializeField] float doorSpeed = 2f;
    [SerializeField] float divergentDistance = 2f; // Qué tanto se alejan las minis antes de juntarse

    private Vector3 destination;
    private bool isToOpen = false;
    private bool opening = false;

    private void Start()
    {
        if (!PlayerPrefs.HasKey(doorName))
        {
            PlayerPrefs.SetInt(doorName, 0);
        }
        else if (PlayerPrefs.GetInt(doorName) == 3)
        {
            destination = openPos.position;
            isToOpen = true;
        }
        else if (PlayerPrefs.GetInt(doorName) == 4)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (opening) return;

        if (collision.CompareTag("Player") && !isToOpen)
        {
            int e = 3 - PlayerPrefs.GetInt(doorName);

            adTxt.text = "Unleash your power to access";
            adTxt.gameObject.SetActive(true);
        }
        else
        {
            opening = true;
            StartCoroutine(OpenDoor());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            adTxt.gameObject.SetActive(false);
        }
    }

    IEnumerator OpenDoor()
    {
        // 1. Inicializar minipartículas en la posición del jugador
        GetComponent<AudioSource>().Play();
        Vector3 startPos = playerTr.position;
        part01.transform.position = startPos;
        part02.transform.position = startPos;
        part03.transform.position = startPos;

        part01.SetActive(true);
        part02.SetActive(true);
        part03.SetActive(true);

        // 2. FASE DE DIVERGENCIA (Se expanden hacia afuera)
        // Definimos 3 puntos hacia donde saldrán disparadas
        Vector3 target01 = startPos + new Vector3(-divergentDistance, divergentDistance, 0);
        Vector3 target02 = startPos + new Vector3(0, divergentDistance * 1.5f, 0);
        Vector3 target03 = startPos + new Vector3(divergentDistance, divergentDistance, 0);

        float t = 0;
        while (t < 3f)
        {
            t += Time.deltaTime * particleSpeed;
            part01.transform.position = Vector3.MoveTowards(part01.transform.position, target01, particleSpeed * Time.deltaTime);
            part02.transform.position = Vector3.MoveTowards(part02.transform.position, target02, particleSpeed * Time.deltaTime);
            part03.transform.position = Vector3.MoveTowards(part03.transform.position, target03, particleSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. FASE DE CONVERGENCIA (Se juntan en un punto medio hacia la puerta)
        Vector3 convergencePoint = Vector3.Lerp(startPos, orbPos.position, 0.5f); // Se juntan un poco adelantados al jugador

        while (Vector3.Distance(part01.transform.position, convergencePoint) > 0.1f)
        {
            part01.transform.position = Vector3.MoveTowards(part01.transform.position, convergencePoint, particleSpeed * 1.5f * Time.deltaTime);
            part02.transform.position = Vector3.MoveTowards(part02.transform.position, convergencePoint, particleSpeed * 1.5f * Time.deltaTime);
            part03.transform.position = Vector3.MoveTowards(part03.transform.position, convergencePoint, particleSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        // 4. TRANSICIÓN A LA PARTÍCULA GRANDE
        part01.SetActive(false);
        part02.SetActive(false);
        part03.SetActive(false);

        particle.transform.position = convergencePoint;
        particle.SetActive(true);

        // 5. MOVIMIENTO AL ORBE Y APERTURA (Lógica anterior)
        while (Vector3.Distance(particle.transform.position, orbPos.position) > 0.1f)
        {
            particle.transform.position = Vector3.MoveTowards(particle.transform.position, orbPos.position, particleSpeed * Time.deltaTime);
            yield return null;
        }

        lightDoor.SetActive(false);
        GetComponent<AudioSource>().clip = musicFinal;
        GetComponent<AudioSource>().Play();
        particle.transform.SetParent(orbPos);
        Vector3 targetDoorPos = destination;

        while (Vector3.Distance(transform.parent.position, targetDoorPos) > 0.01f)
        {
            transform.parent.position = Vector3.MoveTowards(transform.parent.position, targetDoorPos, doorSpeed * Time.deltaTime);
            yield return null;
        }

        PlayerPrefs.SetInt(doorName, 4);
        Destroy(transform.parent.gameObject);
    }
}