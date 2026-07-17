using UnityEngine;

namespace PetOffline
{
    public sealed partial class Day2Level : LevelController
    {
        private enum Phase
        {
            Opening,
            FirstSun,
            Confirmation,
            DisableCamera,
            BackupLesson,
            FinalSun,
            Report,
            Choice,
            Ending
        }

        [Header("Tuning")]
        [SerializeField, Min(0.1f)] private float firstSunSeconds = 10f;
        [SerializeField, Min(0.1f)] private float ignoredConfirmationSeconds = 9f;
        [SerializeField, Min(0.1f)] private float backupLessonSeconds = 10f;
        [SerializeField, Min(0.1f)] private float finalSunSeconds = 20f;
        [SerializeField, Min(0.1f)] private float restoreConfirmationSeconds = 4f;
        [SerializeField, Min(0.1f)] private float quietEndingSeconds = 9f;

        [Header("World")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Carryable banana;
        [SerializeField] private TriggerZone balconySunZone;
        [SerializeField] private TriggerZone livingRoomSunZone;
        [SerializeField] private TriggerZone feederZone;
        [SerializeField] private TriggerZone sideDoorZone;
        [SerializeField] private GoalZone bananaPathGoal;
        [SerializeField] private BananaSlipZone bananaSlipZone;
        [SerializeField] private CameraSensor mainCameraSensor;
        [SerializeField] private CameraSensor backupCameraSensor;
        [SerializeField] private RobotPatrol robot;
        [SerializeField] private SpriteRenderer feederVisual;
        [SerializeField] private Transform feederImpact;
        [SerializeField] private Transform playerSpawn;

        private Phase phase;
        private int dialogueIndex;
        private float sunProgress;
        private float confirmationElapsed;
        private float slipAttemptRemaining;
        private bool confirmationAlert;
        private bool backupCameraActivated;
        private bool backupConfirmationWaiting;
        private bool slipAttemptActive;

        public override bool CanPause => phase == Phase.FirstSun
            || phase == Phase.DisableCamera
            || (phase == Phase.BackupLesson && !backupConfirmationWaiting)
            || phase == Phase.FinalSun;

        private void Update()
        {
            if (!IsReady || Session.IsPaused)
            {
                return;
            }

            switch (phase)
            {
                case Phase.FirstSun: UpdateFirstSun(); break;
                case Phase.Confirmation: UpdateFirstConfirmation(); break;
                case Phase.DisableCamera: UpdateDisableCamera(); break;
                case Phase.BackupLesson: UpdateBackupLesson(); break;
                case Phase.FinalSun: UpdateFinalSun(); break;
                case Phase.Ending: UpdateEnding(); break;
            }
        }

        protected override void Begin()
        {
            phase = Phase.Opening;
            dialogueIndex = 0;
            bananaSlipZone.RobotSlipped += HandleRobotSlipped;
            robot.SlipFinished += HandleRobotSlipFinished;
            mainCameraSensor.Discovered += HandleMainCameraDiscovery;
            backupCameraSensor.Discovered += HandleBackupCameraDiscovery;
            ResetWorld();
            UI.BeginLevel(true, 1);
            ShowOpeningLine();
        }

        public override void HandleInput(PlayerInput input)
        {
            if (phase == Phase.Opening)
            {
                player.ApplyInput(default, false);
                if (input.InteractPressed)
                {
                    AdvanceOpening();
                }

                return;
            }

            if (!AllowsPlayerControl())
            {
                if (phase != Phase.Ending || endingChoice != FinalChoice.KeepQuiet)
                {
                    player.ApplyInput(default, false);
                }

                return;
            }

            player.ApplyInput(input, true);
            if (input.InteractPressed)
            {
                HandleInteraction();
            }

            if (input.BarkPressed)
            {
                player.Bark();
            }
        }

        public override void ContinueReport()
        {
            if (phase != Phase.Report)
            {
                return;
            }

            phase = Phase.Choice;
            UI.SetObjective("Choose what Latte should do with the home connection.");
            UI.SetCamera(CameraUiState.Hidden);
            UI.SetProgress(0f, string.Empty);
            UI.ShowChoice();
        }

        public override void SubmitChoice(FinalChoice choice)
        {
            if (phase != Phase.Choice || choice == FinalChoice.None)
            {
                return;
            }

            phase = Phase.Ending;
            endingChoice = choice;
            StartEnding();
        }

        private void ResetWorld()
        {
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            banana.ResetHome();
            banana.SetAvailable(false);
            bananaSlipZone.SetArmed(false);
            robot.ResetPatrol();
            robot.SetPaused(true);
            SetFeederOffline(false);
            mainCameraSensor.SetOffline(false);
            mainCameraSensor.ResetSensor();
            mainCameraSensor.SetDetectionEnabled(true);
            backupCameraSensor.ResetSensor();
            backupCameraSensor.SetDetectionEnabled(false);
            backupCameraSensor.SetOffline(true);
        }

        private void ShowOpeningLine()
        {
            switch (dialogueIndex)
            {
                case 0:
                    UI.ShowDialogue("OWNER", "Good morning, Latte. Stay where the cameras can see you.");
                    break;
                case 1:
                    UI.ShowDialogue("LATTE", "The balcony is warm, but every quiet moment asks for confirmation.");
                    break;
                default:
                    UI.ShowDialogue("OWNER", "Come back to the feeder whenever I call.");
                    break;
            }

            UI.SetObjective("Press E to continue.");
            UI.SetCamera(CameraUiState.Scanning);
        }

        private void AdvanceOpening()
        {
            dialogueIndex++;
            if (dialogueIndex < 3)
            {
                ShowOpeningLine();
                return;
            }

            phase = Phase.FirstSun;
            robot.SetPaused(false);
            PresentPhase();
        }

        private void HandleInteraction()
        {
            if (phase == Phase.BackupLesson && backupConfirmationWaiting)
            {
                FinishBackupConfirmation();
            }
            else if (phase == Phase.DisableCamera)
            {
                player.TryToggleCarry();
            }
        }

        private void UpdateFirstSun()
        {
            if (balconySunZone.ContainsPlayer && player.IsLying)
            {
                sunProgress += Time.deltaTime;
            }

            UI.SetProgress(
                sunProgress / firstSunSeconds,
                $"Balcony sun: {Mathf.Min(sunProgress, firstSunSeconds):0.0} / {firstSunSeconds:0}s");
            if (sunProgress >= firstSunSeconds)
            {
                sunProgress = firstSunSeconds;
                phase = Phase.Confirmation;
                PresentPhase();
            }
        }

        private void UpdateFirstConfirmation()
        {
            confirmationElapsed += Time.deltaTime;
            if (!confirmationAlert && confirmationElapsed >= ignoredConfirmationSeconds)
            {
                confirmationAlert = true;
                UI.SetCamera(CameraUiState.Alert);
                UI.ShowDialogue(
                    "OWNER",
                    "No response received. Return to the feeder before sunbathing again.");
            }

            if (!feederZone.ContainsPlayer)
            {
                return;
            }

            phase = Phase.DisableCamera;
            sunProgress = 0f;
            confirmationElapsed = 0f;
            confirmationAlert = false;
            PresentPhase();
        }

        private void UpdateDisableCamera()
        {
            bool bananaPlaced = bananaPathGoal.Contains(banana) && !banana.IsHeld;
            bananaSlipZone.SetArmed(bananaPlaced && !slipAttemptActive);
            if (!slipAttemptActive)
            {
                return;
            }

            slipAttemptRemaining -= Time.deltaTime;
            if (slipAttemptRemaining <= 0f)
            {
                ResetSlipAttempt("The robot missed. Place the banana on its route and try again.");
            }
        }

        private void HandleRobotSlipped(RobotPatrol slippedRobot)
        {
            if (phase != Phase.DisableCamera || slipAttemptActive)
            {
                return;
            }

            slipAttemptActive = true;
            slipAttemptRemaining = 5f;
            slippedRobot.SlipTo(feederImpact.position, 5.5f);
            UI.SetObjective("The robot is sliding toward the feeder!");
        }

        private void HandleRobotSlipFinished()
        {
            if (!slipAttemptActive || phase != Phase.DisableCamera)
            {
                return;
            }

            if (Vector2.Distance(robot.transform.position, feederImpact.position) > 0.5f)
            {
                ResetSlipAttempt("The impact missed. The banana and robot were reset.");
                return;
            }

            slipAttemptActive = false;
            phase = Phase.BackupLesson;
            mainCameraSensor.SetOffline(true);
            SetFeederOffline(true);
            banana.SetAvailable(false);
            PresentPhase();
        }

        private void ResetSlipAttempt(string message)
        {
            slipAttemptActive = false;
            player.DropHeldItem();
            banana.ResetHome();
            banana.SetAvailable(true);
            bananaSlipZone.SetArmed(false);
            robot.ResetPatrol();
            UI.SetObjective(message);
        }

        private void UpdateBackupLesson()
        {
            if (!backupCameraActivated && sideDoorZone.ContainsPlayer)
            {
                backupCameraActivated = true;
                backupCameraSensor.SetOffline(false);
                backupCameraSensor.SetDetectionEnabled(true);
                UI.SetObjective("Use the balcony sun zone while the backup camera watches.");
            }

            if (!backupCameraActivated || backupConfirmationWaiting)
            {
                return;
            }

            if (balconySunZone.ContainsPlayer && player.IsLying)
            {
                sunProgress += Time.deltaTime;
            }

            UI.SetProgress(
                sunProgress / backupLessonSeconds,
                $"Backup confirmation: {Mathf.Min(sunProgress, backupLessonSeconds):0.0} / {backupLessonSeconds:0}s");
            if (sunProgress >= backupLessonSeconds)
            {
                backupConfirmationWaiting = true;
                UI.ShowDialogue(
                    "OWNER",
                    "Backup check received. Return to the feeder and press E to confirm.");
            }
        }

        private void FinishBackupConfirmation()
        {
            if (!feederZone.ContainsPlayer)
            {
                UI.ShowDialogue("OWNER", "I need you beside the feeder before you confirm.");
                return;
            }

            backupConfirmationWaiting = false;
            sunProgress = 0f;
            phase = Phase.FinalSun;
            PresentPhase();
        }

        private void UpdateFinalSun()
        {
            if (livingRoomSunZone.ContainsPlayer && player.IsLying)
            {
                sunProgress += Time.deltaTime;
            }

            UI.SetProgress(
                sunProgress / finalSunSeconds,
                $"Living-room sun: {Mathf.Min(sunProgress, finalSunSeconds):0.0} / {finalSunSeconds:0}s");
            if (sunProgress >= finalSunSeconds)
            {
                phase = Phase.Report;
                UI.SetTasks(1, 1);
                PresentPhase();
            }
        }

        private void PresentPhase()
        {
            UI.SetProgress(0f, string.Empty);
            switch (phase)
            {
                case Phase.FirstSun:
                    ShowGameplay("Hold Shift in the balcony sun zone for 10 seconds.", CameraUiState.Scanning);
                    break;
                case Phase.Confirmation:
                    UI.SetObjective("Sun progress is paused. Return to the feeder.");
                    UI.SetProgress(1f, "Awaiting owner confirmation at the feeder.");
                    UI.ShowDialogue("OWNER", "Latte, confirm that you are safe. Come back to the feeder.");
                    break;
                case Phase.DisableCamera:
                    banana.SetAvailable(true);
                    ShowGameplay("Place the banana on the robot's route to disable the main camera.", CameraUiState.Alert);
                    break;
                case Phase.BackupLesson:
                    sunProgress = 0f;
                    ShowGameplay("Pass through the side door to activate the backup camera.", CameraUiState.Offline);
                    break;
                case Phase.FinalSun:
                    sunProgress = 0f;
                    ShowGameplay("Use only the living-room safe route and hold Shift for 20 seconds.", CameraUiState.Safe);
                    break;
                case Phase.Report:
                    robot.SetPaused(true);
                    mainCameraSensor.SetDetectionEnabled(false);
                    backupCameraSensor.SetDetectionEnabled(false);
                    UI.SetCamera(CameraUiState.Hidden);
                    UI.ShowReport(true);
                    break;
            }
        }

        private void ShowGameplay(string objective, CameraUiState cameraState)
        {
            UI.ShowWorld();
            UI.SetObjective(objective);
            UI.SetCamera(cameraState);
        }

        private void SetFeederOffline(bool offline)
        {
            feederVisual.color = offline
                ? new Color(0.42f, 0.48f, 0.55f, 1f)
                : Color.white;
        }

        private void HandleMainCameraDiscovery()
        {
            if (phase != Phase.Report && phase != Phase.Choice)
            {
                UI.SetCamera(CameraUiState.Alert);
            }
        }

        private void HandleBackupCameraDiscovery()
        {
            if (phase == Phase.BackupLesson || phase == Phase.FinalSun)
            {
                UI.SetCamera(CameraUiState.Alert);
            }
        }

        private bool AllowsPlayerControl()
        {
            return phase == Phase.FirstSun
                || phase == Phase.Confirmation
                || phase == Phase.DisableCamera
                || phase == Phase.BackupLesson
                || phase == Phase.FinalSun
                || (phase == Phase.Ending
                    && endingChoice == FinalChoice.RestoreConnection
                    && !endingComplete);
        }

    }
}
