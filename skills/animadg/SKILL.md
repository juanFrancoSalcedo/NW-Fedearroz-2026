---
name: animadg
description: Configure declarative DOTween animations with Anima DG Control in Unity (AnimationController, AnimationUIController, AnimationTextController). Use AnimationAssistant/listAux via the custom inspector or Unity Pipeline; never edit the vendor scripts.
---

# Anima DG Control (AnimaDG)

## Qué es

Asset/plugin de Unity (no es un paquete UPM) que funciona como una **capa declarativa sobre DOTween**. Vive en `Assets/Anima DG Control` (v1.4.3). Permite configurar animaciones por inspector con componentes y datos serializados, sin escribir código.

**Regla:** no editar los scripts del asset. Se actualizan y se pisan. Usar el inspector de Anima o los campos serializados vía pipeline.

## Dependencias

- **DOTween** (obligatorio). Sin `DG.Tweening` no compila Anima.
- **DOTween Pro / TMP Animator**: necesario para `DOTweenTMPAnimator`, `PerCharacters` y las funciones de texto Pro.
- `DirectivaDoTweens` activa solo el define `ANIMA_DOTWEEN_PRO` si existe:
  `Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.dll`
- En este repo: runtime free + `DOTweenPro.dll` (runtime) + `DOTweenTextMeshPro.cs` en `Assets/Plugins/Demigiant`; `ANIMA_DOTWEEN_PRO` está activo. No están el editor Pro ni el DOTween Utility Panel (se quitaron para evitar el cuelgue de importación).

## Controladores

| Componente | Target requerido | Anima |
|---|---|---|
| `AnimationUIController` | `RectTransform` + `Image` | Position, Scale, Rotation, SizeDelta, Fade, Color, Sprite, PixelMultiplier |
| `AnimationController` | `Transform` (+ `SpriteRenderer`) | Position, Scale, Rotation |
| `AnimationTextController` | `RectTransform` + `TMP_Text` | Position, Scale, Rotation, SizeDelta, Fade, Color/Outline/Text (Pro), spacing de caracteres/palabras/líneas |
| `BaseDoAnimationController` | abstracta | `listAux`, `currentAnimation`, `useTimeScale`, `inLoop`, `rewindOnDisable`, callbacks |

## Modelo de datos (`AnimationAssistant`)

Cada elemento de `listAux` es una animación:

| Campo | Uso |
|---|---|
| `timeAnimation`, `delay` | Duración y retardo |
| `animationCurve` | **Ease** (enums DOTween: `Linear`, `InOutQuad`, ...), no es una AnimationCurve |
| `loops` | `0` = una vez, `-1` = infinito |
| `playOnAwake` | Se reproduce al activar/entrar (el índice 0 al `OnEnable`) |
| `atractor` | Transform; usa su **localPosition** |
| `display` | Solo foldout del inspector; no activa nada |
| `displayPosition`, `targetPosition` | Mover a posición local |
| `displayScale`, `targetScale` | Escalar |
| `displayRotation`, `targetRotation`, `rotationType` | Rotar (`RotateMode`) |
| `displaySizeDelta`, `targetSizeDelta` | Cambiar tamaño UI |
| `displayTexture`, `spriteShift` | Cambiar sprite al completar |
| `displayColor`, `colorTarget` | Cambiar color (`Image`/TMP) |
| `displayFade`, `fadeTarget`, `applyOnCanvasGroup` | Fade por `Image` o `CanvasGroup` |
| `displayPixelMultiplier`, `pixelMultiplier` | `Image.pixelsPerUnitMultiplier` (9-slice) |
| `displayTextCharaterSplit`, `charSplittTarget` | Tracking de caracteres (TMP) |
| `displayTextWordSplit`, `wordSplitTarget` | Tracking de palabras |
| `displayTextLineSplit`, `lineSplitTarget` | Tracking de líneas |
| `displayTextOutlineColor`, `displayTextChange`, `newText` | Solo con DOTween Pro (`ANIMA_DOTWEEN_PRO`) |

`AnimationUIController` no usa los campos de texto. `AnimationController` no usa color/sprite/fade aunque estén en los datos.

## Configurar

### Inspector (humano)

1. Agregar el componente al objeto correcto.
2. En el inspector de Anima, `[Add Animation]` agrega un `AnimationAssistant`.
3. `[n]` expande, `%` duplica, `-` elimina, `<` / `>` reordenan.
4. Activar el `displayX` correspondiente y completar target/tiempo/ease/loops/`playOnAwake`.

### Pipeline (AI)

`listAux` es `[HideInInspector]`; se configura por campos anidados. En Git Bash usar `MSYS_NO_PATHCONV=1` para las rutas con espacios/corchetes.

```bash
P="/Canvas (Holobox)/[Screen] Holobox/.../[Image] RotationLinesRadar"

unity command component add --target "$P" --type "AnimationUIController"
# Devuelve un globalId; usarlo como target de set_field

GID="GlobalObjectId_V1-2-..."
unity command component set_field --target "$GID" --field "listAux.Array.size" --value 1
unity command component set_field --target "$GID" --field "listAux.Array.data[0].displayRotation" --value true
unity command component set_field --target "$GID" --field "listAux.Array.data[0].targetRotation" --value '{"x":0,"y":0,"z":360}'
unity command component set_field --target "$GID" --field "listAux.Array.data[0].timeAnimation" --value 4
unity command component set_field --target "$GID" --field "listAux.Array.data[0].animationCurve" --value "Linear"
unity command component set_field --target "$GID" --field "listAux.Array.data[0].rotationType" --value "FastBeyond360"
unity command component set_field --target "$GID" --field "listAux.Array.data[0].loops" --value=-1
unity command component set_field --target "$GID" --field "listAux.Array.data[0].playOnAwake" --value true

unity command component get_fields --target "$GID" --field "listAux.Array.data[0].targetRotation"
unity command scene save
```

Formatos: Vector3 como `{"x":0,"y":0,"z":360}`, enums por nombre (`Linear`, `FastBeyond360`), `loops=-1` con `--value=-1` para que no lo tome como flag.

## Recetas

### Rotar continuamente (radar)

`targetRotation = (0,0,360)`, `timeAnimation = 4`, `animationCurve = Linear`, `rotationType = FastBeyond360`, `loops = -1`, `playOnAwake = true`, `displayRotation = true`.

`FastBeyond360` es obligatorio para una vuelta completa: `Fast` (default) toma el camino más corto y 360 puede no rotar.

### Fade de UI

`displayFade = true`, `fadeTarget = 0`, `applyOnCanvasGroup = false` (usa `Image.DOFade`) o `true` (requiere `CanvasGroup`). Si el objeto no tiene `Image`/`CanvasGroup`, falla.

### Mover / escalar

`displayPosition` + `targetPosition` (local) o `displayScale` + `targetScale`. `atractor` reemplaza el target de posición usando su `localPosition`.

### Secuencia

Agregar varios `AnimationAssistant` en `listAux`; al completar uno, `PlusAnimationIndex` avanza al siguiente. `inLoop` en el componente reinicia la lista al terminar. `OnEndedCallBack` se invoca al finalizar la lista (no si `loops = -1`).

### Texto por caracteres (`PerCharacters`)

Requiere `ANIMA_DOTWEEN_PRO` y `DOTweenTMPAnimator`. Estrategias disponibles: `AnimCharRise` (sube + fade), `AnimCharDisplayByFall` (cae desde arriba), `AnimCharHorizon` (entra desde un lado), `AnimCharScaleFade` (fade simple), `AnimCharRandomPos`, `AnimCharFallInvert`.

El demo incluido usa dos componentes:

- `DemoDialogAnimation` en el `TMP_Text`: `textComponent`, `timePerChar` (segundos por caracter), `curve`.
- `DemoDialogManager` en un `[Handler]`: `dialogDisplayer`, `dialogs[0]` (texto), `interfaces[0]` (nombre de la estrategia). `Next()` usa solo el primer elemento de cada lista y se ejecuta una vez en `Start`.

Ejemplo en Holobox (`[Text] Tipos de arroz`):

```text
DemoDialogAnimation.textComponent = TextMeshProUGUI del texto
DemoDialogAnimation.timePerChar = 0.04
DemoDialogAnimation.curve = OutQuad
DemoDialogManager.dialogDisplayer = DemoDialogAnimation
DemoDialogManager.dialogs[0] = "Más de 7.000 tipos de arroz conservados"
DemoDialogManager.interfaces[0] = "AnimCharRise"
```

Para cambiar el efecto, poner otra estrategia como primer elemento de `interfaces`. El demo no reinicia limpio (`DemoDialogAnimation.AnimateText` corta la rutina si ya existe); para repetir, usar un handler propio que instancie la estrategia y el `DOTweenTMPAnimator`.

## Trampas conocidas (no corregir el asset, solo evitarlas)

- `sequence` puede ser null en `OnDisable`/`StopAnimations` si nunca se creó (por ejemplo `playOnAwake = false`). Evitar desactivar antes de la primera animación.
- Volver a llamar `ActiveAnimation` sin `StopAnimations()` superpone secuencias sobre el mismo target.
- `displayPixelMultiplier` lanza un coroutine que el `OnComplete` actual no detiene; evitar ese modo salvo necesidad.
- `AnimationController` asume `SpriteRenderer` en `OnDisable`; si el objeto no lo tiene, puede fallar.
- `AnimationUIController` necesita `Image`; `applyOnCanvasGroup` necesita `CanvasGroup`.
- Con `loops = -1`, `OnEndedCallBack`/`OnComplete` no se disparan.
- `ActiveAnimation(int)` no valida el índice; mantener `currentAnimation` dentro de `listAux`.
- `DORotate`/`DOLocalMove` trabajan sobre transform/world/local; con padres rotados o LayoutGroup pueden dar resultados inesperados.
- `animationCurve` es un enum `Ease`, no una curva.
- `display` solo abre/cierra el bloque en el inspector.
- `ANIMA_DOTWEEN_PRO` se define solo si existe `DOTweenPro.dll`; en este repo está activo.

## Verificación

- Play Mode: si `playOnAwake` está activo, la animación arranca sola. Para re-disparar: `ActiveAnimation(0)` o `RewindAndActiveAnimation()`.
- `component get_fields` para confirmar los valores serializados.
- `capture game_view` en Play Mode para inspeccionar el resultado (el Game view debe estar en el aspecto de la build).
- Guardar con `scene save`; la animación vive en la escena, no en un asset aparte.

## Referencias

- `Assets/Anima DG Control/Scripts/BaseDoAnimationController.cs`
- `Assets/Anima DG Control/Scripts/AnimationUIController.cs`
- `Assets/Anima DG Control/Scripts/AnimationController.cs`
- `Assets/Anima DG Control/Scripts/AnimationTextController.cs`
- `Assets/Anima DG Control/Scripts/Editor/BaseEditorAnimator.cs`
- `Assets/Anima DG Control/Scripts/Editor/EditorDoUIAnimator.cs`
- `Assets/Anima DG Control/Scripts/DirectivaDoTweens.cs`
- `skills/urigger/SKILL.md` (escena/wiring), `skills/uui/SKILL.md` (jerarquía UI)
