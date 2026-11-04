using System.Collections.Generic;
using UnityEngine;

public class NFCReaderAndroid : MonoBehaviour
{
    public ControlNFC control;   // arrastra aquí tu objeto ControlNFC

    AndroidJavaObject nfcAdapter;
    AndroidJavaObject activity;
    AndroidJavaProxy readerCallback;
    readonly Queue<string> uids = new Queue<string>();

    const int FLAG_READER_NFC_A = 0x01;
    const int FLAG_READER_SKIP_NDEF_CHECK = 0x80;

    void Start()
    {
        using (var cls = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            activity = cls.GetStatic<AndroidJavaObject>("currentActivity");

        using (var cls = new AndroidJavaClass("android.nfc.NfcAdapter"))
            nfcAdapter = cls.CallStatic<AndroidJavaObject>("getDefaultAdapter", activity);

        if (nfcAdapter == null)
            Debug.LogError("Este dispositivo no tiene NFC");

        readerCallback = new ReaderCallback(uids);
    }

    void OnApplicationFocus(bool focus)
    {
        if (nfcAdapter == null) return;
        if (focus)
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                nfcAdapter.Call("enableReaderMode", activity, readerCallback,
                    FLAG_READER_NFC_A | FLAG_READER_SKIP_NDEF_CHECK, null)));
        else
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                nfcAdapter.Call("disableReaderMode", activity)));
    }

    void Update()
    {
        lock (uids)
            while (uids.Count > 0)
                control.OnTagDetected(uids.Dequeue());
    }

    class ReaderCallback : AndroidJavaProxy
    {
        Queue<string> cola;
        public ReaderCallback(Queue<string> c)
            : base("android.nfc.NfcAdapter$ReaderCallback") { cola = c; }

        // Lo llama Android en un hilo de fondo al acercar una tarjeta
        void onTagDiscovered(AndroidJavaObject tag)
        {
            byte[] id = tag.Call<byte[]>("getId");
            string uid = System.BitConverter.ToString(id).Replace("-", "");
            lock (cola) cola.Enqueue(uid);
        }
    }
}
