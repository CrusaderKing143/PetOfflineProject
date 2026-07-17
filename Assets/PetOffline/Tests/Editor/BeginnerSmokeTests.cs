using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PetOffline.Tests
{
    public sealed class BeginnerSmokeTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/StartPanel.unity",
            "Assets/Scenes/Main1.unity",
            "Assets/Scenes/Main2.unity"
        };

        private bool previousEnterPlayModeOptionsEnabled;
        private EnterPlayModeOptions previousEnterPlayModeOptions;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (Application.isPlaying)
            {
                yield return new ExitPlayMode();
            }

            EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
            previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            yield return null;
        }

        [Test]
        public void ScenesHaveExpectedComponentsAndNoMissingScripts()
        {
            System.Type[][] expectedComponents =
            {
                new[] { typeof(GameSession), typeof(GameUI) },
                new[] { typeof(Day1Level), typeof(PlayerController) },
                new[] { typeof(Day2Level), typeof(PlayerController) }
            };

            for (int index = 0; index < ScenePaths.Length; index++)
            {
                string path = ScenePaths[index];
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int missing = scene.GetRootGameObjects().Sum(CountMissingScripts);
                Assert.AreEqual(0, missing, path);
                foreach (System.Type type in expectedComponents[index])
                {
                    bool found = scene.GetRootGameObjects()
                        .Any(root => root.GetComponentInChildren(type, true));
                    Assert.IsTrue(found, path + " 缺少 " + type.Name);
                }
            }
        }

        [UnityTest]
        public IEnumerator TitleLoadsDay1AndReturns()
        {
            yield return new EnterPlayMode();

            GameSession session = Object.FindObjectOfType<GameSession>();
            Assert.IsNotNull(session);
            Assert.IsTrue(session.IsAtTitle);
            session.NewGame();
            yield return WaitFor(() => session.CurrentLevel is Day1Level);
            Assert.IsFalse(session.IsAtTitle);

            session.ReturnTitle();
            yield return WaitFor(() => session.IsAtTitle && !session.IsBusy);
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            if (Application.isPlaying)
            {
                yield return new ExitPlayMode();
            }

            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
        }

        private static IEnumerator WaitFor(System.Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + 8f;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(condition(), "Timed out waiting for the scene transition.");
        }

        private static int CountMissingScripts(GameObject root)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            foreach (Transform child in root.transform)
            {
                count += CountMissingScripts(child.gameObject);
            }

            return count;
        }
    }
}
