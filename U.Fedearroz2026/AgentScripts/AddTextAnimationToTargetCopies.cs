using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddTextAnimationToTargetCopies
{
    public static string Execute()
    {
        Scene scene = SceneManager.GetActiveScene();
        ControlNFC control = UnityEngine.Object.FindFirstObjectByType<ControlNFC>();
        if (control == null)
            throw new System.Exception("ControlNFC no encontrado.");

        const string texto = "Más de 7.000 tipos de arroz conservados";
        List<GameObject> targets = new List<GameObject>();
        foreach (ControlNFC.TarjetaNFC tarjeta in control.tarjetas)
        {
            if (tarjeta == null || tarjeta.contenido == null)
                continue;
            if (!targets.Contains(tarjeta.contenido))
                targets.Add(tarjeta.contenido);
        }

        int added = 0;
        foreach (GameObject target in targets)
        {
            Transform copy = null;
            foreach (Transform child in target.transform)
            {
                if (child.name.StartsWith("[Screen] Holobox"))
                {
                    copy = child;
                    break;
                }
            }
            if (copy == null)
                continue;

            Transform textTransform = null;
            foreach (Transform tr in copy.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name == "[Text] Tipos de arroz")
                {
                    textTransform = tr;
                    break;
                }
            }
            if (textTransform == null)
                continue;

            DemoDialogAnimation animation = textTransform.GetComponent<DemoDialogAnimation>();
            if (animation == null)
                continue;

            SerializedObject animationObject = new SerializedObject(animation);
            animationObject.FindProperty("textComponent").objectReferenceValue = textTransform.GetComponent<TMPro.TMP_Text>();
            animationObject.ApplyModifiedPropertiesWithoutUndo();

            Transform handlerTransform = null;
            foreach (Transform child in target.transform)
            {
                if (child.name == "[Handler] Animacion Texto")
                {
                    handlerTransform = child;
                    break;
                }
            }

            GameObject handler;
            if (handlerTransform == null)
            {
                handler = new GameObject("[Handler] Animacion Texto");
                handler.transform.SetParent(target.transform, false);
                added++;
            }
            else
            {
                handler = handlerTransform.gameObject;
            }

            DemoDialogManager manager = handler.GetComponent<DemoDialogManager>();
            if (manager == null)
                manager = handler.AddComponent<DemoDialogManager>();

            SerializedObject managerObject = new SerializedObject(manager);
            managerObject.FindProperty("dialogDisplayer").objectReferenceValue = animation;
            SerializedProperty dialogs = managerObject.FindProperty("dialogs");
            dialogs.arraySize = 1;
            dialogs.GetArrayElementAtIndex(0).stringValue = texto;
            SerializedProperty interfaces = managerObject.FindProperty("interfaces");
            interfaces.arraySize = 1;
            interfaces.GetArrayElementAtIndex(0).stringValue = "AnimCharRise";
            managerObject.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return $"Managers de texto agregados: {added}.";
    }
}
