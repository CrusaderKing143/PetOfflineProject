using UnityEngine;

namespace PetOffline.Core
{
    [DisallowMultipleComponent]
    public sealed class LegacyInputRouter : MonoBehaviour
    {
        [SerializeField] private GameSession session;

        private void Awake()
        {
            if (session == null)
            {
                session = GetComponent<GameSession>();
            }
        }

        private void Update()
        {
            if (session == null || !session.CanRouteInput)
            {
                return;
            }

            session.RouteInput(ReadInput());
        }

        private static LevelInputFrame ReadInput()
        {
            float horizontal = ReadAxis(KeyCode.A, KeyCode.D);
            float vertical = ReadAxis(KeyCode.S, KeyCode.W);
            Vector2 move = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

            return new LevelInputFrame(
                move,
                Input.GetKeyDown(KeyCode.E),
                Input.GetKeyDown(KeyCode.Space),
                Input.GetKeyDown(KeyCode.Q),
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                Input.GetKeyDown(KeyCode.Escape));
        }

        private static float ReadAxis(KeyCode negative, KeyCode positive)
        {
            float value = Input.GetKey(positive) ? 1f : 0f;
            return Input.GetKey(negative) ? value - 1f : value;
        }
    }
}
