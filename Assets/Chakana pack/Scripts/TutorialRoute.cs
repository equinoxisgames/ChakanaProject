using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRoute : MonoBehaviour
{
    [SerializeField] private GameObject canvasObj;
    [SerializeField] private GameObject aditionalObj;
    [SerializeField] private int tutoNum;

    private bool isActive;

    void Start()
    {
        if(PlayerPrefs.HasKey("tutorial" + tutoNum))
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        TutoDetector();       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            canvasObj.SetActive(true);
            if (aditionalObj != null) aditionalObj.SetActive(true);
            isActive = true;
        }
    }

    private void TutoDetector()
    {
        if (isActive)
        {
            if(tutoNum == 1 && Input.GetAxis("Jump") == 1)
            {
                canvasObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1); 
                Destroy(gameObject);
            }
            if (tutoNum == 2 && Input.GetAxis("Interact") == 1)
            {
                canvasObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 3 && Input.GetAxis("Atacar") == 1)
            {
                canvasObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 4 && Input.GetAxis("Dash") == 1)
            {
                canvasObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
            if (tutoNum == 5 && Input.GetAxis("Horizontal") != 0)
            {
                canvasObj.SetActive(false);
                PlayerPrefs.SetInt("tutorial" + tutoNum, 1);
                Destroy(gameObject);
            }
        }
    }
}
