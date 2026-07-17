using System;
using UnityEngine;

namespace PetOffline.Core
{
    public sealed partial class GameSession
    {
        public void NewGame()
        {
            if (!CanStartCommand())
            {
                return;
            }

            _saveService.Clear();
            BeginLoadLevel(LevelId.Day1);
        }

        public void Continue()
        {
            if (!CanStartCommand() || !CanContinue)
            {
                return;
            }

            BeginLoadLevel(LevelId.Day2);
        }

        public void ContinueReport()
        {
            TrySendRuntimeCommand(GameCommand.ContinueReport);
        }

        public void SubmitChoice(FinalChoice choice)
        {
            if (IsEndingChoice(choice))
            {
                TrySendRuntimeCommand(GameCommand.ForChoice(choice));
            }
        }

        public void ReturnTitle()
        {
            if (!CanStartCommand() || IsAtTitle)
            {
                return;
            }

            BeginReturnToTitle(string.Empty);
        }

        public void Restart()
        {
            if (!CanStartCommand())
            {
                return;
            }

            _saveService.Clear();
            BeginLoadLevel(LevelId.Day1);
        }

        private bool CanStartCommand()
        {
            return _saveService != null && !IsBusy;
        }

        private void TrySendRuntimeCommand(GameCommand command)
        {
            if (_runtime == null || IsBusy || _paused || _runtimeCommandPending)
            {
                return;
            }

            _runtimeCommandPending = true;
            try
            {
                if (!_runtime.TryHandleCommand(command))
                {
                    _runtimeCommandPending = false;
                }
            }
            catch (Exception exception)
            {
                _runtimeCommandPending = false;
                Debug.LogException(exception);
                ReturnToTitleAfterFailure("The level command could not be completed.");
            }
        }
    }
}
