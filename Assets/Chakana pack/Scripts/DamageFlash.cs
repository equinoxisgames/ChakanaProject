using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DamageFlash : MonoBehaviour
{
    [SerializeField] float flashTime;
    [SerializeField] AnimationCurve flashSpeedCurve;
    [SerializeField] List<SpriteRenderer> charSprites;

    List<Material> charMats = new List<Material>();

    Coroutine damageFlashCorrutine;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        for(int i = 0; i < charSprites.Count; i++)
        {
            charMats.Add(charSprites[i].material);
        }
    }

    public void CallDamageFlash()
    {
        if (!gameObject.activeSelf) return;

        damageFlashCorrutine = StartCoroutine(DamageFlasher());
    }

    public void CallPlayerDeath()
    {
        StopAllCoroutines();
        SetFlashAmount(0);
        damageFlashCorrutine = StartCoroutine(DissolveFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        float currentFlashAmount = 0;
        float elapsedTime = 0;

        while(elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashAmount = Mathf.Lerp(1f, flashSpeedCurve.Evaluate(elapsedTime), (elapsedTime / flashTime));
            SetFlashAmount(currentFlashAmount);

            yield return null;
        }
    }

    private IEnumerator DissolveFlasher()
    {
        float currentFlashAmount = 0;
        float elapsedTime = 0;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount += Time.deltaTime;

            SetDissolveAmount(currentFlashAmount);

            yield return null;
        }
    }

    private void SetFlashAmount(float amount)
    {
        for (int i = 0; i < charMats.Count; i++)
        {
            charMats[i].SetFloat("_FlashAmount", amount);
        }
    }

    private void SetDissolveAmount(float amount)
    {
        for (int i = 0; i < charMats.Count; i++)
        {
            charMats[i].SetFloat("_DissolveAmount", amount);
        }
    }
}
