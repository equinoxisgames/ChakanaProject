using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopScenes : MonoBehaviour
{
    string escena;

    private LoopScenes instance;

    public LoopScenes Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
            Destroy(gameObject);

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        escena = SceneManager.GetActiveScene().name;
        if (escena != "14-Boss Room" && escena != "00- Main Menu 0")
        {
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.Log("Awake() Aplica Destroy(this.gameObject) al objeto; escena: " + escena);
            Destroy(this.gameObject);
        }
    }
}
