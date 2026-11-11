using System;

[Serializable]
public struct NfcModel
{
    public string Uid;
    public string Contenido;

    public NfcModel(string uid, string contenido)
    {
        Uid = uid;
        Contenido = contenido;
    }
}
