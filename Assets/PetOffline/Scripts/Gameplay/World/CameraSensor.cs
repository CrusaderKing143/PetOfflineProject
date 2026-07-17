using System;
using PetOffline.Core;
using UnityEngine;

namespace PetOffline.Gameplay
{
    public sealed class CameraSensor : MonoBehaviour
    {
        [SerializeField] private CameraScanConfig config;
        [SerializeField] private Transform sensorHead;
        [SerializeField] private Transform target;

        private float scanDirection = 1f;
        private float scanAngle;
        private float visibleSeconds;
        private float detectionMultiplier = 1f;
        private bool detectionEnabled = true;

        public CameraUiState State { get; private set; } = CameraUiState.Scanning;
        public event Action Discovered;

        private void Update()
        {
            UpdateScan();
            UpdateDetection();
        }

        public void SetTarget(Transform value)
        {
            target = value;
        }

        public void SetDetectionEnabled(bool value)
        {
            detectionEnabled = value;
            if (!value)
            {
                visibleSeconds = 0f;
                State = CameraUiState.Safe;
            }
        }

        public void SetAlertMultiplier(float multiplier)
        {
            detectionMultiplier = Mathf.Max(0.1f, multiplier);
            State = multiplier > 1f ? CameraUiState.Alert : CameraUiState.Scanning;
        }

        public void SetOffline(bool value)
        {
            enabled = !value;
            visibleSeconds = 0f;
            State = value ? CameraUiState.Offline : CameraUiState.Scanning;
        }

        public void ResetSensor()
        {
            visibleSeconds = 0f;
            scanAngle = 0f;
            scanDirection = 1f;
            detectionMultiplier = 1f;
            State = CameraUiState.Scanning;
            ApplyHeadRotation();
        }

        private void UpdateScan()
        {
            scanAngle += scanDirection * config.ScanDegreesPerSecond * Time.deltaTime;
            if (scanAngle >= config.MaximumScanAngle || scanAngle <= config.MinimumScanAngle)
            {
                scanDirection *= -1f;
                scanAngle = Mathf.Clamp(scanAngle, config.MinimumScanAngle, config.MaximumScanAngle);
            }

            ApplyHeadRotation();
        }

        private void UpdateDetection()
        {
            if (!detectionEnabled || target == null || !CanSeeTarget())
            {
                visibleSeconds = 0f;
                return;
            }

            visibleSeconds += Time.deltaTime * detectionMultiplier;
            State = CameraUiState.Alert;
            if (visibleSeconds < config.DiscoveryDuration)
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
            if (toTarget.magnitude > config.Distance)
            {
                return false;
            }

            float angle = Vector2.Angle(sensorHead.up, toTarget);
            if (angle > config.HalfAngle)
            {
                return false;
            }

            RaycastHit2D obstruction = Physics2D.Linecast(
                origin,
                target.position,
                config.ObstructionMask);
            return obstruction.collider == null;
        }

        private void ApplyHeadRotation()
        {
            sensorHead.localRotation = Quaternion.Euler(0f, 0f, scanAngle);
        }

        private void OnDrawGizmosSelected()
        {
            if (sensorHead == null || config == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Vector3 left = Quaternion.Euler(0f, 0f, config.HalfAngle) * sensorHead.up;
            Vector3 right = Quaternion.Euler(0f, 0f, -config.HalfAngle) * sensorHead.up;
            Gizmos.DrawRay(sensorHead.position, left * config.Distance);
            Gizmos.DrawRay(sensorHead.position, right * config.Distance);
        }
    }
}
