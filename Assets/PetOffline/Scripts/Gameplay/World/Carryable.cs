using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class Carryable : MonoBehaviour
    {
        [SerializeField] private CarryableId id;
        [SerializeField] private PlayerCarryStyle carryStyle;
        [SerializeField] private CarryableConfig config;
        [SerializeField] private bool dropOnBark;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Collider2D itemCollider;
        [SerializeField] private SpriteRenderer itemRenderer;

        private Transform originalParent;
        private Vector2 homePosition;
        private Vector3 homeScale;
        private bool available = true;

        public CarryableId Id => id;
        public PlayerCarryStyle CarryStyle => carryStyle;
        public bool DropOnBark => dropOnBark;
        public bool IsHeld { get; private set; }
        public bool IsAvailable => available;
        public float MoveMultiplier => id == CarryableId.Pillow ? config.PillowMoveMultiplier : 1f;

        private void Awake()
        {
            originalParent = transform.parent;
            homePosition = transform.position;
            homeScale = transform.localScale;
            body.gravityScale = 0f;
            body.drag = config.DroppedLinearDrag;
        }

        private void Start()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.BodyCollider != null)
            {
                Physics2D.IgnoreCollision(itemCollider, player.BodyCollider, true);
            }
        }

        public void PickUp(Transform anchor)
        {
            if (!available || IsHeld)
            {
                return;
            }

            IsHeld = true;
            body.simulated = false;
            itemCollider.enabled = false;
            itemRenderer.enabled = carryStyle == PlayerCarryStyle.Standard;
            transform.SetParent(anchor, true);
            transform.position = anchor.position;
        }

        public void Drop(Vector2 position)
        {
            transform.SetParent(originalParent, true);
            transform.position = position;
            transform.localScale = homeScale;
            IsHeld = false;
            itemRenderer.enabled = true;
            itemCollider.enabled = true;
            body.simulated = true;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        public void ResetHome()
        {
            transform.SetParent(originalParent, true);
            transform.position = homePosition;
            transform.localScale = homeScale;
            IsHeld = false;
            itemRenderer.enabled = true;
            itemCollider.enabled = true;
            body.simulated = true;
            body.velocity = Vector2.zero;
        }

        public void SetAvailable(bool value, bool visibleWhenUnavailable = false)
        {
            available = value;
            bool visible = value || visibleWhenUnavailable;
            itemRenderer.enabled = visible && (!IsHeld || carryStyle == PlayerCarryStyle.Standard);
            itemCollider.enabled = value && !IsHeld;
        }
    }
}
