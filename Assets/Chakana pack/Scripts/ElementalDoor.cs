using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalDoor : MonoBehaviour
{
    [SerializeField] string doorName;
    [SerializeField] Transform openP, closeP;

    private Vector3 destination;
    private bool isMove = false;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();

        if (!PlayerPrefs.HasKey(doorName))
        {
            PlayerPrefs.SetInt(doorName, 0);
            destination = closeP.position;
            isMove = true;
        }
        else if (PlayerPrefs.GetInt(doorName) < 3)
        {
            transform.position = closeP.position;
        }
        else if (PlayerPrefs.GetInt(doorName) == 3)
        {
            PlayerPrefs.SetInt(doorName, 4);
            destination = openP.position;
            isMove = true;
        }
        else
        {
            transform.position = openP.position;
        }
    }

    void LateUpdate()
    {
        if (isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, 5 * Time.deltaTime);

            if (transform.position == destination) isMove = false;
        }
    }
}
