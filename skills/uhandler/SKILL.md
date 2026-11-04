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