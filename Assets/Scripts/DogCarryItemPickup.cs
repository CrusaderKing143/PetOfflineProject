using UnityEngine;

[DisallowMultipleComponent]
public sealed class DogCarryItemPickup : MonoBehaviour
{
    [SerializeField] private DogCarryItemType itemType = DogCarryItemType.Slipper;

    private bool isLocked;

    public bool CanBePickedUp => !isLocked;
    public DogCarryItemType ItemType => itemType;

    public void Lock()
    {
        isLocked = true;
    }
}
