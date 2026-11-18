using DG.Tweening;
using UnityEngine;

/// <summary>
/// Anima las barras de la telemetría en ping-pong infinito con DOTween.
/// Cada barra usa su altura inicial como referencia:
/// si arranca pequeña (por debajo del umbral) sube hasta <see cref="alturaMaxima"/>,
/// si arranca alta baja hasta <see cref="alturaMinima"/>, y vuelve, eternamente.
/// </summary>
public class TelemetriaBarsAnimator : MonoBehaviour
{
    [Header("Barras")]
    [Tooltip("Si se deja vacío, se toman todos los hijos del objeto en orden.")]
    [SerializeField] private RectTransform[] barras;

    [Header("Rango de alturas")]
    [Tooltip("Altura mínima a la que baja una barra alta.")]
    [SerializeField] private float alturaMinima = 10f;

    [Tooltip("Altura máxima a la que sube una barra pequeña.")]
    [SerializeField] private float alturaMaxima = 60f;

    [Tooltip("Alturas iniciales por debajo de este umbral se consideran pequeñas y crecen; el resto se encoge.")]
    [SerializeField] private float umbralAltura = 35f;

    [Header("Tiempo")]
    [Tooltip("Duración de un recorrido (ida o vuelta), en segundos.")]
    [SerializeField] private float duracion = 1.2f;

    [Tooltip("Retardo acumulado por barra para que no se muevan todas a la vez. 0 = sincronizadas.")]
    [SerializeField] private float desfase = 0.06f;

    [Tooltip("Si está activo, la animación sigue aunque Time.timeScale sea 0.")]
    [SerializeField] private bool tiempoNoEscalado = false;

    [Header("Ciclo de vida")]
    [Tooltip("Devuelve las barras a su altura original al desactivarse el objeto.")]
    [SerializeField] private bool reiniciarAlDesactivar = true;

    private float[] alturasIniciales;
    private float[] anchos;
    private Tween[] tweens;

    private void Awake()
    {
        if (barras == null || barras.Length == 0)
        {
            barras = new RectTransform[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
                barras[i] = transform.GetChild(i) as RectTransform;
        }

        alturasIniciales = new float[barras.Length];
        anchos = new float[barras.Length];
        tweens = new Tween[barras.Length];

        for (int i = 0; i < barras.Length; i++)
        {
            if (barras[i] == null)
                continue;

            anchos[i] = barras[i].sizeDelta.x;
            alturasIniciales[i] = barras[i].sizeDelta.y;
        }
    }

    private void OnEnable()
    {
        Reproducir();
    }

    private void OnDisable()
    {
        Detener();

        if (reiniciarAlDesactivar)
            AplicarAlturas(alturasIniciales);
    }

    /// <summary>
    /// (Re)inicia el ping-pong de todas las barras desde su altura actual.
    /// </summary>
    [ContextMenu("Reproducir")]
    public void Reproducir()
    {
        if (barras == null || alturasIniciales == null)
            return;

        Detener();

        for (int i = 0; i < barras.Length; i++)
        {
            RectTransform barra = barras[i];

            if (barra == null)
                continue;

            float inicial = alturasIniciales[i];
            float destino = inicial <= umbralAltura ? alturaMaxima : alturaMinima;

            // Si la altura inicial ya coincide con el extremo, invertimos el destino
            // para que la barra siempre tenga recorrido.
            if (Mathf.Approximately(inicial, destino))
                destino = inicial <= umbralAltura ? alturaMinima : alturaMaxima;

            tweens[i] = barra
                .DOSizeDelta(new Vector2(anchos[i], destino), duracion)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(desfase * i)
                .SetUpdate(tiempoNoEscalado);
        }
    }

    /// <summary>
    /// Detiene la animación y deja las barras donde estén.
    /// </summary>
    [ContextMenu("Detener")]
    public void Detener()
    {
        if (tweens == null)
            return;

        for (int i = 0; i < tweens.Length; i++)
        {
            if (tweens[i] != null && tweens[i].IsActive())
                tweens[i].Kill();

            tweens[i] = null;
        }
    }

    private void AplicarAlturas(float[] alturas)
    {
        if (barras == null || alturas == null)
            return;

        for (int i = 0; i < barras.Length; i++)
        {
            if (barras[i] == null)
                continue;

            Vector2 size = barras[i].sizeDelta;
            size.y = alturas[i];
            barras[i].sizeDelta = size;
        }
    }
}
