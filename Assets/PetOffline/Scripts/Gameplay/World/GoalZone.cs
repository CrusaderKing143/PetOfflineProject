using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GoalZone : MonoBehaviour
    {
        private Collider2D zoneCollider;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider2D>();
        }

        public bool Contains(Carryable item)
        {
            return zoneCollider.OverlapPoint(item.transform.position);
        }
    }
}
