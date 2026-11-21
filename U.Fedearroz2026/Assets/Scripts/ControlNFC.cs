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

    [Header("Reemplazo de tarjetas (evento)")]
    [Tooltip("Evento opcional para mostrar mensajes de estado en un Text de la UI")]
    public UnityEvent<string> alCambiarEstado;

    Dictionary<string, int> porUid;    // UID → índice en la lista
    string buffer = "";
    readonly List<Keyboard> keyboards = new List<Keyboard>();

    // Índice de la tarjeta que está esperando un UID nuevo (-1 = modo normal)
    int slotPendiente = -1;

    void OnEnable()
    {
        BindKeyboards();
    }

    void OnDisable()
    {
        UnbindKeyboards();
        slotPendiente = -1;
    }

    void Start()
    {
        // Aplicar reemplazos guardados en sesiones anteriores (sobreviven al cerrar la app)
        for (int i = 0; i < tarjetas.Count; i++)
        {
            string guardado = PlayerPrefs.GetString("nfc_uid_" + i, "");
            if (!string.IsNullOrEmpty(guardado))
                tarjetas[i].uid = guardado;
        }

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
            ProcesarLectura(buffer);
    }

    // ===== MODO REEMPLAZO: se llama desde botones de la UI o del gatillo VR =====

    // indice = posición de la tarjeta en la lista (0 = la primera)
    public void IniciarReemplazo(int indice)
    {
        if (indice < 0 || indice >= tarjetas.Count)
            return;

        slotPendiente = indice;
        alCambiarEstado?.Invoke($"REEMPLAZO: acerque la tarjeta NUEVA de \"{tarjetas[indice].nombre}\"");
    }

    public void CancelarReemplazo()
    {
        if (slotPendiente < 0)
            return;

        slotPendiente = -1;
        alCambiarEstado?.Invoke("Reemplazo cancelado.");
    }

    void ReemplazarTarjeta(string nuevoUid)
    {
        // Si la tarjeta ya pertenece a otro slot, avisar y seguir esperando
        if (porUid.TryGetValue(nuevoUid, out int ocupado) && ocupado != slotPendiente)
        {
            alCambiarEstado?.Invoke($"Esa tarjeta ya es de \"{tarjetas[ocupado].nombre}\". Acerque la tarjeta NUEVA.");
            return;
        }

        string viejo = tarjetas[slotPendiente].uid;
        if (!string.IsNullOrEmpty(viejo) && porUid.ContainsKey(viejo))
            porUid.Remove(viejo);

        tarjetas[slotPendiente].uid = nuevoUid;
        porUid[nuevoUid] = slotPendiente;
        PlayerPrefs.SetString("nfc_uid_" + slotPendiente, nuevoUid);
        PlayerPrefs.Save();

        alCambiarEstado?.Invoke($"Listo: \"{tarjetas[slotPendiente].nombre}\" ahora usa {nuevoUid}");
        slotPendiente = -1;
    }

    // ===========================================================

    // Compatibilidad con la versión anterior del script.
    public void SearchProcesor(string elBuffer)
    {
        ProcesarLectura(elBuffer);
    }

    public void ProcesarLectura(string valor)
    {
        string normalizado = (valor ?? string.Empty).Trim().ToUpperInvariant();
        buffer = string.Empty;

        if (string.IsNullOrEmpty(normalizado))
            return;

        // Modo reemplazo: cualquier tarjeta que se acerque se convierte en la nueva
        if (slotPendiente >= 0)
        {
            ReemplazarTarjeta(normalizado);
            return;
        }

        if (porUid != null && porUid.TryGetValue(normalizado, out int idx))
        {
            Activar(idx);
            return;
        }

        var compo = tarjetas.Find(c => !string.IsNullOrEmpty(c.uid) && c.uid.ToUpperInvariant() == normalizado);
        if (compo != null)
        {
            int indice = tarjetas.IndexOf(compo);
            NfcService.NotifyTag(new NfcModel(normalizado, compo.nombre));
            Activar(indice);
            return;
        }

        if (int.TryParse(normalizado, out int indiceNumerico) && indiceNumerico >= 0 && indiceNumerico < tarjetas.Count)
        {
            Activar(indiceNumerico);
            string uid = tarjetas[indiceNumerico].uid;
            NfcService.NotifyTag(new NfcModel(string.IsNullOrEmpty(uid) ? normalizado : uid, (indiceNumerico + 1).ToString()));
            return;
        }

        Debug.LogWarning($"[NFC] Tarjeta no registrada: {normalizado}");
    }

    // Lo llama NFCReaderAndroid, que ya emitió NfcService.NotifyTag con el contenido NDEF.
    public void OnTagDetected(string uid)
    {
        string normalizado = (uid ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(normalizado))
            return;

        // Modo reemplazo: la tarjeta nueva se registra en el slot pendiente
        if (slotPendiente >= 0)
        {
            ReemplazarTarjeta(normalizado);
            return;
        }

        if (porUid != null && porUid.TryGetValue(normalizado, out int idx))
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
        slotPendiente = -1;

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
        if (i < 0 || i >= tarjetas.Count)
            return;

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
