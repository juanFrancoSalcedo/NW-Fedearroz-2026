using System.Collections.Generic;
using System.IO;
using B_Extensions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildHoloboxUI
{
    private const string ScenePath = "Assets/Level/Scenes/Holobox.unity";
    private const string Art = "Assets/Art/2D/UI/Holobox/";
    private const string Generated = "Assets/Art/2D/UI/Holobox/Generated";
    private const string Fonts = "Assets/Art/Fonts/Roboto Static/";
    private const string TmpFonts = "Assets/Art/Fonts/TMP";
    private const string ScreenName = "[Screen] Holobox";

    private static readonly Color BackdropColor = new Color32(142, 142, 142, 255);
    private static readonly Color SubtitleColor = new Color32(192, 212, 190, 255);
    private static readonly Color ButtonTextColor = new Color32(36, 82, 47, 255);
    private static readonly Color BottomTextColor = new Color32(186, 226, 212, 255);
    private static readonly Color AccentColor = new Color32(112, 255, 85, 255);
    private static readonly Color DotColor = new Color32(0, 255, 115, 255);
    private static readonly Color PanelFill = new Color32(75, 83, 79, 255);
    private static readonly Color ButtonFill = new Color32(114, 165, 137, 255);
    private static readonly Color RadarFill = new Color32(137, 164, 150, 255);

    public static string Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new System.Exception($"Active scene is '{scene.path}', expected '{ScenePath}'.");

        Canvas canvas = FindCanvas(scene);
        if (canvas == null)
            throw new System.Exception("Canvas not found in Holobox scene.");

        canvas.gameObject.name = "Canvas (Holobox)";
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        GameObject previous = FindDeep(scene, ScreenName);
        if (previous != null)
            Object.DestroyImmediate(previous);

        Sprite panel = GenerateOpaqueSprite("Cuadro fondo.png", "Cuadro fondo Opaque.png", PanelFill, 24f, 220, false);
        Sprite frame = LoadSprite("Marco exterior · doble contorno.png");
        Sprite titleContainer = GeneratePlateSprite();
        Sprite logo = LoadSprite("Fedearroz_Title_Logo_1.png");
        Sprite telemetry = LoadSprite("Telemetría.png");
        Sprite radar = GenerateOpaqueSprite("Radar.png", "Radar Opaque.png", RadarFill, 0f, 245, true);
        Sprite buttonSelected = GenerateOpaqueSprite("Button1.png", "Button1 Opaque.png", ButtonFill, 0f, 220, false);
        Sprite buttonIdle = GenerateOpaqueSprite("Button2.png", "Button2 Opaque.png", ButtonFill, 0f, 220, false);
        Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ExternalAssets/B_Extension/Base/Default Images/circleQuad.png");

        TMP_FontAsset bold = EnsureFont(Fonts + "Roboto-Bold.ttf", "Roboto-Bold SDF");
        TMP_FontAsset regular = EnsureFont(Fonts + "Roboto-Regular.ttf", "Roboto-Regular SDF");
        TMP_FontAsset medium = EnsureFont(Fonts + "Roboto-Medium.ttf", "Roboto-Medium SDF");

        GameObject screen = NewGroup(ScreenName, canvas.transform, true);

        Image backdrop = NewImage("[Image] Fondo", screen.transform, null, BackdropColor);
        Stretch(backdrop.rectTransform);

        Image marco = NewImage("[Image] Marco exterior", screen.transform, frame, Color.white);
        Place(marco.rectTransform, 31f, 11f, 1016f, 1887f);

        GameObject header = NewGroup("[Panel] Encabezado", screen.transform, true);

        Image container = NewImage("[Image] Title Container", header.transform, titleContainer, Color.white);
        Place(container.rectTransform, 344f, -12f, 392f, 100f);

        Image logoImage = NewImage("[Image] Logo Fedearroz", header.transform, logo, Color.white);
        Place(logoImage.rectTransform, 422.5f, 13f, 235f, 72f);

        Image telemetryImage = NewImage("[Image] Telemetria", header.transform, telemetry, Color.white);
        Place(telemetryImage.rectTransform, 142f, 114f, 156f, 62f);

        Image dot = NewImage("[Image] Estado", header.transform, circle, DotColor);
        Place(dot.rectTransform, 830f, 168f, 16f, 16f);

        TextMeshProUGUI activo = NewText("[Text] Estado", header.transform, medium, "ACTIVO", 19f, AccentColor, TextAlignmentOptions.Left);
        Place(activo.rectTransform, 870f, 162f, 120f, 30f);

        GameObject content = NewGroup("[Panel] Beneficio Presente", screen.transform, true);

        Image titlePanel = NewImage("[Image] Cuadro Titulo", content.transform, panel, Color.white, Image.Type.Sliced);
        Place(titlePanel.rectTransform, 119f, 204f, 844f, 228f);

        TextMeshProUGUI title = NewText("[Text] Titulo", content.transform, bold, "BENEFICIO PRESENTE", 48f, Color.white, TextAlignmentOptions.Center);
        Place(title.rectTransform, 131f, 233f, 800f, 60f);

        TextMeshProUGUI subtitle = NewText("[Text] Descripcion", content.transform, regular,
            "Lo que hoy aporta la diversidad\nVariedades Fedearroz que hoy llegan al agricultor y a su mesa",
            28f, SubtitleColor, TextAlignmentOptions.Center);
        Place(subtitle.rectTransform, 132f, 330f, 800f, 81f);

        Image radarPanel = NewImage("[Image] Cuadro Radar", content.transform, panel, Color.white, Image.Type.Sliced);
        Place(radarPanel.rectTransform, 93f, 470f, 895f, 935f);

        Image radarImage = NewImage("[Image] Radar", content.transform, radar, Color.white);
        Place(radarImage.rectTransform, 130f, 534f, 829f, 829f);

        GameObject selector = NewGroup("[Panel] Selector", screen.transform, true);

        Image leftGlow = NewImage("[Image] Glow Beneficio presente", selector.transform, buttonSelected, Color.white);
        Place(leftGlow.rectTransform, 161f, 1439f, 353f, 102f);

        Image selection = NewImage("[Image] Seleccion", selector.transform, LoadSprite("Triangle Selection.png"), Color.white);
        Place(selection.rectTransform, 308f, 1513f, 59f, 62f);

        GameObject leftButton = CreateButton("[Button] Beneficio presente", selector.transform, buttonSelected, medium, "Beneficio presente", 32f, ButtonTextColor);
        Place(leftButton.GetComponent<RectTransform>(), 164f, 1442f, 347f, 96f);

        GameObject rightButton = CreateButton("[Button] Beneficio futuro", selector.transform, buttonIdle, medium, "Beneficio futuro", 32f, ButtonTextColor);
        Place(rightButton.GetComponent<RectTransform>(), 570f, 1442f, 347f, 96f);

        GameObject data = NewGroup("[Panel] Dato", screen.transform, true);

        Image dataPanel = NewImage("[Image] Cuadro Dato", data.transform, panel, Color.white, Image.Type.Sliced);
        Place(dataPanel.rectTransform, 119f, 1596f, 844f, 228f);

        TextMeshProUGUI dataText = NewText("[Text] Tipos de arroz", data.transform, bold,
            "Más de 7.000 tipos de arroz conservados", 40f, BottomTextColor, TextAlignmentOptions.Center);
        Place(dataText.rectTransform, 145f, 1668f, 800f, 88f);

        Sprite triangle = LoadSprite("Triangle Selection.png");
        Image wFillLeft = NewImage("[Image] W Fondo A", screen.transform, triangle, new Color(1f, 1f, 1f, 0.42f));
        Place(wFillLeft.rectTransform, 516f, 1849f, 27f, 50f);
        wFillLeft.rectTransform.localEulerAngles = new Vector3(0f, 0f, 180f);

        Image wFillRight = NewImage("[Image] W Fondo B", screen.transform, triangle, new Color(1f, 1f, 1f, 0.42f));
        Place(wFillRight.rectTransform, 539f, 1849f, 29f, 50f);
        wFillRight.rectTransform.localEulerAngles = new Vector3(0f, 0f, 180f);

        Color wColor = new Color(0.4392f, 1f, 0.3333f, 0.9f);
        CreateLine("[Image] W Izquierda", screen.transform, new Vector2(452f, 1866f), new Vector2(506f, 1866f), 2.5f, wColor);
        CreateLine("[Image] W Diagonal Izquierda", screen.transform, new Vector2(506f, 1866f), new Vector2(523f, 1897f), 2.5f, wColor);
        CreateLine("[Image] W 1", screen.transform, new Vector2(518f, 1849f), new Vector2(531f, 1897f), 3f, wColor);
        CreateLine("[Image] W 2", screen.transform, new Vector2(531f, 1897f), new Vector2(541f, 1849f), 3f, wColor);
        CreateLine("[Image] W 3", screen.transform, new Vector2(541f, 1849f), new Vector2(553f, 1897f), 3f, wColor);
        CreateLine("[Image] W 4", screen.transform, new Vector2(553f, 1897f), new Vector2(566f, 1849f), 3f, wColor);
        CreateLine("[Image] W Diagonal Derecha", screen.transform, new Vector2(566f, 1849f), new Vector2(584f, 1866f), 3f, wColor);
        CreateLine("[Image] W Derecha", screen.transform, new Vector2(584f, 1866f), new Vector2(636f, 1866f), 3f, wColor);

        screen.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "Holobox UI built.";
    }

    private static Canvas FindCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                return canvas;
        }
        return null;
    }

    private static GameObject FindDeep(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null;
    }

    private static GameObject NewGroup(string name, Transform parent, bool stretch)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        if (stretch)
            Stretch(rt);
        return go;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color, Image.Type type = Image.Type.Simple)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = type;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.rectTransform.localScale = Vector3.one;
        return image;
    }

    private static GameObject CreateButton(string name, Transform parent, Sprite sprite, TMP_FontAsset font, string label, float size, Color color)
    {
        Image image = NewImage(name, parent, sprite, Color.white);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        image.gameObject.AddComponent<BaseButtonAttendant>();

        TextMeshProUGUI text = NewText("[Text] Label", image.transform, font, label, size, color, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(0f, 4f);
        text.rectTransform.offsetMax = new Vector2(0f, 4f);
        return image.gameObject;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.text = value;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.localScale = Vector3.one;
        return text;
    }

    private static Image CreateLine(string name, Transform parent, Vector2 from, Vector2 to, float thickness, Color color)
    {
        Image image = NewImage(name, parent, null, color);
        RectTransform rt = image.rectTransform;
        float dx = to.x - from.x;
        float dy = to.y - from.y;
        float length = Mathf.Sqrt(dx * dx + dy * dy);
        Vector2 mid = (from + to) * 0.5f;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(length, thickness);
        rt.anchoredPosition = new Vector2(mid.x - 540f, 960f - mid.y);
        rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg);
        return image;
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

    private static Sprite LoadSprite(string fileName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + fileName);
        if (sprite == null)
            throw new System.Exception($"Sprite not found: {Art + fileName}");
        return sprite;
    }

    private static Sprite GenerateOpaqueSprite(string sourceName, string outputName, Color32 fillColor, float border, int fillMax, bool quadrants)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string sourcePath = Path.Combine(projectRoot, "Assets", "Art", "2D", "UI", "Holobox", sourceName);
        byte[] sourceBytes = File.ReadAllBytes(sourcePath);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(sourceBytes);

        int width = texture.width;
        int height = texture.height;
        Color32[] pixels = texture.GetPixels32();
        bool[] visited = new bool[width * height];
        Queue<int> queue = new Queue<int>();

        if (quadrants)
        {
            int cx = width / 2;
            int cy = height / 2;
            int radius = Mathf.Min(width, height) / 4;
            EnqueueSeed(pixels, visited, queue, width, height, cx - radius, cy - radius, fillMax);
            EnqueueSeed(pixels, visited, queue, width, height, cx + radius, cy - radius, fillMax);
            EnqueueSeed(pixels, visited, queue, width, height, cx - radius, cy + radius, fillMax);
            EnqueueSeed(pixels, visited, queue, width, height, cx + radius, cy + radius, fillMax);
        }
        else
        {
            EnqueueSeed(pixels, visited, queue, width, height, width / 2, height / 2, fillMax);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            byte alpha = pixels[index].a;
            if (alpha <= 8 || alpha >= fillMax)
                continue;

            pixels[index] = fillColor;
            int x = index % width;
            int y = index / width;

            if (x > 0) Visit(index - 1, visited, queue);
            if (x < width - 1) Visit(index + 1, visited, queue);
            if (y > 0) Visit(index - width, visited, queue);
            if (y < height - 1) Visit(index + width, visited, queue);
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return SaveGenerated(texture, outputName, border);
    }

    private static Sprite GeneratePlateSprite()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string sourcePath = Path.Combine(projectRoot, "Assets", "Art", "2D", "UI", "Holobox", "Title Container.png");
        byte[] sourceBytes = File.ReadAllBytes(sourcePath);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(sourceBytes);

        Color32[] pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            byte alpha = pixels[i].a;
            if (alpha < 140)
                continue;
            int inverse = 255 - alpha;
            pixels[i] = new Color32(
                (byte)((pixels[i].r * alpha + 30 * inverse) / 255),
                (byte)((pixels[i].g * alpha + 30 * inverse) / 255),
                (byte)((pixels[i].b * alpha + 30 * inverse) / 255),
                255);
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return SaveGenerated(texture, "Title Container Opaque.png", 0f);
    }

    private static Sprite SaveGenerated(Texture2D texture, string outputName, float border)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        byte[] outputBytes = texture.EncodeToPNG();
        Object.DestroyImmediate(texture);

        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder("Assets/Art/2D/UI/Holobox", "Generated");

        string outputPath = Generated + "/" + outputName;
        string absoluteOutput = Path.Combine(projectRoot, outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutput));
        File.WriteAllBytes(absoluteOutput, outputBytes);
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        ConfigureSprite(outputPath, border);
        return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
    }

    private static void EnqueueSeed(Color32[] pixels, bool[] visited, Queue<int> queue, int width, int height, int seedX, int seedY, int fillMax)
    {
        seedX = Mathf.Clamp(seedX, 0, width - 1);
        seedY = Mathf.Clamp(seedY, 0, height - 1);
        int maxRadius = Mathf.Max(width, height);

        for (int radius = 0; radius < maxRadius; radius++)
        {
            int xMin = Mathf.Max(0, seedX - radius);
            int xMax = Mathf.Min(width - 1, seedX + radius);
            int yMin = Mathf.Max(0, seedY - radius);
            int yMax = Mathf.Min(height - 1, seedY + radius);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    int index = y * width + x;
                    if (visited[index])
                        continue;
                    byte alpha = pixels[index].a;
                    if (alpha > 100 && alpha < fillMax)
                    {
                        visited[index] = true;
                        queue.Enqueue(index);
                        return;
                    }
                }
            }
        }
    }

    private static void Visit(int index, bool[] visited, Queue<int> queue)
    {
        if (visited[index])
            return;
        visited[index] = true;
        queue.Enqueue(index);
    }

    private static void ConfigureSprite(string path, float border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new System.Exception($"No texture importer at {path}");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = new Vector4(border, border, border, border);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static TMP_FontAsset EnsureFont(string ttfPath, string assetName)
    {
        string assetPath = TmpFonts + "/" + assetName + ".asset";
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null)
            return existing;

        if (!AssetDatabase.IsValidFolder(TmpFonts))
            AssetDatabase.CreateFolder("Assets/Art/Fonts", "TMP");

        Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font == null)
            throw new System.Exception($"Font not found: {ttfPath}");

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);
        if (fontAsset == null)
            throw new System.Exception($"Could not create TMP font asset from {ttfPath}.");

        fontAsset.name = assetName;
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.isMultiAtlasTexturesEnabled = true;
        fontAsset.atlasTextures[0].name = assetName + " Atlas";
        fontAsset.material.name = assetName + " Material";
        fontAsset.atlasTextures[0].hideFlags = HideFlags.None;
        fontAsset.material.hideFlags = HideFlags.None;

        AssetDatabase.CreateAsset(fontAsset, assetPath);
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        fontAsset.TryAddCharacters(AllCharacters(), out string missing);
        if (!string.IsNullOrEmpty(missing))
            Debug.LogWarning($"TMP font '{assetName}' missing characters: {missing}");

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
    }

    private static string AllCharacters()
    {
        return "BENEFICIO PRESENTE" +
               "Lo que hoy aporta la diversidad" +
               "Variedades Fedearroz que hoy llegan al agricultor y a su mesa" +
               "Beneficio presente" +
               "Beneficio futuro" +
               "ACTIVO" +
               "Más de 7.000 tipos de arroz conservados";
    }
}
