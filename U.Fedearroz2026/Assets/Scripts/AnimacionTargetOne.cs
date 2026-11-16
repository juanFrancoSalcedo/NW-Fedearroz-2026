using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimacionTargetOne : MonoBehaviour
{
    [Header("Objetos UI")]
    [Tooltip("Los 4 RectTransform que van a orbitar. Deben compartir padre.")]
    [SerializeField] private List<RectTransform> objetos = new List<RectTransform>();

    [Tooltip("Centro de la órbita en coordenadas locales del padre. (0,0) = centro de pantalla.")]
    [SerializeField] private Vector2 centroOrbita = Vector2.zero;

    [Header("Movimiento")]
    [SerializeField] private float zInicial = 1200f;
    [SerializeField] private float zFinal = 0f;
    [SerializeField] private float radio = 240f;
    [SerializeField] private float vueltas = 1f;
    [SerializeField] private float duracion = 3.5f;
    [SerializeField] private float anguloInicial = 0f;
    [SerializeField] private bool horario = true;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool usarTiempoNoEscalado = false;

    [Header("Acomodo final")]
    [Tooltip("Desde qué punto de la animación (0-1) empiezan a mezclarse las posiciones finales.")]
    [Range(0f, 1f)]
    [SerializeField] private float inicioAcomodo = 0.55f;

    [SerializeField] private List<Vector2> posicionesFinales = new List<Vector2>
    {
        new Vector2(200f, -280f),
        new Vector2(300f, 0f),
        new Vector2(-300f, 0f),
        new Vector2(-200f, -280f)
    };

    [Header("Reproducción")]
    [SerializeField] private bool playOnEnable = true;

    private Tween tween;

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        tween?.Kill();
        tween = null;
    }

    [ContextMenu("Reproducir animación")]
    public void Play()
    {
        tween?.Kill();
        tween = null;

        if (objetos == null || objetos.Count == 0)
            return;

        float direction = horario ? -1f : 1f;
        float startAngle = anguloInicial * Mathf.Deg2Rad;
        float finalAngle = startAngle + direction * vueltas * Mathf.PI * 2f;
        int count = objetos.Count;

        tween = DOTween.To(() => 0f, t =>
        {
            float angle = Mathf.Lerp(startAngle, finalAngle, t);
            float z = Mathf.Lerp(zInicial, zFinal, t);
            float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inicioAcomodo, 1f, t));

            for (int i = 0; i < count; i++)
            {
                RectTransform objeto = objetos[i];
                if (objeto == null)
                    continue;

                float a = angle + i * Mathf.PI * 2f / count;
                Vector3 orbit = new Vector3(
                    centroOrbita.x + Mathf.Cos(a) * radio,
                    centroOrbita.y + Mathf.Sin(a) * radio,
                    z);

                Vector2 final;
                if (posicionesFinales != null && i < posicionesFinales.Count)
                {
                    final = posicionesFinales[i];
                }
                else
                {
                    final = new Vector2(Mathf.Cos(a) * radio, Mathf.Sin(a) * radio);
                }

                Vector3 finalPosition = new Vector3(
                    centroOrbita.x + final.x,
                    centroOrbita.y + final.y,
                    z);

                objeto.localPosition = Vector3.LerpUnclamped(orbit, finalPosition, blend);
            }
        }, 1f, duracion)
        .SetEase(ease)
        .SetUpdate(usarTiempoNoEscalado)
        .OnComplete(() => tween = null);
    }
}
