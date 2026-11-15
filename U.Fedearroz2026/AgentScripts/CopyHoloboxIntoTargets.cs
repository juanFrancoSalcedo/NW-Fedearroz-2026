using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CopyHoloboxIntoTargets
{
    public static string Execute()
    {
        Scene scene = SceneManager.GetActiveScene();

        GameObject screen = GameObject.Find("[Screen] Holobox");
        GameObject radarGroup = GameObject.Find("[Panel] Radar");
        GameObject titleGroup = GameObject.Find("[Panel] Beneficio Presente");
        ControlNFC control = UnityEngine.Object.FindFirstObjectByType<ControlNFC>();

        if (screen == null || radarGroup == null || titleGroup == null || control == null)
            throw new System.Exception("Faltan Screen Holobox, Panel Radar, Beneficio Presente o ControlNFC.");

        if (radarGroup.transform.parent != screen.transform)
            radarGroup.transform.SetParent(screen.transform, false);
        radarGroup.transform.SetSiblingIndex(titleGroup.transform.GetSiblingIndex() + 1);
        radarGroup.SetActive(true);
        control.contenidoPorDefecto = radarGroup;

        List<GameObject> targets = new List<GameObject>();
        foreach (ControlNFC.TarjetaNFC tarjeta in control.tarjetas)
        {
            if (tarjeta == null || tarjeta.contenido == null)
                continue;
            if (!targets.Contains(tarjeta.contenido))
                targets.Add(tarjeta.contenido);
        }

        int copies = 0;
        foreach (GameObject target in targets)
        {
            bool alreadyCopied = false;
            foreach (Transform child in target.transform)
            {
                if (child.name.StartsWith("[Screen] Holobox"))
                {
                    alreadyCopied = true;
                    break;
                }
            }

            if (alreadyCopied)
                continue;

            GameObject copy = Object.Instantiate(screen, target.transform, false);
            copy.name = "[Screen] Holobox " + target.name.Replace("[Panel] ", "");
            copy.SetActive(true);
            copies++;
        }

        foreach (ControlNFC.TarjetaNFC tarjeta in control.tarjetas)
            if (tarjeta != null && tarjeta.contenido != null)
                tarjeta.contenido.SetActive(false);

        screen.SetActive(true);
        radarGroup.SetActive(true);

        EditorUtility.SetDirty(control);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return $"Copias creadas: {copies}. Targets: {targets.Count}.";
    }
}
