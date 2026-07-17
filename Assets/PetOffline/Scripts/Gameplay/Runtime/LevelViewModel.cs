using System;
using PetOffline.Core;

namespace PetOffline.Gameplay
{
    public sealed class LevelViewModel : ILevelViewModel
    {
        public LevelViewModel(LevelId level, int totalTasks)
        {
            Level = level;
            TotalTasks = totalTasks;
            UiMode = LevelUiMode.Dialogue;
            CameraState = CameraUiState.Hidden;
            Objective = string.Empty;
            ProgressLabel = string.Empty;
            DialogueSpeaker = string.Empty;
            DialogueText = string.Empty;
            ReportId = string.Empty;
            EndingId = string.Empty;
        }

        public LevelId Level { get; }
        public LevelUiMode UiMode { get; private set; }
        public string Objective { get; private set; }
        public float Progress01 { get; private set; }
        public string ProgressLabel { get; private set; }
        public int CompletedTasks { get; private set; }
        public int TotalTasks { get; }
        public int TaskMask { get; private set; }
        public CameraUiState CameraState { get; private set; }
        public string DialogueSpeaker { get; private set; }
        public string DialogueText { get; private set; }
        public string ReportId { get; private set; }
        public string EndingId { get; private set; }
        public event Action Changed;

        public void SetMode(LevelUiMode mode)
        {
            if (UiMode == mode)
            {
                return;
            }

            UiMode = mode;
            RaiseChanged();
        }

        public void SetObjective(string objective)
        {
            objective = objective ?? string.Empty;
            if (Objective == objective)
            {
                return;
            }

            Objective = objective;
            RaiseChanged();
        }

        public void SetProgress(float value, string label)
        {
            value = UnityEngine.Mathf.Clamp01(value);
            label = label ?? string.Empty;
            if (UnityEngine.Mathf.Approximately(Progress01, value) && ProgressLabel == label)
            {
                return;
            }

            Progress01 = value;
            ProgressLabel = label;
            RaiseChanged();
        }

        public void SetTasks(int completed, int mask)
        {
            if (CompletedTasks == completed && TaskMask == mask)
            {
                return;
            }

            CompletedTasks = completed;
            TaskMask = mask;
            RaiseChanged();
        }

        public void SetCamera(CameraUiState state)
        {
            if (CameraState == state)
            {
                return;
            }

            CameraState = state;
            RaiseChanged();
        }

        public void SetDialogue(string speaker, string text)
        {
            speaker = speaker ?? string.Empty;
            text = text ?? string.Empty;
            if (DialogueSpeaker == speaker && DialogueText == text)
            {
                return;
            }

            DialogueSpeaker = speaker;
            DialogueText = text;
            RaiseChanged();
        }

        public void SetReport(string reportId)
        {
            reportId = reportId ?? string.Empty;
            if (ReportId == reportId)
            {
                return;
            }

            ReportId = reportId;
            RaiseChanged();
        }

        public void SetEnding(string endingId)
        {
            endingId = endingId ?? string.Empty;
            if (EndingId == endingId)
            {
                return;
            }

            EndingId = endingId;
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
