using PetOffline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class PausePresenter : SessionPresenterBase
    {
        [SerializeField] private GameObject pauseRoot;
        [SerializeField] private Text pauseText;
        [SerializeField] private Button returnTitleButton;
        [SerializeField] private string pausedMessage = "PAUSED\nPress Esc to resume.";

        private bool _commandSubmitted;

        protected override void AddButtonListeners()
        {
            if (returnTitleButton != null)
            {
                returnTitleButton.onClick.AddListener(HandleReturnTitleClicked);
            }
        }

        protected override void RemoveButtonListeners()
        {
            if (returnTitleButton != null)
            {
                returnTitleButton.onClick.RemoveListener(HandleReturnTitleClicked);
            }

            _commandSubmitted = false;
        }

        protected override void Redraw()
        {
            bool visible = SessionView != null && SessionView.Paused;
            if (!visible)
            {
                _commandSubmitted = false;
            }

            SetActive(pauseRoot, visible);
            if (pauseText != null)
            {
                pauseText.text = visible ? pausedMessage : string.Empty;
            }

            if (returnTitleButton != null)
            {
                returnTitleButton.interactable = visible
                    && !_commandSubmitted
                    && SessionView != null
                    && !SessionView.IsBusy;
            }
        }

        private void HandleReturnTitleClicked()
        {
            if (_commandSubmitted
                || CommandSink == null
                || SessionView == null
                || !SessionView.Paused
                || SessionView.IsBusy)
            {
                return;
            }

            _commandSubmitted = true;
            returnTitleButton.interactable = false;
            CommandSink.ReturnTitle();
            Redraw();
        }
    }
}
