---
name: usheet
description: Data wrapper pattern where Sheet (ScriptableObject) wraps Model (ICopy). Injected to handlers during configuration. Do not create ICopy if missing, alert the user.
---

# 1️⃣ Features Sheet (Módulos Funcionales)
## Regla clave

**Desacoplamiento de Unity**:
    - `Sheet`. wrapper del `Model` que es la data del sheet debe heredar del ICopy
    - ya existe el ICopy no lo crees, si no lo encuentras en B_Extensions.ICopy dimelo inmediatamente

---

##  Ejemplo: Boss Feature

### BossModel (Estado puro)
Mi modelo no usa constructor, solo se utiliza para mostrar y obtener los datos.
En el projecto ya existe una interfaz que se llama ICopy para obtener una copia de mi modelo

```csharp
[System.Serializable]
public class BossModel: ICopy<BossModel>
{
    public float Health;
    public float MoveSpeed;

    public BossModel Copy()
    {
        return (BossModel)this.MemberwiseClone();
    }
}
```

---


### BossSheet (Data)

es un scriptable object y suele estar en una lista del manager e injectarse al handler en la configuración

```csharp
public class BossSheet:ScriptableObject
{
    [Serializedfield] private Sprite iconBoss;
    public Sprite IconBoss => iconBoss;
    [Serializedfield] private BossModel model;
    public BossModel ModelCopy =>model.Copy();
    
}
```