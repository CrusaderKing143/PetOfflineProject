using System;
using UnityEngine;

namespace PetOffline.Core
{
    public enum LevelId
    {
        None = 0,
        Day1 = 1,
        Day2 = 2
    }

    public enum LevelUiMode
    {
        Title = 0,
        Gameplay = 1,
        Dialogue = 2,
        Report = 3,
        Choice = 4,
        Ending = 5
    }

    public enum FinalChoice
    {
        None = 0,
        RestoreConnection = 1,
        KeepQuiet = 2
    }

    public enum CameraUiState
    {
        Hidden = 0,
        Safe = 1,
        Scanning = 2,
        Alert = 3,
        Offline = 4
    }

    public enum GameCommandType
    {
        NewGame = 0,
        Continue = 1,
        ContinueReport = 2,
        SubmitChoice = 3,
        ReturnTitle = 4,
        Restart = 5
    }

    public readonly struct GameCommand : IEquatable<GameCommand>
    {
        public GameCommand(GameCommandType type, FinalChoice choice = FinalChoice.None)
        {
            Type = type;
            Choice = choice;
        }

        public GameCommandType Type { get; }

        public FinalChoice Choice { get; }

        public bool Equals(GameCommand other)
        {
            return Type == other.Type && Choice == other.Choice;
        }

        public override bool Equals(object obj)
        {
            return obj is GameCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Type * 397) ^ (int)Choice;
        }

        public static bool operator ==(GameCommand left, GameCommand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameCommand left, GameCommand right)
        {
            return !left.Equals(right);
        }

        public static GameCommand ContinueReport =>
            new GameCommand(GameCommandType.ContinueReport);

        public static GameCommand ForChoice(FinalChoice choice)
        {
            return new GameCommand(GameCommandType.SubmitChoice, choice);
        }
    }

    public readonly struct LevelInputFrame
    {
        public LevelInputFrame(
            Vector2 move,
            bool interactPressed,
            bool barkPressed,
            bool dashPressed,
            bool lieHeld,
            bool pausePressed)
        {
            Move = move;
            InteractPressed = interactPressed;
            BarkPressed = barkPressed;
            DashPressed = dashPressed;
            LieHeld = lieHeld;
            PausePressed = pausePressed;
        }

        public Vector2 Move { get; }

        public bool InteractPressed { get; }

        public bool BarkPressed { get; }

        public bool DashPressed { get; }

        public bool LieHeld { get; }

        public bool PausePressed { get; }

        public static LevelInputFrame Empty =>
            new LevelInputFrame(Vector2.zero, false, false, false, false, false);
    }
}
