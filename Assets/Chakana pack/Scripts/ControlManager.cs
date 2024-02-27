using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEngine.UI;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> controlParent = new List<GameObject>();

    private List<GameObject> keyObj = new List<GameObject>();
    private List<GameObject> joyObj = new List<GameObject>();
    private bool joystick;

    //public Slider sliderMaster;

    Vector3 ultimaPosicionDelMouse;

    void Start()
    {
        for(int i = 0; i < controlParent.Count; i++)
        {
            keyObj.Add(controlParent[i].transform.GetChild(0).gameObject);
            joyObj.Add(controlParent[i].transform.GetChild(1).gameObject);
        }

        ultimaPosicionDelMouse = Input.mousePosition;

        int joystickCount = Input.GetJoystickNames().Length;

        if (joystickCount > 0)
        {
            if (!joystick)
            {
                joystick = true;
                //sliderMaster.Select();
                Cursor.visible = false;
                ChangeControls(false);

            }
        }
        else
        {
            if (joystick)
            {
                joystick = false;
                Cursor.visible = true;
                ChangeControls(true);
            }
        }

    }

    void Update()
    {
        Vector3 posicionActualDelMouse = Input.mousePosition;

        if (posicionActualDelMouse != ultimaPosicionDelMouse)
        {
            // El mouse se ha movido
            ultimaPosicionDelMouse = posicionActualDelMouse;

            if (joystick)
            {
                joystick = false;
                Cursor.visible = true;

                ChangeControls(true);
            }
        }

        if (Input.anyKeyDown)
        {
            if (joystick)
            {
                joystick = false;
                Cursor.visible = true;
                ChangeControls(true);
            }
        }

        if (Input.GetButtonDown("JoystickButton") || Input.GetAxis("HorizontalJ") != 0f || Input.GetAxis("VerticalJ") != 0f)
        {
            if (!joystick)
            {
                joystick = true;
                //sliderMaster.Select();
                Cursor.visible = false;
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
