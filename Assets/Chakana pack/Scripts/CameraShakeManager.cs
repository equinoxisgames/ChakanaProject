using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("Configuración")]
    public bool usarCamaraPrincipal = true;
    public List<Camera> camarasManuales = new List<Camera>();

    private List<Camera> camarasActivas = new List<Camera>();
    private Dictionary<Camera, Vector3> posicionesOriginales = new Dictionary<Camera, Vector3>();

    private float tiempoActual;
    private float duracionTotal;
    private float intensidad;
    private Vector3 vectorShake;
    private bool estaVibrando;

    // Guardar el timescale original por si acaso ya estaba modificado (ej. cámara lenta)
    private float timeScaleOriginal;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void OnEnable()
    {
        if (GraphicsSettings.currentRenderPipeline == null)
        {
            Camera.onPreRender += AlPreRender;
            Camera.onPostRender += AlPostRender;
        }
        else
        {
            RenderPipelineManager.beginCameraRendering += AlPreRenderURP;
            RenderPipelineManager.endCameraRendering += AlPostRenderURP;
        }
    }

    void OnDisable()
    {
        if (GraphicsSettings.currentRenderPipeline == null)
        {
            Camera.onPreRender -= AlPreRender;
            Camera.onPostRender -= AlPostRender;
        }
        else
        {
            RenderPipelineManager.beginCameraRendering -= AlPreRenderURP;
            RenderPipelineManager.endCameraRendering -= AlPostRenderURP;
        }
    }

    // --- MÉTODOS DE DISPARO ---
    // (Duración Shake, Fuerza Shake, Duración Hitstop)
    public void ShakeDanio() => IniciarVibracion(0.2f, 0.45f, 0.1f);
    public void ShakeMuerteEnemigo() => IniciarVibracion(0.5f, 0.75f, 0.2f);

    public void IniciarVibracion(float duracion, float fuerza, float duracionHitstop = 0f)
    {
        // REGLA: Si ya está vibrando, ignoramos esta nueva llamada para evitar acumulación
        if (estaVibrando) return;

        PrepararCamaras();
        duracionTotal = duracion;
        intensidad = fuerza;
        tiempoActual = 0;
        estaVibrando = true;

        // Si se solicitó hitstop, ejecutamos la corrutina
        if (duracionHitstop > 0f)
        {
            StartCoroutine(RutinaHitstop(duracionHitstop));
        }
    }

    private void PrepararCamaras()
    {
        camarasActivas.Clear();
        posicionesOriginales.Clear();

        if (usarCamaraPrincipal && Camera.main != null) camarasActivas.Add(Camera.main);
        foreach (var c in camarasManuales) if (c != null && !camarasActivas.Contains(c)) camarasActivas.Add(c);

        foreach (var cam in camarasActivas) posicionesOriginales[cam] = cam.transform.localPosition;
    }

    void Update()
    {
        if (!estaVibrando) return;

        if (tiempoActual < duracionTotal)
        {
            // VITAL: Usamos unscaledDeltaTime para que el temblor no se congele durante el Hitstop
            tiempoActual += Time.unscaledDeltaTime;
            float decaimiento = 1f - (tiempoActual / duracionTotal);

            vectorShake = new Vector3(
                Random.Range(-1f, 1f) * intensidad * decaimiento,
                Random.Range(-1f, 1f) * intensidad * decaimiento,
                0f
            );
        }
        else
        {
            estaVibrando = false;
            vectorShake = Vector3.zero;
        }
    }

    // --- SISTEMA DE HITSTOP ---
    private IEnumerator RutinaHitstop(float duracion)
    {
        yield return new WaitForSecondsRealtime(0.01f);
        // Guardamos el TimeScale actual y congelamos el juego
        timeScaleOriginal = Time.timeScale;
        Time.timeScale = 0f;

        // Esperamos en tiempo real (ignorando el congelamiento)
        yield return new WaitForSecondsRealtime(duracion);

        // Restauramos el tiempo a la normalidad
        Time.timeScale = timeScaleOriginal;
    }

    // --- LÓGICA DE RENDERIZADO ---
    private void AlPreRender(Camera cam)
    {
        if (estaVibrando && posicionesOriginales.ContainsKey(cam))
        {
            posicionesOriginales[cam] = cam.transform.localPosition;
            cam.transform.localPosition += vectorShake;
        }
    }

    private void AlPostRender(Camera cam)
    {
        if (posicionesOriginales.ContainsKey(cam))
        {
            cam.transform.localPosition = posicionesOriginales[cam];
        }
    }

    // Adaptadores para URP
    private void AlPreRenderURP(ScriptableRenderContext ctx, Camera cam) => AlPreRender(cam);
    private void AlPostRenderURP(ScriptableRenderContext ctx, Camera cam) => AlPostRender(cam);
}