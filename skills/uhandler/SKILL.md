---
name: uhandler
description: First-line functional module with single responsibility logic. Distinguishes Handler (has logic, communicates with Manager) from EventListener (only listens, exposes UnityEvents).
---

# 1️⃣ Handler (Módulos Funcionales)

Un Handler hace una logica especifica y es la primera linea, su logica es muy especifica trata de usar el principio de responsabilidad única


## Regla clave

**Desacoplamiento de Unity**:
    - `Handler`, `EventListener`. puede tener ese sufijo en el nombre
    - `Handler`, `EventListener`. la diferencia es que el handler tiene lógica y puede comunicarse con otros scripts incluso con un 
    `Manager`
    - El `EventListener` solo escucha y expone UnityEvents, es posible que tenga lógica específica pero no es lo natural
---

##  Ejemplo: Manejador de Cajas

### BoxHandler
```csharp
public class BoxHandler:Monobehaviour
{
    [SerializedField] private TriggerDetector detector;
    BoxManger boxManager;

    private void Start()
    {
        boxManager = BoxManager.Instance;
    }

    public void OnEnable()
    {
        detector.OnTriggerEntered += MoveDirection;
    }

    public void OnDisable()
    {
        detector.OnTriggerEntered += MoveDirection;
    }

    public void MoveDirection(Transfomr other)
    {
        // got to direction
    }
}
```


### BoxColliderEventListener

```csharp
public class BoxColliderEventListener:Monobehaviour
{
    [SerializedField] private TriggerDetector detector;
    [SerializedField] UnityEvent onEnter;
    public event Action OnEnter;

    public void OnEnable()
    {
        detector.onEnter += MoveDirection;
        dtector.OnEnter += MoveDirection;
    }

    public void OnDisable()
    {
        detector.onEnter += MoveDirection;
        dtector.OnEnter += MoveDirection;
    }

    public void Detect(Transfomr other)
    {
        onEnter?.Invoke();
        OnEnter?.Invoke();
    }
}
```

---

## 📡 EventListener que escucha un Service

Cuando el origen es un `Service` global (`uservice`), el `EventListener` se suscribe a su evento C# y lo re-expone como `UnityEvent` para cablearlo en el Inspector. Propaga los datos del `Model`.

### NfcEventListener (escucha NfcService.OnTagRead)

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NfcEventListener : MonoBehaviour
{
    [SerializeField] private bool subscribeOnEnable = true;

    [Header("Display")]
    [SerializeField] private TMP_Text display;                 // muestra el contenido leído

    [Header("Eventos")]
    [SerializeField] private UnityEvent<NfcModel> onTagRead;   // propaga los datos completos
    [SerializeField] private UnityEvent<string> onContenido;   // solo el número en texto

    private void OnEnable()
    {
        if (subscribeOnEnable) Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Subscribe()
    {
        NfcService.OnTagRead -= HandleTagRead;   // idempotente: evita duplicados
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
```

> El `EventListener` **solo escucha y expone UnityEvents**. La lógica real (leer NFC, decidir qué hacer con el número) vive en el `Service` y en los `Handler`.
