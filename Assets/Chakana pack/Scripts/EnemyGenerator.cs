using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] List<GameObject> enemies = new List<GameObject>();
    [SerializeField] Transform door1, door2;
    [SerializeField] GameObject invokeFX;
    [SerializeField] GameObject treasure;

    bool isOn, isMove, isOnBattle, isWin, isCollision;
    private Vector3 destination1, destination2;
    private Vector3 originalPos1, originalPos2;

    void Start()
    {
        destination1 = door1.position;
        originalPos1 = door1.position;
        destination1.y -= 4;

        destination2 = door2.position;
        originalPos2 = door2.position;
        destination2.y -= 4;
    }

    private void Update()
    {
        if(isCollision && isWin && Input.GetKeyDown(KeyCode.E))
        {
            isWin = false;

            Destroy(treasure);

            GetComponent<EnemyGenerator>().enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (isMove)
        {
            if (isOnBattle)
            {
                door1.position = Vector3.MoveTowards(door1.position, destination1, 5 * Time.deltaTime);
                door2.position = Vector3.MoveTowards(door2.position, destination2, 5 * Time.deltaTime);

                if (transform.position == destination1) isMove = false;
            }
            else
            {
                door1.position = Vector3.MoveTowards(door1.position, originalPos1, 5 * Time.deltaTime);
                door2.position = Vector3.MoveTowards(door2.position, originalPos2, 5 * Time.deltaTime);

                if (transform.position == destination1) isMove = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isCollision = true;

            if (!isOn)
            {
                isOnBattle = true;
                isMove = true;

                StartCoroutine(StartCombat());

                isOn = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isCollision = false;
        }
    }

    IEnumerator StartCombat()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            yield return new WaitForSeconds(2);
            Destroy(Instantiate(invokeFX, enemies[i].transform.position, Quaternion.identity), 2);
            yield return new WaitForSeconds(0.2f);
            enemies[i].SetActive(true);
        }

        yield return new WaitForSeconds(5);

        isOnBattle = false;
        isMove = true;
        isWin = true;

        treasure.SetActive(true);
    }
}
