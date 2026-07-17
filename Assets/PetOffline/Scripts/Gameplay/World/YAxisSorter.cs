using UnityEngine;

namespace PetOffline
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YAxisSorter : MonoBehaviour
    {
        [SerializeField] private int offset;
        private SpriteRenderer targetRenderer;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            targetRenderer.sortingOrder = offset - Mathf.RoundToInt(transform.position.y * 100f);
        }
    }
}
