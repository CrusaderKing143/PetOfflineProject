using UnityEngine;

namespace PetOffline.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YAxisSorter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private int offset;

        private void LateUpdate()
        {
            targetRenderer.sortingOrder = offset - Mathf.RoundToInt(transform.position.y * 100f);
        }
    }
}
