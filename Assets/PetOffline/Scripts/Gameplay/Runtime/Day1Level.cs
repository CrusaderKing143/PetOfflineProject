using UnityEngine;

namespace PetOffline
{
    public sealed class Day1Level : LevelController
    {
        private enum Phase
        {
            Opening,
            Shoes,
            Pillow,
            FinalBark,
            Report,
            Ending,
            Complete
        }

        [Header("Tuning")]
        [SerializeField, Min(0.1f)] private float shoesHoldSeconds = 2f;
        [SerializeField, Min(0.1f)] private float firstBossCallSeconds = 14f;
        [SerializeField, Min(0.1f)] private float laterBossCallSeconds = 26f;
        [SerializeField, Min(0.1f)] private float responseWindowSeconds = 3.6f;
        [SerializeField, Min(0f)] private float safeWindowSeconds = 3f;
        [SerializeField, Min(0f)] private float alertSeconds = 7f;

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

        private Phase phase;
        private int dialogueIndex;
        private float goalHold;
        private float bossTimer;
        private float bossResponse;
        private float safeRemaining;
        private float alertRemaining;
        private float failureMessageRemaining;
        private bool bossCallActive;

        public override bool CanPause => IsInteractive && !bossCallActive;
        private bool IsInteractive => phase == Phase.Shoes
            || phase == Phase.Pillow
            || phase == Phase.FinalBark;

        private void Update()
        {
            if (!IsReady || Session.IsPaused)
            {
                return;
            }

            UpdateTimers();
            if (!bossCallActive)
            {
                UpdateTask();
            }

            UpdateBossCall();
            UpdateCameraState();
        }

        protected override void Begin()
        {
            phase = Phase.Opening;
            dialogueIndex = 0;
            bossTimer = firstBossCallSeconds;
            cameraB.Discovered += HandleCameraDiscovery;
            ResetWorld();
            UI.BeginLevel(false, 2);
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

            if (!IsInteractive)
            {
                player.ApplyInput(default, false);
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

        public override void ContinueReport()
        {
            if (phase != Phase.Report)
            {
                return;
            }

            phase = Phase.Ending;
            StartEnding();
        }

        private void ResetWorld()
        {
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            shoes.ResetHome();
            pillow.ResetHome();
            shoes.SetAvailable(true);
            pillow.SetAvailable(false);
            cameraB.ResetSensor();
            robot.ResetPatrol();
            robot.SetPaused(true);
        }

        private void ShowOpeningLine()
        {
            switch (dialogueIndex)
            {
                case 0:
                    UI.ShowDialogue("BOSS", "Latte, keep the apartment tidy while I am away.");
                    break;
                case 1:
                    UI.ShowDialogue("LATTE", "The cameras are watching. I can still fix a few things.");
                    break;
                default:
                    UI.ShowDialogue("BOSS", "Start with the shoes, then return my pillow.");
                    break;
            }

            UI.SetObjective("Press E to continue.");
            UI.SetCamera(CameraUiState.Hidden);
        }

        private void AdvanceOpening()
        {
            dialogueIndex++;
            if (dialogueIndex < 3)
            {
                ShowOpeningLine();
                return;
            }

            phase = Phase.Shoes;
            PresentPhase();
        }

        private void UpdateTask()
        {
            if (phase == Phase.Shoes)
            {
                UpdateShoesTask();
            }
            else if (phase == Phase.Pillow && pillowGoal.Contains(pillow) && !pillow.IsHeld)
            {
                CompletePillow();
            }
        }

        private void UpdateShoesTask()
        {
            if (!shoesGoal.Contains(shoes) || shoes.IsHeld)
            {
                goalHold = 0f;
                UI.SetProgress(0f, "Hold the shoes in Camera A's zone for 2 seconds.");
                return;
            }

            goalHold += Time.deltaTime;
            UI.SetProgress(
                goalHold / shoesHoldSeconds,
                $"Camera A hold: {Mathf.Min(goalHold, shoesHoldSeconds):0.0}s");
            if (goalHold >= shoesHoldSeconds)
            {
                CompleteShoes();
            }
        }

        private void CompleteShoes()
        {
            phase = Phase.Pillow;
            shoes.SetAvailable(false, true);
            pillow.SetAvailable(true);
            goalHold = 0f;
            UI.SetTasks(1, 2);
            PresentPhase();
        }

        private void CompletePillow()
        {
            phase = Phase.FinalBark;
            pillow.SetAvailable(false, true);
            UI.SetTasks(2, 2);
            PresentPhase();
        }

        private void HandleBark()
        {
            if (bossCallActive)
            {
                AnswerBossCall();
            }
            else if (phase == Phase.FinalBark && dogBed.ContainsPlayer)
            {
                phase = Phase.Report;
                PresentPhase();
            }
        }

        private void UpdateBossCall()
        {
            if (!IsInteractive)
            {
                return;
            }

            if (bossCallActive)
            {
                bossResponse -= Time.deltaTime;
                UI.SetProgress(
                    bossResponse / responseWindowSeconds,
                    "Bark before the call window closes.");
                if (bossResponse <= 0f)
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
            bossResponse = responseWindowSeconds;
            bossTimer = laterBossCallSeconds;
            UI.ShowDialogue("BOSS", "Latte? Bark now so I know you are behaving.");
        }

        private void AnswerBossCall()
        {
            bossCallActive = false;
            safeRemaining = safeWindowSeconds;
            alertRemaining = 0f;
            cameraB.SetAlertMultiplier(1f);
            PresentPhase();
        }

        private void MissBossCall()
        {
            bossCallActive = false;
            alertRemaining = alertSeconds;
            safeRemaining = 0f;
            cameraB.SetAlertMultiplier(1.8f);
            PresentPhase();
        }

        private void UpdateTimers()
        {
            safeRemaining = Mathf.Max(0f, safeRemaining - Time.deltaTime);
            alertRemaining = Mathf.Max(0f, alertRemaining - Time.deltaTime);
            if (alertRemaining <= 0f)
            {
                cameraB.SetAlertMultiplier(1f);
            }

            if (failureMessageRemaining > 0f)
            {
                failureMessageRemaining -= Time.deltaTime;
                if (failureMessageRemaining <= 0f)
                {
                    SetPhaseObjective();
                }
            }
        }

        private void UpdateCameraState()
        {
            if (!IsInteractive)
            {
                UI.SetCamera(CameraUiState.Hidden);
                return;
            }

            bool carryingTask = player.HeldItem == CurrentTaskItem();
            cameraB.SetDetectionEnabled(carryingTask && safeRemaining <= 0f);
            CameraUiState state = alertRemaining > 0f
                ? CameraUiState.Alert
                : safeRemaining > 0f ? CameraUiState.Safe : CameraUiState.Scanning;
            UI.SetCamera(state);
        }

        private void HandleCameraDiscovery()
        {
            player.DropHeldItem();
            player.ResetTo(playerSpawn.position);
            CurrentTaskItem().ResetHome();
            cameraB.ResetSensor();
            goalHold = 0f;
            safeRemaining = 0f;
            alertRemaining = 0f;
            failureMessageRemaining = 3f;
            UI.SetObjective("Camera B spotted you. The current task has been reset.");
        }

        private Carryable CurrentTaskItem()
        {
            return phase == Phase.Shoes ? shoes : pillow;
        }

        private void PresentPhase()
        {
            UI.SetProgress(0f, string.Empty);
            if (phase == Phase.Report)
            {
                robot.SetPaused(true);
                UI.SetCamera(CameraUiState.Hidden);
                UI.ShowReport(false);
                return;
            }

            robot.SetPaused(false);
            UI.ShowWorld();
            SetPhaseObjective();
        }

        private void SetPhaseObjective()
        {
            string objective = phase == Phase.Shoes
                ? "Carry the shoes to Camera A's marked zone."
                : phase == Phase.Pillow
                    ? "Return the boss's pillow to Latte's bed."
                    : "Stand in Latte's bed and bark to finish the day.";
            UI.SetObjective(objective);
        }

        private void StartEnding()
        {
            UI.SetObjective("Latte is putting the room back in order...");
            UI.ShowDialogue("LATTE", "One last quiet lap before the next day begins.");
            UI.SetCamera(CameraUiState.Hidden);
            robot.SetPaused(false);
            if (endingPath.Length == 0)
            {
                FinishEnding();
                return;
            }

            Vector2[] path = new Vector2[endingPath.Length];
            for (int index = 0; index < endingPath.Length; index++)
            {
                path[index] = endingPath[index].position;
            }

            player.BeginAutoMove(path, FinishEnding);
        }

        private void FinishEnding()
        {
            phase = Phase.Complete;
            Session.LoadDay2();
        }
    }
}
