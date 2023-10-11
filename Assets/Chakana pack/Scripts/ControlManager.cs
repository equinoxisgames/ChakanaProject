using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> controlParent = new List<GameObject>();

    private List<GameObject> keyObj = new List<GameObject>();
    private List<GameObject> joyObj = new List<GameObject>();
    private bool joystick;

    void Start()
    {
        for(int i = 0; i < controlParent.Count; i++)
        {
            keyObj.Add(controlParent[i].transform.GetChild(0).gameObject);
            joyObj.Add(controlParent[i].transform.GetChild(1).gameObject);
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (joystick)
            {
                joystick = false;

                ChangeControls(true);
            }
        }

        if (Input.GetButtonDown("JoystickButton") || Input.GetAxis("HorizontalJ") != 0f || Input.GetAxis("VerticalJ") != 0f)
        {
            if (!joystick)
            {
                joystick = true;

                ChangeControls(false);
            }
        }
    }

    private void ChangeControls(bool t)
    {
        if (t)
        {
            for (int i = 0; i < controlParent.Count; i++)
            {
                keyObj[i].SetActive(true);
                joyObj[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < controlParent.Count; i++)
            {
                keyObj[i].SetActive(false);
                joyObj[i].SetActive(true);
            }
        }
    }
}
