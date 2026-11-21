using System.Reflection;
using UnityEngine;

/// <summary>
/// Cicla entre el modo normal y el modo reemplazo de cada tarjeta usando el gatillo
/// de Google Cardboard (Api.IsTriggerPressed).
///
/// El plugin se consulta por reflexión: si todavía no está instalado, el componente
/// no rompe la compilación y simplemente ignora el gatillo. También expone
/// <see cref="Siguiente"/> para conectarlo a un botón de UI (útil en PC/holobox).
/// </summary>
public class ReemplazoVR : MonoBehaviour
{
    [Tooltip("ControlNFC al que se le envía el cambio de modo.")]
    public ControlNFC control;

    static PropertyInfo propiedadGatillo;
    static bool yaBuscado;
    static bool avisoMostrado;

    bool triggerPrevio;
    int estado = 0; // 0 = normal, 1..N = reemplazando la tarjeta (estado - 1)

    void Update()
    {
        if (!TryLeerGatillo(out bool trigger))
            return;

        // Reacciona solo en el instante exacto en que se presiona el gatillo
        if (trigger && !triggerPrevio)
            Siguiente();

        triggerPrevio = trigger;
    }

    /// <summary>
    /// Avanza al siguiente estado: normal → reemplazo tarjeta 1 → tarjeta 2 → ... → normal.
    /// Se puede llamar desde el OnClick de un botón.
    /// </summary>
    [ContextMenu("Siguiente reemplazo")]
    public void Siguiente()
    {
        if (control == null || control.tarjetas.Count == 0)
            return;

        estado = (estado + 1) % (control.tarjetas.Count + 1);

        if (estado == 0)
            control.CancelarReemplazo();
        else
            control.IniciarReemplazo(estado - 1);
    }

    static bool TryLeerGatillo(out bool presionado)
    {
        presionado = false;

        if (!yaBuscado)
        {
            yaBuscado = true;

            System.Type tipoApi = System.Type.GetType("Google.XR.Cardboard.Api, Google.XR.Cardboard");

            if (tipoApi != null)
                propiedadGatillo = tipoApi.GetProperty("IsTriggerPressed",
                    BindingFlags.Public | BindingFlags.Static);
        }

        if (propiedadGatillo == null)
        {
            if (!avisoMostrado)
            {
                avisoMostrado = true;
                Debug.Log("[ReemplazoVR] Google Cardboard no está instalado: se ignora el gatillo. " +
                          "Usa Siguiente() desde un botón o instala el plugin (ver guía v16, R2.2).");
            }

            return false;
        }

        object valor = propiedadGatillo.GetValue(null);
        presionado = valor is bool b && b;
        return true;
    }
}
