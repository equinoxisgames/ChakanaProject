using UnityEngine;

public class CameraTopDown : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camaraPrincipal;
    [SerializeField] private GameObject camaraVistazo;

    [Header("Configuración")]
    [SerializeField] private float velocidadCamara = 15f;
    [SerializeField] private float distanciaMaximaVistazo = 8f;
    [SerializeField] private float umbralVistazo = 0.2f;

    private bool estaDandoVistazo = false;
    private Vector3 posicionOrigen;

    void Update()
    {
        float verticalVistazo = Input.GetAxis("Vertical2");
        float horizontalVistazo = Input.GetAxis("Horizontal2");
        float movimientoJugador = Input.GetAxis("Horizontal");

        bool quiereMirar = Mathf.Abs(verticalVistazo) > umbralVistazo;
        bool jugadorQuieto = Mathf.Abs(movimientoJugador) < 0.1f;
        bool sinVistazoHorizontal = Mathf.Abs(horizontalVistazo) <= 0.2f;

        // 1. INICIAR EL VISTAZO
        // Solo se activa si el jugador está quieto, quiere mirar y la cámara estaba apagada
        if (quiereMirar && jugadorQuieto && sinVistazoHorizontal && !estaDandoVistazo)
        {
            estaDandoVistazo = true;
            camaraVistazo.SetActive(true);
            camaraVistazo.transform.position = posicionOrigen;
            // IMPORTANTE: Al volverse true 'estaDandoVistazo', la posicionOrigen se "congela" 
            // y deja de actualizarse en el bloque 'else' de abajo.
        }

        // 2. MIENTRAS EL VISTAZO ESTÉ ACTIVO (Yendo hacia arriba/abajo o regresando)
        if (estaDandoVistazo)
        {
            // Si el jugador decide moverse repentinamente, apagamos el vistazo de inmediato
            // para que no siga viendo el cielo mientras su personaje corre a ciegas.
            if (!jugadorQuieto || !sinVistazoHorizontal)
            {
                ApagarVistazo();
                return;
            }

            Vector3 destino;

            if (quiereMirar)
            {
                // Sube o baja progresivamente según cuánto empujes el joystick
                destino = posicionOrigen + new Vector3(0, verticalVistazo * distanciaMaximaVistazo, 0);
            }
            else
            {
                // Si suelta el joystick, el destino vuelve a ser el centro congelado
                destino = posicionOrigen;
            }

            // Movemos la cámara hacia el destino actual
            camaraVistazo.transform.position = Vector3.MoveTowards(
                camaraVistazo.transform.position,
                destino,
                velocidadCamara * Time.deltaTime
            );

            // Si el jugador soltó el botón Y la cámara ya regresó a su posición de origen, se apaga.
            if (!quiereMirar && Vector3.Distance(camaraVistazo.transform.position, posicionOrigen) < 0.01f)
            {
                ApagarVistazo();
            }
        }
        else
        {
            // 3. MIENTRAS LA CÁMARA DE VISTAZO ESTÉ APAGADA
            // Aquí sí mantenemos la 'posicionOrigen' actualizada constantemente 
            // siguiendo a la cámara principal por si el jugador está caminando por el mapa.
            posicionOrigen = camaraPrincipal.position;
        }
    }

    private void ApagarVistazo()
    {
        estaDandoVistazo = false;
        if (camaraVistazo.activeSelf)
        {
            camaraVistazo.SetActive(false);
        }
    }
}