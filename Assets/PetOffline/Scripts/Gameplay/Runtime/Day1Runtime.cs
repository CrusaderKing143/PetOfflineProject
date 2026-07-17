using PetOffline.Core;
using UnityEngine;

namespace PetOffline.Gameplay
{
    public sealed class Day1Runtime : MonoBehaviour, ILevelRuntime
    {
        [Header("Data")]
        [SerializeField] private Day1Config config;
        [SerializeField] private DialogueScript openingDialogue;

        [Header("World")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Carryable shoes;
        [SerializeField] private Carryable pillow;
        [SerializeField] private GoalZone shoesGoal;
        [SerializeField] private GoalZone pillowGoal;
        [SerializeField] private TriggerZone dogBed;
        [SerializeField] private CameraSensor cameraB;
        [SerializeField] private RobotPatrol robot;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform[] endingPath;

        private Day1FlowState flow;
        private LevelViewModel viewModel;
        private LevelRuntimeContext context;
        private int dialogueIndex;
        private float goalHoldSeconds;
        private float bossTimer;
        private float bossResponseRemaining;
        private float safeRemaining;
        private float alertRemaining;
        private float failureMessageRemaining;
        private bool bossCallActive;

        public LevelId Level => LevelId.Day1;
        public ILevelViewModel ViewModel => viewModel;

        private void Update()
        {
            if (context == null || context.Host.IsPaused)
            {
                return;
            }

            UpdateTemporaryTimers();
            if (!bossCallActive)
            {
                UpdateCurrentTask();
            }

            UpdateBossCall();
            UpdateCameraState();
        }

        public void Bind(LevelRuntimeContext runtimeContext)
        {
            context = runtimeContext;
            flow = new Day1FlowState();
            viewModel = new LevelViewModel(LevelId.Day1, 2);
            bossTimer = config.FirstBossCallSeconds;
            cameraB.Discovered += HandleCameraDiscovery;
            cameraB.SetTarget(player.transform);
            ResetWorld();
            ShowOpeningLine();
        }

        public void Unbind()
        {
            cameraB.Discovered -= HandleCameraDiscovery;
            cameraB.SetDetectionEnabled(false);
            robot.SetPaused(true);
            player.ApplyInput(LevelInputFrame.Empty, false);
            context = null;
        }

        public void HandleInput(LevelInputFrame input)
        {
            if (flow.Phase == Day1Phase.Opening)
            {
                player.ApplyInput(LevelInputFrame.Empty, false);
                if (input.InteractPressed)
                {
                    AdvanceOpening();
                }

                return;
            }

            if (!IsInteractivePhase())
            {
                player.ApplyInput(LevelInputFrame.Empty, false);
                return;
            }

            player.ApplyInput(input, true);
            if (input.InteractPressed)
            {
                player.TryToggleCarry();
            }

            if (input.BarkPressed)
            {
                player.Bark();
                HandleBark();
            }
        }

        public bool TryHandleCommand(GameCommand command)
        {
            if (command.Type != GameCommandType.ContinueReport || !flow.ContinueReport())
            {
                return false;
            }

            StartEndingPerformance();
            return true;
        }

        private void ResetWorld()
        {
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            shoes.ResetHome();
            pillow.ResetHome();
            shoes.SetAvailable(true);
            pillow.SetAvailable(false);
            shoesGoal.ResetZone();
            pillowGoal.ResetZone();
            cameraB.ResetSensor();
            robot.ResetPatrol();
            robot.SetPaused(true);
        }

        private void ShowOpeningLine()
        {
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetObjective("Press E to continue.");
            SetDialogueLine(dialogueIndex);
            viewModel.SetCamera(CameraUiState.Hidden);
        }

        private void AdvanceOpening()
        {
            dialogueIndex++;
            if (dialogueIndex < openingDialogue.Count)
            {
                SetDialogueLine(dialogueIndex);
                return;
            }

            flow.FinishOpening();
            PresentCurrentPhase();
        }

        private void SetDialogueLine(int index)
        {
            DialogueLine line = openingDialogue.GetLine(index);
            viewModel.SetDialogue(line.speaker, line.text);
        }

        private void UpdateCurrentTask()
        {
            if (flow.Phase == Day1Phase.Shoes)
            {
                UpdateShoesTask();
            }
            else if (flow.Phase == Day1Phase.Pillow && pillowGoal.Contains(pillow) && !pillow.IsHeld)
            {
                CompletePillowTask();
            }
        }

        private void UpdateShoesTask()
        {
            if (!shoesGoal.Contains(shoes) || shoes.IsHeld)
            {
                goalHoldSeconds = 0f;
                viewModel.SetProgress(0f, "Hold the shoes in Camera A's zone for 2 seconds.");
                return;
            }

            goalHoldSeconds += Time.deltaTime;
            float progress = goalHoldSeconds / config.ShoesHoldSeconds;
            viewModel.SetProgress(progress, $"Camera A hold: {Mathf.Min(goalHoldSeconds, config.ShoesHoldSeconds):0.0}s");
            if (goalHoldSeconds >= config.ShoesHoldSeconds)
            {
                CompleteShoesTask();
            }
        }

        private void CompleteShoesTask()
        {
            if (!flow.CompleteShoes())
            {
                return;
            }

            shoes.SetAvailable(false, true);
            pillow.SetAvailable(true);
            goalHoldSeconds = 0f;
            PresentCurrentPhase();
        }

        private void CompletePillowTask()
        {
            if (!flow.CompletePillow())
            {
                return;
            }

            pillow.SetAvailable(false, true);
            PresentCurrentPhase();
        }

        private void HandleBark()
        {
            if (bossCallActive)
            {
                AnswerBossCall();
                return;
            }

            if (flow.Phase != Day1Phase.FinalBark || !dogBed.ContainsPlayer)
            {
                return;
            }

            if (flow.CompleteFinalBark())
            {
                PresentCurrentPhase();
            }
        }

        private void UpdateBossCall()
        {
            if (!IsInteractivePhase())
            {
                return;
            }

            if (bossCallActive)
            {
                bossResponseRemaining -= Time.deltaTime;
                viewModel.SetProgress(
                    bossResponseRemaining / config.ResponseWindowSeconds,
                    "Bark before the call window closes.");
                if (bossResponseRemaining <= 0f)
                {
                    MissBossCall();
                }

                return;
            }

            bossTimer -= Time.deltaTime;
            if (bossTimer <= 0f)
            {
                StartBossCall();
            }
        }

        private void StartBossCall()
        {
            bossCallActive = true;
            bossResponseRemaining = config.ResponseWindowSeconds;
            bossTimer = config.LaterBossCallSeconds;
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetDialogue("BOSS", "Latte? Bark now so I know you are behaving.");
        }

        private void AnswerBossCall()
        {
            bossCallActive = false;
            safeRemaining = config.SafeWindowSeconds;
            alertRemaining = 0f;
            cameraB.SetAlertMultiplier(1f);
            PresentCurrentPhase();
        }

        private void MissBossCall()
        {
            bossCallActive = false;
            alertRemaining = config.AlertSeconds;
            safeRemaining = 0f;
            cameraB.SetAlertMultiplier(1.8f);
            PresentCurrentPhase();
        }

        private void UpdateTemporaryTimers()
        {
            safeRemaining = Mathf.Max(0f, safeRemaining - Time.deltaTime);
            alertRemaining = Mathf.Max(0f, alertRemaining - Time.deltaTime);
            failureMessageRemaining = Mathf.Max(0f, failureMessageRemaining - Time.deltaTime);
            if (alertRemaining <= 0f)
            {
                cameraB.SetAlertMultiplier(1f);
            }

            if (failureMessageRemaining <= 0f && IsInteractivePhase() && !bossCallActive)
            {
                SetPhaseObjective();
            }
        }

        private void UpdateCameraState()
        {
            bool carryingTask = player.HeldItem != null && player.HeldItem == CurrentTaskItem();
            bool canDetect = carryingTask && safeRemaining <= 0f && IsInteractivePhase();
            cameraB.SetDetectionEnabled(canDetect);
            CameraUiState state = alertRemaining > 0f
                ? CameraUiState.Alert
                : safeRemaining > 0f ? CameraUiState.Safe : CameraUiState.Scanning;
            viewModel.SetCamera(IsInteractivePhase() ? state : CameraUiState.Hidden);
        }

        private void HandleCameraDiscovery()
        {
            Carryable current = CurrentTaskItem();
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            current?.ResetHome();
            shoesGoal.ResetZone();
            pillowGoal.ResetZone();
            cameraB.ResetSensor();
            goalHoldSeconds = 0f;
            safeRemaining = 0f;
            alertRemaining = 0f;
            failureMessageRemaining = 3f;
            viewModel.SetObjective("Camera B spotted you. The current task has been reset.");
        }

        private Carryable CurrentTaskItem()
        {
            if (flow.Phase == Day1Phase.Shoes)
            {
                return shoes;
            }

            return flow.Phase == Day1Phase.Pillow ? pillow : null;
        }

        private bool IsInteractivePhase()
        {
            return flow != null && (flow.Phase == Day1Phase.Shoes
                || flow.Phase == Day1Phase.Pillow
                || flow.Phase == Day1Phase.FinalBark);
        }

        private void PresentCurrentPhase()
        {
            int visibleTaskMask = flow.TaskMask & 0b011;
            viewModel.SetTasks(Mathf.Min(flow.CompletedTasks, 2), visibleTaskMask);
            viewModel.SetDialogue(string.Empty, string.Empty);
            viewModel.SetProgress(0f, string.Empty);
            robot.SetPaused(flow.Phase == Day1Phase.Report);
            if (flow.Phase == Day1Phase.Report)
            {
                viewModel.SetReport("day1");
                viewModel.SetMode(LevelUiMode.Report);
                viewModel.SetCamera(CameraUiState.Hidden);
                return;
            }

            viewModel.SetMode(LevelUiMode.Gameplay);
            SetPhaseObjective();
        }

        private void SetPhaseObjective()
        {
            string objective = flow.Phase == Day1Phase.Shoes
                ? "Carry the shoes to Camera A's marked zone."
                : flow.Phase == Day1Phase.Pillow
                    ? "Return the boss's pillow to Latte's bed."
                    : "Stand in Latte's bed and bark to finish the day.";
            viewModel.SetObjective(objective);
        }

        private void StartEndingPerformance()
        {
            viewModel.SetMode(LevelUiMode.Dialogue);
            viewModel.SetObjective("Latte is putting the room back in order...");
            viewModel.SetDialogue("LATTE", "One last quiet lap before the next day begins.");
            robot.SetPaused(false);
            Vector2[] path = BuildEndingPath();
            if (path.Length == 0)
            {
                FinishEndingPerformance();
                return;
            }

            player.BeginAutoMove(path, FinishEndingPerformance);
        }

        private Vector2[] BuildEndingPath()
        {
            Vector2[] path = new Vector2[endingPath.Length];
            for (int i = 0; i < endingPath.Length; i++)
            {
                path[i] = endingPath[i].position;
            }

            return path;
        }

        private void FinishEndingPerformance()
        {
            if (flow.FinishEnding())
            {
                context.Host.CompleteLevel(LevelId.Day1, FinalChoice.None);
            }
        }
    }
}
