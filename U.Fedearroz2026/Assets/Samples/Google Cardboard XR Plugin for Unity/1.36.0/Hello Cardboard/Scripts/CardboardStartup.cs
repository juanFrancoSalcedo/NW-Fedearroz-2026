using Google.XR.Cardboard;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Inicializa Cardboard de forma manual y segura:
/// 1. Inicializa el loader (sin arrancar el XR todavía)
/// 2. Guarda el perfil del visor si no existe
/// 3. Recién entonces arranca el XR -> VR
/// </summary>
public class CardboardStartup : MonoBehaviour
{
    public void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;

        var manager = XRGeneralSettings.Instance.Manager;

        // 1. Inicializa el loader de Cardboard (aún no arranca nada gráfico)
        if (manager.activeLoader == null)
        {
            manager.InitializeLoaderSync();
        }

        if (manager.activeLoader == null)
        {
            Debug.LogError("No se pudo inicializar el loader de Cardboard.");
            return;
        }

        // 2. Con el loader activo el Api funciona. Guarda el perfil
        //    genérico Cardboard v2 si no hay ninguno guardado.
        if (!Api.HasDeviceParams())
        {
            Api.SaveDeviceParams("https://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAASEIAAEhCAABIQgAASEJYADUpXA89OggeZnc-Ej6aPlAAYAM");
            Debug.Log("[Cardboard] Perfil genérico Cardboard v2 guardado.");
        }

        // 3. Ahora sí arranca el XR -> VR con el perfil ya guardado
        manager.StartSubsystems();
        Debug.Log("[Cardboard] XR iniciado.");
    }

    public void Update()
    {
        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

        if (Api.IsCloseButtonPressed)
        {
            Application.Quit();
        }

        if (Api.IsTriggerHeldPressed)
        {
            Api.Recenter();
        }

        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

#if !UNITY_EDITOR
        Api.UpdateScreenParams();
#endif
    }
}