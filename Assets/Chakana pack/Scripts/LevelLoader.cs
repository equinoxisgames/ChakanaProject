using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] int sceneNum;
    [SerializeField] int scenePos;
    [SerializeField] int actualPos;
    [SerializeField] Transform player;
    Transform appearPos;

    [SerializeField] private GameObject loadScenePanel;

    private void Awake()
    {
        appearPos = transform.GetChild(0);

        if (PlayerPrefs.HasKey("scenePos"))
        {
            if (PlayerPrefs.GetInt("scenePos") == actualPos)
            {
                //loadScenePanel = GameObject.Find("LoadPanel");

                player.position = appearPos.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            player.GetComponent<Hoyustus>().SavePlayerData();
            PlayerPrefs.SetInt("scenePos", scenePos);
            //SceneManager.UnloadSceneAsync(actualScene);
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        loadScenePanel.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);

        // Hacer que la carga se realice en segundo plano
        asyncLoad.allowSceneActivation = false;

        // Esperar hasta que la carga de la escena esté completa
        while (!asyncLoad.isDone)
        {
            // Actualizar la barra de progreso o cualquier otro elemento de la pantalla de carga según sea necesario
            // Puedes usar asyncLoad.progress para obtener el progreso de carga de la escena

            // Si la carga de la escena está completa y no se ha mostrado por completo la pantalla de carga, activarla
            if (asyncLoad.progress >= 0.9f)
            {
                // Mostrar la pantalla de carga por un breve período de tiempo adicional para que el jugador tenga tiempo de verla
                // Aquí puedes desactivar el objeto que representa la pantalla de carga en la interfaz de usuario

                // Permitir que la carga de la escena se complete y se muestre al jugador
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

