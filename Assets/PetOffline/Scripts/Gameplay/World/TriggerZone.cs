using System;
using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TriggerZone : MonoBehaviour
    {
        private int playerContacts;

        public bool ContainsPlayer => playerContacts > 0;
        public event Action<PlayerController> PlayerEntered;
        public event Action<PlayerController> PlayerExited;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }

            playerContacts++;
            PlayerEntered?.Invoke(player);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }

            playerContacts = Mathf.Max(0, playerContacts - 1);
            PlayerExited?.Invoke(player);
        }
    }
}
