using Google.XR.Cardboard;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Arranque para la vista VR "estática" (dos ojos, sin giroscopio).
///
/// Cardboard se inicializa a mano para guardar el perfil del visor antes del
/// primer frame (la inicialización automática provoca el cierre de la app), pero
/// NO se aplica la pose de la cabeza: el Tracked Pose Driver queda desactivado en
/// la cámara, así que la imagen se ve en estéreo y no se mueve con el teléfono.
/// </summary>
public class CardboardStaticStartup : MonoBehaviour
{
    [Tooltip("Bloquea la pantalla en horizontal para que el estéreo no se rote.")]
    [SerializeField] private bool bloquearHorizontal = true;

    [Tooltip("Mantiene el perfil de visor genérico (Cardboard v2) si no hay ninguno guardado.")]
    [SerializeField] private bool guardarPerfilGenerico = true;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;

        if (bloquearHorizontal)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        var manager = XRGeneralSettings.Instance.Manager;

        if (manager.activeLoader == null)
            manager.InitializeLoaderSync();

        if (manager.activeLoader == null)
        {
            Debug.LogError("[Cardboard] No se pudo inicializar el loader.");
            return;
        }

        if (guardarPerfilGenerico && !Api.HasDeviceParams())
        {
            Api.SaveDeviceParams("https://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAASEIAAEhCAABIQgAASEJYADUpXA89OggeZnc-Ej6aPlAAYAM");
            Debug.Log("[Cardboard] Perfil genérico Cardboard v2 guardado.");
        }

        manager.StartSubsystems();
        Debug.Log("[Cardboard] XR iniciado en modo estático (sin seguimiento de cabeza).");
    }

    private void Update()
    {
        if (Api.IsGearButtonPressed)
            Api.ScanDeviceParams();

        if (Api.IsCloseButtonPressed)
            Application.Quit();

        if (Api.HasNewDeviceParams())
            Api.ReloadDeviceParams();

#if !UNITY_EDITOR
        // Recalcula la proyección al tamaño/orientación real de la pantalla.
        // No mueve la cámara: solo ajusta el estéreo.
        Api.UpdateScreenParams();
#endif
    }
}
