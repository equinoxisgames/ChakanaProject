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

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void OnEnable()
    {
        // Registro de callbacks según el Pipeline de Renderizado (URP o Built-in)
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
    public void ShakeDanio() => IniciarVibracion(0.2f, 0.4f);
    public void ShakeMuerteEnemigo() => IniciarVibracion(0.5f, 0.7f);

    public void IniciarVibracion(float duracion, float fuerza)
    {
        PrepararCamaras();
        duracionTotal = duracion;
        intensidad = fuerza;
        tiempoActual = 0;
        estaVibrando = true;
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
            tiempoActual += Time.deltaTime;
            float decaimiento = 1f - (tiempoActual / duracionTotal);

            // Generar vector aleatorio similar a CartoonFX
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

    // --- LÓGICA DE RENDERIZADO (El "Corazón" del script de CartoonFX) ---
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