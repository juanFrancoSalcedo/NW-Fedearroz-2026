using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildHoloboxSerialNfc
{
    private const string ScenePath = "Assets/Level/Scenes/Holobox.unity";

    public static string Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new Exception($"Active scene is '{scene.path}', expected '{ScenePath}'.");

        GameObject reader = FindInScene(scene, "[Handler] NfcReader");
        if (reader == null)
            throw new Exception("'[Handler] NfcReader' not found in Holobox.");

        GameObject target1 = FindInScene(scene, "[Panel] Target 1");
        GameObject target2 = FindInScene(scene, "[Panel] Target 2");
        GameObject target3 = FindInScene(scene, "[Panel] Target 3");

        if (target1 == null || target2 == null || target3 == null)
            throw new Exception("One or more '[Panel] Target N' objects were not found.");

        NFCReaderAndroid androidReader = reader.GetComponent<NFCReaderAndroid>();
        if (androidReader != null)
            UnityEngine.Object.DestroyImmediate(androidReader);

        ControlNFC control = reader.GetComponent<ControlNFC>();
        if (control == null)
            control = reader.AddComponent<ControlNFC>();

        SerializedObject so = new SerializedObject(control);

        SerializedProperty tarjetas = so.FindProperty("tarjetas");
        tarjetas.arraySize = 3;

        SetTarjeta(tarjetas.GetArrayElementAtIndex(0), "Target 1", "FBFE2A54", target1);
        SetTarjeta(tarjetas.GetArrayElementAtIndex(1), "Target 2", "400F8C8E", target2);
        SetTarjeta(tarjetas.GetArrayElementAtIndex(2), "Target 3", "", target3);

        so.FindProperty("soloUnContenidoVisible").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return "Holobox now reads NFC through ControlNFC (serial/keyboard).";
    }

    private static void SetTarjeta(SerializedProperty tarjeta, string nombre, string uid, GameObject contenido)
    {
        tarjeta.FindPropertyRelative("nombre").stringValue = nombre;
        tarjeta.FindPropertyRelative("uid").stringValue = uid;
        tarjeta.FindPropertyRelative("contenido").objectReferenceValue = contenido;
        tarjeta.FindPropertyRelative("alDetectar").FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize = 0;
    }

    private static GameObject FindInScene(Scene scene, string name)
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
}
