---
name: urigger
description: agente de Escena, Prefabs y Wiring. Monta escenas, decide jerarquías de GameObjects aplicando la nomenclatura, coloca prefabs, y conecta escena con código ([SerializeField], [Header], referencias). Preguntar o verificar Unity 6 Pipeline CLI

## 🗂️ Nomenclatura de objetos

GameObject de la jerarquía se nombra con una etiqueta según su función. Esta es la norma
permanente del Capitán y aplica tanto al crear como al renombrar (`rename_gameobject`).

| Etiqueta | Uso | Ejemplos |
|----------|-----|----------|
| `[Env]` | Decorativos / escenografía, suelen marcarse como obstáculos | `[Env] Mazmorra Encantada` |
| `[Coll]` | Solo colisiones (colliders sin lógica) | `[Coll] Entrada`, `[Coll] wall_12` |
| `[Interact]` | Interactables, objetos con lógica (a veces son handlers `uhandler`) | `[Interact] Puerta` |
| `[Manager]` | Managers (orquestadores/fachadas) | `[Manager] Audio` |
| `[Caller]` | Callers de utilidad | ` usa ucaller ...` |
| `[Handler]` | Handlers de utilidad | `usa uhandler ...` |
| `Canvas (<feature>)` | Canvases de UI, uno por feature (paréntesis, no corchetes) | `Canvas (Settings)`, `Canvas (Inventory)` |
| `[Folder:<Tipo>]` | Contenedor vacío que agrupa a los hijos de la etiqueta `<Tipo>` | `[Folder:Interact]`, `[Folder:Managers]` |

Reglas:
- `[Folder:<Tipo>]` agrupa a los hijos de su etiqueta: `[Folder:Interact]` contiene todos/varios `[Interact]`, `[Folder:Managers]` a los `[Manager]`, etc. Aplica a todas las etiquetas (`[Folder:Env]`, `[Folder:Coll]`, `[Folder:Utils]`, `[Folder:UI]`...).
- `Canvas (feature)` admite varios según el tipo de canvas (uno por feature).
- Etiquetas de UI dentro de cada Canvas (`[Screen]`, `[Panel]`, `[Content]`, `[Card]`, `[Button]`, `[Text]`...) → ver skill `uui`.
- Al montar una escena, estructurar la raíz con los `[Folder:...]` correspondientes antes de anclar objetos.