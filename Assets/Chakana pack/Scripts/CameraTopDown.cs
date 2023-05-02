using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTopDown : MonoBehaviour
{
    private float vertical, horizontal;
    private float timer;
    private Vector3 destination;
    private bool isMove;

    [SerializeField] private GameObject cinemaC;
    [SerializeField] private float velocity;

    void Start()
    {
        
    }

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

                if (timer >= 1.2f)
                {
                    if (cinemaC.activeSelf) cinemaC.SetActive(false);

                    if (!isMove)
                    {
                        destination = transform.position;
                        destination.y += 7;
                        isMove = true;
                    }

                    transform.position = Vector3.MoveTowards(transform.position, destination, velocity * Time.deltaTime);
                }

            }
            else if(a <= -0.9)
            {
                timer += 1 * Time.deltaTime;

                if(timer >= 1.2f)
                {
                    if (cinemaC.activeSelf) cinemaC.SetActive(false);

                    if (!isMove)
                    {
                        destination = transform.position;
                        destination.y -= 7;
                        isMove = true;
                    }

                    transform.position = Vector3.MoveTowards(transform.position, destination, velocity * Time.deltaTime);
                }
            }
            else
            {
                if (timer != 0) timer = 0;
                if (!cinemaC.activeSelf) cinemaC.SetActive(true);
                isMove = false;
            }
        }
        else
        {
            if (timer != 0) timer = 0;
            if (!cinemaC.activeSelf) cinemaC.SetActive(true);
            isMove = false;
        }
    }
}
