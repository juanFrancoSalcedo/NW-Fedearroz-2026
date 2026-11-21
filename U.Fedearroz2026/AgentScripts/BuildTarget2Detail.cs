using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildTarget2Detail
{
    private const string ScenePath = "Assets/Level/Scenes/Holobox.unity";
    private const string Art = "Assets/Art/2D/UI/Holobox/";
    private const string Generated = "Assets/Art/2D/UI/Holobox/Generated";
    private const string Fonts = "Assets/ExternalAssets/TextMesh Pro/Resources/Fonts & Materials/";
    private const string DetailName = "[Screen] Detalle Alta Temperatura";

    private static readonly Color SubtitleColor = new Color32(192, 212, 190, 255);
    private static readonly Color BannerColor = new Color32(186, 226, 212, 255);

    public static string Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new System.Exception($"Active scene is '{scene.path}', expected '{ScenePath}'.");

        GameObject panel = FindPanel("[Panel] Target 2");
        if (panel == null)
            throw new System.Exception("[Panel] Target 2 not found.");

        Transform menu = panel.transform.Find("[Screen] Holobox Target 2");
        if (menu == null)
            throw new System.Exception("[Screen] Holobox Target 2 not found.");

        Transform previous = panel.transform.Find(DetailName);
        if (previous != null)
            Object.DestroyImmediate(previous.gameObject);

        Sprite cuadro = EnsureSprite(Generated + "/Cuadro fondo Opaque.png", 24f);
        Sprite radar = EnsureSprite(Generated + "/Radar Detalle.png", 0f);
        Sprite lines = EnsureSprite(Art + "RotationLinesRadar.png", 0f);
        Sprite halo = EnsureSprite(Generated + "/Halo Beneficio.png", 0f);
        Sprite thermo = EnsureSprite(Generated + "/Termometro.png", 0f);
        Sprite plantGreen = EnsureSprite(Art + "Arroz holográfico con base luminosa 1.png", 0f);
        Sprite plantGold = EnsureSprite(Art + "Planta holográfica de arroz dorado 1.png", 0f);

        TMP_FontAsset bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Fonts + "Roboto-Bold SDF.asset");
        TMP_FontAsset regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Fonts + "Roboto-Regular-Outline SDF.asset");
        if (bold == null || regular == null)
            throw new System.Exception("No se encontraron las fuentes de Roboto.");

        GameObject detail = NewGroup(DetailName, panel.transform);
        Stretch(detail.GetComponent<RectTransform>());

        // BotÃ³n invisible de fondo: tocar en cualquier parte vuelve al menÃº.
        GameObject back = new GameObject("[Button] Volver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        back.layer = 5;
        back.transform.SetParent(detail.transform, false);
        Stretch(back.GetComponent<RectTransform>());
        Image backImage = back.GetComponent<Image>();
        backImage.color = new Color(0f, 0f, 0f, 0f);
        backImage.raycastTarget = true;

        // ---- TÃ­tulo ----
        GameObject title = NewGroup("[Panel] Titulo", detail.transform);
        Stretch(title.GetComponent<RectTransform>());

        NewImage("[Image] Cuadro Titulo", title.transform, cuadro, Image.Type.Sliced, 114f, 205f, 851f, 227f);
        NewText("[Text] Titulo", title.transform, bold, "ALTA TEMPERATURA:", 48f, Color.white,
            TextAlignmentOptions.Center, 136f, 233f, 800f, 60f);
        NewText("[Text] Descripcion", title.transform, regular,
            "El calor pone a prueba la floraciÃ³n\nResisten el calor y llenan el grano", 28f, SubtitleColor,
            TextAlignmentOptions.Center, 132f, 327f, 800f, 81f);

        // ---- Contenido ----
        GameObject content = NewGroup("[Panel] Contenido", detail.transform);
        Stretch(content.GetComponent<RectTransform>());

        NewImage("[Image] Cuadro", content.transform, cuadro, Image.Type.Sliced, 87f, 459f, 906f, 923f);

        // Halo blanco debajo de las lÃ­neas del radar.
        NewImage("[Image] Halo", content.transform, halo, Image.Type.Simple, 120f, 520f, 840f, 840f);

        Image radarImage = NewImage("[Image] Radar", content.transform, radar, Image.Type.Simple, 125f, 525f, 830f, 830f);

        Color crossColor = new Color32(96, 124, 80, 235);
        Image crossH = NewImage("[Image] Linea Horizontal", radarImage.transform, null, Image.Type.Simple, 660f, 2f, centered: true);
        crossH.color = crossColor;
        Image crossV = NewImage("[Image] Linea Vertical", radarImage.transform, null, Image.Type.Simple, 2f, 660f, centered: true);
        crossV.color = crossColor;

        NewImage("[Image] RotationLinesRadar", radarImage.transform, lines, Image.Type.Simple, 700f, 700f, centered: true);

        NewImage("[Image] Planta sana", content.transform, plantGreen, Image.Type.Simple, 73f, 828f, 420f, 420f);
        NewImage("[Image] Planta estres", content.transform, plantGold, Image.Type.Simple, 552f, 826f, 420f, 420f);
        NewImage("[Image] Termometro", content.transform, thermo, Image.Type.Simple, 896f, 494f, 72f, 228f);

        // ---- Variedades ----
        GameObject varieties = NewGroup("[Panel] Variedades", detail.transform);
        Stretch(varieties.GetComponent<RectTransform>());

        NewImage("[Image] Cuadro Dato", varieties.transform, cuadro, Image.Type.Sliced, 114f, 1565f, 851f, 222f);
        NewText("[Text] Titulo", varieties.transform, bold, "Variedades recomendadas:", 40f, BannerColor,
            TextAlignmentOptions.Center, 138f, 1592f, 800f, 52f);
        NewText("[Text] Variedades", varieties.transform, bold, "Fedearroz 70 Â· Fedearroz 75", 48f, BannerColor,
            TextAlignmentOptions.Center, 138f, 1690f, 800f, 62f);

        // ---- Marca W inferior (copiada del menÃº) ----
        foreach (Transform child in menu)
        {
            if (!child.name.StartsWith("[Image] W")) continue;
            GameObject copy = Object.Instantiate(child.gameObject, detail.transform, false);
            copy.name = child.name;
        }

        // ---- Controlador y cableado ----
        Target2DetailController controller = panel.GetComponent<Target2DetailController>();
        if (controller == null)
            controller = panel.AddComponent<Target2DetailController>();

        SerializedObject cso = new SerializedObject(controller);
        cso.FindProperty("menuScreen").objectReferenceValue = menu.gameObject;
        cso.FindProperty("detailAltaTemperatura").objectReferenceValue = detail;
        cso.ApplyModifiedPropertiesWithoutUndo();

        GameObject openButton = FindChild(menu, "[Button] Alta temperatura");
        if (openButton == null)
            throw new System.Exception("No se encontrÃ³ el botÃ³n '[Button] Alta temperatura'.");
        WireClick(openButton.GetComponent<Button>(), controller, "AbrirDetalle");

        WireClick(back.GetComponent<Button>(), controller, "CerrarDetalle");

        // El detalle arranca oculto; el menÃº visible.
        detail.SetActive(false);
        menu.gameObject.SetActive(true);

        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "Detalle Alta Temperatura construido.";
    }

    private static void WireClick(Button button, Object target, string method)
    {
        if (button == null)
            throw new System.Exception("BotÃ³n no encontrado para cablear.");

        SerializedObject so = new SerializedObject(button);
        SerializedProperty calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.arraySize = 1;
        SerializedProperty call = calls.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = method;
        call.FindPropertyRelative("m_Mode").intValue = 1;
        call.FindPropertyRelative("m_CallState").intValue = 2;
        call.FindPropertyRelative("m_Arguments").FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue = "";
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                return t.gameObject;
        return null;
    }

    private static GameObject NewGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().localScale = Vector3.one;
        return go;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Image.Type type,
        float x, float y, float w, float h)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.rectTransform.localScale = Vector3.one;
        Place(image.rectTransform, x, y, w, h);
        return image;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Image.Type type,
        float w, float h, bool centered)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.rectTransform.localScale = Vector3.one;

        RectTransform rt = image.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
        return image;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font, string value,
        float size, Color color, TextAlignmentOptions alignment, float x, float y, float w, float h)
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
        Place(text.rectTransform, x, y, w, h);
        return text;
    }

    private static Sprite EnsureSprite(string path, float border)
    {
        if (AssetImporter.GetAtPath(path) == null || path.Contains("/Generated/"))
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool dirty = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f))
            {
                importer.spritePixelsPerUnit = 100f;
                dirty = true;
            }

            if (border > 0f && !Mathf.Approximately(importer.spriteBorder.x, border))
            {
                importer.spriteBorder = new Vector4(border, border, border, border);
                dirty = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            throw new System.Exception($"No se pudo cargar el sprite: {path}");
        return sprite;
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

    private static GameObject FindPanel(string name)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == name && t.parent != null && t.parent.name == "Canvas (Holobox)")
                return t.gameObject;
        return null;
    }
}

