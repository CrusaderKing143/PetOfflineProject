using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PetOffline.Editor
{
    public static partial class ProjectInstaller
    {
        private const string MenuPath = "Pet Offline/Install Playable Prototype";

        [MenuItem(MenuPath)]
        public static void InstallFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Pet Offline Installer] Installation cancelled.");
                return;
            }

            InstallInternal();
        }

        public static void InstallBatchMode()
        {
            InstallInternal();
        }

        public static void InstallFromCommandLine()
        {
            InstallInternal();
        }

        private static void InstallInternal()
        {
            EnsureProjectFolders();
            ConfigureLayersAndCollisions();
            ProjectInstallAssets assets = ProjectInstallerData.CreateAssets();
            ProjectInstallerStartPanel.Build(assets);
            ProjectInstallerDay1.Build(assets);
            ProjectInstallerDay2.Build(assets);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ProjectInstallerPaths.StartPanelScene);
            ProjectValidator.ValidateOrThrow();
            Debug.Log("[Pet Offline Installer] Playable prototype installation completed.");
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "PetOffline");
            EnsureFolder("Assets/PetOffline", "Data");
            EnsureFolder("Assets/PetOffline/Data", "Generated");
            EnsureFolder("Assets/PetOffline", "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void ConfigureLayersAndCollisions()
        {
            UnityEngine.Object[] settings =
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (settings.Length == 0)
            {
                throw new InvalidOperationException("TagManager.asset could not be loaded.");
            }

            var serialized = new SerializedObject(settings[0]);
            SerializedProperty layers = serialized.FindProperty("layers");
            SetLayer(layers, ProjectInstallerPaths.SightBlockerLayer, "SightBlocker");
            SetLayer(layers, ProjectInstallerPaths.GameplayTriggerLayer, "GameplayTrigger");
            SetLayer(layers, ProjectInstallerPaths.PlayerLayer, "Player");
            SetLayer(layers, ProjectInstallerPaths.CarryableLayer, "Carryable");
            SetLayer(layers, ProjectInstallerPaths.RobotLayer, "Robot");
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Physics2D.IgnoreLayerCollision(
                ProjectInstallerPaths.PlayerLayer,
                ProjectInstallerPaths.CarryableLayer,
                true);
            Physics2D.IgnoreLayerCollision(
                ProjectInstallerPaths.RobotLayer,
                ProjectInstallerPaths.CarryableLayer,
                false);
        }

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            if (layers == null || index < 0 || index >= layers.arraySize)
            {
                throw new InvalidOperationException($"Layer slot {index} is unavailable.");
            }

            layers.GetArrayElementAtIndex(index).stringValue = name;
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ProjectInstallerPaths.StartPanelScene, true),
                new EditorBuildSettingsScene(ProjectInstallerPaths.Main1Scene, true),
                new EditorBuildSettingsScene(ProjectInstallerPaths.Main2Scene, true)
            };
        }
    }
}
