using System.Collections.Generic;
using UnityEngine;

public enum SweepingRobotAnimationSet
{
    Scene1 = 1,
    Scene2 = 2
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class SweepingRobotRouteFollower : MonoBehaviour
{
    private static readonly Vector2[] Directions =
    {
        new Vector2(-1f, -0.5f).normalized,
        new Vector2(1f, -0.5f).normalized,
        new Vector2(-1f, 0.5f).normalized,
        new Vector2(1f, 0.5f).normalized
    };

    private static readonly string[] DirectionNames =
    {
        "左下",
        "右下",
        "左上",
        "右上"
    };

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SweepingRobotAnimationSet animationSet = SweepingRobotAnimationSet.Scene1;
    [SerializeField, Min(0f)] private float normalSpeed = 1.5f;
    [SerializeField, Min(0f)] private float boostedSpeed = 3f;
    [SerializeField, Min(0.001f)] private float arrivalThreshold = 0.05f;
    [SerializeField] private List<Vector2> routePoints = new List<Vector2>();

    private float boostTimeRemaining;
    private int currentStateHash;
    private int targetPointIndex;
    private bool singlePointReached;

    public IReadOnlyList<Vector2> RoutePoints => routePoints;

    private void Awake()
    {
        ResolveComponents();
        ConfigureBody();
    }

    private void OnEnable()
    {
        targetPointIndex = 0;
        singlePointReached = false;
        currentStateHash = 0;
        PauseAnimation();
    }

    private void Update()
    {
        if (boostTimeRemaining > 0f)
        {
            boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!TryGetMovement(out Vector2 direction, out Vector2 target))
        {
            PauseAnimation();
            return;
        }

        bool isBoosted = boostTimeRemaining > 0f;
        float speed = isBoosted ? boostedSpeed : normalSpeed;
        Vector2 nextPosition = Vector2.MoveTowards(
            body.position,
            target,
            speed * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);
        PlayMovementAnimation(direction, isBoosted);
    }

    public void ActivateBoost(float duration)
    {
        boostTimeRemaining = Mathf.Max(0f, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BananaPeelBoostTrigger banana = other.GetComponentInParent<BananaPeelBoostTrigger>();
        if (banana == null)
        {
            return;
        }

        float duration = banana.BoostDuration;
        if (banana.TryConsume())
        {
            ActivateBoost(duration);
        }
    }

    private bool TryGetMovement(out Vector2 direction, out Vector2 target)
    {
        direction = Vector2.zero;
        target = body.position;
        if (routePoints.Count == 0 || singlePointReached)
        {
            return false;
        }

        targetPointIndex = Mathf.Clamp(targetPointIndex, 0, routePoints.Count - 1);
        for (int attempt = 0; attempt < routePoints.Count; attempt++)
        {
            target = routePoints[targetPointIndex];
            Vector2 offset = target - body.position;
            if (offset.sqrMagnitude > arrivalThreshold * arrivalThreshold)
            {
                direction = offset.normalized;
                return true;
            }

            AdvanceTarget();
            if (singlePointReached)
            {
                return false;
            }
        }

        return false;
    }

    private void AdvanceTarget()
    {
        if (routePoints.Count == 1)
        {
            singlePointReached = true;
            return;
        }

        targetPointIndex = (targetPointIndex + 1) % routePoints.Count;
    }

    private void PlayMovementAnimation(Vector2 movement, bool isBoosted)
    {
        animator.speed = 1f;
        string directionName = ResolveDirectionName(movement);
        string motionName = isBoosted ? "加速" : "待机";
        string stateName = "扫地机" + directionName + motionName + (int)animationSet;
        int stateHash = Animator.StringToHash(stateName);
        if (stateHash == currentStateHash)
        {
            return;
        }

        animator.Play(stateHash);
        currentStateHash = stateHash;
    }

    private static string ResolveDirectionName(Vector2 movement)
    {
        int bestIndex = 0;
        float bestDot = float.MinValue;

        for (int index = 0; index < Directions.Length; index++)
        {
            float dot = Vector2.Dot(movement, Directions[index]);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = index;
            }
        }

        return DirectionNames[bestIndex];
    }

    private void PauseAnimation()
    {
        if (animator != null)
        {
            animator.speed = 0f;
        }
    }

    private void ResolveComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    private void ConfigureBody()
    {
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void Reset()
    {
        ResolveComponents();
        ConfigureBody();
    }
}
