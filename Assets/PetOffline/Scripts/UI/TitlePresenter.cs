using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class TitlePresenter : SessionPresenterBase
    {
        [Header("Panels")]
        [SerializeField] private GameObject titleRoot;
        [SerializeField] private GameObject newGameConfirmationRoot;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button confirmNewGameButton;
        [SerializeField] private Button cancelNewGameButton;

        [Header("Feedback")]
        [SerializeField] private Text errorText;

        [Header("Opening Video")]
        [SerializeField] private GameObject videoRoot;
        [SerializeField] private RawImage videoImage;
        [SerializeField] private VideoPlayer openingVideo;

        private bool _awaitingNewGameConfirmation;

        protected override void AddButtonListeners()
        {
            AddListener(newGameButton, HandleNewGameClicked);
            AddListener(continueButton, HandleContinueClicked);
            AddListener(confirmNewGameButton, HandleConfirmNewGameClicked);
            AddListener(cancelNewGameButton, HandleCancelNewGameClicked);
        }

        protected override void RemoveButtonListeners()
        {
            RemoveListener(newGameButton, HandleNewGameClicked);
            RemoveListener(continueButton, HandleContinueClicked);
            RemoveListener(confirmNewGameButton, HandleConfirmNewGameClicked);
            RemoveListener(cancelNewGameButton, HandleCancelNewGameClicked);
            _awaitingNewGameConfirmation = false;
            SetActive(newGameConfirmationRoot, false);
            SetVideoState(false);
        }

        protected override void Redraw()
        {
            bool isAtTitle = SessionView != null && SessionView.IsAtTitle;
            bool isBusy = SessionView == null || SessionView.IsBusy;
            if (!isAtTitle || isBusy)
            {
                _awaitingNewGameConfirmation = false;
            }

            DrawTitleAndVideo(isAtTitle && !isBusy);
            DrawConfirmation(isAtTitle && !isBusy);
            DrawButtons(isAtTitle, isBusy);
            DrawError(isAtTitle);
        }

        private void DrawTitleAndVideo(bool isAtTitle)
        {
            if (isAtTitle)
            {
                SetActive(titleRoot, true);
                SetVideoState(true);
                return;
            }

            SetVideoState(false);
            SetActive(titleRoot, false);
        }

        private void DrawConfirmation(bool canConfirm)
        {
            bool visible = canConfirm && _awaitingNewGameConfirmation;
            SetActive(newGameConfirmationRoot, visible);
            SetInteractable(confirmNewGameButton, visible);
            SetInteractable(cancelNewGameButton, visible);
        }

        private void DrawButtons(bool isAtTitle, bool isBusy)
        {
            bool canChoose = isAtTitle && !isBusy && !_awaitingNewGameConfirmation;
            SetInteractable(newGameButton, canChoose);
            SetInteractable(
                continueButton,
                canChoose && SessionView != null && SessionView.CanContinue);
        }

        private void DrawError(bool isAtTitle)
        {
            if (errorText == null)
            {
                return;
            }

            string message = SessionView != null ? SessionView.LastError : string.Empty;
            errorText.text = message ?? string.Empty;
            SetActive(errorText.gameObject, isAtTitle && !string.IsNullOrEmpty(message));
        }

        private void SetVideoState(bool shouldPlay)
        {
            if (shouldPlay)
            {
                SetVideoVisible(true);
                PlayOpeningVideo();
                return;
            }

            if (openingVideo != null && openingVideo.isPlaying)
            {
                openingVideo.Pause();
            }

            SetVideoVisible(false);
        }

        private void PlayOpeningVideo()
        {
            if (openingVideo == null || !Application.isPlaying)
            {
                return;
            }

            openingVideo.isLooping = true;
            if (!openingVideo.isPlaying)
            {
                openingVideo.Play();
            }
        }

        private void SetVideoVisible(bool visible)
        {
            SetActive(videoRoot, visible);
            if (videoImage != null)
            {
                videoImage.enabled = visible;
            }
        }

        private void HandleNewGameClicked()
        {
            if (!CanSendTitleCommand())
            {
                return;
            }

            SetInteractable(newGameButton, false);
            _awaitingNewGameConfirmation = true;
            Redraw();
        }

        private void HandleConfirmNewGameClicked()
        {
            if (!_awaitingNewGameConfirmation || !CanSendTitleCommand())
            {
                return;
            }

            SetInteractable(confirmNewGameButton, false);
            SetInteractable(cancelNewGameButton, false);
            _awaitingNewGameConfirmation = false;
            CommandSink.NewGame();
            Redraw();
        }

        private void HandleCancelNewGameClicked()
        {
            SetInteractable(cancelNewGameButton, false);
            _awaitingNewGameConfirmation = false;
            Redraw();
        }

        private void HandleContinueClicked()
        {
            if (!CanSendTitleCommand() || !SessionView.CanContinue)
            {
                return;
            }

            SetInteractable(continueButton, false);
            CommandSink.Continue();
            Redraw();
        }

        private bool CanSendTitleCommand()
        {
            return SessionView != null
                && CommandSink != null
                && SessionView.IsAtTitle
                && !SessionView.IsBusy;
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
