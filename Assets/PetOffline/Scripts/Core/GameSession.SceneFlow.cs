using System;
using UnityEngine;

namespace PetOffline.Core
{
    public sealed partial class GameSession
    {
        private void BeginLoadLevel(LevelId level)
        {
            if (!PrepareTransition())
            {
                return;
            }

            bool started = sceneFlow.LoadLevel(
                level,
                runtime => HandleLevelLoaded(level, runtime),
                HandleSceneFlowFailure);
            if (!started)
            {
                FinishAtTitle("Another scene operation is already running.");
            }
        }

        private void BeginReturnToTitle(string error)
        {
            if (!PrepareTransition())
            {
                FinishAtTitle(error);
                return;
            }

            bool started = sceneFlow.UnloadWorld(
                () => FinishAtTitle(error),
                flowError => FinishAtTitle(CombineErrors(error, flowError)));
            if (!started)
            {
                FinishAtTitle(CombineErrors(error, "The level could not be unloaded."));
            }
        }

        private bool PrepareTransition()
        {
            if (sceneFlow == null || IsBusy)
            {
                return false;
            }

            _isTransitioning = true;
            _isInputLocked = true;
            _runtimeCommandPending = false;
            _lastError = string.Empty;
            ResetPauseState();
            ReleaseRuntime();
            NotifyChanged();
            return true;
        }

        private void HandleLevelLoaded(LevelId expectedLevel, ILevelRuntime runtime)
        {
            if (runtime == null || runtime.Level != expectedLevel)
            {
                ReturnToTitleAfterFailure("The loaded level runtime is invalid.");
                return;
            }

            try
            {
                BindRuntime(runtime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ReturnToTitleAfterFailure("The loaded level could not be initialized.");
                return;
            }

            _isTransitioning = false;
            _isInputLocked = false;
            _lastError = string.Empty;
            NotifyChanged();
        }

        private void BindRuntime(ILevelRuntime runtime)
        {
            _runtime = runtime;
            _runtime.Bind(new LevelRuntimeContext(this, _saveService.Progress));
            _viewModel = _runtime.ViewModel
                ?? throw new InvalidOperationException("Level runtime has no view model.");
            if (_viewModel.Level != _runtime.Level)
            {
                throw new InvalidOperationException("Level runtime and view model do not match.");
            }

            _viewModel.Changed += HandleViewModelChanged;
            _currentLevel = _runtime.Level;
        }

        private void ReleaseRuntime()
        {
            if (_viewModel != null)
            {
                _viewModel.Changed -= HandleViewModelChanged;
            }

            ILevelRuntime runtime = _runtime;
            _runtime = null;
            _viewModel = null;
            _currentLevel = LevelId.None;
            if (runtime == null)
            {
                return;
            }

            try
            {
                runtime.Unbind();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void HandleSceneFlowFailure(string error)
        {
            FinishAtTitle(error);
        }

        private void ReturnToTitleAfterFailure(string error)
        {
            ReleaseRuntime();
            ResetPauseState();
            _runtimeCommandPending = false;
            _isTransitioning = true;
            _isInputLocked = true;

            if (sceneFlow == null || sceneFlow.IsBusy)
            {
                FinishAtTitle(error);
                return;
            }

            bool started = sceneFlow.UnloadWorld(
                () => FinishAtTitle(error),
                flowError => FinishAtTitle(CombineErrors(error, flowError)));
            if (!started)
            {
                FinishAtTitle(error);
            }
        }

        private void FinishAtTitle(string error)
        {
            ReleaseRuntime();
            ResetPauseState();
            _runtimeCommandPending = false;
            _isTransitioning = false;
            _isInputLocked = false;
            _lastError = error ?? string.Empty;
            NotifyChanged();
        }

        private static string CombineErrors(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first))
            {
                return second ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(second) ? first : $"{first} {second}";
        }
    }
}
