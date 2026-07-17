using PetOffline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class HudPresenter : SessionPresenterBase
    {
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private Image missionBackground;
        [SerializeField] private Sprite day1MissionSprite;
        [SerializeField] private Sprite day2MissionSprite;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;
        [SerializeField] private Text taskCountText;
        [SerializeField] private GameObject[] taskCheckmarks;

        [Header("Camera State")]
        [SerializeField] private GameObject cameraStateRoot;
        [SerializeField] private Text cameraStateText;
        [SerializeField] private Color safeColor = new Color(0.35f, 0.8f, 0.45f);
        [SerializeField] private Color scanningColor = new Color(1f, 0.82f, 0.25f);
        [SerializeField] private Color alertColor = new Color(0.95f, 0.25f, 0.2f);
        [SerializeField] private Color offlineColor = new Color(0.55f, 0.55f, 0.55f);

        protected override void Redraw()
        {
            ILevelViewModel viewModel = ViewModel;
            bool visible = viewModel != null && IsWorldOverlayMode(viewModel.UiMode);
            SetActive(hudRoot, visible);
            if (!visible)
            {
                return;
            }

            DrawObjective(viewModel);
            DrawProgress(viewModel);
            DrawTasks(viewModel);
            DrawCameraState(viewModel.CameraState);
        }

        private static bool IsWorldOverlayMode(LevelUiMode mode)
        {
            return mode == LevelUiMode.Gameplay || mode == LevelUiMode.Dialogue;
        }

        private void DrawObjective(ILevelViewModel viewModel)
        {
            if (missionBackground != null)
            {
                missionBackground.sprite = viewModel.Level == LevelId.Day2
                    ? day2MissionSprite
                    : day1MissionSprite;
            }

            if (objectiveText != null)
            {
                objectiveText.text = viewModel.Objective ?? string.Empty;
            }
        }

        private void DrawProgress(ILevelViewModel viewModel)
        {
            float progress = Mathf.Clamp01(viewModel.Progress01);
            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
            }

            if (progressText != null)
            {
                progressText.text = string.IsNullOrEmpty(viewModel.ProgressLabel)
                    ? Mathf.RoundToInt(progress * 100f) + "%"
                    : viewModel.ProgressLabel;
            }
        }

        private void DrawTasks(ILevelViewModel viewModel)
        {
            if (taskCountText != null)
            {
                taskCountText.text = viewModel.CompletedTasks + "/" + viewModel.TotalTasks;
            }

            if (taskCheckmarks == null)
            {
                return;
            }

            for (int index = 0; index < taskCheckmarks.Length; index++)
            {
                bool isComplete = index < 32 && (viewModel.TaskMask & (1 << index)) != 0;
                SetActive(taskCheckmarks[index], isComplete);
            }
        }

        private void DrawCameraState(CameraUiState state)
        {
            bool visible = state != CameraUiState.Hidden;
            SetActive(cameraStateRoot, visible);
            if (cameraStateText == null)
            {
                return;
            }

            cameraStateText.text = GetCameraStateLabel(state);
            cameraStateText.color = GetCameraStateColor(state);
        }

        private static string GetCameraStateLabel(CameraUiState state)
        {
            switch (state)
            {
                case CameraUiState.Safe:
                    return "SAFE";
                case CameraUiState.Scanning:
                    return "SCANNING";
                case CameraUiState.Alert:
                    return "ALERT";
                case CameraUiState.Offline:
                    return "OFFLINE";
                default:
                    return string.Empty;
            }
        }

        private Color GetCameraStateColor(CameraUiState state)
        {
            switch (state)
            {
                case CameraUiState.Safe:
                    return safeColor;
                case CameraUiState.Alert:
                    return alertColor;
                case CameraUiState.Offline:
                    return offlineColor;
                default:
                    return scanningColor;
            }
        }
    }
}
