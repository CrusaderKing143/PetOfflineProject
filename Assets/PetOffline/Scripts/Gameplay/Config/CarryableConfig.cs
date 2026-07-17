using UnityEngine;

namespace PetOffline.Gameplay
{
    [CreateAssetMenu(menuName = "Pet Offline/Carryable Config")]
    public sealed class CarryableConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float dropDistance = 0.65f;
        [SerializeField, Range(0.1f, 1f)] private float pillowMoveMultiplier = 0.6f;
        [SerializeField, Min(0f)] private float droppedLinearDrag = 2f;

        public float DropDistance => dropDistance;
        public float PillowMoveMultiplier => pillowMoveMultiplier;
        public float DroppedLinearDrag => droppedLinearDrag;
    }
}
