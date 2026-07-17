using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PetOffline.Core;
using PetOffline.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace PetOffline.Tests
{
    public sealed class PlayableSmokeTests
    {
        private const string StartScene = "Assets/Scenes/StartPanel.unity";
        private static readonly LevelInputFrame InteractInput =
            new LevelInputFrame(Vector2.zero, true, false, false, false, false);
        private static readonly LevelInputFrame BarkInput =
            new LevelInputFrame(Vector2.zero, false, true, false, false, false);
        private static readonly LevelInputFrame LieInput =
            new LevelInputFrame(Vector2.zero, false, false, false, true, false);
        private bool _previousEnterPlayModeOptionsEnabled;
        private EnterPlayModeOptions _previousEnterPlayModeOptions;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (Application.isPlaying)
            {
                yield return new ExitPlayMode();
            }

            new StorySaveService().Clear();
            Directory.CreateDirectory(ScreenshotFolder);
            EditorSceneManager.OpenScene(StartScene);
            _previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            new StorySaveService().Clear();
            if (Application.isPlaying)
            {
                yield return new ExitPlayMode();
            }

            EditorSettings.enterPlayModeOptions = _previousEnterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = _previousEnterPlayModeOptionsEnabled;
        }

        [UnityTest]
        public IEnumerator NewGame_CompletesKeepQuietFlowAndReturnsToTitle()
        {
            yield return new EnterPlayMode(false);
            yield return RunNewGameScenario();
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Continue_CompletesRestoreConnectionEnding()
        {
            SeedDay1Save();
            yield return new EnterPlayMode(false);
            yield return RunContinueScenario();
            yield return new ExitPlayMode();
        }

        private static IEnumerator RunNewGameScenario()
        {
            yield return WaitFor(() => Object.FindObjectOfType<GameSession>() != null, 5f, "GameSession did not start.");
            GameSession session = Object.FindObjectOfType<GameSession>();
            Assert.IsNotNull(session);
            DisableLiveInput();
            yield return WaitForTitleVideo();
            ValidatePersistentShell(session);
            yield return Capture("01-title");

            session.NewGame();
            session.NewGame();
            yield return WaitForLevel(session, LevelId.Day1);
            Time.timeScale = 10f;
            yield return RunDay1(session);
            yield return WaitForLevel(session, LevelId.Day2);
            Assert.IsTrue(session.Story.Day1Complete);
            Time.timeScale = 10f;
            yield return RunDay2ToChoice(session);
            AssertActiveButton("Restore");
            AssertActiveButton("KeepQuiet");
            yield return Capture("04-choice");

            session.SubmitChoice(FinalChoice.KeepQuiet);
            session.SubmitChoice(FinalChoice.RestoreConnection);
            yield return WaitFor(() => IsMode(session, LevelUiMode.Ending), 8f, "Keep Quiet ending did not finish.");
            AssertStory(session, FinalChoice.KeepQuiet);
            AssertActiveButton("Restart");
            AssertActiveButton("ReturnTitle");
            yield return Capture("05-keep-quiet-ending");
            session.ReturnTitle();
            yield return WaitFor(() => session.IsAtTitle && !session.IsBusy, 5f, "Return to title did not finish.");
            Assert.IsTrue(session.CanContinue);
            yield return Capture("06-returned-title");
            Time.timeScale = 1f;
        }

        private static IEnumerator RunContinueScenario()
        {
            yield return WaitFor(() => Object.FindObjectOfType<GameSession>() != null, 5f, "GameSession did not start.");
            GameSession session = Object.FindObjectOfType<GameSession>();
            Assert.IsNotNull(session);
            DisableLiveInput();
            StoryProgress persisted = new StorySaveService().Load();
            Assert.IsTrue(
                session.CanContinue,
                $"Continue was disabled. Persisted Day 1: {persisted.Day1Complete}; session scene: {session.gameObject.scene.name}.");
            session.Continue();
            session.Continue();
            yield return WaitForLevel(session, LevelId.Day2);
            Time.timeScale = 10f;
            yield return RunDay2ToChoice(session);

            session.SubmitChoice(FinalChoice.RestoreConnection);
            session.SubmitChoice(FinalChoice.KeepQuiet);
            yield return MovePlayerTo("Feeder Zone");
            yield return HoldLieUntil(session, () => IsMode(session, LevelUiMode.Ending), 5f, "Restore Connection ending did not finish.");
            AssertStory(session, FinalChoice.RestoreConnection);
            AssertActiveButton("Restart");
            AssertActiveButton("ReturnTitle");
            yield return Capture("07-restore-ending");
            Time.timeScale = 1f;
        }

        private static IEnumerator RunDay1(GameSession session)
        {
            Assert.IsFalse(FindSceneObject("OpeningVideo").activeInHierarchy);
            yield return AdvanceOpening(session);
            yield return VerifyUiRootCanRebind(session);
            yield return WaitForBossCallAndAnswer(session);
            yield return VerifyCameraBLocalReset(session);

            yield return MoveItemTo("Shoes", "Camera A Goal");
            yield return WaitFor(() => session.CurrentViewModel.CompletedTasks == 1, 3f, "Shoes task did not complete.");
            yield return MoveItemTo("Pillow", "Pillow Goal");
            yield return WaitFor(() => session.CurrentViewModel.CompletedTasks == 2, 3f, "Pillow task did not complete.");
            yield return AnswerBossCallIfNeeded(session);
            yield return MovePlayerTo("Dog Bed");
            yield return BarkUntilReport(session);
            yield return Capture("02-day1-report");
            session.ContinueReport();
            session.ContinueReport();
        }

        private static IEnumerator RunDay2ToChoice(GameSession session)
        {
            yield return Capture("03-day2-opening");
            yield return AdvanceOpening(session);
            yield return MovePlayerTo("Balcony Sun Zone");
            yield return HoldLieUntil(
                session,
                () => IsMode(session, LevelUiMode.Dialogue) && session.CurrentViewModel.Objective.Contains("paused"),
                4f,
                "First sun confirmation did not start.");
            yield return MovePlayerTo("Feeder Zone");
            yield return WaitFor(
                () => IsMode(session, LevelUiMode.Gameplay) && session.CurrentViewModel.Objective.Contains("banana"),
                3f,
                "Day 2 did not enter the camera-disable task.");

            yield return MoveItemTo("Banana", "Banana Robot Path Goal");
            yield return MovePlayerTo("Living Room Sun Zone");
            yield return WaitFor(
                () => session.CurrentViewModel.Objective.Contains("side door"),
                8f,
                "Robot did not disable the feeder camera.");
            yield return MovePlayerTo("Side Door Trigger");
            yield return WaitFor(() => session.CurrentViewModel.Objective.Contains("balcony"), 3f, "Backup camera did not activate.");
            yield return MovePlayerTo("Balcony Sun Zone");
            yield return HoldLieUntil(
                session,
                () => IsMode(session, LevelUiMode.Dialogue) && session.CurrentViewModel.DialogueText.Contains("Backup check"),
                4f,
                "Backup confirmation did not start.");
            yield return MovePlayerTo("Feeder Zone");
            yield return InteractUntilObjective(session, "living-room", "Final sun phase did not start.");
            yield return MovePlayerTo("Living Room Sun Zone");
            yield return HoldLieUntil(session, () => IsMode(session, LevelUiMode.Report), 5f, "Day 2 report did not open.");
            session.ContinueReport();
            session.ContinueReport();
            yield return WaitFor(() => IsMode(session, LevelUiMode.Choice), 3f, "Final choice did not open.");
        }

        private static IEnumerator VerifyUiRootCanRebind(GameSession session)
        {
            GameObject uiRoot = FindSceneObject("UIRoot");
            Transform robot = FindSceneObject("Robot").transform;
            Vector3 start = robot.position;
            uiRoot.SetActive(false);
            yield return WaitRealtime(0.35f);
            Assert.Greater(Vector3.Distance(start, robot.position), 0.05f, "World stopped while UIRoot was disabled.");
            uiRoot.SetActive(true);
            yield return null;
            Assert.IsTrue(uiRoot.activeInHierarchy);
            Assert.AreEqual(LevelUiMode.Gameplay, session.CurrentViewModel.UiMode);
        }

        private static IEnumerator WaitForBossCallAndAnswer(GameSession session)
        {
            yield return WaitFor(
                () => IsMode(session, LevelUiMode.Dialogue) && session.CurrentViewModel.DialogueSpeaker == "BOSS",
                3f,
                "Day 1 boss call did not start.");
            session.RouteInput(BarkInput);
            yield return WaitFor(() => IsMode(session, LevelUiMode.Gameplay), 2f, "Boss call answer did not resume gameplay.");
        }

        private static IEnumerator VerifyCameraBLocalReset(GameSession session)
        {
            yield return WaitFor(() => session.CurrentViewModel.CameraState != CameraUiState.Safe, 2f, "Camera safe window did not close.");
            yield return MovePlayerTo("Shoes");
            session.RouteInput(InteractInput);
            yield return null;
            PlayerController player = Object.FindObjectOfType<PlayerController>();
            Assert.IsNotNull(player.HeldItem, "Shoes were not picked up.");
            yield return WaitFor(
                () => session.CurrentViewModel.Objective.Contains("spotted"),
                3f,
                "Camera B did not trigger its local reset.");
            Assert.IsNull(player.HeldItem);
            Assert.AreEqual(0, session.CurrentViewModel.CompletedTasks);
        }

        private static IEnumerator AnswerBossCallIfNeeded(GameSession session)
        {
            if (!IsMode(session, LevelUiMode.Dialogue) || session.CurrentViewModel.DialogueSpeaker != "BOSS")
            {
                yield break;
            }

            session.RouteInput(BarkInput);
            yield return WaitFor(() => IsMode(session, LevelUiMode.Gameplay), 2f, "Boss call did not close.");
        }

        private static IEnumerator AdvanceOpening(GameSession session)
        {
            for (var index = 0; index < 3; index++)
            {
                session.RouteInput(InteractInput);
                yield return null;
            }

            yield return WaitFor(() => IsMode(session, LevelUiMode.Gameplay), 2f, "Opening dialogue did not finish.");
        }

        private static IEnumerator MovePlayerTo(string objectName)
        {
            PlayerController player = Object.FindObjectOfType<PlayerController>();
            player.ResetTo(FindSceneObject(objectName).transform.position);
            Physics2D.SyncTransforms();
            yield return null;
            yield return null;
        }

        private static IEnumerator MoveItemTo(string itemName, string targetName)
        {
            Carryable item = FindSceneObject(itemName).GetComponent<Carryable>();
            Rigidbody2D body = item.GetComponent<Rigidbody2D>();
            body.position = FindSceneObject(targetName).transform.position;
            body.velocity = Vector2.zero;
            Physics2D.SyncTransforms();
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitRealtime(float duration)
        {
            float deadline = Time.realtimeSinceStartup + duration;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }

        private static IEnumerator WaitForTitleVideo()
        {
            yield return WaitFor(
                () =>
                {
                    VideoPlayer video = Object.FindObjectOfType<VideoPlayer>();
                    return video != null && video.isPrepared && video.isPlaying && video.frame >= 0;
                },
                8f,
                "Opening video did not prepare and start.");
        }

        private static IEnumerator HoldLieUntil(
            GameSession session,
            Func<bool> completed,
            float timeout,
            string failure)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!completed())
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    PlayerController player = Object.FindObjectOfType<PlayerController>();
                    string contacts = string.Join(", ", Object.FindObjectsOfType<TriggerZone>()
                        .Where(zone => zone.ContainsPlayer)
                        .Select(zone => zone.name));
                    Assert.Fail($"{failure} Mode={session.CurrentViewModel?.UiMode}; "
                        + $"objective='{session.CurrentViewModel?.Objective}'; position={player?.transform.position}; "
                        + $"lying={player != null && player.IsLying}; trigger contacts=[{contacts}].");
                }

                session.RouteInput(LieInput);
                yield return null;
            }
        }

        private static IEnumerator BarkUntilReport(GameSession session)
        {
            float deadline = Time.realtimeSinceStartup + 3f;
            while (!IsMode(session, LevelUiMode.Report))
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    TriggerZone dogBed = FindSceneObject("Dog Bed").GetComponent<TriggerZone>();
                    Assert.Fail($"Day 1 report did not open. Mode={session.CurrentViewModel?.UiMode}; "
                        + $"objective='{session.CurrentViewModel?.Objective}'; inBed={dogBed.ContainsPlayer}.");
                }

                session.RouteInput(BarkInput);
                yield return null;
            }
        }

        private static IEnumerator InteractUntilObjective(
            GameSession session,
            string expectedText,
            string failure)
        {
            float deadline = Time.realtimeSinceStartup + 3f;
            while (!session.CurrentViewModel.Objective.Contains(expectedText))
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    string contacts = string.Join(", ", Object.FindObjectsOfType<TriggerZone>()
                        .Where(zone => zone.ContainsPlayer)
                        .Select(zone => zone.name));
                    Assert.Fail($"{failure} Mode={session.CurrentViewModel.UiMode}; "
                        + $"objective='{session.CurrentViewModel.Objective}'; contacts=[{contacts}].");
                }

                session.RouteInput(InteractInput);
                yield return null;
            }
        }

        private static IEnumerator WaitForLevel(GameSession session, LevelId level)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (session.CurrentLevel != level || session.IsBusy || session.CurrentViewModel == null)
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    SceneFlowService flow = Object.FindObjectOfType<SceneFlowService>();
                    Assert.Fail(DescribeLevelWaitFailure(session, flow, level));
                }

                yield return null;
            }
        }

        private static string DescribeLevelWaitFailure(
            GameSession session,
            SceneFlowService flow,
            LevelId expected)
        {
            string scenes = string.Join(", ", Enumerable.Range(0, SceneManager.sceneCount)
                .Select(index => SceneManager.GetSceneAt(index).name));
            PlayerController player = Object.FindObjectOfType<PlayerController>();
            return $"{expected} did not load. Playing={Application.isPlaying}; "
                + $"level={session.CurrentLevel}; busy={session.IsBusy}; error='{session.LastError}'; "
                + $"flowBusy={flow != null && flow.IsBusy}; flowLevel={(flow == null ? LevelId.None : flow.LoadedLevel)}; "
                + $"player={player?.transform.position}; cutscene={player != null && player.IsInCutscene}; scenes=[{scenes}].";
        }

        private static void SeedDay1Save()
        {
            var save = new StorySaveService();
            Assert.IsTrue(save.MarkDay1Complete());
            Assert.IsTrue(save.Load().Day1Complete);
        }

        private static void DisableLiveInput()
        {
            LegacyInputRouter router = Object.FindObjectOfType<LegacyInputRouter>();
            Assert.IsNotNull(router);
            router.enabled = false;
        }

        private static IEnumerator WaitFor(Func<bool> condition, float timeout, string failure)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    Assert.Fail(failure);
                }

                yield return null;
            }
        }

        private static IEnumerator Capture(string fileName)
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (var index = 0; index < 4; index++)
            {
                yield return null;
            }

            string path = Path.Combine(ScreenshotFolder, fileName + ".png");
            SmokeScreenshotCapture.WritePng(path);
            Assert.IsTrue(File.Exists(path), $"Screenshot '{fileName}' was not written.");
        }

        private static void ValidatePersistentShell(GameSession session)
        {
            Assert.IsTrue(session.IsAtTitle);
            Assert.AreEqual(1, Object.FindObjectsOfType<Camera>().Length);
            Assert.AreEqual(1, Object.FindObjectsOfType<AudioListener>().Length);
            Assert.AreEqual(1, Object.FindObjectsOfType<EventSystem>().Length);
            VideoPlayer video = Object.FindObjectOfType<VideoPlayer>();
            Assert.IsNotNull(video);
            Assert.IsTrue(video.isLooping);
            Assert.IsTrue(video.isPlaying);
            Assert.IsTrue(video.gameObject.activeInHierarchy);
        }

        private static void AssertStory(GameSession session, FinalChoice choice)
        {
            Assert.IsTrue(session.Story.Day1Complete);
            Assert.IsTrue(session.Story.Day2Complete);
            Assert.AreEqual(choice, session.Story.FinalChoice);
        }

        private static bool IsMode(GameSession session, LevelUiMode mode)
        {
            return session.CurrentViewModel != null && session.CurrentViewModel.UiMode == mode;
        }

        private static GameObject FindSceneObject(string name)
        {
            GameObject result = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(item => item.name == name && item.scene.IsValid() && item.scene.isLoaded);
            Assert.IsNotNull(result, $"Scene object '{name}' was not found.");
            return result;
        }

        private static void AssertActiveButton(string name)
        {
            Button button = Resources.FindObjectsOfTypeAll<Button>()
                .FirstOrDefault(item => item.name == name && item.gameObject.activeInHierarchy);
            Assert.IsNotNull(button, $"Active button '{name}' was not found.");
            Assert.IsTrue(button.interactable, $"Button '{name}' should be interactable.");
        }

        private static string ScreenshotFolder =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PetOfflineScreenshots"));
    }
}
