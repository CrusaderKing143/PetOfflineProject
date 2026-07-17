using System;
using PetOffline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class ReportPresenter : SessionPresenterBase
    {
        [SerializeField] private GameObject reportRoot;
        [SerializeField] private Image reportImage;
        [SerializeField] private Button continueButton;

        [Header("Serialized Report Art")]
        [SerializeField] private Sprite day1Sprite;
        [SerializeField] private Sprite day2Sprite;
        [SerializeField] private string day1ReportId = "day1";
        [SerializeField] private string day2ReportId = "day2";

        private bool _commandSubmitted;

        protected override void AddButtonListeners()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        protected override void RemoveButtonListeners()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }

            _commandSubmitted = false;
        }

        protected override void Redraw()
        {
            ILevelViewModel viewModel = ViewModel;
            bool visible = viewModel != null && viewModel.UiMode == LevelUiMode.Report;
            if (!visible)
            {
                _commandSubmitted = false;
            }

            SetActive(reportRoot, visible);
            if (reportImage != null)
            {
                reportImage.sprite = visible ? SelectSprite(viewModel) : null;
            }

            if (continueButton != null)
            {
                continueButton.interactable = visible
                    && SessionView != null
                    && !SessionView.IsBusy
                    && !_commandSubmitted;
            }
        }

        private Sprite SelectSprite(ILevelViewModel viewModel)
        {
            if (MatchesReportId(viewModel.ReportId, day2ReportId))
            {
                return day2Sprite;
            }

            if (MatchesReportId(viewModel.ReportId, day1ReportId))
            {
                return day1Sprite;
            }

            return viewModel.Level == LevelId.Day2 ? day2Sprite : day1Sprite;
        }

        private static bool MatchesReportId(string reportId, string expectedId)
        {
            return !string.IsNullOrEmpty(reportId)
                && !string.IsNullOrEmpty(expectedId)
                && reportId.IndexOf(expectedId, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void HandleContinueClicked()
        {
            if (!CanSubmit())
            {
                return;
            }

            _commandSubmitted = true;
            continueButton.interactable = false;
            CommandSink.ContinueReport();
            Redraw();
        }

        private bool CanSubmit()
        {
            return !_commandSubmitted
                && CommandSink != null
                && SessionView != null
                && !SessionView.IsBusy
                && ViewModel != null
                && ViewModel.UiMode == LevelUiMode.Report;
        }
    }
}
