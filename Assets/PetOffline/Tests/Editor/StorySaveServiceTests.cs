using System;
using NUnit.Framework;
using PetOffline.Core;
using UnityEngine;

namespace PetOffline.Tests
{
    public sealed class StorySaveServiceTests
    {
        private string _saveKey;

        [SetUp]
        public void SetUp()
        {
            _saveKey = "PetOffline.Tests." + Guid.NewGuid().ToString("N");
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(_saveKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void UnsupportedVersion_IsDiscardedAsEmptyProgress()
        {
            int unsupportedVersion = StorySaveService.CurrentVersion + 1;
            PlayerPrefs.SetString(
                _saveKey,
                "{\"version\":" + unsupportedVersion
                + ",\"day1Complete\":true,\"day2Complete\":false,\"finalChoice\":0}");

            var service = new StorySaveService(_saveKey);
            StoryProgress progress = service.Load();

            Assert.IsFalse(progress.HasProgress);
            Assert.IsFalse(progress.Day1Complete);
            Assert.IsFalse(progress.Day2Complete);
            Assert.AreEqual(FinalChoice.None, progress.FinalChoice);
            Assert.IsFalse(PlayerPrefs.HasKey(_saveKey));
        }

        [Test]
        public void Day1Completion_PersistsTheProgressRequiredForContinue()
        {
            var service = new StorySaveService(_saveKey);

            Assert.IsFalse(service.Load().Day1Complete);
            Assert.IsTrue(service.MarkDay1Complete());
            Assert.IsFalse(service.MarkDay1Complete());

            var reloadedService = new StorySaveService(_saveKey);
            StoryProgress progress = reloadedService.Load();

            Assert.IsTrue(progress.Day1Complete);
            Assert.IsFalse(progress.Day2Complete);
            Assert.AreEqual(FinalChoice.None, progress.FinalChoice);
        }

        [Test]
        public void CompletedStory_RemainsEligibleForContinueAndKeepsChoice()
        {
            var service = new StorySaveService(_saveKey);

            Assert.IsTrue(service.MarkDay2Complete(FinalChoice.RestoreConnection));

            var reloadedService = new StorySaveService(_saveKey);
            StoryProgress progress = reloadedService.Load();

            Assert.IsTrue(progress.Day1Complete);
            Assert.IsTrue(progress.Day2Complete);
            Assert.AreEqual(FinalChoice.RestoreConnection, progress.FinalChoice);
        }
    }
}
