using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControlNFC : MonoBehaviour
{
    [System.Serializable]
    public class TarjetaNFC
    {
        [Tooltip("Nombre interno, solo para ti")]
        public string nombre;          // "Tarjeta 1", "Llavero", etc.

        [Tooltip("UID de la tarjeta, mayúsculas sin espacios")]
        public string uid;

        [Tooltip("Objeto que se activa al detectar esta tarjeta: imagen, modelo 3D, video, panel...")]
        public GameObject contenido;

        [Tooltip("Acciones extra que se ejecutan al detectar esta tarjeta")]
        public UnityEvent alDetectar;
    }

    [Header("Todas las tarjetas que necesites: 1, 5, 100...")]
    public List<TarjetaNFC> tarjetas = new List<TarjetaNFC>();

    [Header("Comportamiento")]
    [Tooltip("Si está activado, al detectar una tarjeta se ocultan los contenidos de las demás")]
    public bool soloUnContenidoVisible = true;

    [Tooltip("Se invoca al dejar de leer la tarjeta (simulación o evento LOST del hardware)")]
    public UnityEvent alPerderLectura = new UnityEvent();

    [Header("Contenido por defecto")]
    [Tooltip("Se muestra cuando no hay ninguna tarjeta NFC detectada")]
    public GameObject contenidoPorDefecto;

    Dictionary<string, int> porUid;    // UID → índice en la lista
    string buffer = "";
    readonly List<Keyboard> keyboards = new List<Keyboard>();

    void OnEnable()
    {
        BindKeyboards();
    }

    void OnDisable()
    {
        UnbindKeyboards();
    }

    void Start()
    {
        porUid = new Dictionary<string, int>();
        for (int i = 0; i < tarjetas.Count; i++)
            if (!string.IsNullOrEmpty(tarjetas[i].uid))
                porUid[tarjetas[i].uid.ToUpperInvariant()] = i;

        foreach (TarjetaNFC tarjeta in tarjetas)
            if (tarjeta != null && tarjeta.contenido != null)
                tarjeta.contenido.SetActive(false);

        MostrarContenidoPorDefecto();
    }

    void Update()
    {
        BindKeyboards();

        foreach (Keyboard keyboard in keyboards)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                //ProcesarBuffer();
                break;
            }
        }
    }

    void BindKeyboards()
    {
        for (int i = keyboards.Count - 1; i >= 0; i--)
        {
            Keyboard keyboard = keyboards[i];
            if (keyboard == null || !keyboard.added)
            {
                if (keyboard != null)
                    keyboard.onTextInput -= OnTextInput;

                keyboards.RemoveAt(i);
            }
        }

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device is Keyboard keyboard && !keyboards.Contains(keyboard))
            {
                keyboard.onTextInput += OnTextInput;
                keyboards.Add(keyboard);
            }
        }
    }

    void UnbindKeyboards()
    {
        foreach (Keyboard keyboard in keyboards)
        {
            if (keyboard != null)
                keyboard.onTextInput -= OnTextInput;
        }

        keyboards.Clear();
    }

    void OnTextInput(char c)
    {
        
        // El hardware/Arduino "escribe" el UID como texto (ej. "FBFE2A54\n")
        // También acepta solo dígitos como índice (ej. "042\n") por compatibilidad.
        if (char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
            buffer += c;
        else if (c == '\n' || c == '\r')
            SearchProcesor(buffer);
    }

    public void SearchProcesor(string elBuffer)
    {
        var p = tarjetas.Find(t => t.uid.Equals(elBuffer));
        if (p != null)
            ProcesarLectura(elBuffer);
        else
            print($"Solo no existe el objeto {elBuffer}");
        //tarjetas.Find
        buffer = string.Empty;
    }

    private void ProcesarLectura(string valor)
    {
        string normalizado = valor.ToUpperInvariant();
        if (porUid.TryGetValue(normalizado, out int idx))
        {
            Activar(idx);
            return;
        }
        var compo = tarjetas.Find(c => c.uid.Equals(normalizado));
        NfcService.NotifyTag(new NfcModel(normalizado, compo.nombre));

        if (int.TryParse(valor, out int indice) && indice >= 0 && indice < tarjetas.Count)
        {
            Activar(indice);
            string uid = tarjetas[indice].uid;
            NfcService.NotifyTag(new NfcModel(string.IsNullOrEmpty(uid) ? valor : uid, (indice + 1).ToString()));
            return;
        }

        Debug.LogWarning($"[NFC] Valor no reconocido: {valor}");
    }

    // Lo llama NFCReaderAndroid, que ya emitió NfcService.NotifyTag con el contenido NDEF.
    public void OnTagDetected(string uid)
    {
        if (porUid.TryGetValue(uid.ToUpperInvariant().Trim(), out int idx))
        {
            Debug.Log($"[NFC] Detectada: {tarjetas[idx].nombre}");
            Activar(idx);
        }
        else
            Debug.LogWarning($"[NFC] Tarjeta no registrada: {uid}");
    }

    public void MostrarContenidoPorDefecto()
    {
        if (contenidoPorDefecto != null)
            contenidoPorDefecto.SetActive(true);
    }

    [ContextMenu("Simular fin de lectura NFC")]
    public void SimularFinDeLecturaNfc()
    {
        buffer = string.Empty;

        foreach (TarjetaNFC tarjeta in tarjetas)
        {
            if (tarjeta != null && tarjeta.contenido != null)
                tarjeta.contenido.SetActive(false);
        }

        MostrarContenidoPorDefecto();
        NfcService.Reset();
        alPerderLectura?.Invoke();
        Debug.Log("[NFC] Simulación: tarjeta retirada / búsqueda detenida.");
    }

    private void Activar(int i)
    {
        if (contenidoPorDefecto != null)
            contenidoPorDefecto.SetActive(false);

        if (soloUnContenidoVisible)
            foreach (var t in tarjetas)
                if (t != null && t.contenido != null)
                    t.contenido.SetActive(false);

        if (tarjetas[i].contenido != null)
            tarjetas[i].contenido.SetActive(true);

        tarjetas[i].alDetectar?.Invoke();
    }
}