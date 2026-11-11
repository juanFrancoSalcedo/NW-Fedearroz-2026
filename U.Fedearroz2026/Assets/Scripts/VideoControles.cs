using UnityEngine;
using UnityEngine.Video;

public class VideoControles : MonoBehaviour
{
    [Header("Componentes")]
    public VideoPlayer videoPlayer;     // el VideoPlayer de ESTE video
    public GameObject panelControles;   // el panel con los botones de ESTE video
    public GameObject btnPlay;          // el botón Play de ESTE video
    public GameObject btnStop;          // el botón Stop de ESTE video

    void OnEnable()
    {
        if (panelControles != null) panelControles.SetActive(true);
        Reproducir();                   // arranca solo y muestra Stop
    }

    void OnDisable()
    {
        if (panelControles != null) panelControles.SetActive(false);
    }

    public void Reproducir()
    {
        if (videoPlayer != null) videoPlayer.Play();
        ActualizarBotones(true);
    }

    public void Pausar()
    {
        if (videoPlayer != null) videoPlayer.Pause();
        ActualizarBotones(false);
    }

    public void Detener()
    {
        if (videoPlayer != null) videoPlayer.Stop();   // frena y vuelve al inicio
        ActualizarBotones(false);
    }

    // true = reproduciendo (muestra Stop, oculta Play) | false = al revés
    void ActualizarBotones(bool reproduciendo)
    {
        if (btnPlay != null) btnPlay.SetActive(!reproduciendo);
        if (btnStop != null) btnStop.SetActive(reproduciendo);
    }
}