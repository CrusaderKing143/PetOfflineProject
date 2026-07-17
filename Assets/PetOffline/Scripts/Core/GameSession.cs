using System;
using UnityEngine;

namespace PetOffline.Core
{
    [DisallowMultipleComponent]
    public sealed partial class GameSession : MonoBehaviour,
        IGameCommandSink,
        ILevelHost,
        IGameSessionView
    {
        [SerializeField] private SceneFlowService sceneFlow;
        [SerializeField] private string saveKey = StorySaveService.DefaultPlayerPrefsKey;

        private StorySaveService _saveService;
        private ILevelRuntime _runtime;
        private ILevelViewModel _viewModel;
        private bool _isTransitioning;
        private bool _isInputLocked;
        private bool _paused;
        private bool _runtimeCommandPending;
        private LevelId _currentLevel;
        private string _lastError = string.Empty;

        public event Action Changed;

        public bool IsAtTitle => _currentLevel == LevelId.None;

        public bool IsBusy => _isTransitioning || (sceneFlow != null && sceneFlow.IsBusy);

        public bool Paused => _paused;

        public bool IsPaused => _paused;

        public bool IsInputLocked => _isInputLocked || IsBusy;

        public bool CanContinue => _saveService != null && _saveService.Progress.Day1Complete;

        public bool CanRouteInput => _runtime != null && !IsInputLocked;

        public string LastError => _lastError;

        public LevelId CurrentLevel => _currentLevel;

        public ILevelViewModel CurrentViewModel => _viewModel;

        public StoryProgress Story => _saveService != null
            ? _saveService.Progress
            : StoryProgress.Empty;

        private void Awake()
        {
            if (sceneFlow == null)
            {
                sceneFlow = GetComponent<SceneFlowService>();
            }

            _saveService = new StorySaveService(saveKey);
            _saveService.Load();
            ResetPauseState();
        }

        private void Start()
        {
            NotifyChanged();
        }

        private void OnDestroy()
        {
            ReleaseRuntime();
            ResetPauseState();
        }

        public void RouteInput(LevelInputFrame input)
        {
            if (!CanRouteInput)
            {
                return;
            }

            if (input.PausePressed)
            {
                SetPaused(!_paused);
                return;
            }

            if (_paused)
            {
                return;
            }

            try
            {
                _runtime.HandleInput(input);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ReturnToTitleAfterFailure("The level stopped responding to input.");
            }
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused)
            {
                return;
            }

            if (paused && !CanPause())
            {
                return;
            }

            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            NotifyChanged();
        }

        public void CompleteLevel(LevelId level, FinalChoice choice)
        {
            if (IsBusy || _runtime == null || level != _currentLevel)
            {
                return;
            }

            if (level == LevelId.Day1)
            {
                _saveService.MarkDay1Complete();
                BeginLoadLevel(LevelId.Day2);
                return;
            }

            if (level == LevelId.Day2 && IsEndingChoice(choice))
            {
                _saveService.MarkDay2Complete(choice);
                _runtimeCommandPending = false;
                NotifyChanged();
            }
        }

        private bool CanPause()
        {
            return !IsBusy
                && !_runtimeCommandPending
                && _viewModel != null
                && _viewModel.UiMode == LevelUiMode.Gameplay;
        }

        private void HandleViewModelChanged()
        {
            _runtimeCommandPending = false;
            if (_paused && !CanPause())
            {
                ResetPauseState();
            }

            NotifyChanged();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private void ResetPauseState()
        {
            _paused = false;
            Time.timeScale = 1f;
        }

        private static bool IsEndingChoice(FinalChoice choice)
        {
            return choice == FinalChoice.RestoreConnection
                || choice == FinalChoice.KeepQuiet;
        }
    }
}
