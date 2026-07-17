using PetOffline.Core;
using PetOffline.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerUi
    {
        public static void BuildTitle(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets,
            GameObject videoRoot,
            RawImage videoImage,
            VideoPlayer video)
        {
            GameObject root = ProjectInstallerUiFactory.CreateRect(canvas, "Title");
            ProjectInstallerUiFactory.Stretch(root);
            CreateTitleArt(root.transform, assets);
            Button newGame = CreateTitleButton(root.transform, "NewGame", "NEW GAME", assets, 20f);
            Button continueButton = CreateTitleButton(root.transform, "Continue", "CONTINUE", assets, -105f);
            Text error = ProjectInstallerUiFactory.CreateText(
                root.transform, "Error", string.Empty, assets.Font, 24,
                new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(1000f, 70f),
                TextAnchor.MiddleCenter);
            error.color = new Color(1f, 0.55f, 0.5f, 1f);
            GameObject confirmation = CreateNewGameConfirmation(root.transform, assets, out Button confirm, out Button cancel);
            TitlePresenter presenter = presenterHost.AddComponent<TitlePresenter>();
            BindTitle(presenter, root, confirmation, newGame, continueButton, confirm, cancel, error, videoRoot, videoImage, video);
            presenter.SetGameSession(session);
        }

        public static void BuildHud(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets)
        {
            GameObject root = ProjectInstallerUiFactory.CreateRect(canvas, "HUD");
            ProjectInstallerUiFactory.Stretch(root);
            Image mission = ProjectInstallerUiFactory.CreateImage(
                root.transform, "MissionPanel", assets.MissionSprite,
                new Vector2(0f, 1f), new Vector2(205f, -390f), new Vector2(360f, 326f), Color.white);
            mission.raycastTarget = false;
            Text objective = ProjectInstallerUiFactory.CreateText(
                root.transform, "Objective", "", assets.Font, 28,
                new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(1100f, 80f),
                TextAnchor.MiddleCenter);
            BuildProgress(root.transform, assets, out Image progressFill, out Text progressText);
            Text taskCount = ProjectInstallerUiFactory.CreateText(
                root.transform, "TaskCount", "0/0", assets.Font, 26,
                new Vector2(0f, 1f), new Vector2(205f, -580f), new Vector2(160f, 50f),
                TextAnchor.MiddleCenter);
            GameObject[] checks = BuildCheckmarks(root.transform, assets);
            BuildCameraState(root.transform, assets, out GameObject cameraRoot, out Text cameraText);
            BuildControls(root.transform, assets);
            HudPresenter presenter = presenterHost.AddComponent<HudPresenter>();
            BindHud(
                presenter, root, mission, objective, progressFill, progressText, taskCount,
                checks, cameraRoot, cameraText, assets);
            presenter.SetGameSession(session);
            root.SetActive(false);
        }

        public static void BuildDialogue(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets)
        {
            GameObject root = ProjectInstallerUiFactory.CreateRect(canvas, "Dialogue");
            ProjectInstallerUiFactory.Place(
                root, new Vector2(0.5f, 0f), new Vector2(0f, 155f), new Vector2(1500f, 250f));
            Image background = root.AddComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.13f, 0.92f);
            Image portrait = ProjectInstallerUiFactory.CreateImage(
                root.transform, "ConversationArt", assets.ConversationSprite,
                new Vector2(0f, 0.5f), new Vector2(135f, 0f), new Vector2(240f, 220f), Color.white);
            Text speaker = ProjectInstallerUiFactory.CreateText(
                root.transform, "Speaker", "", assets.Font, 30,
                new Vector2(0f, 1f), new Vector2(420f, -45f), new Vector2(920f, 55f),
                TextAnchor.MiddleLeft);
            speaker.color = new Color(1f, 0.78f, 0.32f, 1f);
            Text dialogue = ProjectInstallerUiFactory.CreateText(
                root.transform, "Line", "", assets.Font, 29,
                new Vector2(0f, 0.5f), new Vector2(760f, -25f), new Vector2(980f, 130f),
                TextAnchor.UpperLeft);
            ProjectInstallerUiFactory.CreateText(
                root.transform, "Hint", "Follow the current objective", assets.Font, 20,
                new Vector2(1f, 0f), new Vector2(-155f, 25f), new Vector2(270f, 40f),
                TextAnchor.MiddleRight);
            DialoguePresenter presenter = presenterHost.AddComponent<DialoguePresenter>();
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "dialogueRoot", root);
                ProjectInstallerSerialization.Reference(serialized, "portraitImage", portrait);
                ProjectInstallerSerialization.Reference(serialized, "bossPortrait", assets.ConversationSprite);
                ProjectInstallerSerialization.Reference(serialized, "ownerPortrait", assets.OwnerPortraitSprite);
                ProjectInstallerSerialization.Reference(serialized, "robotPortrait", assets.RobotPortraitSprite);
                ProjectInstallerSerialization.Reference(serialized, "speakerText", speaker);
                ProjectInstallerSerialization.Reference(serialized, "dialogueText", dialogue);
            });
            presenter.SetGameSession(session);
            root.SetActive(false);
        }

        private static void CreateTitleArt(Transform root, ProjectInstallAssets assets)
        {
            Image title = ProjectInstallerUiFactory.CreateImage(
                root, "TitleArt", assets.TitleSprite,
                new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(900f, 420f), Color.white);
            title.raycastTarget = false;
        }

        private static Button CreateTitleButton(
            Transform root,
            string name,
            string label,
            ProjectInstallAssets assets,
            float y)
        {
            return ProjectInstallerUiFactory.CreateButton(
                root, name, label,
                name == "NewGame" ? assets.PrimaryButtonSprite : assets.BlueButtonSprite,
                assets.Font, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(520f, 105f));
        }

        private static GameObject CreateNewGameConfirmation(
            Transform root,
            ProjectInstallAssets assets,
            out Button confirm,
            out Button cancel)
        {
            GameObject modal = ProjectInstallerUiFactory.CreateModal(
                root, "NewGameConfirmation", new Color(0.02f, 0.025f, 0.04f, 0.92f));
            ProjectInstallerUiFactory.CreateText(
                modal.transform, "Question", "Start a new story? Existing story progress will be cleared.",
                assets.Font, 30, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f),
                new Vector2(900f, 120f), TextAnchor.MiddleCenter);
            confirm = ProjectInstallerUiFactory.CreateButton(
                modal.transform, "Confirm", "START", assets.PrimaryButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(-170f, -70f), new Vector2(280f, 80f));
            cancel = ProjectInstallerUiFactory.CreateButton(
                modal.transform, "Cancel", "CANCEL", assets.BlueButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(170f, -70f), new Vector2(280f, 80f));
            modal.SetActive(false);
            return modal;
        }

        private static void BuildProgress(
            Transform root,
            ProjectInstallAssets assets,
            out Image fill,
            out Text label)
        {
            ProjectInstallerUiFactory.CreateImage(
                root, "ProgressBackground", assets.LatteBackgroundSprite,
                new Vector2(0f, 1f), new Vector2(180f, -92f), new Vector2(340f, 185f), Color.white);
            Image empty = ProjectInstallerUiFactory.CreateImage(
                root, "ProgressTrack", assets.WhiteSprite,
                new Vector2(0f, 1f), new Vector2(220f, -163f), new Vector2(245f, 26f), Color.white);
            empty.preserveAspect = false;
            empty.raycastTarget = false;
            fill = ProjectInstallerUiFactory.CreateImage(
                root, "ProgressFill", assets.LatteGreenSprite,
                new Vector2(0f, 1f), new Vector2(220f, -163f), new Vector2(245f, 26f), Color.white);
            fill.preserveAspect = false;
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            label = ProjectInstallerUiFactory.CreateText(
                root, "ProgressLabel", "0%", assets.Font, 20,
                new Vector2(0f, 1f), new Vector2(205f, -207f), new Vector2(360f, 38f),
                TextAnchor.MiddleCenter);
        }

        private static GameObject[] BuildCheckmarks(Transform root, ProjectInstallAssets assets)
        {
            var result = new GameObject[2];
            for (int index = 0; index < result.Length; index++)
            {
                Text check = ProjectInstallerUiFactory.CreateText(
                    root, "TaskCheck" + (index + 1), "✓", assets.Font, 34,
                    new Vector2(0f, 1f), new Vector2(66f, index == 0 ? -350f : -456f),
                    new Vector2(42f, 42f), TextAnchor.MiddleCenter);
                check.color = new Color(0.2f, 0.72f, 0.35f, 1f);
                check.raycastTarget = false;
                result[index] = check.gameObject;
                result[index].SetActive(false);
            }

            return result;
        }

        private static void BuildControls(Transform root, ProjectInstallAssets assets)
        {
            GameObject controls = ProjectInstallerUiFactory.CreateRect(root, "Controls");
            ProjectInstallerUiFactory.Place(
                controls, new Vector2(0f, 0f), new Vector2(400f, 110f), new Vector2(760f, 200f));
            Image icons = ProjectInstallerUiFactory.CreateImage(
                controls.transform, "Icons", assets.IllustrateIconSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 32f), new Vector2(760f, 163f), Color.white);
            Image words = ProjectInstallerUiFactory.CreateImage(
                controls.transform, "Words", assets.IllustrateWordsSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(760f, 60f), Color.white);
            Image dashPatch = ProjectInstallerUiFactory.CreateImage(
                controls.transform, "DashPatch", assets.WhiteSprite,
                new Vector2(0.5f, 0.5f), new Vector2(96f, -72f), new Vector2(190f, 60f),
                new Color(0.23f, 0.42f, 0.56f, 1f));
            dashPatch.preserveAspect = false;
            Text dash = ProjectInstallerUiFactory.CreateText(
                controls.transform, "Dash", "DASH\n\"Q\"", assets.Font, 20,
                new Vector2(0.5f, 0.5f), new Vector2(96f, -72f), new Vector2(190f, 60f),
                TextAnchor.MiddleCenter);
            icons.raycastTarget = false;
            words.raycastTarget = false;
            dashPatch.raycastTarget = false;
            dash.raycastTarget = false;
        }

        private static void BuildCameraState(
            Transform root,
            ProjectInstallAssets assets,
            out GameObject cameraRoot,
            out Text cameraText)
        {
            cameraRoot = ProjectInstallerUiFactory.CreateRect(root, "CameraState");
            ProjectInstallerUiFactory.Place(
                cameraRoot, new Vector2(1f, 1f), new Vector2(-170f, -80f), new Vector2(290f, 90f));
            Image background = cameraRoot.AddComponent<Image>();
            background.color = new Color(0.04f, 0.06f, 0.09f, 0.88f);
            cameraText = ProjectInstallerUiFactory.CreateText(
                cameraRoot.transform, "Label", "SCANNING", assets.Font, 28,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 60f),
                TextAnchor.MiddleCenter);
        }

        private static void BindTitle(
            TitlePresenter presenter,
            GameObject root,
            GameObject confirmation,
            Button newGame,
            Button continueButton,
            Button confirm,
            Button cancel,
            Text error,
            GameObject videoRoot,
            RawImage videoImage,
            VideoPlayer video)
        {
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "titleRoot", root);
                ProjectInstallerSerialization.Reference(serialized, "newGameConfirmationRoot", confirmation);
                ProjectInstallerSerialization.Reference(serialized, "newGameButton", newGame);
                ProjectInstallerSerialization.Reference(serialized, "continueButton", continueButton);
                ProjectInstallerSerialization.Reference(serialized, "confirmNewGameButton", confirm);
                ProjectInstallerSerialization.Reference(serialized, "cancelNewGameButton", cancel);
                ProjectInstallerSerialization.Reference(serialized, "errorText", error);
                ProjectInstallerSerialization.Reference(serialized, "videoRoot", videoRoot);
                ProjectInstallerSerialization.Reference(serialized, "videoImage", videoImage);
                ProjectInstallerSerialization.Reference(serialized, "openingVideo", video);
            });
        }

        private static void BindHud(
            HudPresenter presenter,
            GameObject root,
            Image mission,
            Text objective,
            Image progressFill,
            Text progressText,
            Text taskCount,
            GameObject[] checks,
            GameObject cameraRoot,
            Text cameraText,
            ProjectInstallAssets assets)
        {
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "hudRoot", root);
                ProjectInstallerSerialization.Reference(serialized, "missionBackground", mission);
                ProjectInstallerSerialization.Reference(serialized, "day1MissionSprite", assets.MissionSprite);
                ProjectInstallerSerialization.Reference(serialized, "day2MissionSprite", assets.Day2MissionSprite);
                ProjectInstallerSerialization.Reference(serialized, "objectiveText", objective);
                ProjectInstallerSerialization.Reference(serialized, "progressFill", progressFill);
                ProjectInstallerSerialization.Reference(serialized, "progressText", progressText);
                ProjectInstallerSerialization.Reference(serialized, "taskCountText", taskCount);
                ProjectInstallerSerialization.References(serialized, "taskCheckmarks", checks);
                ProjectInstallerSerialization.Reference(serialized, "cameraStateRoot", cameraRoot);
                ProjectInstallerSerialization.Reference(serialized, "cameraStateText", cameraText);
            });
        }
    }
}
