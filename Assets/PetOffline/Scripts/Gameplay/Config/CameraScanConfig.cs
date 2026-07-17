using UnityEngine;

namespace PetOffline.Gameplay
{
    [CreateAssetMenu(menuName = "Pet Offline/Camera Scan Config")]
    public sealed class CameraScanConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float distance = 6f;
        [SerializeField, Range(1f, 89f)] private float halfAngle = 28f;
        [SerializeField, Min(0.05f)] private float discoveryDuration = 1.2f;
        [SerializeField] private float minimumScanAngle = -35f;
        [SerializeField] private float maximumScanAngle = 35f;
        [SerializeField, Min(0f)] private float scanDegreesPerSecond = 18f;
        [SerializeField] private LayerMask obstructionMask;

        public float Distance => distance;
        public float HalfAngle => halfAngle;
        public float DiscoveryDuration => discoveryDuration;
        public float MinimumScanAngle => minimumScanAngle;
        public float MaximumScanAngle => maximumScanAngle;
        public float ScanDegreesPerSecond => scanDegreesPerSecond;
        public LayerMask ObstructionMask => obstructionMask;
    }
}
