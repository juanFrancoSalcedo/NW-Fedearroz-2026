---
name: uui
description: Unity UI script creation with naming convention type-first-then-feature. Uses BaseButtonAttendant and BaseToggleAttendant from B_Extension. If missing, alert user to install.
---

# 1️⃣ UI (Creador de scripts Interfaz gráfica)

Ejemplo:
```
Boss/UI/
    CardBoss.cs
    ButtonBossClaimReward.cs
    ToggleBossForgive.cs
    Boss[Nombre-sublógica]Displayer.cs
```

## Regla clave

Al nombrar la UI primero debes indicar lo que es luego su funcionalidad o ufeature asociado,

**Comunicación**:
    Cada UI debe vivir dentro de su carpeta, dentro de Features 
    `Features/`:
    `Features/[Nombre Feature]/UI` -> Contiene Card, Text, Button, Etc.
    Si es para un Service ... 
    `Services/`:
    `Services/[Nombre Service]/UI` -> Contiene Card, Text, Button, Etc.
---

## 🗂️ Jerarquía de GameObjects (escena)

> **TMP SIEMPRE**: todo texto usa `TMP_Text` (TextMeshPro). JAMÁS el `Text` legacy de Unity.

Nomenclatura con etiquetas de corchetes según el rol del GameObject en la jerarquía:

| Etiqueta | Uso | Ejemplo |
|---|---|---|
| `Canvas (feature)` | Raíz de UI, uno por feature (ver `bwrigger`) | `Canvas (Inventory)` |
| `[Screen]` | Pantalla completa | `[Screen] MainMenu` |
| `[Panel]` | Subsección dentro de un screen | `[Panel] Settings` |
| `[Content]` | Contenedor scrolleable (Scroll Rect) | `[Content] ItemList` |
| `[Card]` | Ítem reutilizable | `[Card] Building` |
| `[Button]` | Botón | `[Button] Claim` |
| `[Text]` | Texto TMP | `[Text] BuildingName` |
| `[Image]` | Imagen / thumbnail | `[Image] Thumbnail` |
| `[Toggle]` | Toggle | `[Toggle] Forgive` |
| `[Slider]` | Slider | `[Slider] Volume` |

Cadena flexible (niveles opcionales):

```
Canvas (feature)
 └─ [Screen]            (obligatorio)
     ├─ [Panel]         (opcional)
     │   └─ [Content]   (opcional)
     │       └─ [Card]  (opcional, se repite)
     │           └─ [Button]/[Text]/[Image]/[Toggle]  (hojas)
     └─ [Button]        (hojas directas si no hay panel)
```

Reglas:
- `[Screen]` siempre presente; el resto se omite según la pantalla.
- Las hojas (`[Button]`, `[Text]`, `[Image]`, `[Toggle]`, `[Slider]`) van siempre al final.
- El script (type-first) y el GameObject (etiqueta) conviven: `[Card] Boss` en escena ↔ `CardBoss.cs` en código.

```csharp
public class CardBoss:Monobehaviour
{
    [SerializedField] Image thumbnail;
    [SerializedField] ButtonClaimReward btnClaim;
    [SerializedField] TextBoss textBoss;

    BossModel _bossModel;
    public void InjectBoss(BossModel _bossModel)
    {
        _bossModel = bossModel;
    }
    
    public void Display()
    {
        textBoss.SetText(_bossModel.name);
        btnClaim.EnableButton();
    }
}
```

## Buttons Extensions
Yo uso un plugin creado por mi, llamado B_Extensions donde encuentras
una clase llamada BaseButtonAttendant se utiliza para hacer funcionalidades o llamar métodos, y también puedes obtener el componente Button de unity por medio buttonComponent , también aplica para Toggle = toggleComponent
su nombre es BaseToggleComponent
- si no encuentras el B_Extensions o los scripts BaseButtonAttendant o BaseToggleComponent recuerda al usuario que debe install it

```csharp
public class ButtonClaimReward:BaseButtonAttendant
{
    private void Start()
    {
        buttonComponent.onClick.AddListener(Claim);
    }
    
    public void Claim()
    {
        print("Function");
    }

    public void EnableButton()
    {
        buttonComponent.interactable = true;
    }
}
```

## 🎨 Paleta de colores (B_Extensions)

La paleta del proyecto vive en `Assets/B_Extension/Simple/Palette/paletteColor.json`.
Ventana de editor: `B_Extensions/Palette` (`WindowColorPalette.cs`).

- Formato JSON: array `colores` con `hexCode` en `#RRGGBBAA` (RGBA, 8 dígitos).
- Lectura en código: `ColorUtility.TryParseHtmlString(hex, out color)`.
- Aplicar a un `Image`: `image.color = color;`

Ejemplo de estructura (los colores reales los define el Capitán):

```json
{
    "colores": [
        { "hexCode": "#3C9FBEFF" },
        { "hexCode": "#735DF2DC" },
        { "hexCode": "#22516BC2" }
    ]
}
```

> La paleta es fuente de verdad del Capitán: no hardcodear los colores en los scripts ni en este skill; leerlos siempre del JSON.

