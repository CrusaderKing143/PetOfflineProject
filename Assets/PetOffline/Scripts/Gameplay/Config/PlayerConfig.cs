using UnityEngine;

namespace PetOffline.Gameplay
{
    [CreateAssetMenu(menuName = "Pet Offline/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.25f;
        [SerializeField, Min(1f)] private float dashSpeedMultiplier = 2.5f;
        [SerializeField, Min(0f)] private float dashCooldown = 1f;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1f;
        [SerializeField, Range(0.1f, 1f)] private float isometricVerticalScale = 0.5f;

        public float MoveSpeed => moveSpeed;
        public float DashDuration => dashDuration;
        public float DashSpeedMultiplier => dashSpeedMultiplier;
        public float DashCooldown => dashCooldown;
        public float InteractionRadius => interactionRadius;
        public float IsometricVerticalScale => isometricVerticalScale;
    }
}
