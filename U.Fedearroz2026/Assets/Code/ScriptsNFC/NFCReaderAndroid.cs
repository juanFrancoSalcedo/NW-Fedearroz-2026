using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NFCReaderAndroid : MonoBehaviour
{
    [Tooltip("Opcional: objeto ControlNFC para compatibilidad con el flujo clásico (Proyecto B por UID).")]
    public ControlNFC control;

    AndroidJavaObject nfcAdapter;
    AndroidJavaObject activity;
    AndroidJavaProxy readerCallback;
    readonly Queue<NfcModel> lecturas = new Queue<NfcModel>();

    const int FLAG_READER_NFC_A = 0x01;

    bool isAndroid;

    void Start()
    {
        isAndroid = Application.platform == RuntimePlatform.Android;

        if (!isAndroid)
        {
            enabled = false;
            return;
        }

        using (var cls = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            activity = cls.GetStatic<AndroidJavaObject>("currentActivity");

        using (var cls = new AndroidJavaClass("android.nfc.NfcAdapter"))
            nfcAdapter = cls.CallStatic<AndroidJavaObject>("getDefaultAdapter", activity);

        if (nfcAdapter == null)
            Debug.LogError("Este dispositivo no tiene NFC");

        readerCallback = new ReaderCallback(lecturas);
    }

    void OnApplicationFocus(bool focus)
    {
        if (!isAndroid || nfcAdapter == null) return;
        if (focus)
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                nfcAdapter.Call("enableReaderMode", activity, readerCallback,
                    FLAG_READER_NFC_A, null)));
        else
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                nfcAdapter.Call("disableReaderMode", activity)));
    }

    void Update()
    {
        if (!isAndroid) return;

        lock (lecturas)
            while (lecturas.Count > 0)
            {
                NfcModel modelo = lecturas.Dequeue();

                NfcService.NotifyTag(modelo);

                if (control != null)
                    control.OnTagDetected(modelo.Uid);
            }
    }

    class ReaderCallback : AndroidJavaProxy
    {
        readonly Queue<NfcModel> cola;

        public ReaderCallback(Queue<NfcModel> c)
            : base("android.nfc.NfcAdapter$ReaderCallback") { cola = c; }

        void onTagDiscovered(AndroidJavaObject tag)
        {
            byte[] id = tag.Call<byte[]>("getId");
            string uid = System.BitConverter.ToString(id).Replace("-", "");

            string contenido = LeerNdef(tag);
            if (string.IsNullOrEmpty(contenido))
                contenido = uid;

            lock (cola)
                cola.Enqueue(new NfcModel(uid, contenido));
        }

        string LeerNdef(AndroidJavaObject tag)
        {
            try
            {
                using (var ndefClass = new AndroidJavaClass("android.nfc.tech.Ndef"))
                using (var ndef = ndefClass.CallStatic<AndroidJavaObject>("get", tag))
                {
                    if (ndef == null) return "";

                    AndroidJavaObject msg = ndef.Call<AndroidJavaObject>("getCachedNdefMessage");
                    if (msg == null)
                    {
                        try { msg = ndef.Call<AndroidJavaObject>("getNdefMessage"); }
                        catch { msg = null; }
                    }

                    if (msg == null) return "";

                    using (msg)
                    {
                        var records = msg.Call<AndroidJavaObject[]>("getRecords");
                        foreach (var record in records)
                        {
                            using (record)
                            {
                                short tnf = record.Call<short>("getTnf");
                                if (tnf != 0x01) continue;

                                byte[] type = record.Call<byte[]>("getType");
                                if (type == null || type.Length != 1 || type[0] != 0x54) continue;

                                byte[] payload = record.Call<byte[]>("getPayload");
                                return DecodificarTexto(payload);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[NFC] No se pudo leer NDEF: {e.Message}");
            }
            return "";
        }

        string DecodificarTexto(byte[] payload)
        {
            if (payload == null || payload.Length < 2) return "";
            int langLen = payload[0] & 0x3F;
            if (1 + langLen >= payload.Length) return "";
            return Encoding.UTF8.GetString(payload, 1 + langLen, payload.Length - 1 - langLen);
        }
    }
}
