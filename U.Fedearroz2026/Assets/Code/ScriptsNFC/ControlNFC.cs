using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControlNFC : MonoBehaviour
{
    [System.Serializable]
    public class TarjetaNFC
    {
        public string nombre;
        public string uid;              // UID de la tarjeta (Proyecto B)
        public GameObject contenido;
        public UnityEvent alDetectar;
    }

    public List<TarjetaNFC> tarjetas = new List<TarjetaNFC>();
    public bool soloUnContenidoVisible = true;

    Dictionary<string, int> porUid;
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
        // Proyecto A: índice que llega como texto "042\n" del Arduino
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c)) buffer += c;
            else if ((c == '\n' || c == '\r') && buffer.Length > 0)
            {
                int idx = int.Parse(buffer);
                buffer = "";
                if (idx >= 0 && idx < tarjetas.Count) Activar(idx);
            }
        }
    }

    // Proyecto B: lo llama NFCReaderAndroid con el UID de la tarjeta
    public void OnTagDetected(string uid)
    {
        if (porUid.TryGetValue(uid.ToUpperInvariant().Trim(), out int idx))
            Activar(idx);
        else
            Debug.LogWarning($"[NFC] Tarjeta no registrada: {uid}");
    }

    void Activar(int i)
    {
        if (soloUnContenidoVisible)
            foreach (var t in tarjetas)
                if (t.contenido != null) t.contenido.SetActive(false);

        if (tarjetas[i].contenido != null)
            tarjetas[i].contenido.SetActive(true);

        tarjetas[i].alDetectar.Invoke();
        Debug.Log($"[NFC] Activado: {tarjetas[i].nombre}");
    }
}