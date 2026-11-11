using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NfcEventListener : MonoBehaviour
{
    [SerializeField] private bool subscribeOnEnable = true;

    [Header("Display")]
    [SerializeField] private TMP_Text display;

    [Header("Eventos")]
    [SerializeField] private UnityEvent<NfcModel> onTagRead;
    [SerializeField] private UnityEvent<string> onContenido;

    private void OnEnable()
    {
        if (subscribeOnEnable)
            Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Subscribe()
    {
        NfcService.OnTagRead -= HandleTagRead;
        NfcService.OnTagRead += HandleTagRead;
    }

    public void Unsubscribe()
    {
        NfcService.OnTagRead -= HandleTagRead;
    }

    private void HandleTagRead(NfcModel modelo)
    {
        if (display != null)
            display.text = modelo.Contenido;

        onTagRead?.Invoke(modelo);
        onContenido?.Invoke(modelo.Contenido);
    }
}
