using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TriggerZone : MonoBehaviour
    {
        private const int PlayerLayer = 10;
        private int playerContacts;

        public bool ContainsPlayer => playerContacts > 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == PlayerLayer)
            {
                playerContacts++;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == PlayerLayer)
            {
                playerContacts = Mathf.Max(0, playerContacts - 1);
            }
        }
    }
}
