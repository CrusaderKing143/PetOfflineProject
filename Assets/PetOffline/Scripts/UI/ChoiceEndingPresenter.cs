using PetOffline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class ChoiceEndingPresenter : SessionPresenterBase
    {
        [Header("Choice")]
        [SerializeField] private GameObject choiceRoot;
        [SerializeField] private Button restoreConnectionButton;
        [SerializeField] private Button keepQuietButton;

        [Header("Ending")]
        [SerializeField] private GameObject endingRoot;
        [SerializeField] private Text endingText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button returnTitleButton;

        [Header("Restart Confirmation")]
        [SerializeField] private GameObject restartConfirmationRoot;
        [SerializeField] private Button confirmRestartButton;
        [SerializeField] private Button cancelRestartButton;

        private bool _choiceSubmitted;
        private bool _endingCommandSubmitted;
        private bool _awaitingRestartConfirmation;

        protected override void AddButtonListeners()
        {
            AddListener(restoreConnectionButton, HandleRestoreConnectionClicked);
            AddListener(keepQuietButton, HandleKeepQuietClicked);
            AddListener(restartButton, HandleRestartClicked);
            AddListener(returnTitleButton, HandleReturnTitleClicked);
            AddListener(confirmRestartButton, HandleConfirmRestartClicked);
            AddListener(cancelRestartButton, HandleCancelRestartClicked);
        }

        protected override void RemoveButtonListeners()
        {
            RemoveListener(restoreConnectionButton, HandleRestoreConnectionClicked);
            RemoveListener(keepQuietButton, HandleKeepQuietClicked);
            RemoveListener(restartButton, HandleRestartClicked);
            RemoveListener(returnTitleButton, HandleReturnTitleClicked);
            RemoveListener(confirmRestartButton, HandleConfirmRestartClicked);
            RemoveListener(cancelRestartButton, HandleCancelRestartClicked);
            ResetLocalState();
        }

        protected override void Redraw()
        {
            LevelUiMode mode = ViewModel != null ? ViewModel.UiMode : LevelUiMode.Title;
            bool choiceVisible = ViewModel != null && mode == LevelUiMode.Choice;
            bool endingVisible = ViewModel != null && mode == LevelUiMode.Ending;
            if (!choiceVisible)
            {
                _choiceSubmitted = false;
            }

            if (!endingVisible)
            {
                ResetEndingState();
            }

            SetActive(choiceRoot, choiceVisible);
            SetActive(endingRoot, endingVisible);
            DrawChoiceButtons(choiceVisible);
            DrawEnding(endingVisible);
        }

        private void DrawChoiceButtons(bool visible)
        {
            bool interactable = visible
                && !_choiceSubmitted
                && SessionView != null
                && !SessionView.IsBusy;
            SetInteractable(restoreConnectionButton, interactable);
            SetInteractable(keepQuietButton, interactable);
        }

        private void DrawEnding(bool visible)
        {
            if (endingText != null)
            {
                endingText.text = visible && ViewModel != null
                    ? ViewModel.EndingId ?? string.Empty
                    : string.Empty;
            }

            bool canChoose = visible
                && !_endingCommandSubmitted
                && !_awaitingRestartConfirmation
                && SessionView != null
                && !SessionView.IsBusy;
            SetInteractable(restartButton, canChoose);
            SetInteractable(returnTitleButton, canChoose);
            DrawRestartConfirmation(visible);
        }

        private void DrawRestartConfirmation(bool endingVisible)
        {
            bool visible = endingVisible
                && _awaitingRestartConfirmation
                && SessionView != null
                && !SessionView.IsBusy;
            SetActive(restartConfirmationRoot, visible);
            SetInteractable(confirmRestartButton, visible);
            SetInteractable(cancelRestartButton, visible);
        }

        private void HandleRestoreConnectionClicked()
        {
            SubmitChoice(FinalChoice.RestoreConnection);
        }

        private void HandleKeepQuietClicked()
        {
            SubmitChoice(FinalChoice.KeepQuiet);
        }

        private void SubmitChoice(FinalChoice choice)
        {
            if (!CanSubmitChoice())
            {
                return;
            }

            _choiceSubmitted = true;
            SetInteractable(restoreConnectionButton, false);
            SetInteractable(keepQuietButton, false);
            CommandSink.SubmitChoice(choice);
            Redraw();
        }

        private bool CanSubmitChoice()
        {
            return !_choiceSubmitted
                && CommandSink != null
                && SessionView != null
                && !SessionView.IsBusy
                && ViewModel != null
                && ViewModel.UiMode == LevelUiMode.Choice;
        }

        private void HandleRestartClicked()
        {
            if (!CanSendEndingCommand())
            {
                return;
            }

            SetInteractable(restartButton, false);
            _awaitingRestartConfirmation = true;
            Redraw();
        }

        private void HandleConfirmRestartClicked()
        {
            if (!_awaitingRestartConfirmation || !CanSendEndingCommand())
            {
                return;
            }

            SetInteractable(confirmRestartButton, false);
            SetInteractable(cancelRestartButton, false);
            _awaitingRestartConfirmation = false;
            _endingCommandSubmitted = true;
            CommandSink.Restart();
            Redraw();
        }

        private void HandleCancelRestartClicked()
        {
            SetInteractable(cancelRestartButton, false);
            _awaitingRestartConfirmation = false;
            Redraw();
        }

        private void HandleReturnTitleClicked()
        {
            if (!CanSendEndingCommand())
            {
                return;
            }

            SetInteractable(returnTitleButton, false);
            _endingCommandSubmitted = true;
            CommandSink.ReturnTitle();
            Redraw();
        }

        private bool CanSendEndingCommand()
        {
            return !_endingCommandSubmitted
                && CommandSink != null
                && SessionView != null
                && !SessionView.IsBusy
                && ViewModel != null
                && ViewModel.UiMode == LevelUiMode.Ending;
        }

        private void ResetLocalState()
        {
            _choiceSubmitted = false;
            ResetEndingState();
        }

        private void ResetEndingState()
        {
            _endingCommandSubmitted = false;
            _awaitingRestartConfirmation = false;
            SetActive(restartConfirmationRoot, false);
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void SetInteractable(Selectable selectable, bool value)
        {
            if (selectable != null)
            {
                selectable.interactable = value;
            }
        }
    }
}
