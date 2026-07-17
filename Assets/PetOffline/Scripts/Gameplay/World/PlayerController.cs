using System;
using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerAnimatorDriver))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.25f;
        [SerializeField, Min(1f)] private float dashSpeedMultiplier = 2.5f;
        [SerializeField, Min(0f)] private float dashCooldown = 1f;
        [SerializeField, Range(0.1f, 1f)] private float isometricVerticalScale = 0.5f;

        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float interactionRadius = 1f;
        [SerializeField, Min(0.1f)] private float dropDistance = 0.65f;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private LayerMask interactionMask = ~0;

        private readonly RaycastHit2D[] dashHits = new RaycastHit2D[12];
        private Rigidbody2D body;
        private PlayerAnimatorDriver animatorDriver;
        private Vector2 moveDirection;
        private Vector2 lastDirection = Vector2.down;
        private float dashRemaining;
        private float dashCooldownRemaining;
        private float slideRemaining;
        private float slideSpeed;
        private Vector2 slideDirection;
        private Vector2[] autoPath = Array.Empty<Vector2>();
        private int autoPathIndex;
        private Action autoComplete;

        public Carryable HeldItem { get; private set; }
        public bool IsLying { get; private set; }
        public bool IsSliding => slideRemaining > 0f;
        public bool IsInCutscene => autoPathIndex < autoPath.Length;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animatorDriver = GetComponent<PlayerAnimatorDriver>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            dashRemaining = Mathf.Max(0f, dashRemaining - Time.deltaTime);
            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - Time.deltaTime);
            slideRemaining = Mathf.Max(0f, slideRemaining - Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (MoveAlongAutoPath())
            {
                return;
            }

            Vector2 velocity = ResolveVelocity();
            MoveBody(velocity * Time.fixedDeltaTime, dashRemaining > 0f);
            RefreshAnimation(velocity);
        }

        public void ApplyInput(PlayerInput input, bool movementAllowed)
        {
            if (!movementAllowed || IsInCutscene)
            {
                moveDirection = Vector2.zero;
                IsLying = false;
                return;
            }

            if (input.LieHeld && !IsSliding)
            {
                moveDirection = Vector2.zero;
                IsLying = true;
                return;
            }

            IsLying = false;
            moveDirection = ToIsometric(input.Move);
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                lastDirection = moveDirection.normalized;
            }

            if (input.DashPressed)
            {
                TryDash();
            }
        }

        public bool TryToggleCarry()
        {
            if (HeldItem != null)
            {
                DropHeldItem();
                return true;
            }

            Carryable nearest = FindNearestCarryable();
            if (!nearest)
            {
                return false;
            }

            HeldItem = nearest;
            nearest.PickUp(carryAnchor);
            return true;
        }

        public void Bark()
        {
            if (HeldItem != null && HeldItem.DropOnBark)
            {
                DropHeldItem();
            }
        }

        public void DropHeldItem()
        {
            if (HeldItem == null)
            {
                return;
            }

            Vector2 position = body.position + lastDirection * dropDistance;
            HeldItem.Drop(position);
            HeldItem = null;
        }

        public void ResetTo(Vector2 position)
        {
            moveDirection = Vector2.zero;
            dashRemaining = 0f;
            slideRemaining = 0f;
            autoPath = Array.Empty<Vector2>();
            autoPathIndex = 0;
            body.position = position;
            body.velocity = Vector2.zero;
        }

        public void StartSlide(Vector2 direction, float duration, float speed)
        {
            if (IsInCutscene)
            {
                return;
            }

            slideDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : lastDirection;
            slideRemaining = duration;
            slideSpeed = speed;
            IsLying = false;
        }

        public void BeginAutoMove(Vector2[] path, Action onComplete)
        {
            autoPath = path;
            autoPathIndex = 0;
            autoComplete = onComplete;
            DropHeldItem();
            IsLying = false;
        }

        public void SetCutsceneLie(bool value)
        {
            moveDirection = Vector2.zero;
            dashRemaining = 0f;
            IsLying = value;
        }

        private Vector2 ResolveVelocity()
        {
            if (IsSliding)
            {
                return slideDirection * slideSpeed;
            }

            if (IsLying)
            {
                return Vector2.zero;
            }

            float multiplier = HeldItem == null ? 1f : HeldItem.MoveMultiplier;
            float dash = dashRemaining > 0f ? dashSpeedMultiplier : 1f;
            return moveDirection * moveSpeed * multiplier * dash;
        }

        private void TryDash()
        {
            bool blocked = HeldItem != null || IsLying || IsSliding || IsInCutscene;
            if (blocked || dashCooldownRemaining > 0f || moveDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            dashRemaining = dashDuration;
            dashCooldownRemaining = dashCooldown;
        }

        private Vector2 ToIsometric(Vector2 input)
        {
            Vector2 result = new Vector2(
                input.x - input.y,
                (input.x + input.y) * isometricVerticalScale);
            return result.sqrMagnitude > 1f ? result.normalized : result;
        }

        private void MoveBody(Vector2 delta, bool stopAtTriggers)
        {
            if (!stopAtTriggers || delta.sqrMagnitude < 0.0001f)
            {
                body.MovePosition(body.position + delta);
                return;
            }

            float distance = delta.magnitude;
            int count = body.Cast(delta.normalized, dashHits, distance);
            for (int index = 0; index < count; index++)
            {
                distance = Mathf.Min(distance, Mathf.Max(0f, dashHits[index].distance - 0.02f));
            }

            body.MovePosition(body.position + delta.normalized * distance);
        }

        private Carryable FindNearestCarryable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                body.position, interactionRadius, interactionMask);
            Carryable nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                Carryable item = hit.GetComponentInParent<Carryable>();
                if (!item || !item.IsAvailable || item.IsHeld)
                {
                    continue;
                }

                float distance = ((Vector2)item.transform.position - body.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = item;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private bool MoveAlongAutoPath()
        {
            if (!IsInCutscene)
            {
                return false;
            }

            Vector2 target = autoPath[autoPathIndex];
            Vector2 delta = target - body.position;
            if (delta.magnitude < 0.08f)
            {
                AdvanceAutoPath();
                return true;
            }

            Vector2 velocity = delta.normalized * moveSpeed;
            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
            lastDirection = velocity.normalized;
            RefreshAnimation(velocity);
            return true;
        }

        private void AdvanceAutoPath()
        {
            autoPathIndex++;
            if (autoPathIndex < autoPath.Length)
            {
                return;
            }

            autoPath = Array.Empty<Vector2>();
            autoPathIndex = 0;
            Action callback = autoComplete;
            autoComplete = null;
            callback?.Invoke();
        }

        private void RefreshAnimation(Vector2 velocity)
        {
            PlayerCarryStyle style = HeldItem == null
                ? PlayerCarryStyle.Standard
                : HeldItem.CarryStyle;
            animatorDriver.Refresh(
                lastDirection,
                velocity.sqrMagnitude > 0.001f,
                dashRemaining > 0f,
                IsLying,
                style);
        }
    }
}
