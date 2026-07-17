using System;
using System.Collections.Generic;
using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GoalZone : MonoBehaviour
    {
        [SerializeField] private Collider2D zoneCollider;
        private readonly HashSet<Carryable> containedItems = new HashSet<Carryable>();

        public event Action<Carryable> ItemEntered;
        public event Action<Carryable> ItemExited;

        public bool Contains(Carryable item)
        {
            return item != null &&
                   (containedItems.Contains(item) || zoneCollider.OverlapPoint(item.transform.position));
        }

        public void ResetZone()
        {
            containedItems.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Carryable item = other.GetComponentInParent<Carryable>();
            if (item != null && containedItems.Add(item))
            {
                ItemEntered?.Invoke(item);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Carryable item = other.GetComponentInParent<Carryable>();
            if (item != null && containedItems.Remove(item))
            {
                ItemExited?.Invoke(item);
            }
        }
    }
}
