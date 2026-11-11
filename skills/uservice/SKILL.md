---
name: uservice
description: Global infrastructure services that live the entire game lifetime. Non-MonoBehaviour, single responsibility. Examples include, SaveService
---

# Services (Infraestructura Global)

Un `Service` es un sistema global que vive toda la vida del juego y NO depende de una escena concreta. No hereda de `MonoBehaviour`: es una clase estática (o POCO) con responsabilidad única.

Ejemplos:

- SaveService
- NfcService

Reglas:

- **Persistencia**: No debe depender de objetos específicos de una escena, no deben heredar de `MonoBehaviour`.
- **Single Responsibility**: Un servicio solo hace una cosa (ej. `SaveService` solo guarda/carga).
- **Comunicación por eventos**: expone eventos (C# `event Action<T>`) que otros módulos (un `EventListener` de `uhandler`) escuchan. No empuja datos hacia la UI directamente.
- **Propaga datos**: cuando el evento lleva información, se pasa un `Model` (struct POCO), no datos sueltos.

---

## 📡 Ejemplo: NfcService

Escucha una lectura NFC (desde el lector Android) e invoca un evento propagando los datos de la moneda.

### NfcModel (struct que guarda ID y contenido)

```csharp
using System;

[Serializable]
public struct NfcModel
{
    public string Uid;        // UID hexadecimal de la moneda (ej. "0433DF843F0289")
    public string Contenido;  // número en texto leído del NDEF (ej. "042")

    public NfcModel(string uid, string contenido)
    {
        Uid = uid;
        Contenido = contenido;
    }
}
```

### NfcService (invoca el evento al leer)

```csharp
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
        OnTagRead?.Invoke(modelo);   // propaga los datos a quien escuche
    }

    public static void Reset()
    {
        Ultimo = default;
        HayLectura = false;
    }
}
```

Quién invoca `NotifyTag` es el lector de bajo nivel (`NFCReaderAndroid`, MonoBehaviour), que convierte la señal de Android en un `NfcModel` y se lo entrega al servicio. Quién escucha `OnTagRead` es un `NfcEventListener` (ver skill `uhandler`).
