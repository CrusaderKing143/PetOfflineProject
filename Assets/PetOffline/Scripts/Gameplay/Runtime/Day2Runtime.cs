using PetOffline.Core;
using UnityEngine;

namespace PetOffline.Gameplay
{
    public sealed partial class Day2Runtime : MonoBehaviour, ILevelRuntime
    {
        [Header("Data")]
        [SerializeField] private Day2Config config;
        [SerializeField] private DialogueScript openingDialogue;

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
        [SerializeField] private Transform[] quietEndingPath;

        private Day2FlowState flow;
        private LevelViewModel viewModel;
        private LevelRuntimeContext context;
        private int dialogueIndex;
        private int taskMask;
        private float sunProgress;
        private float confirmationElapsed;
        private float slipAttemptRemaining;
        private bool confirmationAlert;
        private bool backupConfirmationWaiting;
        private bool slipAttemptActive;

        public LevelId Level => LevelId.Day2;
        public ILevelViewModel ViewModel => viewModel;

        private void Update()
        {
            if (context == null || context.Host.IsPaused)
            {
                return;
            }

            switch (flow.Phase)
            {
                case Day2Phase.FirstSun:
                    UpdateFirstSun();
                    break;
                case Day2Phase.Confirmation:
                    UpdateFirstConfirmation();
                    break;
                case Day2Phase.DisableCamera:
                    UpdateDisableCameraTask();
                    break;
                case Day2Phase.BackupLesson:
                    UpdateBackupLesson();
                    break;
                case Day2Phase.FinalSun:
                    UpdateFinalSun();
                    break;
                case Day2Phase.End:
                    UpdateEnding();
                    break;
            }
        }

        public void Bind(LevelRuntimeContext runtimeContext)
        {
            context = runtimeContext;
            flow = new Day2FlowState();
            viewModel = new LevelViewModel(LevelId.Day2, 1);
            bananaSlipZone.RobotSlipped += HandleRobotSlipped;
            robot.SlipFinished += HandleRobotSlipFinished;
            mainCameraSensor.Discovered += HandleMainCameraDiscovery;
            backupCameraSensor.Discovered += HandleBackupCameraDiscovery;
            mainCameraSensor.SetTarget(player.transform);
            backupCameraSensor.SetTarget(player.transform);
            ResetWorld();
            ShowOpeningLine();
        }

        public void Unbind()
        {
            bananaSlipZone.RobotSlipped -= HandleRobotSlipped;
            robot.SlipFinished -= HandleRobotSlipFinished;
            mainCameraSensor.Discovered -= HandleMainCameraDiscovery;
            backupCameraSensor.Discovered -= HandleBackupCameraDiscovery;
            mainCameraSensor.SetDetectionEnabled(false);
            backupCameraSensor.SetDetectionEnabled(false);
            robot.SetPaused(true);
            player.ApplyInput(LevelInputFrame.Empty, false);
            context = null;
        }

        public void HandleInput(LevelInputFrame input)
        {
            if (flow.Phase == Day2Phase.Start)
            {
                player.ApplyInput(LevelInputFrame.Empty, false);
                if (input.InteractPressed)
                {
                    AdvanceOpening();
                }

                return;
            }

            if (!AllowsPlayerControl())
            {
                if (flow.Phase != Day2Phase.End || endingChoice != FinalChoice.KeepQuiet)
                {
                    player.ApplyInput(LevelInputFrame.Empty, false);
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

        public bool TryHandleCommand(GameCommand command)
        {
            if (command.Type == GameCommandType.ContinueReport && flow.ContinueReport())
            {
                PresentChoice();
                return true;
            }

            if (command.Type != GameCommandType.SubmitChoice || !flow.SubmitChoice(command.Choice))
            {
                return false;
            }

            StartEnding(command.Choice);
            return true;
        }

        private void ResetWorld()
        {
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            banana.ResetHome();
            banana.SetAvailable(false);
            bananaPathGoal.ResetZone();
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
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetObjective("Press E to continue.");
            SetDialogueLine(dialogueIndex);
            viewModel.SetCamera(CameraUiState.Scanning);
        }

        private void AdvanceOpening()
        {
            dialogueIndex++;
            if (dialogueIndex < openingDialogue.Count)
            {
                SetDialogueLine(dialogueIndex);
                return;
            }

            flow.FinishStart();
            robot.SetPaused(false);
            PresentPhase();
        }

        private void SetDialogueLine(int index)
        {
            DialogueLine line = openingDialogue.GetLine(index);
            viewModel.SetDialogue(line.speaker, line.text);
        }

        private void HandleInteraction()
        {
            if (flow.Phase == Day2Phase.BackupLesson && backupConfirmationWaiting)
            {
                TryFinishBackupConfirmation();
                return;
            }

            if (flow.Phase == Day2Phase.DisableCamera)
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

            viewModel.SetProgress(
                sunProgress / config.FirstSunSeconds,
                $"Balcony sun: {Mathf.Min(sunProgress, config.FirstSunSeconds):0.0} / {config.FirstSunSeconds:0}s");
            if (sunProgress >= config.FirstSunSeconds && flow.ReachFirstConfirmation())
            {
                sunProgress = config.FirstSunSeconds;
                MarkTask(0);
                PresentPhase();
            }
        }

        private void UpdateFirstConfirmation()
        {
            confirmationElapsed += Time.deltaTime;
            if (!confirmationAlert && confirmationElapsed >= config.IgnoredConfirmationSeconds)
            {
                confirmationAlert = true;
                viewModel.SetCamera(CameraUiState.Alert);
                viewModel.SetDialogue("OWNER", "No response received. Return to the feeder before sunbathing again.");
            }

            if (!feederZone.ContainsPlayer || !flow.ReturnToFeeder())
            {
                return;
            }

            sunProgress = 0f;
            confirmationElapsed = 0f;
            confirmationAlert = false;
            PresentPhase();
        }

        private void UpdateDisableCameraTask()
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
            if (flow.Phase != Day2Phase.DisableCamera || slipAttemptActive)
            {
                return;
            }

            slipAttemptActive = true;
            slipAttemptRemaining = 5f;
            slippedRobot.SlipTo(feederImpact.position, 5.5f);
            viewModel.SetObjective("The robot is sliding toward the feeder!");
        }

        private void HandleRobotSlipFinished()
        {
            if (!slipAttemptActive || flow.Phase != Day2Phase.DisableCamera)
            {
                return;
            }

            float impactDistance = Vector2.Distance(robot.transform.position, feederImpact.position);
            if (impactDistance > 0.5f || !flow.DisableMainCamera())
            {
                ResetSlipAttempt("The impact missed. The banana and robot were reset.");
                return;
            }

            slipAttemptActive = false;
            mainCameraSensor.SetOffline(true);
            SetFeederOffline(true);
            banana.SetAvailable(false);
            MarkTask(1);
            PresentPhase();
        }

        private void ResetSlipAttempt(string message)
        {
            slipAttemptActive = false;
            player.DropHeldItem();
            banana.ResetHome();
            banana.SetAvailable(true);
            bananaPathGoal.ResetZone();
            bananaSlipZone.SetArmed(false);
            robot.ResetPatrol();
            viewModel.SetObjective(message);
        }

        private void UpdateBackupLesson()
        {
            if (!flow.BackupCameraActivated && sideDoorZone.ContainsPlayer)
            {
                flow.ActivateBackupCamera();
                backupCameraSensor.SetOffline(false);
                backupCameraSensor.SetDetectionEnabled(true);
                viewModel.SetObjective("Use the balcony sun zone while the backup camera watches.");
            }

            if (!flow.BackupCameraActivated || backupConfirmationWaiting)
            {
                return;
            }

            if (balconySunZone.ContainsPlayer && player.IsLying)
            {
                sunProgress += Time.deltaTime;
            }

            viewModel.SetProgress(
                sunProgress / config.BackupLessonSeconds,
                $"Backup confirmation: {Mathf.Min(sunProgress, config.BackupLessonSeconds):0.0} / {config.BackupLessonSeconds:0}s");
            if (sunProgress >= config.BackupLessonSeconds && flow.StartBackupConfirmation())
            {
                backupConfirmationWaiting = true;
                viewModel.SetMode(LevelUiMode.Dialogue);
                viewModel.SetDialogue("OWNER", "Backup check received. Return to the feeder and press E to confirm.");
            }
        }

        private void TryFinishBackupConfirmation()
        {
            if (!feederZone.ContainsPlayer || !flow.CompleteBackupLesson())
            {
                viewModel.SetDialogue("OWNER", "I need you beside the feeder before you confirm.");
                return;
            }

            backupConfirmationWaiting = false;
            sunProgress = 0f;
            MarkTask(2);
            PresentPhase();
        }

        private void UpdateFinalSun()
        {
            if (livingRoomSunZone.ContainsPlayer && player.IsLying)
            {
                sunProgress += Time.deltaTime;
            }

            viewModel.SetProgress(
                sunProgress / config.FinalSunSeconds,
                $"Living-room sun: {Mathf.Min(sunProgress, config.FinalSunSeconds):0.0} / {config.FinalSunSeconds:0}s");
            if (sunProgress >= config.FinalSunSeconds && flow.CompleteFinalSun())
            {
                MarkTask(3);
                PresentPhase();
            }
        }

        private void MarkTask(int index)
        {
            int bit = 1 << index;
            if ((taskMask & bit) != 0)
            {
                return;
            }

            taskMask |= bit;
            if (index == 3)
            {
                viewModel.SetTasks(1, 1);
            }
        }

        private void SetFeederOffline(bool offline)
        {
            if (feederVisual != null)
            {
                feederVisual.color = offline
                    ? new Color(0.42f, 0.48f, 0.55f, 1f)
                    : Color.white;
            }
        }

        private void HandleMainCameraDiscovery()
        {
            if (flow.Phase != Day2Phase.Report && flow.Phase != Day2Phase.Choice)
            {
                viewModel.SetCamera(CameraUiState.Alert);
            }
        }

        private void HandleBackupCameraDiscovery()
        {
            if (flow.Phase == Day2Phase.BackupLesson || flow.Phase == Day2Phase.FinalSun)
            {
                viewModel.SetCamera(CameraUiState.Alert);
            }
        }

        private bool AllowsPlayerControl()
        {
            return flow.Phase == Day2Phase.FirstSun
                || flow.Phase == Day2Phase.Confirmation
                || flow.Phase == Day2Phase.DisableCamera
                || flow.Phase == Day2Phase.BackupLesson
                || flow.Phase == Day2Phase.FinalSun
                || (flow.Phase == Day2Phase.End
                    && endingChoice == FinalChoice.RestoreConnection
                    && !endingComplete);
        }

        private void PresentPhase()
        {
            viewModel.SetDialogue(string.Empty, string.Empty);
            viewModel.SetProgress(0f, string.Empty);
            switch (flow.Phase)
            {
                case Day2Phase.FirstSun:
                    PresentGameplay("Hold Shift in the balcony sun zone for 10 seconds.", CameraUiState.Scanning);
                    break;
                case Day2Phase.Confirmation:
                    viewModel.SetMode(LevelUiMode.Dialogue);
                    viewModel.SetObjective("Sun progress is paused. Return to the feeder.");
                    viewModel.SetDialogue("OWNER", "Latte, confirm that you are safe. Come back to the feeder.");
                    viewModel.SetProgress(1f, "Awaiting owner confirmation at the feeder.");
                    break;
                case Day2Phase.DisableCamera:
                    banana.SetAvailable(true);
                    PresentGameplay("Place the banana on the robot's route to disable the main camera.", CameraUiState.Alert);
                    break;
                case Day2Phase.BackupLesson:
                    sunProgress = 0f;
                    PresentGameplay("Pass through the side door to activate the backup camera.", CameraUiState.Offline);
                    break;
                case Day2Phase.FinalSun:
                    sunProgress = 0f;
                    PresentGameplay("Use only the living-room safe route and hold Shift for 20 seconds.", CameraUiState.Safe);
                    break;
                case Day2Phase.Report:
                    PresentReport();
                    break;
            }
        }

        private void PresentGameplay(string objective, CameraUiState cameraState)
        {
            viewModel.SetMode(LevelUiMode.Gameplay);
            viewModel.SetObjective(objective);
            viewModel.SetCamera(cameraState);
        }

        private void PresentReport()
        {
            robot.SetPaused(true);
            mainCameraSensor.SetDetectionEnabled(false);
            backupCameraSensor.SetDetectionEnabled(false);
            viewModel.SetReport("day2");
            viewModel.SetCamera(CameraUiState.Hidden);
            viewModel.SetMode(LevelUiMode.Report);
        }
    }
}
