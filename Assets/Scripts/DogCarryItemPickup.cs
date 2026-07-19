using UnityEngine;

[DisallowMultipleComponent]
public sealed class DogCarryItemPickup : MonoBehaviour
{
    [SerializeField] private DogCarryItemType itemType = DogCarryItemType.Slipper;

    public DogCarryItemType ItemType => itemType;
}
