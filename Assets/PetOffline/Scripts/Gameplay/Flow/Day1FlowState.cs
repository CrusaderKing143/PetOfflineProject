namespace PetOffline.Gameplay
{
    public sealed class Day1FlowState
    {
        public Day1Phase Phase { get; private set; } = Day1Phase.Opening;
        public int CompletedTasks { get; private set; }
        public int TaskMask { get; private set; }

        public bool FinishOpening()
        {
            return Move(Day1Phase.Opening, Day1Phase.Shoes);
        }

        public bool CompleteShoes()
        {
            if (!Move(Day1Phase.Shoes, Day1Phase.Pillow))
            {
                return false;
            }

            MarkTask(0);
            return true;
        }

        public bool CompletePillow()
        {
            if (!Move(Day1Phase.Pillow, Day1Phase.FinalBark))
            {
                return false;
            }

            MarkTask(1);
            return true;
        }

        public bool CompleteFinalBark()
        {
            if (!Move(Day1Phase.FinalBark, Day1Phase.Report))
            {
                return false;
            }

            MarkTask(2);
            return true;
        }

        public bool ContinueReport()
        {
            return Move(Day1Phase.Report, Day1Phase.Ending);
        }

        public bool FinishEnding()
        {
            return Move(Day1Phase.Ending, Day1Phase.Complete);
        }

        private bool Move(Day1Phase expected, Day1Phase next)
        {
            if (Phase != expected)
            {
                return false;
            }

            Phase = next;
            return true;
        }

        private void MarkTask(int index)
        {
            TaskMask |= 1 << index;
            CompletedTasks++;
        }
    }
}
