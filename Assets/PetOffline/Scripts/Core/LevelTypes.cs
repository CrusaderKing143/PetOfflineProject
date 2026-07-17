using UnityEngine;

namespace PetOffline
{
    public enum FinalChoice
    {
        None,
        RestoreConnection,
        KeepQuiet
    }

    public enum CameraUiState
    {
        Hidden,
        Safe,
        Scanning,
        Alert,
        Offline
    }

    public readonly struct PlayerInput
    {
        public PlayerInput(
            Vector2 move,
            bool interactPressed,
            bool barkPressed,
            bool dashPressed,
            bool lieHeld)
        {
            Move = move;
            InteractPressed = interactPressed;
            BarkPressed = barkPressed;
            DashPressed = dashPressed;
            LieHeld = lieHeld;
        }

        public Vector2 Move { get; }
        public bool InteractPressed { get; }
        public bool BarkPressed { get; }
        public bool DashPressed { get; }
        public bool LieHeld { get; }

        public static PlayerInput Read()
        {
            Vector2 move = Vector2.ClampMagnitude(
                new Vector2(Axis(KeyCode.A, KeyCode.D), Axis(KeyCode.S, KeyCode.W)),
                1f);
            return new PlayerInput(
                move,
                Input.GetKeyDown(KeyCode.E),
                Input.GetKeyDown(KeyCode.Space),
                Input.GetKeyDown(KeyCode.Q),
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        private static float Axis(KeyCode negative, KeyCode positive)
        {
            float value = Input.GetKey(positive) ? 1f : 0f;
            return Input.GetKey(negative) ? value - 1f : value;
        }
    }
}
