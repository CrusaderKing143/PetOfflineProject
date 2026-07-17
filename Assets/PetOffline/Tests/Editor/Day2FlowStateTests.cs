using NUnit.Framework;
using PetOffline.Core;
using PetOffline.Gameplay;

namespace PetOffline.Tests
{
    public sealed class Day2FlowStateTests
    {
        [Test]
        public void BackupLesson_RequiresSideDoorThenConfirmationBeforeFinalSun()
        {
            var state = CreateBackupLessonState();

            Assert.IsFalse(state.StartBackupConfirmation());
            Assert.IsFalse(state.CompleteBackupLesson());
            Assert.IsFalse(state.CompleteFinalSun());
            Assert.AreEqual(Day2Phase.BackupLesson, state.Phase);

            Assert.IsTrue(state.ActivateBackupCamera());
            Assert.IsTrue(state.BackupCameraActivated);
            Assert.IsFalse(state.CompleteBackupLesson());
            Assert.IsFalse(state.CompleteFinalSun());

            Assert.IsTrue(state.StartBackupConfirmation());
            Assert.IsTrue(state.BackupConfirmationStarted);
            Assert.IsTrue(state.CompleteBackupLesson());
            Assert.AreEqual(Day2Phase.FinalSun, state.Phase);
        }

        [Test]
        public void CompleteFlow_AllowsOnlyOneFinalChoice()
        {
            var state = CreateBackupLessonState();

            Assert.IsTrue(state.ActivateBackupCamera());
            Assert.IsTrue(state.StartBackupConfirmation());
            Assert.IsTrue(state.CompleteBackupLesson());
            Assert.IsTrue(state.CompleteFinalSun());
            Assert.IsTrue(state.ContinueReport());
            Assert.AreEqual(Day2Phase.Choice, state.Phase);

            Assert.IsFalse(state.SubmitChoice(FinalChoice.None));
            Assert.AreEqual(Day2Phase.Choice, state.Phase);
            Assert.IsTrue(state.SubmitChoice(FinalChoice.KeepQuiet));
            Assert.AreEqual(Day2Phase.End, state.Phase);
            Assert.AreEqual(FinalChoice.KeepQuiet, state.Choice);

            Assert.IsFalse(state.SubmitChoice(FinalChoice.RestoreConnection));
            Assert.AreEqual(Day2Phase.End, state.Phase);
            Assert.AreEqual(FinalChoice.KeepQuiet, state.Choice);
        }

        private static Day2FlowState CreateBackupLessonState()
        {
            var state = new Day2FlowState();
            Assert.IsTrue(state.FinishStart());
            Assert.IsTrue(state.ReachFirstConfirmation());
            Assert.IsTrue(state.ReturnToFeeder());
            Assert.IsTrue(state.DisableMainCamera());
            Assert.AreEqual(Day2Phase.BackupLesson, state.Phase);
            return state;
        }
    }
}
