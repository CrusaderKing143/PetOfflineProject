using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetOffline.Core
{
    [DisallowMultipleComponent]
    public sealed class SceneFlowService : MonoBehaviour
    {
        [SerializeField] private string startPanelScene = "StartPanel";
        [SerializeField] private string day1Scene = "Main1";
        [SerializeField] private string day2Scene = "Main2";

        private Scene _worldScene;

        public bool IsBusy { get; private set; }

        public LevelId LoadedLevel { get; private set; }

        public ILevelRuntime CurrentRuntime { get; private set; }

        public bool LoadLevel(
            LevelId level,
            Action<ILevelRuntime> completed,
            Action<string> failed)
        {
            if (IsBusy || !TryGetSceneName(level, out string sceneName))
            {
                return false;
            }

            IsBusy = true;
            StartCoroutine(LoadLevelRoutine(level, sceneName, completed, failed));
            return true;
        }

        public bool UnloadWorld(Action completed, Action<string> failed)
        {
            if (IsBusy)
            {
                return false;
            }

            if (!HasLoadedWorld())
            {
                ResetWorldState();
                ActivateStartPanel();
                completed?.Invoke();
                return true;
            }

            IsBusy = true;
            StartCoroutine(UnloadWorldRoutine(completed, failed));
            return true;
        }

        private IEnumerator LoadLevelRoutine(
            LevelId level,
            string sceneName,
            Action<ILevelRuntime> completed,
            Action<string> failed)
        {
            if (HasLoadedWorld())
            {
                yield return UnloadTrackedScene();
                if (HasLoadedWorld())
                {
                    FinishWithFailure("The previous level could not be unloaded.", failed);
                    yield break;
                }
            }

            AsyncOperation loadOperation = TryStartLoad(sceneName);
            if (loadOperation == null)
            {
                FinishWithFailure($"Level scene '{sceneName}' could not be loaded.", failed);
                yield break;
            }

            yield return loadOperation;
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                FinishWithFailure($"Level scene '{sceneName}' did not finish loading.", failed);
                yield break;
            }

            _worldScene = loadedScene;
            List<ILevelRuntime> runtimes = FindRuntimes(loadedScene);
            if (runtimes.Count != 1 || runtimes[0].Level != level)
            {
                yield return UnloadTrackedScene();
                string message = runtimes.Count == 1
                    ? $"Level scene '{sceneName}' contains the wrong runtime."
                    : $"Level scene '{sceneName}' must contain exactly one level runtime.";
                FinishWithFailure(message, failed);
                yield break;
            }

            LoadedLevel = level;
            CurrentRuntime = runtimes[0];
            SceneManager.SetActiveScene(loadedScene);
            IsBusy = false;
            completed?.Invoke(CurrentRuntime);
        }

        private IEnumerator UnloadWorldRoutine(Action completed, Action<string> failed)
        {
            yield return UnloadTrackedScene();
            if (HasLoadedWorld())
            {
                FinishWithFailure("The current level could not be unloaded.", failed);
                yield break;
            }

            ResetWorldState();
            ActivateStartPanel();
            IsBusy = false;
            completed?.Invoke();
        }

        private IEnumerator UnloadTrackedScene()
        {
            if (!HasLoadedWorld())
            {
                ResetWorldState();
                yield break;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(_worldScene);
            if (operation != null)
            {
                yield return operation;
            }

            if (!_worldScene.IsValid() || !_worldScene.isLoaded)
            {
                ResetWorldState();
            }
        }

        private static AsyncOperation TryStartLoad(string sceneName)
        {
            try
            {
                return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError(exception.Message);
                return null;
            }
        }

        private static List<ILevelRuntime> FindRuntimes(Scene scene)
        {
            var runtimes = new List<ILevelRuntime>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is ILevelRuntime runtime)
                    {
                        runtimes.Add(runtime);
                    }
                }
            }

            return runtimes;
        }

        private bool TryGetSceneName(LevelId level, out string sceneName)
        {
            sceneName = level == LevelId.Day1
                ? day1Scene
                : level == LevelId.Day2 ? day2Scene : string.Empty;
            return !string.IsNullOrWhiteSpace(sceneName);
        }

        private bool HasLoadedWorld()
        {
            return _worldScene.IsValid() && _worldScene.isLoaded;
        }

        private void ResetWorldState()
        {
            _worldScene = default;
            LoadedLevel = LevelId.None;
            CurrentRuntime = null;
        }

        private void ActivateStartPanel()
        {
            Scene scene = SceneManager.GetSceneByName(startPanelScene);
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
            }
        }

        private void FinishWithFailure(string message, Action<string> failed)
        {
            if (!HasLoadedWorld())
            {
                ResetWorldState();
            }

            ActivateStartPanel();
            IsBusy = false;
            failed?.Invoke(message);
        }
    }
}
