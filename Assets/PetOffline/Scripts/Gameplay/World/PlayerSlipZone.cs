using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerSlipZone : MonoBehaviour
    {
        private const int PlayerLayer = 10;

        [SerializeField] private Vector2 slideDirection = Vector2.right;
        [SerializeField, Min(0.1f)] private float slideSeconds = 0.7f;
        [SerializeField, Min(0.1f)] private float slideSpeed = 5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == PlayerLayer)
            {
                other.GetComponentInParent<PlayerController>()
                    .StartSlide(slideDirection, slideSeconds, slideSpeed);
            }
        }
    }
}
