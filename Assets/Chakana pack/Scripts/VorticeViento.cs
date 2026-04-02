using System.Collections;
using UnityEngine;

public class VorticeViento : MonoBehaviour
{
    [Header("Configuración del Vórtice")]
    [Tooltip("Tiempo en segundos antes de que el vórtice se detenga.")]
    [SerializeField] private float tiempoDeVida = 5f;
    [Tooltip("Fuerza con la que el jugador es arrastrado hacia el centro.")]
    [SerializeField] private float fuerzaAtraccion = 15f;

    [Header("Configuración de Daño y Efectos")]
    [SerializeField] private float danioVortice = 1f;
    [Tooltip("Cada cuántos segundos el jugador recibirá daño mientras esté dentro.")]
    [SerializeField] private float tiempoEntreDanio = 0.5f;

    private float temporizadorDanio;
    private ParticleSystem[] sistemasDeParticulas;
    private Collider2D colisionador;

    void Start()
    {
        // Obtiene el ParticleSystem de este objeto y de todos sus hijos (los 2 extra que mencionaste)
        sistemasDeParticulas = GetComponentsInChildren<ParticleSystem>();
        colisionador = GetComponent<Collider2D>();

        // Inicia la cuenta regresiva para la destrucción del vórtice
        StartCoroutine(RutinaDeVida());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // En tus scripts la capa 11 representa el cuerpo del jugador
        if (collision.gameObject.layer == 11)
        {
            Rigidbody2D rbJugador = collision.GetComponent<Rigidbody2D>();
            Hoyustus scriptJugador = collision.GetComponent<Hoyustus>();

            if (rbJugador != null && scriptJugador != null)
            {
                AtraerJugador(rbJugador);
                AplicarDanio(scriptJugador);
                ImbuirViento(scriptJugador);
            }
        }
    }

    private void AtraerJugador(Rigidbody2D rbJugador)
    {
        // Calcula la dirección exacta hacia el centro (transform.position) del vórtice
        Vector2 direccionAlCentro = (transform.position - rbJugador.transform.position).normalized;

        // Se aplica una fuerza continua para arrastrar al jugador
        rbJugador.AddForce(direccionAlCentro * fuerzaAtraccion, ForceMode2D.Force);
    }

    private void AplicarDanio(Hoyustus jugador)
    {
        temporizadorDanio += Time.fixedDeltaTime; // fixedDeltaTime es mejor para la física dentro de un Trigger

        if (temporizadorDanio >= tiempoEntreDanio)
        {
            jugador.RecibirDanio(danioVortice);

            temporizadorDanio = 0f;
        }
    }

    private void ImbuirViento(Hoyustus jugador)
    {
        // Aquí conectas tu lógica de estados elementales
        // Por ejemplo, podrías llamar a un método en el script de Sinchi que le aplique un debuff o cambie un booleano:
        // jugador.AplicarEstadoElemental("Viento");
        // jugador.afectacionViento = 0.5f; // Basado en variables que vi en el movimiento de tus enemigos
    }

    private IEnumerator RutinaDeVida()
    {
        // 1. El vórtice permanece activo haciendo daño y atrayendo por X segundos
        yield return new WaitForSeconds(tiempoDeVida);

        // 2. Apagamos el collider. A partir de este frame, el jugador ya no es arrastrado ni recibe daño
        if (colisionador != null) colisionador.enabled = false;

        // 3. Detenemos la emisión de todas las partículas (la principal y las hijas)
        // Usamos .Stop() en lugar de desactivar el objeto para que las partículas que ya nacieron terminen su animación naturalmente
        float maxTiempoParticula = 0f;
        foreach (var particula in sistemasDeParticulas)
        {
            particula.Stop();

            // Calculamos cuánto tarda en morir la partícula más longeva para saber cuándo destruir el GameObject
            if (particula.main.startLifetime.constantMax > maxTiempoParticula)
            {
                maxTiempoParticula = particula.main.startLifetime.constantMax;
            }
        }

        // 4. Esperamos a que la última partícula desaparezca de la pantalla
        yield return new WaitForSeconds(maxTiempoParticula);

        // 5. Destruimos el objeto limpiamente
        Destroy(gameObject);
    }
}