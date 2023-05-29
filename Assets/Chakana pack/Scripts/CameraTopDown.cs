using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTopDown : MonoBehaviour
{
    private float vertical, horizontal;
    private float timer;
    private Vector3 destination, originalPos;
    private bool isMove;

    [SerializeField] private GameObject cinemaC, cinemaC2;
    [SerializeField] private float velocity;

    void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        DoUCamera(vertical, horizontal);
    }

    private void DoUCamera(float a, float b)
    {
        if(b <= 0.2 && b >= -0.2)
        {
            if(a >= 0.9)
            {
                timer += 1 * Time.deltaTime;

                if (timer >= 0.2f)
                {
                    if (!cinemaC2.activeSelf)
                    {
                        cinemaC2.SetActive(true);
                        cinemaC2.transform.position = cinemaC.transform.position;
                    }

                    if (!isMove)
                    {
                        originalPos = cinemaC2.transform.position;
                        destination = cinemaC2.transform.position;
                        destination.y += 8;
                        isMove = true;
                    }

                    cinemaC2.transform.position = Vector3.MoveTowards(cinemaC2.transform.position, destination, velocity * Time.deltaTime);
                }

            }
            else if(a <= -0.9)
            {
                timer += 1 * Time.deltaTime;

                if(timer >= 0.2f)
                {
                    if (!cinemaC2.activeSelf)
                    {
                        cinemaC2.SetActive(true);
                        cinemaC2.transform.position = cinemaC.transform.position;
                    }

                    if (!isMove)
                    {
                        originalPos = cinemaC2.transform.position;
                        destination = cinemaC2.transform.position;
                        destination.y -= 8;
                        isMove = true;
                    }

                    cinemaC2.transform.position = Vector3.MoveTowards(cinemaC2.transform.position, destination, velocity * Time.deltaTime);
                }
            }
            else
            {
                if (timer != 0) timer = 0;
                if (cinemaC2.activeSelf) cinemaC2.SetActive(false);
                isMove = false;
            }
        }
        else
        {
            if (timer != 0) timer = 0;
            if (cinemaC2.activeSelf) cinemaC2.SetActive(false);
            isMove = false;
        }
    }
}
