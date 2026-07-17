using System;
using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RobotPatrol : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform[] waypoints;
        [SerializeField, Min(0.1f)] private float speed = 1.4f;

        private int waypointIndex;
        private Vector2 homePosition;
        private Vector2 slipTarget;
        private float slipSpeed;
        private bool slipping;
        private bool paused;
        private string animationState;

        public bool IsSlipping => slipping;
        public event Action SlipFinished;

        private void Awake()
        {
            homePosition = body.position;
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            if (paused)
            {
                return;
            }

            if (slipping)
            {
                MoveToward(slipTarget, slipSpeed, true);
                return;
            }

            if (waypoints.Length > 0)
            {
                Patrol();
            }
        }

        public void SlipTo(Vector2 target, float speedValue)
        {
            slipTarget = target;
            slipSpeed = speedValue;
            slipping = true;
        }

        public void ResetPatrol()
        {
            body.position = homePosition;
            body.velocity = Vector2.zero;
            waypointIndex = 0;
            slipping = false;
            paused = false;
        }

        public void SetPaused(bool value)
        {
            paused = value;
        }

        private void Patrol()
        {
            Vector2 target = waypoints[waypointIndex].position;
            if (MoveToward(target, speed, false))
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
        }

        private bool MoveToward(Vector2 target, float moveSpeed, bool finishSlip)
        {
            Vector2 delta = target - body.position;
            if (delta.magnitude < 0.08f)
            {
                if (finishSlip)
                {
                    slipping = false;
                    SlipFinished?.Invoke();
                }

                return true;
            }

            Vector2 direction = delta.normalized;
            body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
            PlayDirection(direction, finishSlip);
            return false;
        }

        private void PlayDirection(Vector2 direction, bool fast)
        {
            string diagonal = direction.y >= 0f
                ? direction.x < 0f ? "左上" : "右上"
                : direction.x < 0f ? "左下" : "右下";
            string state = "扫地机" + diagonal + (fast ? "加速1" : "待机1");
            if (state == animationState)
            {
                return;
            }

            animationState = state;
            animator.Play(state, 0, 0f);
        }
    }
}
