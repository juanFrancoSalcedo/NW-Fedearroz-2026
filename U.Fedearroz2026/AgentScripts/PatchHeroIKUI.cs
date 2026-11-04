using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PatchHeroIKUI
{
    public static void Patch()
    {
        GameObject handlerGO = FindInScene("[Handler] HeroIKVariant");
        if (handlerGO == null)
            throw new Exception("No se encontró '[Handler] HeroIKVariant'.");

        var handler = handlerGO.GetComponent<HeroIKVariantHandler>();
        if (handler == null)
            throw new Exception("No se encontró el componente HeroIKVariantHandler.");

        GameObject panelGO = FindInScene("[Panel] HeroIKVariant");
        if (panelGO == null)
            throw new Exception("No se encontró '[Panel] HeroIKVariant'.");

        // Repopulate targets with every IK variant at scene root.
        var variants = new List<GameObject>();
        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name.IndexOf("IK_Variant", StringComparison.OrdinalIgnoreCase) >= 0)
                variants.Add(root);
        }

        var so = new SerializedObject(handler);
        var targetsProp = so.FindProperty("targets");
        targetsProp.ClearArray();
        targetsProp.arraySize = variants.Count;
        for (int i = 0; i < variants.Count; i++)
            targetsProp.GetArrayElementAtIndex(i).objectReferenceValue = variants[i];

        so.FindProperty("currentIndex").intValue = 0;

        // Create navigation row only if it does not exist yet.
        if (FindInScene("[Row] Navigation") == null)
        {
            TMP_FontAsset font = GetDefaultFont();

            GameObject navRow = new GameObject("[Row] Navigation", typeof(RectTransform));
            navRow.transform.SetParent(panelGO.transform, false);
            navRow.layer = 5;
            navRow.transform.localScale = Vector3.one;

            LayoutElement rowLE = navRow.AddComponent<LayoutElement>();
            rowLE.minHeight = 36f;
            rowLE.preferredHeight = 36f;

            HorizontalLayoutGroup hlg = navRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            Button previous = CreateButton(navRow.transform, "Previous", font);
            Button next = CreateButton(navRow.transform, "Next", font);

            navRow.transform.SetSiblingIndex(1);

            so.FindProperty("previousButton").objectReferenceValue = previous;
            so.FindProperty("nextButton").objectReferenceValue = next;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
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

    private static Button CreateButton(Transform parent, string label, TMP_FontAsset font)
    {
        GameObject go = new GameObject("[Button] " + label,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.layer = 5;
        go.transform.localScale = Vector3.one;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.24f, 0.55f, 0.9f, 1f);

        Button button = go.GetComponent<Button>();
        button.targetGraphic = img;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 28f;

        TMP_Text text = CreateText(go.transform, "[Text] " + label, label, font, 16f, FontStyles.Normal);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        RectTransform rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return button;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, TMP_FontAsset font, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        go.layer = 5;
        go.transform.localScale = Vector3.one;

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
}
