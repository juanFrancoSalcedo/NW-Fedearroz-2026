using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupRadarDefault
{
    public static string Setup()
    {
        Scene scene = SceneManager.GetActiveScene();

        GameObject screen = GameObject.Find("[Screen] Holobox");
        GameObject titleGroup = GameObject.Find("[Panel] Beneficio Presente");
        GameObject radarPanel = GameObject.Find("[Image] Cuadro Radar");
        GameObject radarImage = GameObject.Find("[Image] Radar");
        ControlNFC control = UnityEngine.Object.FindFirstObjectByType<ControlNFC>();

        if (screen == null || titleGroup == null || radarPanel == null || radarImage == null || control == null)
            throw new System.Exception("No se encontraron todos los objetos necesarios para configurar el radar por defecto.");

        GameObject radarGroup = GameObject.Find("[Panel] Radar");
        if (radarGroup == null)
        {
            radarGroup = new GameObject("[Panel] Radar", typeof(RectTransform));
            radarGroup.layer = 5;
            radarGroup.transform.SetParent(screen.transform, false);
        }

        RectTransform groupRect = radarGroup.GetComponent<RectTransform>();
        groupRect.anchorMin = Vector2.zero;
        groupRect.anchorMax = Vector2.one;
        groupRect.offsetMin = Vector2.zero;
        groupRect.offsetMax = Vector2.zero;
        groupRect.pivot = new Vector2(0.5f, 0.5f);
        groupRect.localScale = Vector3.one;

        radarGroup.transform.SetSiblingIndex(titleGroup.transform.GetSiblingIndex() + 1);

        if (radarPanel.transform.parent != radarGroup.transform)
            radarPanel.transform.SetParent(radarGroup.transform, false);
        if (radarImage.transform.parent != radarGroup.transform)
            radarImage.transform.SetParent(radarGroup.transform, false);

        radarPanel.transform.SetSiblingIndex(0);
        radarImage.transform.SetSiblingIndex(1);

        control.contenidoPorDefecto = radarGroup;

        foreach (ControlNFC.TarjetaNFC tarjeta in control.tarjetas)
            if (tarjeta != null && tarjeta.contenido != null)
                tarjeta.contenido.SetActive(false);

        radarGroup.SetActive(true);

        EditorUtility.SetDirty(control);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "Radar configurado como contenido por defecto.";
    }
}
