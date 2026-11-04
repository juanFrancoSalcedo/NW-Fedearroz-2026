using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BuildHeroIKUI
{
    public static void All()
    {
        EnsureTmpEssentials();
        TMP_FontAsset font = GetDefaultFont();

        GameObject hero = FindInScene("RPG_Hero_IK_Variant");
        if (hero == null)
            throw new Exception("No se encontró 'RPG_Hero_IK_Variant' en la escena activa.");
        Transform target = hero.transform;

        // Canvas
        GameObject canvasGO = new GameObject("Canvas (HeroIKVariant)",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = 5;
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem (new input system)
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            es.layer = 5;
        }

        // Panel
        GameObject panelGO = new GameObject("[Panel] HeroIKVariant",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        SetParent(panelGO.transform, canvasGO.transform);
        Image panelImg = panelGO.GetComponent<Image>();
        panelImg.color = new Color(0.09f, 0.09f, 0.13f, 0.96f);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        panelRect.sizeDelta = new Vector2(440f, 0f);

        VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter csf = panelGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Title
        CreateText(panelGO.transform, "[Text] Title", "Hero IK Variant", font, 24f, FontStyles.Bold);

        // Slider rows
        Slider positionX = CreateSliderRow(panelGO.transform, "Position X", -10f, 10f, target.localPosition.x, font, out TMP_Text positionXText);
        Slider positionY = CreateSliderRow(panelGO.transform, "Position Y", -10f, 10f, target.localPosition.y, font, out TMP_Text positionYText);
        Slider positionZ = CreateSliderRow(panelGO.transform, "Position Z", -10f, 10f, target.localPosition.z, font, out TMP_Text positionZText);
        Slider scale = CreateSliderRow(panelGO.transform, "Scale", 0.1f, 2f, target.localScale.x, font, out TMP_Text scaleText);
        Slider rotationY = CreateSliderRow(panelGO.transform, "Rotation Y", -180f, 180f, Mathf.DeltaAngle(0f, target.localEulerAngles.y), font, out TMP_Text rotationYText);

        // Handler
        GameObject handlerGO = new GameObject("[Handler] HeroIKVariant", typeof(HeroIKVariantHandler));
        SetParent(handlerGO.transform, canvasGO.transform);
        var handler = handlerGO.GetComponent<HeroIKVariantHandler>();

        var so = new SerializedObject(handler);
        so.FindProperty("target").objectReferenceValue = target;
        so.FindProperty("positionX").objectReferenceValue = positionX;
        so.FindProperty("positionY").objectReferenceValue = positionY;
        so.FindProperty("positionZ").objectReferenceValue = positionZ;
        so.FindProperty("scale").objectReferenceValue = scale;
        so.FindProperty("rotationY").objectReferenceValue = rotationY;
        so.FindProperty("positionXText").objectReferenceValue = positionXText;
        so.FindProperty("positionYText").objectReferenceValue = positionYText;
        so.FindProperty("positionZText").objectReferenceValue = positionZText;
        so.FindProperty("scaleText").objectReferenceValue = scaleText;
        so.FindProperty("rotationYText").objectReferenceValue = rotationYText;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureTmpEssentials()
    {
        if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
            return;
        TMP_PackageResourceImporter.ImportResources(true, false, false);
        AssetDatabase.Refresh();
    }

    private static TMP_FontAsset GetDefaultFont()
    {
        const string knownPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(knownPath);
        if (font != null)
            return font;

        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (string guid in guids)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (font != null)
                return font;
        }
        return null;
    }

    private static GameObject FindInScene(string name)
    {
        GameObject found = GameObject.Find(name);
        if (found != null)
            return found;

        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null;
    }

    private static Slider CreateSliderRow(Transform panel, string label, float min, float max, float value, TMP_FontAsset font, out TMP_Text valueText)
    {
        GameObject row = new GameObject("[Row] " + label, typeof(RectTransform));
        SetParent(row.transform, panel);
        LayoutElement rowLE = row.AddComponent<LayoutElement>();
        rowLE.minHeight = 30f;
        rowLE.preferredHeight = 30f;

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        TMP_Text labelText = CreateText(row.transform, "[Text] " + label, label, font, 16f, FontStyles.Normal);
        LayoutElement labelLE = labelText.gameObject.AddComponent<LayoutElement>();
        labelLE.minWidth = 100f;
        labelLE.preferredWidth = 100f;

        Slider slider = CreateSlider(row.transform, "[Slider] " + label, min, max, value);
        LayoutElement sliderLE = slider.gameObject.AddComponent<LayoutElement>();
        sliderLE.flexibleWidth = 1f;
        sliderLE.minWidth = 180f;

        valueText = CreateText(row.transform, "[Text] " + label + " Value", value.ToString("F2"), font, 16f, FontStyles.Normal);
        valueText.alignment = TextAlignmentOptions.Right;
        LayoutElement valueLE = valueText.gameObject.AddComponent<LayoutElement>();
        valueLE.minWidth = 60f;
        valueLE.preferredWidth = 60f;

        return slider;
    }

    private static Slider CreateSlider(Transform parent, string name, float min, float max, float value)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        SetParent(go.transform, parent);
        RectTransform sliderRect = go.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(0f, 20f);

        Slider slider = go.AddComponent<Slider>();

        RectTransform bgRect = CreateUIObject("Background", go.transform);
        SetRect(bgRect, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero);
        Image bgImg = bgRect.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.25f, 0.25f, 0.32f, 1f);

        RectTransform fillAreaRect = CreateUIObject("Fill Area", go.transform);
        SetRect(fillAreaRect, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero);

        RectTransform fillRect = CreateUIObject("Fill", fillAreaRect);
        SetRect(fillRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image fillImg = fillRect.gameObject.AddComponent<Image>();
        fillImg.color = new Color(0.25f, 0.65f, 1f, 1f);

        RectTransform handleAreaRect = CreateUIObject("Handle Slide Area", go.transform);
        SetRect(handleAreaRect, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));

        RectTransform handleRect = CreateUIObject("Handle", handleAreaRect);
        SetRect(handleRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image handleImg = handleRect.gameObject.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        return slider;
    }

    private static RectTransform CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        SetParent(go.transform, parent);
        return go.GetComponent<RectTransform>();
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, TMP_FontAsset font, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        SetParent(go.transform, parent);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.text = text;
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }

    private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static void SetParent(Transform child, Transform parent)
    {
        child.SetParent(parent, false);
        child.gameObject.layer = 5;
        child.localScale = Vector3.one;
        child.localRotation = Quaternion.identity;
    }
}
