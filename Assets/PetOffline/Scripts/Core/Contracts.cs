using System;

namespace PetOffline.Core
{
    public interface IGameCommandSink
    {
        void NewGame();

        void Continue();

        void ContinueReport();

        void SubmitChoice(FinalChoice choice);

        void ReturnTitle();

        void Restart();
    }

    public interface ILevelViewModel
    {
        LevelId Level { get; }

        LevelUiMode UiMode { get; }

        string Objective { get; }

        float Progress01 { get; }

        string ProgressLabel { get; }

        int CompletedTasks { get; }

        int TotalTasks { get; }

        int TaskMask { get; }

        CameraUiState CameraState { get; }

        string DialogueSpeaker { get; }

        string DialogueText { get; }

        string ReportId { get; }

        string EndingId { get; }

        event Action Changed;
    }

    public interface ILevelRuntime
    {
        LevelId Level { get; }

        ILevelViewModel ViewModel { get; }

        void Bind(LevelRuntimeContext context);

        void Unbind();

        void HandleInput(LevelInputFrame input);

        bool TryHandleCommand(GameCommand command);
    }

    public interface ILevelHost
    {
        bool IsPaused { get; }

        bool IsInputLocked { get; }

        void SetPaused(bool paused);

        void CompleteLevel(LevelId level, FinalChoice choice);
    }

    public interface IGameSessionView
    {
        bool IsAtTitle { get; }

        bool IsBusy { get; }

        bool Paused { get; }

        bool CanContinue { get; }

        bool CanRouteInput { get; }

        string LastError { get; }

        LevelId CurrentLevel { get; }

        ILevelViewModel CurrentViewModel { get; }

        event Action Changed;
    }
}
