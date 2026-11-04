---
name: material-components-ui
description: Construye una "colección de UI" (UI kit / catálogo de componentes web en HTML+CSS+JS) con sistema de tokens, paleta copiable, panel, botón de ejemplo, cartas de estado y herramientas por componente (descargar PNG, escala de grises, sin texto, ocultar botones). Incluye 20 direcciones de estilo (glassmorphism, neumorphism, brutalismo, claymorphism, material, HUD, etc.). Cargar cuando se pida una colección de UI, UI kit, component gallery, design system, página de componentes o exportar componentes a PNG.
---

# material-components-ui · Colección de UI

Skill **standalone** (no depende de la tripulación). Define qué es una "colección
de UI" y cómo se construye una página de componentes con las mismas capacidades
que el kit de referencia (`index.html`, `design-system.css`, `app.css`, `kit.js`).

> Documento extendido: `UI Components/SPEC-COLECCION-UI.md`.

## Cuándo cargar

- "Haz una colección de UI / UI kit / catálogo de componentes para `<proyecto>`".
- "Página de componentes" / "design system" / "component gallery".
- "Necesito exportar componentes a PNG (gris / sin texto / sin botones)".
- "Aplica estilo glassmorphism / brutalismo / ... a la UI".

## Salida obligatoria

| Archivo | Rol |
| --- | --- |
| `index.html` | Estructura, catálogo, paleta y ejemplos. |
| `design-system.css` | Tokens (`:root`) + componentes `.mc-*` portables. |
| `app.css` | Layout de la página (tabs, sheet, grid, tiles, swatches). |
| `kit.js` | Exportar, modos y copiar tokens. |
| `html2canvas` | Única dependencia, por CDN. |

Regla: `design-system.css` debe poder copiarse a otro proyecto sin arrastrar
`app.css`. Usa prefijo `mc-` para componentes portables; sin prefijo para layout.

## Flujo de trabajo

1. **Elegir estilo** de la sección "20 direcciones" y anotar los tokens a tocar.
2. **Definir tokens** en `:root`: escalas `--mc-<color>-900…200`, semánticos
   `ok/bad`, `--mc-ink/-soft/-mute`, radios, espacios, tipografía, sombras,
   `--mc-glass` y `--mc-focus`. Cero colores literales en componentes.
3. **Montar las 3 vistas** sin JS: Componentes · Paleta · Ejemplos.
4. **Construir las piezas** (panel, botón de ejemplo, carta de estado, tiles).
5. **Cablear las 4 herramientas** por componente (ver abajo).
6. **Pasar el checklist** de aceptación antes de entregar.

## Piezas obligatorias

### Panel
```
.mc-panel
├── __rail      # gradiente de marca (primario → acento)
├── __header    # __mark + __eyebrow + __title + __close
├── __body      # texto + .mc-divider + contenido
└── __footer    # acciones (.mc-button--sm) alineadas a la derecha
```
Translúcido con `backdrop-filter: blur(14px) saturate(1.15)` + `--mc-glass`,
radio lg, `--mc-shadow-panel`. Es exportable (`data-export-node`) y se exporta
con **fondo transparente** (el lienzo no pinta color detrás: las esquinas
redondeadas quedan limpias, aunque la superficie `--mc-glass` del panel sí se
conserva).

### Botón de ejemplo
Es la demo interactiva y el nodo que se captura. `.mc-button` base + variantes:
`--primary`, `--accent`, `--ghost`, `--danger`, tamanos `--sm`/`--lg`,
`--block`, `--icon`, y `:disabled`. Hover `translateY(-2px)` + glow, active
`scale(.985)`, `:focus-visible` con `--mc-focus`. Se exporta con **fondo
transparente**.

### Carta de estado
Dos resultados conmutables **sin JS**: `<input type="checkbox">` oculto +
`:has(:checked)`. Base = incorrecta (peligro); marcada = correcta (acento).
Cambian borde, fondo, glow, icono, texto de estado y label de acción. Se exporta
con **fondo transparente** (lienzo sin color; conserva su superficie radial).

### Tile
```
.tile [data-export-name="slug"]
├── .tile__head → .tile__name + .tile__tools
└── .tile__stage.capture-area → [data-export-node]
```
`.tile--wide` para paneles/cartas. El título se pinta en acento cuando hay un
modo activo (`:has(.is-grayscale|.is-textless|.is-no-buttons)`).

## Las 4 herramientas (obligatorias por tile)

| Herramienta | Atributo | Efecto | Estado |
| --- | --- | --- | --- |
| Descargar PNG | `data-download` | Exporta a PNG. | `data-state="busy"`/`"error"` |
| Escala de grises | `data-mode="grayscale"` | `.is-grayscale`. | `aria-pressed` |
| Sin texto | `data-mode="textless"` | `.is-textless`. | `aria-pressed` |
| Ocultar botones | `data-mode="no-buttons"` | `.is-no-buttons`. | `aria-pressed` |

- **Export PNG:** html2canvas `scale:2`, `useCORS`, `logging:false`; fondo
  transparente por defecto para todo componente del sistema —botones, paneles y
  cartas de estado— vía la lista `TRANSPARENT_NODES` (`.mc-button`, `.mc-panel`,
  `.mc-verify`). Solo se usa fondo sólido si el tile lo fuerza con
  `data-export-bg="solid"`; `data-export-bg="transparent"` refuerza el default.
  Nunca dejar un lienzo sólido detrás de un componente que debe ir sobre otra
  superficie. Durante captura añadir `.is-capturing` (anula sombras, filtros,
  backdrop y transiciones).
- **Gris:** vista con `filter: grayscale(1)`; PNG con luminancia real en canvas
  (`0.2126R + 0.7152G + 0.0722B`).
- **Sin texto:** `color: transparent` en todo **excepto SVG**
  (`:not(svg):not(svg *)`), conservando iconos y formas.
- **Ocultar botones:** `:is(button, .mc-button) { visibility: hidden; }`.
- **Nombre de archivo:** `[slug, ...sufijos].join("-")`, orden
  `grayscale` → `sin-texto` → `sin-botones`.

## Contrato `data-*` (no cambiar)

| Atributo | Dónde | Significado |
| --- | --- | --- |
| `data-export-name` | tile/item | Slug base del PNG. |
| `data-export-node` | hijo del tile | Nodo raíz a capturar (uno). |
| `data-export-bg` | tile | `transparent`\|`solid` (opcional). |
| `data-download` | tool | Dispara exportación. |
| `data-mode` | tool | `grayscale`\|`textless`\|`no-buttons`. |
| `data-token` | swatch | Token a resolver/copiar. |
| `data-copy` | swatch | Valor resuelto (lo escribe `kit.js`). |

## Vista Paleta

Un swatch por token (chip + nombre + valor). Click copia
(`navigator.clipboard`, fallback `textarea`+`execCommand`), feedback "copiado"
1.2 s. Resolver `var()` encadenados (hasta 8 saltos) y tokens `1px solid …`.
Soportar chips sólidos, gradientes y de borde.

## Estructura de página (sin JS)

Radio inputs + `:checked ~` para las 3 vistas: **Componentes** (bloques y tiles),
**Paleta** (swatches) y **Ejemplos** (casos reales numerados). `topbar` sticky,
`sheet` centrado `min(100%, 880px)`, raíz con `class="mc-scope"`.

## 20 direcciones de estilo

Elige **una**. No cambia estructura ni herramientas: cambia tokens, radios,
tipografía y superficies.

1. **Glassmorphism** — superficies de cristal. `backdrop-filter`, capas `rgba`,
   borde claro 20–40 %, brillo interior. Tokens: `glass`, sombras, border.
2. **Neumorphism (Soft UI)** — relieve suave del mismo material. Doble sombra,
   sin bordes, bajo contraste, fondo≈superficie. Radios grandes.
3. **Brutalismo / Neo-brutalismo** — crudo y directo. Bordes negros gruesos,
   sombras **duras**, sin gradientes, alto contraste, giros 1–3°.
4. **Minimalismo** — silencio visual. Aire, 1–2 colores, líneas finas, sin
   sombras. Radios sm.
5. **Flat** — color plano. Cero sombras/gradientes, iconos sólidos, varios
   acentos, sans geométrica.
6. **Material Design 3** — sistema y elevación. Sombras escaladas, ripples,
   superficies tonales, FAB. Radios 12–28 px.
7. **Skeuomorphism** — objetos físicos. Gradientes realistas, texturas,
   chaflanes, sombras profundas. Paleta material.
8. **Claymorphism** — plastilina 3D. Sombra interna+externa de color, radios
   20–32 px, pasteles, tipografía redondeada.
9. **Aurora / Mesh Gradient** — fondos fluidos. Gradientes radiales múltiples,
   `blur()`, glows grandes, violeta–cian–magenta.
10. **Cyberpunk / Neón** — noche urbana. Casi negro, neones, glow, rejillas,
    scanlines. Radios sm, mono tech.
11. **Retro Terminal (CRT)** — fósforo. Negro, mono verde/ámbar, cursor, viñeta,
    scanlines. Radios 0.
12. **Vaporwave** — nostalgia 80/90. Degradados rosa→cian, rejilla en
    perspectiva, tipografía itálica.
13. **Pixel Art / 8-bit** — sprites. `image-rendering: pixelated`, bordes duros,
    paleta 8–16, sombras offset, bitmap font.
14. **HUD / Blueprint técnico** — consola/plano. Líneas finas, brackets, rejilla,
    micro-labels, transparencias de datos. Ámbar para alertas.
15. **Editorial / Paper** — revista. Off-white, serif en títulos, reglas
    horizontales, márgenes amplios, sin sombras.
16. **Bento Grid** — cajas modulares. Grid asimétrico, tarjetas de tamaños
    distintos, radios lg, acentos por módulo.
17. **Dark OLED / High Contrast** — negro puro. `#000`, acento brillante, texto
    casi blanco, foco muy visible.
18. **Art Deco** — años 20. Simetría, líneas doradas, marcos, chaflanes,
    negro/verde + oro, display geométrica.
19. **Mid-Century Modern** — cálido 50/60. Formas orgánicas, ilustración plana,
    mostaza/terracota/oliva/crema, radios amables.
20. **Fluent / Corporate** — productivo y sobrio. Acrílico, elevación sutil,
    radios 4–8 px, foco marcado, azul corporativo + neutrales.

Matriz rápida: XR/datos → Glass/HUD/Cyberpunk · corporativo → Fluent/Material/
Minimalismo · lúdico → Claymorphism/Pixel/Material · contenido → Editorial/
Mid-Century/Minimalismo · evento → Brutalismo/Art Deco/Vaporwave · nocturno/
accesible → Dark OLED/Minimalismo/Fluent.

## Reglas de implementación

- Todo control es `<button>` real (o `label`+input) y enfocable; `aria-label` en
  cada tool, `aria-pressed` en modos, `role="status"` en estados.
- Grid `repeat(auto-fill, minmax(320px, 1fr))`; sin anchos fijos; móvil sin
  scroll horizontal.
- Estados con `:has()`, `:focus-visible` y atributos; JS mínimo con delegación
  de eventos; transiciones 160–280 ms; respetar `prefers-reduced-motion`.
- SVG inline para iconos. Degradar si `html2canvas` no carga (estado `error`).

## Checklist de aceptación

- [ ] `design-system.css` con todos los tokens y portable.
- [ ] 4 archivos + html2canvas CDN.
- [ ] 3 vistas funcionales sin JS.
- [ ] Cada tile con 4 herramientas y un único `data-export-node`.
- [ ] PNG con fondo transparente por defecto (botones, paneles y cartas) y
  nombre correctos; modos combinables en el nombre.
- [ ] Paleta resuelve `var()` encadenados y copia al portapapeles.
- [ ] Estilo elegido coherente en tokens, radios y tipografía.
- [ ] Sin colores literales fuera de `:root`; teclado + aria correctos.

## Anti-patrones (prohibido)

- Mezclar prefijo portable (`mc-`) con layout; usar `data-*` fuera del contrato.
- Colores literales en componentes; dependencias fuera de html2canvas.
- Capturar con lienzo sólido un componente que debe ser transparente (botón,
  panel o carta); el fondo del PNG debe salir limpio para montarlo sobre otra
  superficie.
- Componentes no exportables (sin `data-export-node`).
- Incoherencia de estilo entre las 3 vistas.

## Prompt de arranque

```
Construye una colección de UI para <PROYECTO>.
Estilo: <una de las 20 direcciones>.
Textos: <dominio del proyecto>. Idioma UI: <es/en>.
Sigue esta skill: tokens en :root, 3 vistas sin JS, panel, botón de ejemplo
(10 variantes), carta de estado, tiles con 4 herramientas (descargar PNG, gris,
sin texto, ocultar botones), paleta copiable y checklist de aceptación.
```
