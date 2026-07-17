using UnityEngine;

namespace PetOffline
{
    public enum PlayerCarryStyle
    {
        Standard,
        Shoes,
        Pillow
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class Carryable : MonoBehaviour
    {
        [SerializeField] private PlayerCarryStyle carryStyle;
        [SerializeField] private bool dropOnBark;
        [SerializeField, Range(0.1f, 1f)] private float pillowMoveMultiplier = 0.6f;
        [SerializeField, Min(0f)] private float droppedLinearDrag = 2f;

        private Rigidbody2D body;
        private Collider2D itemCollider;
        private SpriteRenderer itemRenderer;
        private Transform originalParent;
        private Vector2 homePosition;
        private Vector3 homeScale;
        private bool available = true;

        public PlayerCarryStyle CarryStyle => carryStyle;
        public bool DropOnBark => dropOnBark;
        public bool IsHeld { get; private set; }
        public bool IsAvailable => available;
        public float MoveMultiplier => carryStyle == PlayerCarryStyle.Pillow
            ? pillowMoveMultiplier
            : 1f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            itemCollider = GetComponent<Collider2D>();
            itemRenderer = GetComponent<SpriteRenderer>();
            originalParent = transform.parent;
            homePosition = transform.position;
            homeScale = transform.localScale;
            body.gravityScale = 0f;
            body.drag = droppedLinearDrag;
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
            body.angularVelocity = 0f;
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
