using NUnit.Framework;
using PetOffline.Gameplay;

namespace PetOffline.Tests
{
    public sealed class Day1FlowStateTests
    {
        [Test]
        public void CompleteFlow_FollowsTheFullLegalSequence()
        {
            var state = new Day1FlowState();

            Assert.AreEqual(Day1Phase.Opening, state.Phase);
            Assert.IsTrue(state.FinishOpening());
            Assert.AreEqual(Day1Phase.Shoes, state.Phase);

            Assert.IsTrue(state.CompleteShoes());
            Assert.AreEqual(Day1Phase.Pillow, state.Phase);
            Assert.AreEqual(1, state.CompletedTasks);
            Assert.AreEqual(0b001, state.TaskMask);

            Assert.IsTrue(state.CompletePillow());
            Assert.AreEqual(Day1Phase.FinalBark, state.Phase);
            Assert.AreEqual(2, state.CompletedTasks);
            Assert.AreEqual(0b011, state.TaskMask);

            Assert.IsTrue(state.CompleteFinalBark());
            Assert.AreEqual(Day1Phase.Report, state.Phase);
            Assert.AreEqual(3, state.CompletedTasks);
            Assert.AreEqual(0b111, state.TaskMask);

            Assert.IsTrue(state.ContinueReport());
            Assert.AreEqual(Day1Phase.Ending, state.Phase);
            Assert.IsTrue(state.FinishEnding());
            Assert.AreEqual(Day1Phase.Complete, state.Phase);
        }

        [Test]
        public void RepeatedOrOutOfOrderCommands_DoNotAdvanceOrCountAgain()
        {
            var state = new Day1FlowState();

            Assert.IsFalse(state.CompleteShoes());
            Assert.AreEqual(Day1Phase.Opening, state.Phase);
            Assert.AreEqual(0, state.CompletedTasks);
            Assert.AreEqual(0, state.TaskMask);

            Assert.IsTrue(state.FinishOpening());
            Assert.IsTrue(state.CompleteShoes());
            Assert.IsFalse(state.CompleteShoes());
            Assert.IsFalse(state.CompleteFinalBark());

            Assert.AreEqual(Day1Phase.Pillow, state.Phase);
            Assert.AreEqual(1, state.CompletedTasks);
            Assert.AreEqual(0b001, state.TaskMask);
        }
    }
}
