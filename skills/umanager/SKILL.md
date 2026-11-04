---
name: umanager
description: Orchestrator and Facade pattern connecting multiple Handlers or Services. Inherits from Singleton T from B_Extension. Do not create your own Singleton if missing, alert the user.
---

# 1️⃣ Manager (Módulos Funcionales)

Una Manager es el conector y orquestador de una logica específica

## Regla clave

**Desacoplamiento de Unity**:
    - `Manager` tiene varios `uhandler` generalmente o tiene un `uservice` estatico no necessariamente los dos
    - es basado en el patron `Facade`
**Singleton**:
    - El manager suele heredar de un singleton, por lo general en nuestros proyectos existe un B_Extensions.Singleton
    en una carpeta dentro de ExternalAssets, IMPORTANTE si no está la carpeta o el script `Singleton` avisame no crees uno propio
    y yo instalare el paquete

---

### BossManager (Orquestador)

Maneja configura o escucha varios handlers, esto es ideal en logica que tiene que comunicarse con varios, o elegir cual es el `Handler` actual

```csharp
public class BossManager:Singleton<BossManager>
{
    private BossHandler[] handlers;
    private BossSheet bossData;

    private void Start()
    {
        foreach(var handler in handlers)
        {
            handler.Configure(bossData);
            handler.OnShot += Listen
        }
    }

    private void Listen()
    {
        Console.Log("Shot");
    }
}
```