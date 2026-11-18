using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public abstract class TargetManagerBase : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("Panel que controla este manager. La secuencia arranca al activarse el panel.")]
    [SerializeField] private GameObject panel;

    [Header("Objetos UI")]
    [Tooltip("RectTransform que orbitan y a los que se les anima el alpha.")]
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

    [Header("Alpha")]
    [Tooltip("Duración del fade-in al empezar la secuencia.")]
    [SerializeField] private float duracionFadeIn = 0.5f;

    [Tooltip("Duración del fade-out al terminar el retroceso, antes de desactivar y mostrar el video.")]
    [SerializeField] private float duracionFadeOut = 0.5f;

    [Header("Retroceso")]
    [Tooltip("Duración de la animación de retroceso de los objetos.")]
    [SerializeField] private float duracionRetroceso = 1.5f;

    [Header("Secuencia")]
    [Tooltip("Textos que se escriben cuando los objetos llegan a su posición final.")]
    [SerializeField] private List<TypingTextHandler> typingTexts = new List<TypingTextHandler>();

    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("Segundos que se mantiene visible el texto antes de la animación de retroceso.")]
    [SerializeField] private float tiempoEsperaVisualizacion = 4f;

    [Header("Rayo de luz (intro)")]
    [Tooltip("CanvasGroup del rayo de luz que aparece antes de la animación orbital. Opcional.")]
    [SerializeField] private CanvasGroup rayoLuz;

    [Tooltip("Duración del fundido de entrada del rayo.")]
    [SerializeField] private float rayoFadeIn = 0.45f;

    [Tooltip("Tiempo que el rayo permanece a plena intensidad mientras crece.")]
    [SerializeField] private float rayoEspera = 0.5f;

    [Tooltip("Escala inicial del rayo, para que crezca mientras aparece.")]
    [SerializeField] private float rayoEscalaInicial = 0.85f;

    [Tooltip("Escala final del rayo.")]
    [SerializeField] private float rayoEscalaFinal = 1.05f;

    [SerializeField] private Ease rayoEase = Ease.OutQuad;

    [Header("Fade de contenedores (Anima)")]
    [Tooltip("Índice de la animación de Anima que hace fade-in al llegar al centro (antes del typing).")]
    [SerializeField] private int indiceFadeInContenedor = 1;

    [Tooltip("Índice de la animación de Anima que hace fade-out al iniciar el retroceso.")]
    [SerializeField] private int indiceFadeOutContenedor = 0;

    [Tooltip("Alpha con el que arranca el contenedor en cada secuencia.")]
    [Range(0f, 1f)]
    [SerializeField] private float alphaInicialContenedor = 0f;

    private Tween moveTween;
    private Tween fadeTween;
    private Tween rayoTween;
    private Coroutine waitRoutine;
    private Coroutine introRoutine;
    private readonly List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
    private readonly List<AnimationUIController> animadoresContenedores = new List<AnimationUIController>();
    private readonly List<Image> imagenesContenedores = new List<Image>();
    private bool panelWasActive;
    private bool running;

    private void Awake()
    {
        CacheCanvasGroups();
        CacheContenedores();
    }

    private void OnEnable()
    {
        panelWasActive = panel != null && panel.activeInHierarchy;
        running = false;
    }

    private void OnDisable()
    {
        StopSequence();
    }

    private void Update()
    {
        if (panel == null)
            return;

        bool active = panel.activeInHierarchy;

        if (active && !panelWasActive)
            StartSequence();
        else if (!active && panelWasActive)
            StopSequence();

        panelWasActive = active;
    }

    private void CacheCanvasGroups()
    {
        canvasGroups.Clear();

        if (objetos == null)
            return;

        foreach (RectTransform objeto in objetos)
        {
            if (objeto == null)
                continue;

            CanvasGroup group = objeto.GetComponent<CanvasGroup>();

            if (group == null)
                group = objeto.gameObject.AddComponent<CanvasGroup>();

            canvasGroups.Add(group);
        }
    }

    private void CacheContenedores()
    {
        animadoresContenedores.Clear();
        imagenesContenedores.Clear();

        if (objetos == null)
            return;

        foreach (RectTransform objeto in objetos)
        {
            if (objeto == null)
                continue;

            animadoresContenedores.Add(objeto.GetComponent<AnimationUIController>());
            imagenesContenedores.Add(objeto.GetComponent<Image>());
        }
    }

    private void PrepararContenedores()
    {
        for (int i = 0; i < animadoresContenedores.Count; i++)
        {
            if (i < imagenesContenedores.Count && imagenesContenedores[i] != null)
            {
                Color color = imagenesContenedores[i].color;
                color.a = alphaInicialContenedor;
                imagenesContenedores[i].color = color;
            }

            if (animadoresContenedores[i] != null)
                animadoresContenedores[i].ActiveAnimation(indiceFadeOutContenedor);
        }
    }

    private void ReproducirFadeContenedores(int indice)
    {
        foreach (AnimationUIController animador in animadoresContenedores)
        {
            if (animador != null)
                animador.ActiveAnimation(indice);
        }
    }

    private void StartSequence()
    {
        if (running)
            return;

        running = true;

        KillTweens();
        StopWait();

        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(false);

        SetObjetosActive(true);
        SetAlpha(0f);
        PrepararContenedores();

        introRoutine = StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        if (rayoLuz != null)
            yield return RayoRoutine();

        introRoutine = null;

        if (!running)
            yield break;

        fadeTween = DOTween.To(() => 0f, SetAlpha, 1f, duracionFadeIn)
            .SetEase(ease)
            .SetUpdate(usarTiempoNoEscalado);

        PlayForward();
    }

    private IEnumerator RayoRoutine()
    {
        GameObject rayo = rayoLuz.gameObject;

        rayo.SetActive(true);
        rayoLuz.alpha = 0f;
        rayoLuz.interactable = false;
        rayoLuz.blocksRaycasts = false;

        Transform rayoTransform = rayoLuz.transform;
        rayoTransform.localScale = Vector3.one * rayoEscalaInicial;

        rayoTween = DOTween.Sequence()
            .SetUpdate(usarTiempoNoEscalado)
            .Join(DOTween.To(() => 0f, SetRayoAlpha, 1f, rayoFadeIn).SetEase(rayoEase))
            .Join(rayoTransform.DOScale(rayoEscalaFinal, rayoFadeIn + rayoEspera).SetEase(Ease.OutSine));

        yield return rayoTween.WaitForCompletion();
        rayoTween = null;

        // El rayo se queda encendido durante toda la animación.
        SetRayoAlpha(1f);
    }

    private void StopSequence()
    {
        running = false;

        KillTweens();
        StopWait();

        SetAlpha(1f);

        if (rayoLuz != null)
        {
            rayoLuz.alpha = 0f;
            rayoLuz.gameObject.SetActive(false);
        }

        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(false);
    }

    private void KillTweens()
    {
        moveTween?.Kill();
        moveTween = null;

        fadeTween?.Kill();
        fadeTween = null;

        rayoTween?.Kill();
        rayoTween = null;
    }

    private void StopWait()
    {
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        if (waitRoutine == null)
            return;

        StopCoroutine(waitRoutine);
        waitRoutine = null;
    }

    private void SetRayoAlpha(float alpha)
    {
        if (rayoLuz != null)
            rayoLuz.alpha = alpha;
    }

    private void PlayForward()
    {
        moveTween?.Kill();
        moveTween = null;

        if (objetos == null || objetos.Count == 0)
        {
            OnForwardComplete();
            return;
        }

        moveTween = DOTween.To(() => 0f, ApplyPose, 1f, duracion)
            .SetEase(ease)
            .SetUpdate(usarTiempoNoEscalado)
            .OnComplete(() =>
            {
                moveTween = null;
                OnForwardComplete();
            });
    }

    private void OnForwardComplete()
    {
        if (!running)
            return;

        ReproducirFadeContenedores(indiceFadeInContenedor);

        foreach (TypingTextHandler typing in typingTexts)
        {
            if (typing != null)
                typing.Play();
        }

        StopWait();
        waitRoutine = StartCoroutine(WaitThenReverse());
    }

    private IEnumerator WaitThenReverse()
    {
        yield return new WaitForSecondsRealtime(tiempoEsperaVisualizacion);
        waitRoutine = null;

        PlayReverse();
    }

    private void PlayReverse()
    {
        ReproducirFadeContenedores(indiceFadeOutContenedor);

        moveTween?.Kill();
        moveTween = null;

        if (objetos == null || objetos.Count == 0)
        {
            OnReverseComplete();
            return;
        }

        moveTween = DOTween.To(() => 1f, ApplyPose, 0f, duracionRetroceso)
            .SetEase(ease)
            .SetUpdate(usarTiempoNoEscalado)
            .OnComplete(() =>
            {
                moveTween = null;
                OnReverseComplete();
            });
    }

    private void OnReverseComplete()
    {
        if (!running)
            return;

        fadeTween?.Kill();
        fadeTween = DOTween.To(() => 1f, SetAlpha, 0f, duracionFadeOut)
            .SetEase(Ease.Linear)
            .SetUpdate(usarTiempoNoEscalado)
            .OnComplete(() =>
            {
                fadeTween = null;
                OnFadeOutComplete();
            });
    }

    private void OnFadeOutComplete()
    {
        if (!running)
            return;

        SetObjetosActive(false);
        SetAlpha(1f);
        ShowVideo();
    }

    private void ShowVideo()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void SetObjetosActive(bool active)
    {
        if (objetos == null)
            return;

        foreach (RectTransform objeto in objetos)
        {
            if (objeto != null)
                objeto.gameObject.SetActive(active);
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (CanvasGroup group in canvasGroups)
        {
            if (group != null)
                group.alpha = alpha;
        }
    }

    private void ApplyPose(float t)
    {
        float direction = horario ? -1f : 1f;
        float startAngle = anguloInicial * Mathf.Deg2Rad;
        float finalAngle = startAngle + direction * vueltas * Mathf.PI * 2f;
        int count = objetos.Count;

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
    }
}
