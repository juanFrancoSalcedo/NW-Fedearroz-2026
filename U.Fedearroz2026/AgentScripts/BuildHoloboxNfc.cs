using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildHoloboxNfc
{
    private const string ExpectedScene = "Assets/Level/Scenes/Holobox.unity";
    private const string CanvasName = "Canvas (HeroIKVariant)";
    private const string FontPath = "Assets/ExternalAssets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    public static string Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ExpectedScene)
            throw new Exception($"Active scene is '{scene.path}', expected '{ExpectedScene}'.");

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
            throw new Exception($"TMP font not found at {FontPath}.");

        GameObject canvas = FindRoot(scene, CanvasName);
        if (canvas == null)
            throw new Exception($"Canvas '{CanvasName}' not found in scene.");

        GameObject target1 = CreateTarget(canvas.transform, 1, font);
        GameObject target2 = CreateTarget(canvas.transform, 2, font);
        GameObject target3 = CreateTarget(canvas.transform, 3, font);

        GameObject numero = CreateNumero(canvas.transform, font);

        GameObject folder = new GameObject("[Folder:Handlers]");
        SceneManager.MoveGameObjectToScene(folder, scene);

        GameObject readerGO = CreateHandler(folder.transform, "[Handler] NfcReader");
        readerGO.AddComponent<NFCReaderAndroid>();

        GameObject listenerGO = CreateHandler(folder.transform, "[Handler] NfcListener");
        NfcEventListener listener = listenerGO.AddComponent<NfcEventListener>();
        SerializedObject listenerSO = new SerializedObject(listener);
        listenerSO.FindProperty("subscribeOnEnable").boolValue = true;
        listenerSO.FindProperty("display").objectReferenceValue = numero.GetComponent<TMP_Text>();
        listenerSO.ApplyModifiedPropertiesWithoutUndo();

        GameObject nfcHandlerGO = CreateHandler(folder.transform, "[Handler] NfcHandler");
        NfcHandler nfcHandler = nfcHandlerGO.AddComponent<NfcHandler>();
        SerializedObject handlerSO = new SerializedObject(nfcHandler);
        SerializedProperty nfcs = handlerSO.FindProperty("nfcs");
        nfcs.arraySize = 3;
        AssignNfc(nfcs.GetArrayElementAtIndex(0), 1, target1);
        AssignNfc(nfcs.GetArrayElementAtIndex(1), 2, target2);
        AssignNfc(nfcs.GetArrayElementAtIndex(2), 3, target3);
        handlerSO.FindProperty("_numberTest").intValue = 2;
        handlerSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return $"NFC detector added to {ExpectedScene}";
    }

    private static void AssignNfc(SerializedProperty element, int id, GameObject target)
    {
        element.FindPropertyRelative("id").intValue = id;
        element.FindPropertyRelative("objetEnable").objectReferenceValue = target;
    }

    private static GameObject CreateTarget(Transform parent, int number, TMP_FontAsset font)
    {
        GameObject go = new GameObject($"[Panel] Target {number}",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image background = go.GetComponent<Image>();
        background.color = new Color(0.03f, 0.05f, 0.08f, 0.82f);
        background.raycastTarget = false;

        GameObject textGO = new GameObject($"[Text] Target {number}",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGO.layer = 5;
        textGO.transform.SetParent(go.transform, false);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(80f, 80f);
        textRect.offsetMax = new Vector2(-80f, -80f);
        textRect.localScale = Vector3.one;

        TMP_Text text = textGO.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = 90f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = $"This is the target ({number})";
        text.raycastTarget = false;

        go.SetActive(false);
        return go;
    }

    private static GameObject CreateNumero(Transform parent, TMP_FontAsset font)
    {
        GameObject go = new GameObject("[Text] Numero",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800f, 200f);
        rect.localScale = Vector3.one;

        TMP_Text text = go.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = 90f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = "Sin lectura";
        text.raycastTarget = false;

        go.SetActive(false);
        return go;
    }

    private static GameObject CreateHandler(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }
}
