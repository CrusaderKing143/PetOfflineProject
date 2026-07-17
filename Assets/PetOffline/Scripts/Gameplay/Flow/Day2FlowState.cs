using PetOffline.Core;

namespace PetOffline.Gameplay
{
    public sealed class Day2FlowState
    {
        public Day2Phase Phase { get; private set; } = Day2Phase.Start;
        public bool BackupCameraActivated { get; private set; }
        public bool BackupConfirmationStarted { get; private set; }
        public FinalChoice Choice { get; private set; }

        public bool FinishStart()
        {
            return Move(Day2Phase.Start, Day2Phase.FirstSun);
        }

        public bool ReachFirstConfirmation()
        {
            return Move(Day2Phase.FirstSun, Day2Phase.Confirmation);
        }

        public bool ReturnToFeeder()
        {
            return Move(Day2Phase.Confirmation, Day2Phase.DisableCamera);
        }

        public bool DisableMainCamera()
        {
            return Move(Day2Phase.DisableCamera, Day2Phase.BackupLesson);
        }

        public bool ActivateBackupCamera()
        {
            if (Phase != Day2Phase.BackupLesson)
            {
                return false;
            }

            BackupCameraActivated = true;
            return true;
        }

        public bool StartBackupConfirmation()
        {
            if (Phase != Day2Phase.BackupLesson || !BackupCameraActivated)
            {
                return false;
            }

            BackupConfirmationStarted = true;
            return true;
        }

        public bool CompleteBackupLesson()
        {
            if (!BackupConfirmationStarted)
            {
                return false;
            }

            return Move(Day2Phase.BackupLesson, Day2Phase.FinalSun);
        }

        public bool CompleteFinalSun()
        {
            return Move(Day2Phase.FinalSun, Day2Phase.Report);
        }

        public bool ContinueReport()
        {
            return Move(Day2Phase.Report, Day2Phase.Choice);
        }

        public bool SubmitChoice(FinalChoice choice)
        {
            if (Phase != Day2Phase.Choice || choice == FinalChoice.None)
            {
                return false;
            }

            Choice = choice;
            Phase = Day2Phase.End;
            return true;
        }

        private bool Move(Day2Phase expected, Day2Phase next)
        {
            if (Phase != expected)
            {
                return false;
            }

            Phase = next;
            return true;
        }
    }
}
