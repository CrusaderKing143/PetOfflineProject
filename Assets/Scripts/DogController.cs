using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public sealed class DogController : MonoBehaviour
{
    private const string IgnoredCollisionTag = "Mission";
    private const float CollisionSkin = 0.01f;
    private const float MinimumBlockingDot = 0.001f;
    private const float MinimumMoveDistance = 0.0001f;
    private const float SlipperScaleMultiplier = 1.2f;
    private const float PillowScaleMultiplier = 1f;
    private const int MaxCollisionIterations = 3;
    private const int DownLeft = 1;
    private const int DownRight = 2;
    private const int UpLeft = 3;
    private const int UpRight = 4;

    private static readonly KeyCode[] DirectionKeys =
    {
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D
    };

    private static readonly Vector2[] DirectionVectors =
    {
        Vector2.zero,
        new Vector2(-1f, -0.5f),
        new Vector2(1f, -0.5f),
        new Vector2(-1f, 0.5f),
        new Vector2(1f, 0.5f)
    };

    [SerializeField] private Animator animator;
    [SerializeField, Min(0f)] private float walkSpeed = 2f;
    [SerializeField, Min(0f)] private float runSpeed = 4f;
    [SerializeField, Min(0f)] private float interactionRadius = 1.2f;
    [SerializeField, Min(0f)] private float dropDistance = 0.8f;
    [SerializeField] private Slider staminaSlider;
    [SerializeField, Min(0.1f)] private float staminaDuration = 6f;

    private readonly List<KeyCode> heldDirectionKeys = new List<KeyCode>(4);
    private readonly RaycastHit2D[] collisionHits = new RaycastHit2D[4];
    private DogCarryItemPickup carriedPickup;
    private DogCarryItemType carryItemType;
    private BoxCollider2D movementCollider;
    private ContactFilter2D movementFilter;
    private int currentStateHash;
    private int facingDirection = DownRight;
    private bool isLyingDown;
    private bool runMode;
    private float stamina = 1f;
    private Vector3 baseScale;

    public DogCarryItemType CarryItemType => carryItemType;
    public event Action<DogCarryItemPickup> ItemDropped;

    private void Awake()
    {
        baseScale = transform.localScale;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        movementCollider = GetComponent<BoxCollider2D>();
        movementFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer),
            useTriggers = false
        };

        if (staminaSlider != null)
        {
            staminaSlider.SetValueWithoutNotify(stamina);
        }
    }

    private void Start()
    {
        UpdateAnimation(false);
    }

    private void Update()
    {
        UpdateHeldDirectionKeys();
        Vector2 movement = ReadMovement();
        bool isMoving = movement.sqrMagnitude > 0f;

        if (isMoving)
        {
            isLyingDown = false;
        }

        HandleStateKeys(isMoving);
        HandleInteractionKey();
        bool isRunning = IsRunning(isMoving);
        UpdateStamina(isRunning);
        if (isRunning && stamina <= 0f)
        {
            runMode = false;
            isRunning = false;
        }

        Move(movement, isRunning);
        UpdateAnimation(isMoving, isRunning);
    }

    public void SetCarryItem(DogCarryItemType itemType)
    {
        carriedPickup = null;
        ApplyCarryItem(itemType);
    }

    public void ClearCarryItem()
    {
        carriedPickup = null;
        carryItemType = DogCarryItemType.None;
        ApplyCarryScale(carryItemType);
    }

    private void UpdateHeldDirectionKeys()
    {
        foreach (KeyCode key in DirectionKeys)
        {
            if (!Input.GetKey(key))
            {
                heldDirectionKeys.Remove(key);
                continue;
            }

            if (Input.GetKeyDown(key) || !heldDirectionKeys.Contains(key))
            {
                heldDirectionKeys.Remove(key);
                heldDirectionKeys.Add(key);
            }
        }

        if (heldDirectionKeys.Count > 0)
        {
            facingDirection = DirectionForKey(heldDirectionKeys[heldDirectionKeys.Count - 1]);
        }
    }

    private Vector2 ReadMovement()
    {
        if (heldDirectionKeys.Count == 0)
        {
            return Vector2.zero;
        }

        KeyCode activeKey = heldDirectionKeys[heldDirectionKeys.Count - 1];
        return DirectionVectors[DirectionForKey(activeKey)].normalized;
    }

    private void HandleStateKeys(bool isMoving)
    {
        if (carryItemType != DogCarryItemType.None)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && (runMode || stamina > 0f))
        {
            runMode = !runMode;
        }

        bool pressedShift = Input.GetKeyDown(KeyCode.LeftShift)
            || Input.GetKeyDown(KeyCode.RightShift);
        if (!isMoving && pressedShift)
        {
            isLyingDown = true;
        }
    }

    private void HandleInteractionKey()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (carryItemType != DogCarryItemType.None)
        {
            DropCarriedItem();
            return;
        }

        TryPickUpNearestItem();
    }

    private void TryPickUpNearestItem()
    {
        DogCarryItemPickup pickup = FindNearestPickup();
        if (pickup == null)
        {
            return;
        }

        carriedPickup = pickup;
        ApplyCarryItem(pickup.ItemType);
        pickup.gameObject.SetActive(false);
    }

    private DogCarryItemPickup FindNearestPickup()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        DogCarryItemPickup nearestPickup = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D itemCollider in colliders)
        {
            DogCarryItemPickup pickup = itemCollider.GetComponentInParent<DogCarryItemPickup>();
            if (pickup == null
                || !pickup.CanBePickedUp
                || pickup.ItemType == DogCarryItemType.None)
            {
                continue;
            }

            float distance = ((Vector2)pickup.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPickup = pickup;
            }
        }

        return nearestPickup;
    }

    private void DropCarriedItem()
    {
        DogCarryItemPickup pickup = carriedPickup;
        ClearCarryItem();
        if (pickup == null)
        {
            return;
        }

        Vector2 dropDirection = DirectionVectors[facingDirection].normalized;
        pickup.transform.position = (Vector2)transform.position + dropDirection * dropDistance;
        pickup.gameObject.SetActive(true);
        ItemDropped?.Invoke(pickup);
    }

    private void ApplyCarryItem(DogCarryItemType itemType)
    {
        carryItemType = itemType;
        isLyingDown = false;
        ApplyCarryScale(itemType);
    }

    private void ApplyCarryScale(DogCarryItemType itemType)
    {
        float multiplier = 1f;
        if (itemType == DogCarryItemType.Slipper)
        {
            multiplier = SlipperScaleMultiplier;
        }
        else if (itemType == DogCarryItemType.Pillow)
        {
            multiplier = PillowScaleMultiplier;
        }

        transform.localScale = baseScale * multiplier;
    }

    private bool IsRunning(bool isMoving)
    {
        return isMoving && !isLyingDown
            && carryItemType == DogCarryItemType.None
            && runMode
            && stamina > 0f;
    }

    private void UpdateStamina(bool isRunning)
    {
        float target = isRunning ? 0f : 1f;
        float duration = Mathf.Max(staminaDuration, 0.1f);
        stamina = Mathf.MoveTowards(stamina, target, Time.deltaTime / duration);
        if (staminaSlider != null)
        {
            staminaSlider.SetValueWithoutNotify(stamina);
        }
    }

    private void Move(Vector2 movement, bool isRunning)
    {
        if (isLyingDown || movement.sqrMagnitude <= 0f)
        {
            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        MoveWithCollisions(movement * speed * Time.deltaTime);
    }

    private void MoveWithCollisions(Vector2 remainingMovement)
    {
        Physics2D.SyncTransforms();
        for (int iteration = 0; iteration < MaxCollisionIterations; iteration++)
        {
            if (remainingMovement.sqrMagnitude <= MinimumMoveDistance * MinimumMoveDistance)
            {
                return;
            }

            Vector2 direction = remainingMovement.normalized;
            float distance = remainingMovement.magnitude;
            int hitCount = CastMovement(direction, distance + CollisionSkin);

            if (!TryGetNearestCollision(hitCount, direction, out RaycastHit2D nearestHit))
            {
                transform.position += (Vector3)remainingMovement;
                return;
            }

            float travelDistance = Mathf.Min(distance, Mathf.Max(0f, nearestHit.distance - CollisionSkin));
            Vector2 traveled = direction * travelDistance;
            transform.position += (Vector3)traveled;
            remainingMovement -= traveled;

            float intoSurface = Vector2.Dot(remainingMovement, nearestHit.normal);
            if (intoSurface < 0f)
            {
                remainingMovement -= nearestHit.normal * intoSurface;
            }

            Physics2D.SyncTransforms();
        }
    }

    private int CastMovement(Vector2 direction, float distance)
    {
        Vector3 scale = movementCollider.transform.lossyScale;
        Vector2 size = Vector2.Scale(
            movementCollider.size,
            new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        Vector2 origin = movementCollider.transform.TransformPoint(movementCollider.offset);
        float angle = movementCollider.transform.eulerAngles.z;
        return Physics2D.BoxCast(
            origin,
            size,
            angle,
            direction,
            movementFilter,
            collisionHits,
            distance);
    }

    private bool TryGetNearestCollision(
        int hitCount,
        Vector2 direction,
        out RaycastHit2D nearestHit)
    {
        nearestHit = default;
        float nearestDistance = float.MaxValue;
        for (int index = 0; index < hitCount; index++)
        {
            RaycastHit2D hit = collisionHits[index];
            if (hit.collider == null
                || hit.collider == movementCollider
                || hit.collider.CompareTag(IgnoredCollisionTag)
                || Vector2.Dot(direction, hit.normal) >= -MinimumBlockingDot
                || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestHit = hit;
            nearestDistance = hit.distance;
        }

        return nearestHit.collider != null;
    }

    private void UpdateAnimation(bool isMoving, bool isRunning = false)
    {
        string stateName = ResolveAnimationName(isMoving, isRunning);
        int stateHash = Animator.StringToHash(stateName);
        if (stateHash == currentStateHash)
        {
            return;
        }

        animator.Play(stateHash);
        currentStateHash = stateHash;
    }

    private string ResolveAnimationName(bool isMoving, bool isRunning)
    {
        if (isLyingDown)
        {
            return "dog_liedown";
        }

        if (carryItemType == DogCarryItemType.Slipper)
        {
            return (isMoving ? "slipper" : "slipper_idle") + facingDirection;
        }

        if (carryItemType == DogCarryItemType.Pillow
            || carryItemType == DogCarryItemType.Banana)
        {
            return (isMoving ? "pillow" : "pillow_idle") + facingDirection;
        }

        string normalState = isMoving ? (isRunning ? "dog_run" : "dog_walk") : "dog_idle";
        return normalState + facingDirection;
    }

    private static int DirectionForKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W:
                return UpRight;
            case KeyCode.A:
                return UpLeft;
            case KeyCode.S:
                return DownLeft;
            default:
                return DownRight;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
