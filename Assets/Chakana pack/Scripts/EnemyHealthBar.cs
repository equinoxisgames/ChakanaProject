using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healtBar;
    public Image healtBar2;
    public GameObject backGround;

    [Header("Configuración de Animación")]
    public float lerpSpeed = 5f;
    public float tiempoVisible = 3f;

    private Transform focusTr;
    private float temporizadorOcultar;
    private bool estaVisible;

    private void Start()
    {
        if (backGround != null)
        {
            backGround.SetActive(false);
        }
        estaVisible = false;
    }

    private void Update()
    {
        // Solo animamos la barra secundaria si es mayor a la principal (cuando recibe daño)
        if (healtBar2.fillAmount > healtBar.fillAmount)
        {
            healtBar2.fillAmount = Mathf.Lerp(healtBar2.fillAmount, healtBar.fillAmount, Time.deltaTime * lerpSpeed);
        }

        if (estaVisible)
        {
            temporizadorOcultar -= Time.deltaTime;

            if (temporizadorOcultar <= 0f)
            {
                OcultarBarra();
            }
        }
    }

    private void LateUpdate()
    {
        if (focusTr != null)
        {
            transform.position = focusTr.position;
        }
    }

    public void SetFocus(Transform tr)
    {
        focusTr = tr;
    }

    public void SetHealthValue(float e)
    {
        // Comparamos el valor entrante con el actual. 
        // Usamos una pequeña tolerancia (0.001f) porque al trabajar con floats 
        // a veces hay micro-diferencias en los decimales.
        if (Mathf.Abs(healtBar.fillAmount - e) > 0.001f)
        {
            // Si el enemigo recupera vida, ajustamos la barra secundaria de golpe 
            // para que no haga una transición extraña hacia arriba.
            if (e > healtBar.fillAmount)
            {
                healtBar2.fillAmount = e;
            }

            // Actualizamos la barra principal
            healtBar.fillAmount = e;

            // Mostramos el fondo y reiniciamos el temporizador
            MostrarBarra();
            temporizadorOcultar = tiempoVisible;
        }
    }

    private void MostrarBarra()
    {
        if (!estaVisible)
        {
            backGround.SetActive(true);
            estaVisible = true;
        }
    }

    private void OcultarBarra()
    {
        if (backGround != null)
        {
            backGround.SetActive(false);
        }
        estaVisible = false;
    }
}