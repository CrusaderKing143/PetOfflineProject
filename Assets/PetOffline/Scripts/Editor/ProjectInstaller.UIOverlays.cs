using PetOffline.Core;
using PetOffline.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerUiOverlays
    {
        public static void BuildReport(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets)
        {
            GameObject root = ProjectInstallerUiFactory.CreateModal(
                canvas, "Report", new Color(0.025f, 0.03f, 0.05f, 0.96f));
            Image reportImage = ProjectInstallerUiFactory.CreateImage(
                root.transform, "ReportImage", assets.Day1ReportSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(1320f, 780f), Color.white);
            Button continueButton = ProjectInstallerUiFactory.CreateButton(
                root.transform, "Continue", "CONTINUE", assets.PrimaryButtonSprite, assets.Font,
                new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(360f, 80f));
            ReportPresenter presenter = presenterHost.AddComponent<ReportPresenter>();
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "reportRoot", root);
                ProjectInstallerSerialization.Reference(serialized, "reportImage", reportImage);
                ProjectInstallerSerialization.Reference(serialized, "continueButton", continueButton);
                ProjectInstallerSerialization.Reference(serialized, "day1Sprite", assets.Day1ReportSprite);
                ProjectInstallerSerialization.Reference(serialized, "day2Sprite", assets.Day2ReportSprite);
            });
            presenter.SetGameSession(session);
            root.SetActive(false);
        }

        public static void BuildChoiceAndEnding(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets)
        {
            GameObject choiceRoot = CreateChoiceRoot(canvas, assets, out Button restore, out Button quiet);
            GameObject endingRoot = CreateEndingRoot(
                canvas, assets, out Text endingText, out Button restart, out Button returnTitle,
                out GameObject confirmation, out Button confirmRestart, out Button cancelRestart);
            ChoiceEndingPresenter presenter = presenterHost.AddComponent<ChoiceEndingPresenter>();
            BindChoicePresenter(
                presenter, choiceRoot, restore, quiet, endingRoot, endingText, restart, returnTitle,
                confirmation, confirmRestart, cancelRestart);
            presenter.SetGameSession(session);
            choiceRoot.SetActive(false);
            endingRoot.SetActive(false);
        }

        public static void BuildPause(
            Transform canvas,
            GameObject presenterHost,
            GameSession session,
            ProjectInstallAssets assets)
        {
            GameObject root = ProjectInstallerUiFactory.CreateModal(
                canvas, "Pause", new Color(0.015f, 0.02f, 0.035f, 0.88f));
            Text pauseText = ProjectInstallerUiFactory.CreateText(
                root.transform, "PauseText", "PAUSED\nPress Esc to resume.", assets.Font, 46,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(800f, 180f),
                TextAnchor.MiddleCenter);
            Button returnTitle = ProjectInstallerUiFactory.CreateButton(
                root.transform, "ReturnTitle", "RETURN TO TITLE", assets.BlueButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(440f, 85f));
            PausePresenter presenter = presenterHost.AddComponent<PausePresenter>();
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "pauseRoot", root);
                ProjectInstallerSerialization.Reference(serialized, "pauseText", pauseText);
                ProjectInstallerSerialization.Reference(serialized, "returnTitleButton", returnTitle);
            });
            presenter.SetGameSession(session);
            root.SetActive(false);
        }

        private static GameObject CreateChoiceRoot(
            Transform canvas,
            ProjectInstallAssets assets,
            out Button restore,
            out Button quiet)
        {
            GameObject root = ProjectInstallerUiFactory.CreateModal(
                canvas, "Choice", new Color(0.025f, 0.03f, 0.05f, 0.94f));
            ProjectInstallerUiFactory.CreateText(
                root.transform, "Prompt", "What should Latte do with the connection?", assets.Font, 38,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1000f, 90f),
                TextAnchor.MiddleCenter);
            restore = ProjectInstallerUiFactory.CreateButton(
                root.transform, "Restore", "RESTORE CONNECTION", assets.PrimaryButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(-260f, -80f), new Vector2(440f, 95f));
            quiet = ProjectInstallerUiFactory.CreateButton(
                root.transform, "KeepQuiet", "KEEP QUIET", assets.BlueButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(260f, -80f), new Vector2(440f, 95f));
            return root;
        }

        private static GameObject CreateEndingRoot(
            Transform canvas,
            ProjectInstallAssets assets,
            out Text endingText,
            out Button restart,
            out Button returnTitle,
            out GameObject confirmation,
            out Button confirmRestart,
            out Button cancelRestart)
        {
            GameObject root = ProjectInstallerUiFactory.CreateModal(
                canvas, "Ending", new Color(0.025f, 0.03f, 0.05f, 0.96f));
            endingText = ProjectInstallerUiFactory.CreateText(
                root.transform, "EndingText", "", assets.Font, 46,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 110f),
                TextAnchor.MiddleCenter);
            restart = ProjectInstallerUiFactory.CreateButton(
                root.transform, "Restart", "RESTART", assets.PrimaryButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(-220f, -110f), new Vector2(360f, 85f));
            returnTitle = ProjectInstallerUiFactory.CreateButton(
                root.transform, "ReturnTitle", "RETURN TO TITLE", assets.BlueButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(220f, -110f), new Vector2(360f, 85f));
            confirmation = CreateRestartConfirmation(root.transform, assets, out confirmRestart, out cancelRestart);
            return root;
        }

        private static GameObject CreateRestartConfirmation(
            Transform root,
            ProjectInstallAssets assets,
            out Button confirm,
            out Button cancel)
        {
            GameObject modal = ProjectInstallerUiFactory.CreateModal(
                root, "RestartConfirmation", new Color(0.01f, 0.015f, 0.025f, 0.96f));
            ProjectInstallerUiFactory.CreateText(
                modal.transform, "Question", "Restart from Day 1 and clear story progress?", assets.Font, 32,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 75f), new Vector2(900f, 90f),
                TextAnchor.MiddleCenter);
            confirm = ProjectInstallerUiFactory.CreateButton(
                modal.transform, "Confirm", "RESTART", assets.PrimaryButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(-170f, -60f), new Vector2(280f, 80f));
            cancel = ProjectInstallerUiFactory.CreateButton(
                modal.transform, "Cancel", "CANCEL", assets.BlueButtonSprite, assets.Font,
                new Vector2(0.5f, 0.5f), new Vector2(170f, -60f), new Vector2(280f, 80f));
            modal.SetActive(false);
            return modal;
        }

        private static void BindChoicePresenter(
            ChoiceEndingPresenter presenter,
            GameObject choiceRoot,
            Button restore,
            Button quiet,
            GameObject endingRoot,
            Text endingText,
            Button restart,
            Button returnTitle,
            GameObject confirmation,
            Button confirmRestart,
            Button cancelRestart)
        {
            ProjectInstallerSerialization.Edit(presenter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "choiceRoot", choiceRoot);
                ProjectInstallerSerialization.Reference(serialized, "restoreConnectionButton", restore);
                ProjectInstallerSerialization.Reference(serialized, "keepQuietButton", quiet);
                ProjectInstallerSerialization.Reference(serialized, "endingRoot", endingRoot);
                ProjectInstallerSerialization.Reference(serialized, "endingText", endingText);
                ProjectInstallerSerialization.Reference(serialized, "restartButton", restart);
                ProjectInstallerSerialization.Reference(serialized, "returnTitleButton", returnTitle);
                ProjectInstallerSerialization.Reference(serialized, "restartConfirmationRoot", confirmation);
                ProjectInstallerSerialization.Reference(serialized, "confirmRestartButton", confirmRestart);
                ProjectInstallerSerialization.Reference(serialized, "cancelRestartButton", cancelRestart);
            });
        }
    }
}
