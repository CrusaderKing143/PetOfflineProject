using UnityEngine;

[DisallowMultipleComponent]
public sealed class BananaPeelBoostTrigger : MonoBehaviour
{
    [SerializeField, Min(0f)] private float boostDuration = 3f;

    private bool consumed;

    public float BoostDuration => boostDuration;

    public bool TryConsume()
    {
        if (consumed)
        {
            return false;
        }

        consumed = true;
        gameObject.SetActive(false);
        return true;
    }
}
