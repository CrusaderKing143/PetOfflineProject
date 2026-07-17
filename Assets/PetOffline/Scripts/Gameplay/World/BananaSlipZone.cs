using System;
using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BananaSlipZone : MonoBehaviour
    {
        private const int PlayerLayer = 10;
        private const int RobotLayer = 12;

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
            if (!armed || banana.IsHeld || !banana.IsAvailable)
            {
                return;
            }

            if (other.gameObject.layer == PlayerLayer)
            {
                other.GetComponentInParent<PlayerController>().StartSlide(
                    playerSlideDirection, playerSlideSeconds, playerSlideSpeed);
            }
            else if (other.gameObject.layer == RobotLayer)
            {
                armed = false;
                RobotSlipped?.Invoke(other.GetComponentInParent<RobotPatrol>());
            }
        }
    }
}
