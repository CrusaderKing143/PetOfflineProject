using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class DogController : MonoBehaviour
{
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

    private readonly List<KeyCode> heldDirectionKeys = new List<KeyCode>(4);
    private DogCarryItemPickup carriedPickup;
    private DogCarryItemType carryItemType;
    private int currentStateHash;
    private int facingDirection = DownRight;
    private bool isLyingDown;
    private bool runMode;

    public DogCarryItemType CarryItemType => carryItemType;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
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
        Move(movement);
        UpdateAnimation(isMoving);
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

        if (Input.GetKeyDown(KeyCode.Q))
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
            if (pickup == null || pickup.ItemType == DogCarryItemType.None)
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
    }

    private void ApplyCarryItem(DogCarryItemType itemType)
    {
        carryItemType = itemType;
        isLyingDown = false;
    }

    private void Move(Vector2 movement)
    {
        if (isLyingDown || movement.sqrMagnitude <= 0f)
        {
            return;
        }

        bool canRun = carryItemType == DogCarryItemType.None && runMode;
        float speed = canRun ? runSpeed : walkSpeed;
        transform.position += (Vector3)(movement * speed * Time.deltaTime);
    }

    private void UpdateAnimation(bool isMoving)
    {
        string stateName = ResolveAnimationName(isMoving);
        int stateHash = Animator.StringToHash(stateName);
        if (stateHash == currentStateHash)
        {
            return;
        }

        animator.Play(stateHash);
        currentStateHash = stateHash;
    }

    private string ResolveAnimationName(bool isMoving)
    {
        if (isLyingDown)
        {
            return "dog_liedown";
        }

        if (carryItemType == DogCarryItemType.Slipper)
        {
            return (isMoving ? "slipper" : "slipper_idle") + facingDirection;
        }

        if (carryItemType == DogCarryItemType.Pillow)
        {
            return (isMoving ? "pillow" : "pillow_idle") + facingDirection;
        }

        string normalState = isMoving ? (runMode ? "dog_run" : "dog_walk") : "dog_idle";
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
