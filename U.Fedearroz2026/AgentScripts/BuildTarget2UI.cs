using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildTarget2UI
{
    private const string ScenePath = "Assets/Level/Scenes/Holobox.unity";
    private const string Generated = "Assets/Art/2D/UI/Holobox/Generated";
    private const string Fonts = "Assets/ExternalAssets/TextMesh Pro/Resources/Fonts & Materials/";
    private const string ButtonSpriteName = "Boton Beneficio.png";
    private const bool ForceRebuild = false;

    // Medidas tomadas de MockUp2.0.png (1080 x 1920).
    private const float Pad = 20f;              // aire para el glow del sprite
    private const float ButtonW = 375f;
    private const float ButtonH = 96f;
    private const float ColLeft = 158f;
    private const float ColRight = 562f;
    private static readonly float[] RowTops = { 591f, 778f, 965f, 1152f };

    private static readonly Color FillColor = new Color32(43, 134, 33, 255);
    private static readonly Color BorderColor = new Color32(0, 61, 28, 255);
    private static readonly Color GlowColor = new Color32(0, 235, 100, 255);
    private static readonly Color LabelColor = Color.white;

    private static readonly string[,] Labels =
    {
        { "Alta temperatura", "Baja temperatura" },
        { "Alta radiación", "Baja radiación" },
        { "Alta presión de\nenfermedades", "Baja presión de\nenfermedades" },
        { "Sistema de siembra:\nRiego", "Sistema de siembra:\nSecano" }
    };

    private static readonly string[,] ObjectNames =
    {
        { "Alta temperatura", "Baja temperatura" },
        { "Alta radiacion", "Baja radiacion" },
        { "Alta presion enfermedades", "Baja presion enfermedades" },
        { "Sistema de siembra Riego", "Sistema de siembra Secano" }
    };

    public static string Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new System.Exception($"Active scene is '{scene.path}', expected '{ScenePath}'.");

        GameObject panel = FindInactive("[Panel] Target 2");
        if (panel == null)
            throw new System.Exception("[Panel] Target 2 not found.");

        Transform screen = panel.transform.Find("[Screen] Holobox Target 2");
        if (screen == null)
            throw new System.Exception("[Screen] Holobox Target 2 not found.");

        // El fondo del panel tapaba el encabezado (logo/ACTIVO) que sí se ve en el mockup.
        Image panelBackdrop = panel.GetComponent<Image>();
        if (panelBackdrop != null)
            panelBackdrop.enabled = false;

        // Placeholder "This is the target (2)"
        Transform placeholder = panel.transform.Find("[Text] Target 2");
        if (placeholder != null)
            Object.DestroyImmediate(placeholder.gameObject);

        // Selector Beneficio presente / futuro: no aparece en MockUp2.0.
        Transform selector = screen.Find("[Panel] Selector");
        if (selector != null)
            selector.gameObject.SetActive(false);

        Sprite optionSprite = EnsureOptionSprite();

        // Panel de contenido (radar) visible y con las medidas del mockup.
        Transform radarPanel = screen.Find("[Panel] Radar");
        if (radarPanel == null)
            throw new System.Exception("[Panel] Radar not found inside Target 2 screen.");
        radarPanel.gameObject.SetActive(true);

        Transform cuadroRadar = radarPanel.Find("[Image] Cuadro Radar");
        if (cuadroRadar != null)
            Place(cuadroRadar.GetComponent<RectTransform>(), 87f, 459f, 906f, 923f);

        Transform radarImage = radarPanel.Find("[Image] Radar");
        if (radarImage != null)
        {
            Place(radarImage.GetComponent<RectTransform>(), 125f, 525f, 830f, 830f);
            Image radarGraphic = radarImage.GetComponent<Image>();
            if (radarGraphic != null)
                radarGraphic.sprite = EnsureRadarSprite();
        }

        Transform rotationLines = radarImage != null ? radarImage.Find("[Image] RotationLinesRadar") : null;
        if (rotationLines != null)
            Place(rotationLines.GetComponent<RectTransform>(), 190f, 590f, 700f, 700f);

        // Panel de título con las medidas del mockup.
        Transform titleGroup = screen.Find("[Panel] Beneficio Presente");
        if (titleGroup != null)
        {
            Transform cuadroTitulo = titleGroup.Find("[Image] Cuadro Titulo");
            if (cuadroTitulo != null)
                Place(cuadroTitulo.GetComponent<RectTransform>(), 114f, 205f, 851f, 227f);

            Transform titulo = titleGroup.Find("[Text] Titulo");
            if (titulo != null)
                Place(titulo.GetComponent<RectTransform>(), 136f, 233f, 800f, 60f);

            Transform descripcion = titleGroup.Find("[Text] Descripcion");
            if (descripcion != null)
                Place(descripcion.GetComponent<RectTransform>(), 132f, 327f, 800f, 81f);
        }

        // Banner inferior.
        Transform dato = screen.Find("[Panel] Dato");
        if (dato != null)
        {
            Transform cuadroDato = dato.Find("[Image] Cuadro Dato");
            if (cuadroDato != null)
                Place(cuadroDato.GetComponent<RectTransform>(), 114f, 1565f, 851f, 222f);

            Transform tipos = dato.Find("[Text] Tipos de arroz");
            if (tipos != null)
                Place(tipos.GetComponent<RectTransform>(), 138f, 1633f, 800f, 88f);
        }

        // Grupo de opciones.
        Transform previous = radarPanel.Find("[Panel] Opciones");
        if (previous != null)
            Object.DestroyImmediate(previous.gameObject);

        GameObject options = NewGroup("[Panel] Opciones", radarPanel);
        Stretch(options.GetComponent<RectTransform>());

        TMP_FontAsset bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Fonts + "Roboto-Bold SDF.asset");
        if (bold == null)
            throw new System.Exception("Roboto-Bold SDF not found.");

        for (int row = 0; row < 4; row++)
        {
            CreateOption(options.transform, optionSprite, bold, ObjectNames[row, 0], Labels[row, 0], ColLeft, RowTops[row]);
            CreateOption(options.transform, optionSprite, bold, ObjectNames[row, 1], Labels[row, 1], ColRight, RowTops[row]);
        }

        EditorUtility.SetDirty(panel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "Target 2 UI built (8 opciones).";
    }

    private static GameObject CreateOption(Transform parent, Sprite sprite, TMP_FontAsset font, string name, string label, float x, float y)
    {
        GameObject go = new GameObject("[Button] " + name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        Place(rt, x - Pad, y - Pad, ButtonW + Pad * 2f, ButtonH + Pad * 2f);

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        image.raycastTarget = true;
        image.preserveAspect = false;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        GameObject textGo = new GameObject("[Text] Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.layer = 5;
        textGo.transform.SetParent(go.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.offsetMin = new Vector2(Pad + 8f, Pad + 5f);
        textRt.offsetMax = new Vector2(-(Pad + 8f), -(Pad + 3f));
        textRt.localScale = Vector3.one;

        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 32f;
        text.color = LabelColor;
        text.alignment = TextAlignmentOptions.Center;
        text.text = label;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.lineSpacing = 6f;
        text.rectTransform.localScale = Vector3.one;

        return go;
    }

    private static Sprite EnsureOptionSprite()
    {
        string assetPath = Generated + "/" + ButtonSpriteName;
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existing != null && !ForceRebuild)
            return existing;

        int ss = 4;
        int pad = Mathf.RoundToInt(Pad);
        int width = Mathf.RoundToInt(ButtonW) + pad * 2;
        int height = Mathf.RoundToInt(ButtonH) + pad * 2;
        int texW = width * ss;
        int texH = height * ss;

        float halfW = ButtonW * 0.5f;
        float halfH = ButtonH * 0.5f;
        float radius = 31f;
        float border = 3f;
        float glowFalloff = 8.5f;
        float glowStrength = 0.58f;
        float glowExponent = 2f;

        Texture2D texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[texW * texH];

        for (int py = 0; py < texH; py++)
        {
            float y = (py + 0.5f) / ss - pad - halfH;
            for (int px = 0; px < texW; px++)
            {
                float x = (px + 0.5f) / ss - pad - halfW;

                float qx = Mathf.Abs(x) - (halfW - radius);
                float qy = Mathf.Abs(y) - (halfH - radius);
                float ox = Mathf.Max(qx, 0f);
                float oy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;

                Color c;
                if (dist <= -border)
                {
                    c = FillColor;
                }
                else if (dist <= 0f)
                {
                    c = BorderColor;
                }
                else
                {
                    float a = glowStrength * Mathf.Exp(-Mathf.Pow(dist / glowFalloff, glowExponent));
                    c = new Color(GlowColor.r, GlowColor.g, GlowColor.b, a);
                }

                pixels[py * texW + px] = c;
            }
        }

        // Promediar el supersampling para bordes suaves.
        int outW = width;
        int outH = height;
        Texture2D outTex = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
        Color32[] outPixels = new Color32[outW * outH];
        float inv = 1f / (ss * ss);

        for (int y = 0; y < outH; y++)
        {
            for (int x = 0; x < outW; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                for (int sy = 0; sy < ss; sy++)
                {
                    int row = (y * ss + sy) * texW;
                    for (int sx = 0; sx < ss; sx++)
                    {
                        Color32 p = pixels[row + x * ss + sx];
                        float pa = p.a / 255f;
                        r += p.r * pa;
                        g += p.g * pa;
                        b += p.b * pa;
                        a += pa;
                    }
                }

                if (a <= 0.0001f)
                {
                    outPixels[y * outW + x] = new Color32(0, 0, 0, 0);
                }
                else
                {
                    outPixels[y * outW + x] = new Color32(
                        (byte)Mathf.Clamp(Mathf.RoundToInt(r / a), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(g / a), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(b / a), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(a * inv * 255f), 0, 255));
                }
            }
        }

        outTex.SetPixels32(outPixels);
        outTex.Apply();
        Object.DestroyImmediate(texture);

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        byte[] png = outTex.EncodeToPNG();
        Object.DestroyImmediate(outTex);

        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder("Assets/Art/2D/UI/Holobox", "Generated");

        string absolute = Path.Combine(projectRoot, assetPath);
        File.WriteAllBytes(absolute, png);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new System.Exception($"No texture importer at {assetPath}");

        int slice = Mathf.RoundToInt(Pad) + Mathf.RoundToInt(radius);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = new Vector4(slice, slice, slice, slice);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite EnsureRadarSprite()
    {
        const string sourceName = "Radar.png";
        const string outputName = "Radar Beneficio.png";
        string assetPath = Generated + "/" + outputName;

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string sourcePath = Path.Combine(projectRoot, "Assets", "Art", "2D", "UI", "Holobox", sourceName);
        byte[] sourceBytes = File.ReadAllBytes(sourcePath);

        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        source.LoadImage(sourceBytes);

        int width = source.width;
        int height = source.height;
        Color32[] pixels = source.GetPixels32();
        float cx = (width - 1) * 0.5f;
        float cy = (height - 1) * 0.5f;
        float diskRadius = 339f;

        // El disco del radar del mockup es un oliva oscuro; el PNG original es gris claro.
        const float tintR = 0.70f;
        const float tintG = 0.68f;
        const float tintB = 0.55f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > diskRadius)
                    continue;

                int index = y * width + x;
                Color32 p = pixels[index];

                // Degradado radial suave: el disco se apaga hacia el borde.
                float t = Mathf.Clamp01(distance / diskRadius);
                float shade = Mathf.Lerp(1.08f, 0.72f, Mathf.Pow(t, 1.5f));

                pixels[index] = new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(p.r * tintR * shade), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(p.g * tintG * shade), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(p.b * tintB * shade), 0, 255),
                    p.a);
            }
        }

        source.SetPixels32(pixels);
        source.Apply();
        byte[] png = source.EncodeToPNG();
        Object.DestroyImmediate(source);

        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder("Assets/Art/2D/UI/Holobox", "Generated");

        string absolute = Path.Combine(projectRoot, assetPath);
        File.WriteAllBytes(absolute, png);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static GameObject NewGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().localScale = Vector3.one;
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x + width * 0.5f - 540f, 960f - y - height * 0.5f);
        rt.localScale = Vector3.one;
        rt.localEulerAngles = Vector3.zero;
    }

    private static GameObject FindInactive(string name)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == name && t.parent != null && t.parent.name == "Canvas (Holobox)")
                return t.gameObject;
        return null;
    }
}
