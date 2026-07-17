using UnityEngine;

namespace PetOffline
{
    public sealed partial class Day2Level
    {
        [SerializeField] private Transform[] quietEndingPath;

        private FinalChoice endingChoice;
        private float restoreProgress;
        private float quietEndingRemaining;
        private int quietSubtitleStage = -1;
        private bool quietEndingActive;
        private bool endingComplete;

        private void StartEnding()
        {
            robot.SetPaused(true);
            UI.SetProgress(0f, string.Empty);
            if (endingChoice == FinalChoice.RestoreConnection)
            {
                StartRestoreEnding();
            }
            else
            {
                StartQuietEnding();
            }
        }

        private void StartRestoreEnding()
        {
            SetFeederOffline(false);
            mainCameraSensor.SetOffline(false);
            mainCameraSensor.SetDetectionEnabled(true);
            backupCameraSensor.SetOffline(true);
            UI.SetObjective("Return to the feeder and hold Shift to confirm with the owner.");
            UI.ShowDialogue(
                "OWNER",
                "Connection restored. Stay beside the feeder while I verify it.");
            UI.SetCamera(CameraUiState.Scanning);
        }

        private void StartQuietEnding()
        {
            UI.SetObjective("Latte quietly checks the shoes and returns to bed...");
            UI.ShowDialogue("LATTE", "No alarms. Just the familiar route home.");
            UI.SetCamera(CameraUiState.Offline);
            if (quietEndingPath.Length == 0)
            {
                BeginQuietSubtitles();
                return;
            }

            Vector2[] path = new Vector2[quietEndingPath.Length];
            for (int index = 0; index < quietEndingPath.Length; index++)
            {
                path[index] = quietEndingPath[index].position;
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

            UI.SetProgress(
                restoreProgress / restoreConfirmationSeconds,
                $"Owner confirmation: {Mathf.Min(restoreProgress, restoreConfirmationSeconds):0.0} / {restoreConfirmationSeconds:0}s");
            if (restoreProgress >= restoreConfirmationSeconds)
            {
                FinishGame("RESTORED CONNECTION");
            }
        }

        private void BeginQuietSubtitles()
        {
            quietEndingActive = true;
            quietEndingRemaining = quietEndingSeconds;
            player.SetCutsceneLie(true);
            ShowQuietSubtitle(0);
        }

        private void UpdateQuietEnding()
        {
            if (!quietEndingActive)
            {
                return;
            }

            quietEndingRemaining -= Time.deltaTime;
            float elapsed = quietEndingSeconds - quietEndingRemaining;
            ShowQuietSubtitle(elapsed < 3f ? 0 : elapsed < 6f ? 1 : 2);
            UI.SetProgress(
                1f - quietEndingRemaining / quietEndingSeconds,
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
            UI.ShowDialogue("LATTE", text);
        }

        private void FinishGame(string endingText)
        {
            if (endingComplete)
            {
                return;
            }

            endingComplete = true;
            quietEndingActive = false;
            UI.SetProgress(1f, "Story complete.");
            UI.SetCamera(CameraUiState.Hidden);
            UI.ShowEnding(endingText);
        }
    }
}
