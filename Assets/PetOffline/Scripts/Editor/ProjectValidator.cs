using System;
using System.Collections.Generic;
using PetOffline.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PetOffline.Editor
{
    public static partial class ProjectValidator
    {
        private const string MenuPath = "Pet Offline/Validate Project";
        private const string StartPanelPath = "Assets/Scenes/StartPanel.unity";
        private const string Main1Path = "Assets/Scenes/Main1.unity";
        private const string Main2Path = "Assets/Scenes/Main2.unity";

        private static readonly string[] ScenePaths =
        {
            StartPanelPath,
            Main1Path,
            Main2Path
        };

        private static readonly string[] ExpectedAssemblyNames =
        {
            "PetOffline.Core",
            "PetOffline.Gameplay",
            "PetOffline.UI",
            "PetOffline.Editor",
            "PetOffline.Tests.Editor"
        };

        private static readonly string[] StartPanelComponentNames =
        {
            "PetOffline.Core.GameSession",
            "PetOffline.Core.SceneFlowService",
            "PetOffline.Core.LegacyInputRouter",
            "PetOffline.UI.TitlePresenter",
            "PetOffline.UI.HudPresenter",
            "PetOffline.UI.DialoguePresenter",
            "PetOffline.UI.ReportPresenter",
            "PetOffline.UI.ChoiceEndingPresenter",
            "PetOffline.UI.PausePresenter"
        };

        private static readonly HashSet<string> ReferenceOwnerNames =
            new HashSet<string>(StartPanelComponentNames, StringComparer.Ordinal)
            {
                "PetOffline.Gameplay.RobotPatrol",
                "PetOffline.Gameplay.PlayerController",
                "PetOffline.Gameplay.PlayerAnimatorDriver",
                "PetOffline.Gameplay.Carryable",
                "PetOffline.Gameplay.CameraSensor",
                "PetOffline.Gameplay.GoalZone",
                "PetOffline.Gameplay.BananaSlipZone",
                "PetOffline.Gameplay.YAxisSorter"
            };

        [MenuItem(MenuPath)]
        public static void ValidateProject()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Pet Offline Validator] Validation cancelled because open scenes were not saved.");
                return;
            }

            RunAndReport(false);
        }

        public static void ValidateProjectBatchMode()
        {
            RunAndReport(true);
        }

        public static void ValidateOrThrow()
        {
            RunAndReport(true);
        }

        public static IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            ValidateAssemblies(errors);
            ValidateBuildSettings(errors);
            ValidateSceneAssets(errors);
            return errors;
        }

        private static void RunAndReport(bool throwOnFailure)
        {
            IReadOnlyList<string> errors = CollectValidationErrors();
            LogResults(errors);
            if (throwOnFailure && errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Pet Offline project validation failed:\n" + string.Join("\n", errors));
            }
        }

        private static void LogResults(IReadOnlyList<string> errors)
        {
            if (errors.Count == 0)
            {
                Debug.Log("[Pet Offline Validator] Validation passed.");
                return;
            }

            for (var index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[Pet Offline Validator] ERROR {index + 1}: {errors[index]}");
            }

            Debug.LogError($"[Pet Offline Validator] Validation failed with {errors.Count} error(s).");
        }
    }
}
