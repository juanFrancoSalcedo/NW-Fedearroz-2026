using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    Dictionary<string, int> porUid;    // UID → índice en la lista
    string buffer = "";

    void Start()
    {
        porUid = new Dictionary<string, int>();
        for (int i = 0; i < tarjetas.Count; i++)
            if (!string.IsNullOrEmpty(tarjetas[i].uid))
                porUid[tarjetas[i].uid.ToUpperInvariant()] = i;
    }

    void Update()
    {
        // El hardware/Arduino "escribe" el UID como texto (ej. "FBFE2A54\n")
        // También acepta solo dígitos como índice (ej. "042\n") por compatibilidad.
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                buffer += c;
            else if ((c == '\n' || c == '\r') && buffer.Length > 0)
            {
                string valor = buffer;
                buffer = "";

                if (porUid.ContainsKey(valor.ToUpperInvariant()))
                    OnTagDetected(valor);            // era un UID (FBFE2A54...)
                else if (int.TryParse(valor, out int idx) && idx >= 0 && idx < tarjetas.Count)
                    Activar(idx);                    // era un índice (000, 001...)
                else
                    Debug.LogWarning($"[NFC] Valor no reconocido: {valor}");
            }
        }
    }

    // Lo llama el plugin Android (Proyecto B) o el Update de arriba (Proyecto A)
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

    void Activar(int i)
    {
        if (soloUnContenidoVisible)
            foreach (var t in tarjetas)
                if (t != null && t.contenido != null)
                    t.contenido.SetActive(false);

        if (tarjetas[i].contenido != null)
            tarjetas[i].contenido.SetActive(true);

        tarjetas[i].alDetectar?.Invoke();
    }
}