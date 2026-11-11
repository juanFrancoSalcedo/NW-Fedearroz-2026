using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FedeArroz.EditorTools
{
    public static class BuildConfigureRuleta
    {
        private const string RuletaScenePath = "Assets/Level/Scenes/Ruleta.unity";
        private const string HoloboxScenePath = "Assets/Level/Scenes/Holobox.unity";

        [MenuItem("Build Configure Ruleta/Ruleta (Android)", false, 1)]
        public static void ConfigureRuleta()
        {
            Configure(
                "Ruleta",
                RuletaScenePath,
                BuildTargetGroup.Android,
                BuildTarget.Android,
                1920,
                1080,
                true);
        }

        [MenuItem("Build Configure Ruleta/Holobox (Windows)", false, 2)]
        public static void ConfigureHolobox()
        {
            Configure(
                "Holobox",
                HoloboxScenePath,
                BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64,
                1080,
                1920,
                false);
        }

        private static void Configure(
            string label,
            string scenePath,
            BuildTargetGroup group,
            BuildTarget target,
            int width,
            int height,
            bool androidLandscape)
        {
            if (!File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog(
                    "Build Configure Ruleta",
                    $"No se encontró la escena:\n{scenePath}",
                    "Aceptar");
                return;
            }

            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                EditorUtility.DisplayDialog(
                    "Build Configure Ruleta",
                    $"El módulo de build para {target} no está instalado.\nInstalalo desde Unity Hub y volvé a intentar.",
                    "Aceptar");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            if (target == BuildTarget.Android)
            {
                PlayerSettings.Android.defaultWindowWidth = width;
                PlayerSettings.Android.defaultWindowHeight = height;

                if (androidLandscape)
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                    PlayerSettings.allowedAutorotateToPortrait = false;
                    PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                    PlayerSettings.allowedAutorotateToLandscapeLeft = true;
                    PlayerSettings.allowedAutorotateToLandscapeRight = false;
                }
            }
            else
            {
                PlayerSettings.defaultIsNativeResolution = true;
                PlayerSettings.resizableWindow = false;
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();

            if (EditorUserBuildSettings.activeBuildTarget != target)
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(group, target);

            Debug.Log(
                target == BuildTarget.Android
                    ? $"[Build Configure Ruleta] {label}: escena {scenePath}, target {target}, resolución {width}x{height}."
                    : $"[Build Configure Ruleta] {label}: escena {scenePath}, target {target}, fullscreen nativo.");
        }
    }
}
