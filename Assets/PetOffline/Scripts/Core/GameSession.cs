using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetOffline
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameUI))]
    public sealed class GameSession : MonoBehaviour
    {
        private const string StartScene = "StartPanel";
        private const string Day1Scene = "Main1";
        private const string Day2Scene = "Main2";

        private Scene worldScene;
        private LevelController level;
        private bool busy;
        private bool paused;

        public GameUI UI { get; private set; }
        public bool IsAtTitle => !worldScene.IsValid();
        public bool IsBusy => busy;
        public bool IsPaused => paused;
        public LevelController CurrentLevel => level;

        private void Awake()
        {
            UI = GetComponent<GameUI>();
            Time.timeScale = 1f;
        }

        private void Start()
        {
            UI.ShowTitle();
        }

        private void Update()
        {
            if (busy || !level)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
                return;
            }

            if (!paused)
            {
                level.HandleInput(PlayerInput.Read());
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        public void NewGame()
        {
            StartLevel(Day1Scene);
        }

        public void Restart()
        {
            StartLevel(Day1Scene);
        }

        public void LoadDay2()
        {
            StartLevel(Day2Scene);
        }

        public void ContinueReport()
        {
            if (!busy && !paused && level)
            {
                level.ContinueReport();
            }
        }

        public void SubmitChoice(FinalChoice choice)
        {
            if (!busy && !paused && level)
            {
                level.SubmitChoice(choice);
            }
        }

        public void ReturnTitle()
        {
            if (!busy && worldScene.IsValid())
            {
                StartCoroutine(ReturnTitleRoutine());
            }
        }

        private void StartLevel(string sceneName)
        {
            if (!busy)
            {
                StartCoroutine(LoadLevelRoutine(sceneName));
            }
        }

        private IEnumerator LoadLevelRoutine(string sceneName)
        {
            BeginTransition();
            yield return UnloadWorld();
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            worldScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(worldScene);
            level = FindObjectOfType<LevelController>();
            if (!level)
            {
                Debug.LogError($"Scene '{sceneName}' has no LevelController.");
                yield return ReturnTitleRoutine();
                yield break;
            }

            level.Initialize(this);
            busy = false;
        }

        private IEnumerator ReturnTitleRoutine()
        {
            BeginTransition();
            yield return UnloadWorld();
            worldScene = default;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(StartScene));
            busy = false;
            UI.ShowTitle();
        }

        private IEnumerator UnloadWorld()
        {
            if (worldScene.IsValid() && worldScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(worldScene);
            }
        }

        private void BeginTransition()
        {
            busy = true;
            level = null;
            SetPaused(false);
            UI.ShowLoading();
        }

        private void TogglePause()
        {
            if (level.CanPause)
            {
                SetPaused(!paused);
            }
        }

        private void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = value ? 0f : 1f;
            UI.ShowPause(value);
        }
    }
}
