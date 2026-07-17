using System;
using UnityEngine;

namespace PetOffline
{
    public sealed class CameraSensor : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField] private Transform sensorHead;
        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float distance = 12f;
        [SerializeField, Range(1f, 89f)] private float halfAngle = 30f;
        [SerializeField] private float minimumScanAngle = -35f;
        [SerializeField] private float maximumScanAngle = 35f;
        [SerializeField, Min(0f)] private float scanDegreesPerSecond = 18f;

        [Header("Detection")]
        [SerializeField, Min(0.05f)] private float discoveryDuration = 1.2f;
        [SerializeField] private LayerMask obstructionMask = 1 << 8;

        private float scanDirection = 1f;
        private float scanAngle;
        private float visibleSeconds;
        private float detectionMultiplier = 1f;
        private bool detectionEnabled = true;

        public event Action Discovered;

        private void Update()
        {
            UpdateScan();
            UpdateDetection();
        }

        public void SetDetectionEnabled(bool value)
        {
            detectionEnabled = value;
            if (!value)
            {
                visibleSeconds = 0f;
            }
        }

        public void SetAlertMultiplier(float multiplier)
        {
            detectionMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void SetOffline(bool value)
        {
            enabled = !value;
            visibleSeconds = 0f;
        }

        public void ResetSensor()
        {
            visibleSeconds = 0f;
            scanAngle = 0f;
            scanDirection = 1f;
            detectionMultiplier = 1f;
            ApplyHeadRotation();
        }

        private void UpdateScan()
        {
            scanAngle += scanDirection * scanDegreesPerSecond * Time.deltaTime;
            if (scanAngle >= maximumScanAngle || scanAngle <= minimumScanAngle)
            {
                scanDirection *= -1f;
                scanAngle = Mathf.Clamp(scanAngle, minimumScanAngle, maximumScanAngle);
            }

            ApplyHeadRotation();
        }

        private void UpdateDetection()
        {
            if (!detectionEnabled || !CanSeeTarget())
            {
                visibleSeconds = 0f;
                return;
            }

            visibleSeconds += Time.deltaTime * detectionMultiplier;
            if (visibleSeconds < discoveryDuration)
            {
                return;
            }

            visibleSeconds = 0f;
            Discovered?.Invoke();
        }

        private bool CanSeeTarget()
        {
            Vector2 origin = sensorHead.position;
            Vector2 toTarget = (Vector2)target.position - origin;
            if (toTarget.magnitude > distance
                || Vector2.Angle(sensorHead.up, toTarget) > halfAngle)
            {
                return false;
            }

            RaycastHit2D obstruction = Physics2D.Linecast(
                origin, target.position, obstructionMask);
            return !obstruction;
        }

        private void ApplyHeadRotation()
        {
            sensorHead.localRotation = Quaternion.Euler(0f, 0f, scanAngle);
        }

        private void OnDrawGizmosSelected()
        {
            if (!sensorHead)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Vector3 left = Quaternion.Euler(0f, 0f, halfAngle) * sensorHead.up;
            Vector3 right = Quaternion.Euler(0f, 0f, -halfAngle) * sensorHead.up;
            Gizmos.DrawRay(sensorHead.position, left * distance);
            Gizmos.DrawRay(sensorHead.position, right * distance);
        }
    }
}
