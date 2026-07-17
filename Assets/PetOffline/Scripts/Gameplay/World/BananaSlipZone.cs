using System;
using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BananaSlipZone : MonoBehaviour
    {
        [SerializeField] private Carryable banana;
        [SerializeField] private Vector2 playerSlideDirection = Vector2.right;
        [SerializeField, Min(0.1f)] private float playerSlideSeconds = 0.7f;
        [SerializeField, Min(0.1f)] private float playerSlideSpeed = 5f;
        private bool armed = true;

        public event Action<RobotPatrol> RobotSlipped;

        public void SetArmed(bool value)
        {
            armed = value;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!armed)
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null && BananaIsPlaced())
            {
                player.StartSlide(playerSlideDirection, playerSlideSeconds, playerSlideSpeed);
                return;
            }

            RobotPatrol robot = other.GetComponentInParent<RobotPatrol>();
            if (robot != null && banana != null && BananaIsPlaced())
            {
                armed = false;
                RobotSlipped?.Invoke(robot);
            }
        }

        private bool BananaIsPlaced()
        {
            return banana == null || (!banana.IsHeld && banana.IsAvailable);
        }
    }
}
