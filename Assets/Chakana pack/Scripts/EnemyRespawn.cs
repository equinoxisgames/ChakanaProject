using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawn : MonoBehaviour
{
    public string sceneName;

    public List<GameObject> enemiesName = new List<GameObject>();

    private void Awake()
    {
        if (PlayerPrefs.HasKey("reset" + sceneName) && enemiesName.Count > 0)
        {
            PlayerPrefs.DeleteKey("reset" + sceneName);

            for(int i = 0; i < enemiesName.Count; i++)
            {
                PlayerPrefs.DeleteKey(sceneName + i);
            }
        }

        for (int i = 0; i < enemiesName.Count; i++)
        {
            if(PlayerPrefs.HasKey(sceneName + i))
            {
                Destroy(enemiesName[i]);
            }
        }
    }

    public void ResetEnemies()
    {
        PlayerPrefs.SetInt("resetenemies01", 1);
        PlayerPrefs.SetInt("resetenemies02", 1);
        PlayerPrefs.SetInt("resetenemies03", 1);
        PlayerPrefs.SetInt("resetenemies04", 1);
        PlayerPrefs.SetInt("resetenemies05", 1);
        PlayerPrefs.SetInt("resetenemies06", 1);
    }

    public void EnemyDeath()
    {
        StartCoroutine(GetEnemyName());
    }

    IEnumerator GetEnemyName()
    {
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < enemiesName.Count; i++)
        {
            if (enemiesName[i] == null)
            {
                PlayerPrefs.SetInt(sceneName + i, 1);
            }
        }
    }
}
