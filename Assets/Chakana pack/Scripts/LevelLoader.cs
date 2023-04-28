using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    public float xPosition;
    public float yPosition;
    public float zPosition;
    public string sceneName;
    private string actualScene;
    private int counter = 1;


    private void Start()
    {
        actualScene = SceneManager.GetActiveScene().name;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (counter == 1 && collider.gameObject.tag == "Player")
        {
            counter = 0;
            PlayerPrefs.SetFloat("nextPositionX", xPosition);
            PlayerPrefs.SetFloat("nextPositionY", yPosition);
            PlayerPrefs.SetInt("firstRun", 0);
            PlayerPrefs.SetInt("flipFlag", 1);
            SceneManager.UnloadSceneAsync(actualScene);
            SceneManager.LoadSceneAsync(sceneName);
            //collider.gameObject.transform.position = new Vector3(xPosition, yPosition, zPosition);
        }
    }

}

