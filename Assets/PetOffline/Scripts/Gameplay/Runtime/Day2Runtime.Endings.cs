using PetOffline.Core;
using UnityEngine;

namespace PetOffline.Gameplay
{
    public sealed partial class Day2Runtime
    {
        private FinalChoice endingChoice;
        private float restoreProgress;
        private float quietEndingRemaining;
        private int quietSubtitleStage = -1;
        private bool quietEndingActive;
        private bool endingComplete;

        private void PresentChoice()
        {
            viewModel.SetMode(LevelUiMode.Choice);
            viewModel.SetObjective("Choose what Latte should do with the home connection.");
            viewModel.SetCamera(CameraUiState.Hidden);
            viewModel.SetProgress(0f, string.Empty);
        }

        private void StartEnding(FinalChoice choice)
        {
            endingChoice = choice;
            robot.SetPaused(true);
            viewModel.SetProgress(0f, string.Empty);
            if (choice == FinalChoice.RestoreConnection)
            {
                StartRestoreEnding();
                return;
            }

            StartQuietEnding();
        }

        private void StartRestoreEnding()
        {
            SetFeederOffline(false);
            mainCameraSensor.SetOffline(false);
            mainCameraSensor.SetDetectionEnabled(true);
            backupCameraSensor.SetOffline(true);
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetObjective("Return to the feeder and hold Shift to confirm with the owner.");
            viewModel.SetDialogue("OWNER", "Connection restored. Stay beside the feeder while I verify it.");
            viewModel.SetCamera(CameraUiState.Scanning);
        }

        private void StartQuietEnding()
        {
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetObjective("Latte quietly checks the shoes and returns to bed...");
            viewModel.SetDialogue("LATTE", "No alarms. Just the familiar route home.");
            viewModel.SetCamera(CameraUiState.Offline);
            Vector2[] path = BuildQuietPath();
            if (path.Length == 0)
            {
                BeginQuietSubtitles();
                return;
            }

            player.BeginAutoMove(path, BeginQuietSubtitles);
        }

        private void UpdateEnding()
        {
            if (endingChoice == FinalChoice.RestoreConnection)
            {
                UpdateRestoreEnding();
            }
            else if (endingChoice == FinalChoice.KeepQuiet)
            {
                UpdateQuietEnding();
            }
        }

        private void UpdateRestoreEnding()
        {
            if (feederZone.ContainsPlayer && player.IsLying)
            {
                restoreProgress += Time.deltaTime;
            }

            viewModel.SetProgress(
                restoreProgress / config.RestoreConfirmationSeconds,
                $"Owner confirmation: {Mathf.Min(restoreProgress, config.RestoreConfirmationSeconds):0.0} / {config.RestoreConfirmationSeconds:0}s");
            if (restoreProgress >= config.RestoreConfirmationSeconds)
            {
                FinishGame("RESTORED CONNECTION");
            }
        }

        private void BeginQuietSubtitles()
        {
            quietEndingActive = true;
            quietEndingRemaining = config.QuietEndingSeconds;
            player.SetCutsceneLie(true);
            viewModel.SetMode(LevelUiMode.Dialogue);
            ShowQuietSubtitle(0);
        }

        private void UpdateQuietEnding()
        {
            if (!quietEndingActive)
            {
                return;
            }

            quietEndingRemaining -= Time.deltaTime;
            float elapsed = config.QuietEndingSeconds - quietEndingRemaining;
            int stage = elapsed < 3f ? 0 : elapsed < 6f ? 1 : 2;
            ShowQuietSubtitle(stage);
            viewModel.SetProgress(
                1f - quietEndingRemaining / config.QuietEndingSeconds,
                "A quiet home, for one more night.");
            if (quietEndingRemaining <= 0f)
            {
                FinishGame("KEPT QUIET");
            }
        }

        private void ShowQuietSubtitle(int stage)
        {
            if (stage == quietSubtitleStage)
            {
                return;
            }

            quietSubtitleStage = stage;
            string text = stage == 0
                ? "Latte checks the familiar scent on the shoes."
                : stage == 1
                    ? "No camera turns. No feeder light answers."
                    : "Latte curls up and lets the apartment stay quiet.";
            viewModel.SetDialogue("LATTE", text);
        }

        private Vector2[] BuildQuietPath()
        {
            Vector2[] path = new Vector2[quietEndingPath.Length];
            for (int i = 0; i < quietEndingPath.Length; i++)
            {
                path[i] = quietEndingPath[i].position;
            }

            return path;
        }

        private void FinishGame(string endingId)
        {
            if (endingComplete)
            {
                return;
            }

            endingComplete = true;
            quietEndingActive = false;
            viewModel.SetEnding(endingId);
            viewModel.SetProgress(1f, "Story complete.");
            viewModel.SetMode(LevelUiMode.Ending);
            viewModel.SetCamera(CameraUiState.Hidden);
            context.Host.CompleteLevel(LevelId.Day2, endingChoice);
        }
    }
}
