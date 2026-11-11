using System;
using UnityEngine;

public static class NfcService
{
    public static event Action<NfcModel> OnTagRead;

    public static NfcModel Ultimo { get; private set; }
    public static bool HayLectura { get; private set; }

    public static void NotifyTag(NfcModel modelo)
    {
        Ultimo = modelo;
        HayLectura = true;

        OnTagRead?.Invoke(modelo);
        Debug.Log($"[NFC] UID={modelo.Uid} Contenido={modelo.Contenido}");
    }

    public static void Reset()
    {
        Ultimo = default;
        HayLectura = false;
    }
}
