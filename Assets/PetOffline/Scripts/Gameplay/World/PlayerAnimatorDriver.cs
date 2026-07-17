using UnityEngine;

namespace PetOffline.Gameplay
{
    public sealed class PlayerAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private int direction = 4;
        private string currentState;

        public void Refresh(
            Vector2 movement,
            bool moving,
            bool running,
            bool lying,
            PlayerCarryStyle carryStyle)
        {
            if (movement.sqrMagnitude > 0.001f)
            {
                direction = ResolveDirection(movement);
            }

            string state = ResolveState(moving, running, lying, carryStyle);
            if (state == currentState)
            {
                return;
            }

            currentState = state;
            animator.Play(state, 0, 0f);
        }

        private string ResolveState(
            bool moving,
            bool running,
            bool lying,
            PlayerCarryStyle carryStyle)
        {
            if (lying)
            {
                return "dog_liedown";
            }

            if (carryStyle == PlayerCarryStyle.Shoes)
            {
                return (moving ? "slipper" : "slipper_idle") + direction;
            }

            if (carryStyle == PlayerCarryStyle.Pillow)
            {
                return (moving ? "pillow" : "pillow_idle") + direction;
            }

            string prefix = running ? "dog_run" : moving ? "dog_walk" : "dog_idle";
            return prefix + direction;
        }

        private static int ResolveDirection(Vector2 movement)
        {
            if (movement.y >= 0f)
            {
                return movement.x < 0f ? 3 : 4;
            }

            return movement.x < 0f ? 1 : 2;
        }
    }
}
