using System;

namespace PetOffline.Core
{
    public readonly struct StoryProgress
    {
        public StoryProgress(
            bool day1Complete,
            bool day2Complete,
            FinalChoice finalChoice)
        {
            Day1Complete = day1Complete;
            Day2Complete = day2Complete;
            FinalChoice = finalChoice;
        }

        public bool Day1Complete { get; }

        public bool Day2Complete { get; }

        public FinalChoice FinalChoice { get; }

        public bool HasProgress => Day1Complete || Day2Complete;

        public static StoryProgress Empty =>
            new StoryProgress(false, false, FinalChoice.None);
    }

    public sealed class LevelRuntimeContext
    {
        public LevelRuntimeContext(ILevelHost host, StoryProgress story)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Story = story;
        }

        public ILevelHost Host { get; }

        public StoryProgress Story { get; }
    }
}
