using System;
using System.Collections.Generic;
using System.Linq;
using PetOffline.Core;
using PetOffline.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PetOffline.Editor
{
    public static partial class ProjectValidator
    {
        private static void ValidateBuildSettings(ICollection<string> errors)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length != ScenePaths.Length)
            {
                errors.Add($"Build Settings must contain exactly {ScenePaths.Length} scenes; found {scenes.Length}.");
            }

            int count = Math.Min(scenes.Length, ScenePaths.Length);
            for (var index = 0; index < count; index++)
            {
                EditorBuildSettingsScene scene = scenes[index];
                if (!string.Equals(scene.path, ScenePaths[index], StringComparison.Ordinal))
                {
                    errors.Add($"Build Settings scene {index} must be {ScenePaths[index]}, but is {scene.path}.");
                }

                if (!scene.enabled)
                {
                    errors.Add($"Build Settings scene {index} ({scene.path}) must be enabled.");
                }
            }
        }

        private static void ValidateSceneAssets(List<string> errors)
        {
            foreach (string scenePath in ScenePaths)
            {
                ValidateSceneDependencies(scenePath, errors);
            }

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string scenePath in ScenePaths)
                {
                    ValidateOpenedScene(scenePath, errors);
                }
            }
            finally
            {
                TryRestoreSceneSetup(setup, errors);
            }
        }

        private static void TryRestoreSceneSetup(SceneSetup[] setup, ICollection<string> errors)
        {
            try
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
            catch (Exception exception)
            {
                errors.Add($"Could not restore the previous scene setup: {exception.Message}");
            }
        }

        private static void ValidateOpenedScene(string scenePath, List<string> errors)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                errors.Add($"Required scene asset is missing: {scenePath}");
                return;
            }

            try
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ValidateSceneContents(scene, scenePath, errors);
            }
            catch (Exception exception)
            {
                errors.Add($"Could not validate scene {scenePath}: {exception.Message}");
            }
        }

        private static void ValidateSceneContents(Scene scene, string scenePath, List<string> errors)
        {
            ValidateMissingScripts(scene, scenePath, errors);
            List<MonoBehaviour> behaviours = GetSceneBehaviours(scene);
            if (string.Equals(scenePath, StartPanelPath, StringComparison.Ordinal))
            {
                ValidateStartPanel(scene, behaviours, errors);
            }
            else
            {
                string expectedRuntime = scenePath == Main1Path
                    ? "PetOffline.Gameplay.Day1Runtime"
                    : "PetOffline.Gameplay.Day2Runtime";
                ValidateWorldScene(scene, scenePath, expectedRuntime, behaviours, errors);
            }

            ValidateReferenceOwners(scenePath, behaviours, errors);
            ValidateCanvasScalers(scene, scenePath, scenePath == StartPanelPath, errors);
            ValidateCameraSensorOrigins(scene, scenePath, errors);
            ValidateRobotPatrolPaths(scene, scenePath, errors);
        }

        private static void ValidateStartPanel(
            Scene scene,
            IReadOnlyCollection<MonoBehaviour> behaviours,
            ICollection<string> errors)
        {
            ValidateComponentCount<Camera>(scene, StartPanelPath, 1, errors);
            ValidateComponentCount<AudioListener>(scene, StartPanelPath, 1, errors);
            ValidateComponentCount<EventSystem>(scene, StartPanelPath, 1, errors);
            foreach (string fullTypeName in StartPanelComponentNames)
            {
                int count = behaviours.Count(component =>
                    string.Equals(component.GetType().FullName, fullTypeName, StringComparison.Ordinal));
                if (count != 1)
                {
                    errors.Add($"StartPanel must contain exactly one {fullTypeName}; found {count}.");
                }
            }
        }

        private static void ValidateWorldScene(
            Scene scene,
            string scenePath,
            string expectedRuntimeName,
            IReadOnlyCollection<MonoBehaviour> behaviours,
            ICollection<string> errors)
        {
            ValidateComponentCount<Camera>(scene, scenePath, 0, errors);
            ValidateComponentCount<AudioListener>(scene, scenePath, 0, errors);
            ValidateComponentCount<Canvas>(scene, scenePath, 0, errors);
            List<MonoBehaviour> runtimes = behaviours.Where(component => component is ILevelRuntime).ToList();
            if (runtimes.Count != 1)
            {
                errors.Add($"{scenePath} must contain exactly one ILevelRuntime; found {runtimes.Count}.");
                return;
            }

            string actualName = runtimes[0].GetType().FullName;
            if (!string.Equals(actualName, expectedRuntimeName, StringComparison.Ordinal))
            {
                errors.Add($"{scenePath} requires {expectedRuntimeName}, but contains {actualName}.");
            }
        }

        private static void ValidateComponentCount<T>(
            Scene scene,
            string scenePath,
            int expectedCount,
            ICollection<string> errors) where T : Component
        {
            int count = GetSceneComponents<T>(scene).Count;
            if (count != expectedCount)
            {
                errors.Add(
                    $"{scenePath} must contain {expectedCount} {typeof(T).Name} component(s); found {count}.");
            }
        }

        private static void ValidateMissingScripts(
            Scene scene,
            string scenePath,
            ICollection<string> errors)
        {
            foreach (GameObject gameObject in GetSceneGameObjects(scene))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (count > 0)
                {
                    errors.Add(
                        $"{scenePath}: {GetHierarchyPath(gameObject.transform)} has {count} missing script(s).");
                }
            }
        }

        private static void ValidateSceneDependencies(string scenePath, ICollection<string> errors)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            foreach (string dependency in AssetDatabase.GetDependencies(scenePath, true))
            {
                if (IsForbiddenSceneDependency(dependency))
                {
                    errors.Add($"{scenePath} references forbidden full-background animation asset: {dependency}");
                }
            }
        }

        private static bool IsForbiddenSceneDependency(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            string fileName = normalized.Substring(normalized.LastIndexOf('/') + 1);
            return normalized.IndexOf("scene1_bg_animation/合成 1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(fileName, "scene1_bg.anim", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fileName, "bg.controller", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateCanvasScalers(
            Scene scene,
            string scenePath,
            bool requireAtLeastOne,
            ICollection<string> errors)
        {
            List<CanvasScaler> scalers = GetSceneComponents<CanvasScaler>(scene);
            if (requireAtLeastOne && scalers.Count == 0)
            {
                errors.Add($"{scenePath} must contain a CanvasScaler configured for 1920x1080.");
            }

            foreach (CanvasScaler scaler in scalers)
            {
                Vector2 resolution = scaler.referenceResolution;
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    errors.Add(
                        $"{scenePath}: CanvasScaler at {GetHierarchyPath(scaler.transform)} must use Scale With Screen Size.");
                }

                if (!Mathf.Approximately(resolution.x, 1920f) || !Mathf.Approximately(resolution.y, 1080f))
                {
                    errors.Add(
                        $"{scenePath}: CanvasScaler at {GetHierarchyPath(scaler.transform)} uses " +
                        $"{resolution.x}x{resolution.y}; expected 1920x1080.");
                }
            }
        }

        private static void ValidateCameraSensorOrigins(
            Scene scene,
            string scenePath,
            ICollection<string> errors)
        {
            int blockerMask = 1 << ProjectInstallerPaths.SightBlockerLayer;
            Physics2D.SyncTransforms();
            foreach (CameraSensor sensor in GetSceneComponents<CameraSensor>(scene))
            {
                Collider2D blocker = Physics2D.OverlapPoint(sensor.transform.position, blockerMask);
                if (blocker != null)
                {
                    errors.Add(
                        $"{scenePath}: CameraSensor at {GetHierarchyPath(sensor.transform)} " +
                        $"starts inside sight blocker {GetHierarchyPath(blocker.transform)}.");
                }
            }
        }

        private static void ValidateRobotPatrolPaths(
            Scene scene,
            string scenePath,
            ICollection<string> errors)
        {
            int blockerMask = 1 << ProjectInstallerPaths.SightBlockerLayer;
            Physics2D.SyncTransforms();
            foreach (RobotPatrol robot in GetSceneComponents<RobotPatrol>(scene))
            {
                var serialized = new SerializedObject(robot);
                SerializedProperty waypoints = serialized.FindProperty("waypoints");
                CircleCollider2D collider = robot.GetComponent<CircleCollider2D>();
                if (waypoints == null || waypoints.arraySize == 0 || collider == null)
                {
                    continue;
                }

                Vector2 start = robot.transform.position;
                for (var index = 0; index <= waypoints.arraySize; index++)
                {
                    int targetIndex = index % waypoints.arraySize;
                    var target = waypoints.GetArrayElementAtIndex(targetIndex).objectReferenceValue as Transform;
                    if (target == null || !ValidateRobotSegment(robot, start, target.position, collider, blockerMask, scenePath, errors))
                    {
                        break;
                    }

                    start = target.position;
                }
            }
        }

        private static bool ValidateRobotSegment(
            RobotPatrol robot,
            Vector2 start,
            Vector2 end,
            CircleCollider2D collider,
            int blockerMask,
            string scenePath,
            ICollection<string> errors)
        {
            Vector2 delta = end - start;
            if (delta.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            Vector3 scale = robot.transform.lossyScale;
            float radius = collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            RaycastHit2D hit = Physics2D.CircleCast(start, radius, delta.normalized, delta.magnitude, blockerMask);
            if (hit.collider == null)
            {
                return true;
            }

            errors.Add(
                $"{scenePath}: RobotPatrol at {GetHierarchyPath(robot.transform)} crosses " +
                $"sight blocker {GetHierarchyPath(hit.collider.transform)} between {start} and {end}.");
            return false;
        }

        private static List<MonoBehaviour> GetSceneBehaviours(Scene scene)
        {
            return GetSceneComponents<MonoBehaviour>(scene)
                .Where(component => component != null)
                .ToList();
        }

        private static List<T> GetSceneComponents<T>(Scene scene) where T : Component
        {
            var result = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                result.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return result;
        }

        private static List<GameObject> GetSceneGameObjects(Scene scene)
        {
            var result = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                result.AddRange(root.GetComponentsInChildren<Transform>(true).Select(item => item.gameObject));
            }

            return result;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            for (Transform current = transform; current != null; current = current.parent)
            {
                names.Push(current.name);
            }

            return string.Join("/", names);
        }
    }
}
