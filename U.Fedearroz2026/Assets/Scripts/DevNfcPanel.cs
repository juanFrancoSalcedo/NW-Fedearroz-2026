using UnityEngine;

/// <summary>
/// Panel de pruebas (solo dev): activa los targets de ControlNFC sin acercar una tarjeta física.
/// Simula la lectura por índice, igual que si el lector NFC hubiera enviado el UID.
/// </summary>
[DisallowMultipleComponent]
public class DevNfcPanel : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("ControlNFC de la escena que se va a simular")]
    public ControlNFC control;

    [Tooltip("Contenedor con los botones (para ocultar/mostrar el panel)")]
    public GameObject cuerpo;

    void Awake()
    {
        if (control == null)
            control = FindFirstObjectByType<ControlNFC>(FindObjectsInactive.Include);
    }

    /// <summary>Activa la tarjeta en la posición indicada de la lista de ControlNFC.</summary>
    public void ActivarTarjeta(int indice)
    {
        if (control == null)
        {
            Debug.LogWarning("[DevNFC] No hay ControlNFC asignado.");
            return;
        }

        if (indice < 0 || indice >= control.tarjetas.Count)
        {
            Debug.LogWarning($"[DevNFC] Índice de tarjeta fuera de rango: {indice}");
            return;
        }

        // ProcesarLectura con índice replica la lectura NFC (incluye NfcService.NotifyTag).
        control.ProcesarLectura(indice.ToString());
        Debug.Log($"[DevNFC] Tarjeta simulada: [{indice}] {control.tarjetas[indice].nombre}");
    }

    /// <summary>Simula el retiro de la tarjeta: vuelve al contenido por defecto.</summary>
    public void QuitarTarjeta()
    {
        if (control == null)
        {
            Debug.LogWarning("[DevNFC] No hay ControlNFC asignado.");
            return;
        }

        control.SimularFinDeLecturaNfc();
    }

    /// <summary>Muestra u oculta el cuerpo del panel de pruebas.</summary>
    public void AlternarPanel()
    {
        if (cuerpo != null)
            cuerpo.SetActive(!cuerpo.activeSelf);
    }
}
