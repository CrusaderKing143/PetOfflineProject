using UnityEngine;

namespace PetOffline.Gameplay
{
    [CreateAssetMenu(menuName = "Pet Offline/Day 1 Config")]
    public sealed class Day1Config : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float shoesHoldSeconds = 2f;
        [SerializeField, Min(0.1f)] private float firstBossCallSeconds = 14f;
        [SerializeField, Min(0.1f)] private float laterBossCallSeconds = 26f;
        [SerializeField, Min(0.1f)] private float responseWindowSeconds = 3.6f;
        [SerializeField, Min(0f)] private float safeWindowSeconds = 3f;
        [SerializeField, Min(0f)] private float alertSeconds = 7f;

        public float ShoesHoldSeconds => shoesHoldSeconds;
        public float FirstBossCallSeconds => firstBossCallSeconds;
        public float LaterBossCallSeconds => laterBossCallSeconds;
        public float ResponseWindowSeconds => responseWindowSeconds;
        public float SafeWindowSeconds => safeWindowSeconds;
        public float AlertSeconds => alertSeconds;
    }
}
