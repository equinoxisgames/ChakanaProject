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

    private void Awake()
    {
        appearPos = transform.GetChild(0);

        if (PlayerPrefs.HasKey("scenePos"))
        {
            if(PlayerPrefs.GetInt("scenePos") == actualPos)
            {
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
            SceneManager.LoadScene(sceneNum);
            //collider.gameObject.transform.position = new Vector3(xPosition, yPosition, zPosition);
        }
    }

}

